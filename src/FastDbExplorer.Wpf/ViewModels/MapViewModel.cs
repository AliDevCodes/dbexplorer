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
/// Phase 2 adds the Excel coordinate layers: import, list, visibility, radius (metres), refresh, zoom, remove.
/// </summary>
public sealed partial class MapViewModel : ObservableObject
{
    private readonly IMapSourceFactory _factory;
    private readonly IFileDialogService _dialogs;
    private readonly IExcelImportService _excel;
    private readonly ICoordinateLayerStore _store;
    private readonly SemaphoreSlim _storeGate = new(1, 1); // one store operation at a time (the store writes whole files)
    private bool _coordinateLayersLoaded;

    public MapViewModel(IMapSourceFactory factory, IFileDialogService dialogs, IExcelImportService excel, ICoordinateLayerStore store)
    {
        _factory = factory;
        _dialogs = dialogs;
        _excel = excel;
        _store = store;
        CoordinateLayers.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasCoordinateLayers));
            OnPropertyChanged(nameof(ShowCoordinatePanel));
        };
        ResetInfo();
    }

    public event Action? BackRequested;
    public event Action? SourceChanged;
    public event Action<string, bool>? LayerToggled;
    public event Action? FitRequested;

    /// <summary>A coordinate layer was added, or its data/radius changed: the view (re)sends it to the page.</summary>
    public event Action<MapLayer>? CoordinateLayerChanged;
    public event Action<Guid, bool>? CoordinateLayerVisibilityChanged;
    public event Action<Guid>? CoordinateLayerRemoved;
    public event Action<Guid>? CoordinateLayerZoomRequested;

    /// <summary>The opened map file. The view reads tiles from it.</summary>
    public IMapSource? Source { get; private set; }

    public ObservableCollection<MapLayerItem> Layers { get; } = [];

    public ObservableCollection<CoordinateLayerItem> CoordinateLayers { get; } = [];

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
    [ObservableProperty] private string _coordinateStatus = "";
    [ObservableProperty] private bool _coordinateStatusIsProblem;

    public bool ShowLayersPanel => HasMap && IsLayersOpen && HasLayers;

    public bool HasCoordinateLayers => CoordinateLayers.Count > 0;

    public bool HasCoordinateStatus => CoordinateStatus.Length > 0;

    /// <summary>The coordinate panel needs the map (the layers are drawn on it) and has something to show.</summary>
    public bool ShowCoordinatePanel => HasMap && (HasCoordinateLayers || HasCoordinateStatus);

    partial void OnHasMapChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowLayersPanel));
        OnPropertyChanged(nameof(ShowCoordinatePanel));
    }

    partial void OnIsLayersOpenChanged(bool value) => OnPropertyChanged(nameof(ShowLayersPanel));
    partial void OnHasLayersChanged(bool value) => OnPropertyChanged(nameof(ShowLayersPanel));

    partial void OnCoordinateStatusChanged(string value)
    {
        OnPropertyChanged(nameof(HasCoordinateStatus));
        OnPropertyChanged(nameof(ShowCoordinatePanel));
    }

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

    // ---- Excel coordinate layers (Phase 2) --------------------------------------------------------------------

    /// <summary>Loads the saved layers once (called by the view when it appears). Safe to call repeatedly.</summary>
    public async Task EnsureCoordinateLayersLoadedAsync()
    {
        if (_coordinateLayersLoaded) return;
        _coordinateLayersLoaded = true;
        try
        {
            IReadOnlyList<MapLayer> stored;
            await _storeGate.WaitAsync();
            try { stored = await _store.LoadAllAsync(); }
            finally { _storeGate.Release(); }

            foreach (var layer in stored)
                if (CoordinateLayers.All(i => i.Id != layer.Id)) AddCoordinateLayer(layer);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _coordinateLayersLoaded = false;
            SetCoordinateStatus(CoordinateStrings.LoadFailed + ex.Message, true);
        }
    }

    [RelayCommand]
    private async Task ImportExcelAsync()
    {
        var path = _dialogs.PickExcelFile();
        if (path is null) return;

        var result = await _excel.ImportAsync(path, CancellationToken.None);
        if (result.Layer is not { } layer)
        {
            SetCoordinateStatus(CoordinateStrings.ImportFailed(result.Issues), true);
            return;
        }

        AddCoordinateLayer(layer);
        SetCoordinateStatus(
            CoordinateStrings.Imported(result.RowsImported, result.RowsSkipped, result.Issues.Count > 0 ? result.Issues[0] : null),
            false);
        await SaveLayerAsync(layer); // a save problem replaces the success text
    }

    private void AddCoordinateLayer(MapLayer layer)
    {
        CoordinateLayers.Add(new CoordinateLayerItem(layer, this));
        CoordinateLayerChanged?.Invoke(layer);
    }

    internal void OnCoordinateLayerVisibilityChanged(CoordinateLayerItem item, bool visible)
    {
        item.Layer.Visibility = visible;
        CoordinateLayerVisibilityChanged?.Invoke(item.Id, visible);
        _ = SaveLayerAsync(item.Layer);
    }

    internal async Task ApplyRadiusAsync(CoordinateLayerItem item)
    {
        // The only place where user text becomes metres; everything after this (model, store, circles) is metres.
        if (!RadiusInput.TryParse(item.RadiusText, out var meters))
        {
            item.RadiusError = CoordinateStrings.RadiusInvalid;
            return;
        }

        item.RadiusError = "";
        item.Layer.RadiusMeters = meters;
        item.RadiusText = CoordinateLayerItem.FormatRadius(meters);
        CoordinateLayerChanged?.Invoke(item.Layer);
        await SaveLayerAsync(item.Layer);
    }

    internal void ZoomToCoordinateLayer(CoordinateLayerItem item) => CoordinateLayerZoomRequested?.Invoke(item.Id);

    /// <summary>Re-reads the layer from the store (the stored copy wins) and redraws it on the map.</summary>
    internal async Task RefreshCoordinateLayerAsync(CoordinateLayerItem item)
    {
        try
        {
            IReadOnlyList<MapLayer> stored;
            await _storeGate.WaitAsync();
            try { stored = await _store.LoadAllAsync(); }
            finally { _storeGate.Release(); }

            var fresh = stored.FirstOrDefault(l => l.Id == item.Id);
            if (fresh is null)
            {
                CoordinateLayerChanged?.Invoke(item.Layer);
                SetCoordinateStatus(CoordinateStrings.NotInStore(item.Name), true);
                return;
            }

            item.Sync(fresh);
            CoordinateLayerChanged?.Invoke(fresh);
            SetCoordinateStatus(CoordinateStrings.Refreshed(item.Name), false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SetCoordinateStatus(CoordinateStrings.LoadFailed + ex.Message, true);
        }
    }

    internal async Task RemoveCoordinateLayerAsync(CoordinateLayerItem item)
    {
        try
        {
            await _storeGate.WaitAsync();
            try { await _store.RemoveAsync(item.Id); }
            finally { _storeGate.Release(); }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SetCoordinateStatus(CoordinateStrings.RemoveFailed + ex.Message, true);
            return;
        }

        CoordinateLayers.Remove(item);
        CoordinateLayerRemoved?.Invoke(item.Id);
    }

    private async Task SaveLayerAsync(MapLayer layer)
    {
        try
        {
            await _storeGate.WaitAsync();
            try { await _store.SaveAsync(layer); }
            finally { _storeGate.Release(); }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SetCoordinateStatus(CoordinateStrings.SaveFailed + ex.Message, true);
        }
    }

    private void SetCoordinateStatus(string text, bool isProblem)
    {
        CoordinateStatusIsProblem = isProblem;
        CoordinateStatus = text;
    }
}
