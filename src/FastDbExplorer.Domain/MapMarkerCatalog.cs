namespace FastDbExplorer.Domain;

/// <summary>
/// One map marker glyph: a filled shape on a 24x24 grid (SVG path data, even-odd fill). It is drawn in white on a
/// coloured disc, both in the layer list (WPF) and on the map (canvas Path2D), so the two always look the same.
/// All glyphs are drawn in-house for this project (no third-party icon licence).
/// </summary>
public sealed record MapMarkerIcon(string Key, string PathData);

/// <summary>The built-in marker icons and colours a coordinate layer can use. Pure data, no UI types.</summary>
public static class MapMarkerCatalog
{
    public static IReadOnlyList<string> Palette { get; } =
    [
        "#4F46E5", "#2563EB", "#0D9488", "#16A34A", "#D97706",
        "#EA580C", "#DC2626", "#DB2777", "#9333EA", "#475569"
    ];

    public static IReadOnlyList<MapMarkerIcon> Icons { get; } =
    [
        new("pin", "M12 2 C8.1 2 5 5.1 5 9 c0 5.2 7 13 7 13 s7 -7.8 7 -13 c0 -3.9 -3.1 -7 -7 -7 z M12 6.5 a2.5 2.5 0 1 0 0 5 a2.5 2.5 0 1 0 0 -5 z"),
        new("dot", "M12 6 a6 6 0 1 0 0 12 a6 6 0 1 0 0 -12 z"),
        new("target", "M12 3 a9 9 0 1 0 0 18 a9 9 0 1 0 0 -18 z M12 6 a6 6 0 1 1 0 12 a6 6 0 1 1 0 -12 z M12 9 a3 3 0 1 0 0 6 a3 3 0 1 0 0 -6 z"),
        new("flag", "M5 3 h2 v18 H5 z M7 4 h12 l-3 4.5 l3 4.5 H7 z"),
        new("star", "M12 2 l3.1 6.3 l6.9 1 l-5 4.9 l1.2 6.8 l-6.2 -3.3 l-6.2 3.3 l1.2 -6.8 l-5 -4.9 l6.9 -1 z"),
        new("heart", "M12 21 C12 21 4 15.6 4 10 a4.5 4.5 0 0 1 8 -2.8 A4.5 4.5 0 0 1 20 10 c0 5.6 -8 11 -8 11 z"),
        new("home", "M12 3 L2 12 h3 v8 h5 v-5 h4 v5 h5 v-8 h3 z"),
        new("building", "M5 3 h10 v18 H5 z M7.5 6 h2 v2 h-2 z M10.5 6 h2 v2 h-2 z M7.5 10 h2 v2 h-2 z M10.5 10 h2 v2 h-2 z M7.5 14 h2 v2 h-2 z M10.5 14 h2 v2 h-2 z M16.5 9 h3 v12 h-3 z"),
        new("bank", "M12 2 L2 8 h20 z M4 10 h3 v8 H4 z M10.5 10 h3 v8 h-3 z M17 10 h3 v8 h-3 z M2 20 h20 v2 H2 z"),
        new("factory", "M2 21 V10 L8 14 V10 L14 14 V10 L18 12.7 V4 H22 V21 z"),
        new("hospital", "M9 3 h6 v6 h6 v6 h-6 v6 H9 v-6 H3 V9 h6 z"),
        new("school", "M12 4 L1 9.5 l11 5.5 l9 -4.5 V16 h2 V9.5 L12 4 z M5.5 13.5 v3.8 c1 0 3 3.2 6.5 3.2 s6.5 -2.2 6.5 -3.2 v-3.8 L12 16.5 l-6.5 -3 z"),
        new("mosque", "M12 3 c-3.3 2.6 -5 4.8 -5 7 v1 h10 v-1 c0 -2.2 -1.7 -4.4 -5 -7 z M6 12 h12 v9 H6 z M10.5 21 v-4 a1.5 1.5 0 0 1 3 0 v4 z M2.5 9 h2 v12 h-2 z M19.5 9 h2 v12 h-2 z M3.5 5 l1.2 4 h-2.4 z M20.5 5 l1.2 4 h-2.4 z"),
        new("bolt", "M13 2 L4 14 h6 l-1 8 l9 -12 h-6 z"),
        new("drop", "M12 2.5 C12 2.5 5 10 5 14.5 a7 7 0 0 0 14 0 C19 10 12 2.5 12 2.5 z"),
        new("warning", "M12 2 L1 21 h22 z M11 9 h2 v6 h-2 z M11 17 h2 v2 h-2 z"),
        new("shield", "M12 2 l8 3 v6 c0 5 -3.4 9.3 -8 11 c-4.6 -1.7 -8 -6 -8 -11 V5 z"),
        new("camera", "M4 7 h3 l1.5 -2.5 h7 L17 7 h3 a1 1 0 0 1 1 1 v11 a1 1 0 0 1 -1 1 H4 a1 1 0 0 1 -1 -1 V8 a1 1 0 0 1 1 -1 z M12 9.5 a3.8 3.8 0 1 0 0 7.6 a3.8 3.8 0 1 0 0 -7.6 z"),
        new("tower", "M12 7 l8 14 h-3.2 L12 12.5 L7.2 21 H4 z M12 2.5 a2.2 2.2 0 1 0 0 4.4 a2.2 2.2 0 1 0 0 -4.4 z"),
        new("plane", "M12 2 L13.5 9 L22 14 V16 L13.5 13.5 V18.5 L16 20.5 V22 L12 21 L8 22 V20.5 L10.5 18.5 V13.5 L2 16 V14 L10.5 9 z"),
        new("ship", "M2 16 l3 5 h14 l3 -5 z M7 10 h10 v4 H7 z M10 5 h4 v4 h-4 z"),
        new("bus", "M5 3 h14 a1 1 0 0 1 1 1 v13 a1 1 0 0 1 -1 1 v2 h-3 v-2 H8 v2 H5 v-2 a1 1 0 0 1 -1 -1 V4 a1 1 0 0 1 1 -1 z M6.5 6 h11 v5 h-11 z M7 13.5 a1 1 0 1 0 0 2 a1 1 0 1 0 0 -2 z M17 13.5 a1 1 0 1 0 0 2 a1 1 0 1 0 0 -2 z"),
        new("shop", "M6 8 h12 l1 13 H5 z M9 8 V7 a3 3 0 0 1 6 0 v1 h-2 V7 a1 1 0 0 0 -2 0 v1 z"),
        new("person", "M12 4 a4 4 0 1 0 0 8 a4 4 0 1 0 0 -8 z M4 21 c0 -4.4 3.6 -8 8 -8 s8 3.6 8 8 z")
    ];

    public static MapMarkerIcon? Find(string? key) =>
        key is null ? null : Icons.FirstOrDefault(icon => icon.Key == key);

    /// <summary>"#RRGGBB" only (no names, no alpha), so a stored value can never inject anything into the map page.</summary>
    public static bool IsValidColor(string? value) =>
        value is { Length: 7 } && value[0] == '#' && value.Skip(1).All(Uri.IsHexDigit);

    /// <summary>The colour to draw: the chosen one, else a stable palette colour picked from the layer id.</summary>
    public static string EffectiveColor(Guid layerId, string? color)
    {
        if (IsValidColor(color)) return color!.ToUpperInvariant();

        uint hash = 17;
        foreach (var b in layerId.ToByteArray()) hash = unchecked(hash * 31 + b);
        return Palette[(int)(hash % (uint)Palette.Count)];
    }
}
