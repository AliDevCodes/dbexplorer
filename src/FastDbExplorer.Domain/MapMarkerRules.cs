namespace FastDbExplorer.Domain;

/// <summary>
/// Rules for coordinate-layer markers that the data layer needs: which icon keys exist, which colours are valid, and which
/// colour a layer gets when it has none. Pure data and rules, no drawing data (ADR-0007, finding S6): the SVG path data of
/// the icons lives in the WPF layer and is looked up by these same keys.
/// Icon keys are stored in layer files: an existing key must never be renamed or reused for another shape.
/// </summary>
public static class MapMarkerRules
{
    public static IReadOnlyList<string> Palette { get; } =
    [
        "#4F46E5", "#2563EB", "#0D9488", "#16A34A", "#D97706",
        "#EA580C", "#DC2626", "#DB2777", "#9333EA", "#475569",
        "#EAB308", "#84CC16", "#0891B2", "#0EA5E9", "#92400E", "#111827"
    ];

    /// <summary>Every icon key the application knows. Must equal the keys of the WPF glyph table (checked by a test).</summary>
    public static IReadOnlySet<string> KnownIconKeys { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "pin", "dot", "target", "flag", "star", "heart", "home", "person",
        "hospital", "medical", "pharmacy", "lab",
        "school", "university", "library",
        "mosque", "church", "temple", "cemetery",
        "metro", "bus", "plane", "ship", "car", "fuel", "bridge",
        "parliament", "court", "police", "fire", "post", "embassy", "town_hall", "prison", "customs",
        "shield", "camera", "warning", "military", "radar", "checkpoint", "fort", "submarine", "missile",
        "factory", "bolt", "power_plant", "nuclear", "solar", "wind_turbine", "dam", "refinery", "substation", "oil_rig",
        "drop", "tower", "tree", "mountain",
        "shop", "bank", "building", "hotel", "restaurant", "cafe",
        "museum", "stadium", "ticket", "monument"
    };

    public static bool IsKnownIconKey(string? key) => key is not null && KnownIconKeys.Contains(key);

    /// <summary>"#RRGGBB" only (no names, no alpha), so a stored value can never inject anything into the map page.</summary>
    public static bool IsValidColor(string? value) =>
        value is { Length: 7 } && value[0] == '#' && value.Skip(1).All(Uri.IsHexDigit);

    /// <summary>The colour to draw: the chosen one (upper case), else a stable palette colour picked from the layer id.</summary>
    public static string EffectiveColor(Guid layerId, string? color)
    {
        if (IsValidColor(color)) return color!.ToUpperInvariant();

        uint hash = 17;
        foreach (var b in layerId.ToByteArray()) hash = unchecked(hash * 31 + b);
        return Palette[(int)(hash % (uint)Palette.Count)];
    }
}
