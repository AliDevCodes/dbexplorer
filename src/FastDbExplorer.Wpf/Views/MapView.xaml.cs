using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Services;
using FastDbExplorer.Wpf.ViewModels;
using Microsoft.Web.WebView2.Core;

namespace FastDbExplorer.Wpf.Views;

/// <summary>
/// Hosts the MapLibre page in WebView2. Tiles are never fetched from the network: requests to
/// https://tiles.local/{z}/{x}/{y} are intercepted and answered from the opened map file.
/// </summary>
public partial class MapView : UserControl
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private MapViewModel? _viewModel;
    private bool _pageReady;

    public MapView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) => { Detach(); _viewModel = e.NewValue as MapViewModel; Attach(); };
        Loaded += async (_, _) => { Attach(); await InitializeWebAsync(); };
        Unloaded += (_, _) => { Detach(); Web.Dispose(); };
    }

    private void Attach()
    {
        Detach();
        if (_viewModel is null) return;
        _viewModel.SourceChanged += PushSource;
        _viewModel.LayerToggled += PostLayer;
        _viewModel.FitRequested += PostFit;
    }

    private void Detach()
    {
        if (_viewModel is null) return;
        _viewModel.SourceChanged -= PushSource;
        _viewModel.LayerToggled -= PostLayer;
        _viewModel.FitRequested -= PostFit;
    }

    private async Task InitializeWebAsync()
    {
        if (_viewModel is null) return;

        var assets = Path.Combine(AppContext.BaseDirectory, "Assets", "Map");
        if (!File.Exists(Path.Combine(assets, "maplibre-gl.js")))
        {
            _viewModel.ReportAssetsMissing();
            return;
        }

        try
        {
            var dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FastDbExplorer", "WebView2");
            var environment = await CoreWebView2Environment.CreateAsync(null, dataFolder);
            await Web.EnsureCoreWebView2Async(environment);
        }
        catch (Exception ex) // UI boundary: a missing runtime must become a readable message, not a crash
        {
            _viewModel.ReportWebViewProblem(ex.Message);
            return;
        }

        var core = Web.CoreWebView2;
#if DEBUG
        core.Settings.AreDevToolsEnabled = true; // F12 for diagnosing map problems (debug builds only)
#else
        core.Settings.AreDevToolsEnabled = false;
#endif
        core.Settings.IsStatusBarEnabled = false;
        Web.AllowExternalDrop = false;
        core.SetVirtualHostNameToFolderMapping("app.local", assets, CoreWebView2HostResourceAccessKind.Allow);
        // MapLibre fetches tiles inside Web Workers. The plain AddWebResourceRequestedFilter only sees requests from the
        // page itself (Document), so worker requests would never reach OnTileRequested and the map stays blank.
        core.AddWebResourceRequestedFilter(
            "https://tiles.local/*", CoreWebView2WebResourceContext.All, CoreWebView2WebResourceRequestSourceKinds.All);
        core.WebResourceRequested += OnTileRequested;
        core.WebMessageReceived += OnMessage;
        core.Navigate("https://app.local/map.html");
    }

    private async void OnTileRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        var deferral = e.GetDeferral();
        try
        {
            var parts = new Uri(e.Request.Uri).AbsolutePath.Trim('/').Split('/');
            var source = _viewModel?.Source;
            if (source is null || parts.Length != 3
                || !int.TryParse(parts[0], out var z) || !int.TryParse(parts[1], out var x) || !int.TryParse(parts[2], out var y))
            {
                e.Response = Respond(404, "Not Found", null, null);
                return;
            }

            // Read + gunzip off the UI thread; the page then receives plain protobuf, no Content-Encoding needed.
            var tile = await Task.Run(async () =>
            {
                var t = await source.GetTileAsync(z, x, y, CancellationToken.None);
                return t is { IsGzip: true } ? t with { Data = Gunzip(t.Data), IsGzip = false } : t;
            });
            e.Response = tile is null
                ? Respond(404, "Not Found", null, null)
                : Respond(200, "OK", tile.Data, tile.ContentType);
        }
        catch (Exception) // event-handler boundary: answer 500 so the page keeps working
        {
            e.Response = Respond(500, "Error", null, null);
        }
        finally
        {
            deferral.Complete();
        }
    }

    private static byte[] Gunzip(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output.ToArray();
    }

    private CoreWebView2WebResourceResponse Respond(int status, string reason, byte[]? data, string? contentType)
    {
        var headers = "Access-Control-Allow-Origin: *\r\nCache-Control: max-age=3600";
        if (contentType is not null)
        {
            var gzip = contentType.EndsWith("|gzip", StringComparison.Ordinal);
            headers += $"\r\nContent-Type: {contentType.Replace("|gzip", "")}";
            if (gzip) headers += "\r\nContent-Encoding: gzip"; // the browser unzips vector tiles for us
        }
        return Web.CoreWebView2.Environment.CreateWebResourceResponse(
            new MemoryStream(data ?? []), status, reason, headers);
    }

    private void OnMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (_viewModel is null) return;
        using var doc = JsonDocument.Parse(e.WebMessageAsJson);
        var root = doc.RootElement;
        switch (root.GetProperty("type").GetString())
        {
            case "ready":
                _pageReady = true;
                PushSource();
                break;
            case "cursor":
                _viewModel.SetCursor(root.GetProperty("lng").GetDouble(), root.GetProperty("lat").GetDouble());
                break;
            case "zoom":
                _viewModel.SetZoom(root.GetProperty("zoom").GetDouble());
                break;
        }
    }

    private void PushSource()
    {
        if (!_pageReady || _viewModel?.Source is not { } source) return;
        var info = source.Info;
        Post(new
        {
            type = "load",
            dark = ThemeService.IsDark(),
            info = new
            {
                name = info.Name,
                formatLabel = info.FormatLabel,
                kind = info.Kind == MapTileKind.Vector ? "vector" : "raster",
                minZoom = info.MinZoom,
                maxZoom = info.MaxZoom,
                bounds = info.Bounds,
                center = info.Center,
                layers = info.Layers
            }
        });
        foreach (var layer in _viewModel.Layers.Where(l => !l.IsVisible)) PostLayer(layer.Name, false);
    }

    private void PostLayer(string name, bool visible) => Post(new { type = "layer", name, visible });

    private void PostFit() => Post(new { type = "fit" });

    private void Post(object message)
    {
        if (_pageReady && Web.CoreWebView2 is not null)
            Web.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message, Json));
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (_viewModel is not null && e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
            _ = _viewModel.OpenPathAsync(files[0]);
    }
}
