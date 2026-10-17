using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastDbExplorer.Wpf.Localization;
using FastDbExplorer.Wpf.Services;

namespace FastDbExplorer.Wpf.ViewModels;

/// <summary>One selectable marker icon in a layer's picker (Key null = the default dot marker).</summary>
public sealed partial class IconChoice : ObservableObject
{
    private readonly CoordinateLayerItem _owner;

    public IconChoice(CoordinateLayerItem owner, string? key)
    {
        _owner = owner;
        Key = key;
        Name = key is null ? MapUiStrings.DefaultMarker : MapUiStrings.IconName(key);
        Shape = MapIconGeometry.For(key ?? "dot");
    }

    public string? Key { get; }
    public string Name { get; }
    public Geometry? Shape { get; }

    [ObservableProperty] private bool _isSelected;

    [RelayCommand]
    private void Select() => _owner.SetIcon(Key);
}

/// <summary>One folder of the icon picker (for example "health"): its name, its icons and whether it is the open one.</summary>
public sealed partial class IconCategoryChoice : ObservableObject
{
    public IconCategoryChoice(string key, string name, IReadOnlyList<IconChoice> icons)
    {
        Key = key;
        Name = name;
        Icons = icons;
    }

    public string Key { get; }
    public string Name { get; }
    public IReadOnlyList<IconChoice> Icons { get; }

    [ObservableProperty] private bool _isSelected;
}

/// <summary>One selectable marker colour in a layer's picker.</summary>
public sealed partial class ColorChoice : ObservableObject
{
    private readonly CoordinateLayerItem _owner;

    public ColorChoice(CoordinateLayerItem owner, string hex)
    {
        _owner = owner;
        Hex = hex;
        Brush = MapIconGeometry.BrushFor(hex);
    }

    public string Hex { get; }
    public Brush Brush { get; }

    [ObservableProperty] private bool _isSelected;

    [RelayCommand]
    private void Select() => _owner.SetColor(Hex);
}
