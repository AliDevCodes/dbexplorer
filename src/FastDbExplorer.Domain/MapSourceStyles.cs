namespace FastDbExplorer.Domain;

/// <summary>Where a map layer comes from. The kind decides the marker shape, so the user can tell the sources apart at a glance.</summary>
public enum MapLayerKind
{
    /// <summary>Imported from an Excel file: round marker, icon and colour chosen by the user.</summary>
    Excel,

    /// <summary>Points the user added by hand (typed or picked coordinates): pin marker, layer "my points".</summary>
    Manual,

    /// <summary>Rows sent from the Explorer or from Monitoring: amber diamond marker with a magnifier. Temporary, never saved.</summary>
    Query
}

/// <summary>Fixed look of the non-Excel sources. Pure data (no UI types): the host turns it into the message for the map page.</summary>
public static class MapSourceStyles
{
    public const string QueryColor = "#F59E0B";
    public const string ManualColor = "#0EA5E9";

    // Magnifier glyph (24x24 grid, even-odd fill), drawn in white inside the diamond marker.
    private const string SearchGlyph =
        "M10 3 a7 7 0 1 0 0 14 a7 7 0 1 0 0 -14 z M10 5.5 a4.5 4.5 0 1 1 0 9 a4.5 4.5 0 1 1 0 -9 z M14.6 16 l1.4 -1.4 l5.4 5.4 l-1.4 1.4 z";

    public static string Key(MapLayerKind kind) => kind switch
    {
        MapLayerKind.Manual => "manual",
        MapLayerKind.Query => "query",
        _ => "excel"
    };

    /// <summary>Marker outline: circle (Excel), pin (my points), diamond (query results).</summary>
    public static string Shape(MapLayerKind kind) => kind switch
    {
        MapLayerKind.Manual => "pin",
        MapLayerKind.Query => "diamond",
        _ => "circle"
    };

    /// <summary>A colour the user cannot change (null = the layer's own colour).</summary>
    public static string? FixedColor(MapLayerKind kind) => kind == MapLayerKind.Query ? QueryColor : null;

    /// <summary>A glyph the user cannot change (null = the layer's own icon).</summary>
    public static string? FixedGlyph(MapLayerKind kind) => kind == MapLayerKind.Query ? SearchGlyph : null;
}
