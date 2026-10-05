using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Localization;

namespace FastDbExplorer.Wpf.ViewModels;

/// <summary>One imported Excel layer in the panel: visibility, radius (metres) and the zoom/refresh/remove commands.</summary>
public sealed partial class CoordinateLayerItem : ObservableObject
{
    private readonly MapViewModel _owner;
    private bool _syncing; // true while values are copied from the model: those changes must not be saved again

    public CoordinateLayerItem(MapLayer layer, MapViewModel owner)
    {
        _owner = owner;
        Layer = layer;
        _syncing = true;
        try
        {
            IsVisible = layer.Visibility;
            RadiusText = FormatRadius(layer.RadiusMeters);
        }
        finally
        {
            _syncing = false;
        }
    }

    public MapLayer Layer { get; private set; }

    public Guid Id => Layer.Id;

    public string Name => Layer.Name;

    public string Summary => CoordinateStrings.Summary(Layer.Points.Count, Layer.FileName);

    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private string _radiusText = "";
    [ObservableProperty] private string _radiusError = "";

    public bool HasRadiusError => RadiusError.Length > 0;

    partial void OnRadiusErrorChanged(string value) => OnPropertyChanged(nameof(HasRadiusError));

    partial void OnIsVisibleChanged(bool value)
    {
        if (!_syncing) _owner.OnCoordinateLayerVisibilityChanged(this, value);
    }

    [RelayCommand]
    private Task ApplyRadiusAsync() => _owner.ApplyRadiusAsync(this);

    [RelayCommand]
    private void Zoom() => _owner.ZoomToCoordinateLayer(this);

    [RelayCommand]
    private Task RefreshAsync() => _owner.RefreshCoordinateLayerAsync(this);

    [RelayCommand]
    private Task RemoveAsync() => _owner.RemoveCoordinateLayerAsync(this);

    /// <summary>Takes over a freshly loaded copy of the layer without triggering a save.</summary>
    internal void Sync(MapLayer layer)
    {
        Layer = layer;
        _syncing = true;
        try
        {
            IsVisible = layer.Visibility;
            RadiusText = FormatRadius(layer.RadiusMeters);
            RadiusError = "";
        }
        finally
        {
            _syncing = false;
        }
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Summary));
    }

    internal static string FormatRadius(double meters) => meters.ToString("0.###", CultureInfo.InvariantCulture);
}
