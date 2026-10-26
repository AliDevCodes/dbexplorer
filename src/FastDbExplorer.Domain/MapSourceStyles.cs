namespace FastDbExplorer.Domain;

/// <summary>Where a map layer comes from. The kind decides the marker shape and colour, so the user can tell the sources apart at a glance.</summary>
public enum MapLayerKind
{
    /// <summary>Imported from an Excel file: round marker, icon and colour chosen by the user.</summary>
    Excel,

    /// <summary>Points the user added by hand (typed or picked coordinates): pin marker, layer "my points".</summary>
    Manual,

    /// <summary>Rows sent from the Explorer: amber diamond with a magnifier. Temporary, never saved.</summary>
    Explorer,

    /// <summary>Rows sent from Monitoring: rose rounded square with a bell. Temporary, never saved.</summary>
    Monitoring
}

/// <summary>
/// Fixed look of the non-Excel sources: the marker shape and the fixed colour (rules and colours only).
/// The shapes of the fixed glyphs are drawing data in the WPF layer (<c>MapSourceGlyphs</c>); the host combines them.
/// </summary>
public static class MapSourceStyles
{
    public const string ExplorerColor = "#F59E0B";
    public const string MonitoringColor = "#E11D48";
    public const string ManualColor = "#0EA5E9";

    public static string Key(MapLayerKind kind) => kind switch
    {
        MapLayerKind.Manual => "manual",
        MapLayerKind.Explorer => "explorer",
        MapLayerKind.Monitoring => "monitoring",
        _ => "excel"
    };

    /// <summary>Marker outline: circle (Excel), pin (my points), diamond (Explorer), rounded square (Monitoring).</summary>
    public static string Shape(MapLayerKind kind) => kind switch
    {
        MapLayerKind.Manual => "pin",
        MapLayerKind.Explorer => "diamond",
        MapLayerKind.Monitoring => "square",
        _ => "circle"
    };

    /// <summary>A colour the user cannot change (null = the layer's own colour).</summary>
    public static string? FixedColor(MapLayerKind kind) => kind switch
    {
        MapLayerKind.Explorer => ExplorerColor,
        MapLayerKind.Monitoring => MonitoringColor,
        _ => null
    };
}
