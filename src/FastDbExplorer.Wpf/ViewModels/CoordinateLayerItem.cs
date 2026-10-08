using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Localization;

namespace FastDbExplorer.Wpf.ViewModels;

/// <summary>
/// One imported Excel layer in the panel: name, point count, radius (metres), visibility,
/// and the zoom / refresh / rename / delete commands.
/// </summary>
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

    public string FileName => Layer.FileName;

    public string Summary => CoordinateStrings.Summary(Layer.Points.Count, Layer.FileName);

    /// <summary>"1,250 نقطه"</summary>
    public string PointsText => CoordinateStrings.PointsChip(Layer.Points.Count);

    /// <summary>"شعاع 500 متر" or "بدون شعاع" (the applied radius, not the text being typed).</summary>
    public string RadiusSummary => CoordinateStrings.RadiusChip(Layer.RadiusMeters);

    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private string _radiusText = "";
    [ObservableProperty] private string _radiusError = "";

    [ObservableProperty] private bool _isRenaming;
    [ObservableProperty] private string _editName = "";
    [ObservableProperty] private string _editError = "";
    [ObservableProperty] private bool _isConfirmingDelete;

    public bool HasRadiusError => RadiusError.Length > 0;
    public bool HasEditError => EditError.Length > 0;
    public bool IsNotRenaming => !IsRenaming;
    public bool IsNotConfirmingDelete => !IsConfirmingDelete;

    partial void OnRadiusErrorChanged(string value) => OnPropertyChanged(nameof(HasRadiusError));
    partial void OnEditErrorChanged(string value) => OnPropertyChanged(nameof(HasEditError));
    partial void OnIsRenamingChanged(bool value) => OnPropertyChanged(nameof(IsNotRenaming));
    partial void OnIsConfirmingDeleteChanged(bool value) => OnPropertyChanged(nameof(IsNotConfirmingDelete));

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
    private void BeginRename()
    {
        IsConfirmingDelete = false;
        EditName = Layer.Name;
        EditError = "";
        IsRenaming = true;
    }

    [RelayCommand]
    private Task CommitRenameAsync() => _owner.RenameCoordinateLayerAsync(this);

    [RelayCommand]
    private void CancelRename()
    {
        EditError = "";
        IsRenaming = false;
    }

    [RelayCommand]
    private void RequestDelete()
    {
        IsRenaming = false;
        IsConfirmingDelete = true;
    }

    [RelayCommand]
    private void CancelDelete() => IsConfirmingDelete = false;

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        await _owner.RemoveCoordinateLayerAsync(this);
        IsConfirmingDelete = false; // stays visible only when the removal failed
    }

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
        NotifyLayerDataChanged();
    }

    /// <summary>Raises the change notifications for everything derived from the model (name, counts, radius chip).</summary>
    internal void NotifyLayerDataChanged()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(FileName));
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(PointsText));
        OnPropertyChanged(nameof(RadiusSummary));
    }

    internal static string FormatRadius(double meters) => meters.ToString("0.###", CultureInfo.InvariantCulture);
}
