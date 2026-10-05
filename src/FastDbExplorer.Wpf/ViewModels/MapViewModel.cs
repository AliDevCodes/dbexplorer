using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Localization;
using FastDbExplorer.Wpf.Services;

namespace FastDbExplorer.Wpf.ViewModels;

public sealed partial class MapLayerItem(string name, Action<string, bool> onChanged) : ObservableObject
{
    public string Name => name;

    [ObservableProperty] private bool _isVisible = true;

    partial void OnIsVisibleChanged(bool value) => onChanged(name, value);
}

/// <summary>
/// Map workspace: opens a map file, owns the opened source, and relays commands to the page through events
/// (the view talks to the WebView2/MapLibre page; this class never touches UI controls).
/// </summary>
public sealed partial class MapViewModel : ObservableObject
{
    private readonly IMapSourceFactory _factory;
    private readonly IFileDialogService _dialogs;

    public MapViewModel(IMapSourceFactory factory, IFileDialogService dialogs)
    {
        _factory = factory;
        _dialogs = dialogs;
        ResetInfo();
    }

    public event Action? BackRequested;
    public event Action? SourceChanged;
    public event Action<string, bool>? LayerToggled;
    public event Action? FitRequested;

    /// <summary>The opened map file. The view reads tiles from it.</summary>
    public IMapSource? Source { get; private set; }

    public ObservableCollection<MapLayerItem> Layers { get; } = [];

    [ObservableProperty] private bool _hasMap;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _fileName = "";
    [ObservableProperty] private string _formatText = "";
    [ObservableProperty] private string _cursorText = "";
    [ObservableProperty] private string _zoomText = "";
    [ObservableProperty] private string _infoTitle = "";
    [ObservableProperty] private string _infoBody = "";
    [ObservableProperty] private bool _infoIsProblem;
    [ObservableProperty] private bool _isLayersOpen = true;
    [ObservableProperty] private bool _hasLayers;

    public bool ShowLayersPanel => HasMap && IsLayersOpen && HasLayers;

    partial void OnHasMapChanged(bool value) => OnPropertyChanged(nameof(ShowLayersPanel));
    partial void OnIsLayersOpenChanged(bool value) => OnPropertyChanged(nameof(ShowLayersPanel));
    partial void OnHasLayersChanged(bool value) => OnPropertyChanged(nameof(ShowLayersPanel));

    [RelayCommand]
    private void Back() => BackRequested?.Invoke();

    [RelayCommand]
    private void Fit() => FitRequested?.Invoke();

    [RelayCommand]
    private void ToggleLayers() => IsLayersOpen = !IsLayersOpen;

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var path = _dialogs.PickMapFile();
        if (path is not null) await OpenPathAsync(path);
    }

    public async Task OpenPathAsync(string path)
    {
        IsLoading = true;
        try
        {
            var source = await _factory.OpenAsync(path, CancellationToken.None);

            Source?.Dispose();
            Source = source;
            Layers.Clear();
            foreach (var name in source.Info.Layers)
                Layers.Add(new MapLayerItem(name, (n, v) => LayerToggled?.Invoke(n, v)));
            HasLayers = Layers.Count > 0;

            FileName = Path.GetFileName(path);
            FormatText = $"{source.Info.FormatLabel} · {(source.Info.Kind == MapTileKind.Vector ? "Vector" : "Raster")}";
            CursorText = "";
            ZoomText = "";
            HasMap = true;
            SourceChanged?.Invoke();
        }
        catch (UnsupportedMapFormatException ex)
        {
            ShowProblem(ex.Reason switch
            {
                UnsupportedMapReason.OsmPbfNeedsConversion => (Strings.OsmTitle, Strings.OsmBody + ex.Message),
                UnsupportedMapReason.UnknownSqliteSchema => (Strings.SchemaTitle, Strings.SchemaBody + ex.Message),
                _ => (Strings.UnknownTitle, Strings.UnknownBody + ex.Message)
            });
        }
        catch (Exception ex) when (ex is MapSourceException or IOException or UnauthorizedAccessException)
        {
            ShowProblem((Strings.CorruptTitle, ex.Message));
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ReportAssetsMissing() => ShowProblem((Strings.AssetsMissingTitle, Strings.AssetsMissingBody));

    public void ReportWebViewProblem(string details) =>
        ShowProblem((Strings.WebViewMissingTitle, Strings.WebViewMissingBody + details));

    public void SetCursor(double lng, double lat) =>
        CursorText = string.Create(CultureInfo.InvariantCulture, $"{lat:F5}, {lng:F5}");

    public void SetZoom(double zoom) =>
        ZoomText = string.Create(CultureInfo.InvariantCulture, $"{Strings.Zoom} {zoom:F1}");

    private void ShowProblem((string Title, string Body) problem)
    {
        HasMap = false;
        InfoIsProblem = true;
        (InfoTitle, InfoBody) = problem;
    }

    private void ResetInfo()
    {
        InfoIsProblem = false;
        InfoTitle = Strings.MapEmptyTitle;
        InfoBody = Strings.MapEmptyBody;
    }
}
