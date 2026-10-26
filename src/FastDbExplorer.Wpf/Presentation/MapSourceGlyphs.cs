namespace FastDbExplorer.Wpf.Presentation;

/// <summary>
/// Drawing data of the two fixed glyphs: the search glyph of Explorer results and the bell of Monitoring results.
/// The domain only says which kind has a fixed glyph (MapSourceStyles); the shape itself is presentation data.
/// </summary>
public static class MapSourceGlyphs
{
    private const string SearchGlyph =
        "M10 3 a7 7 0 1 0 0 14 a7 7 0 1 0 0 -14 z M10 5.5 a4.5 4.5 0 1 1 0 9 a4.5 4.5 0 1 1 0 -9 z M14.6 16 l1.4 -1.4 l5.4 5.4 l-1.4 1.4 z";

    private const string BellGlyph =
        "M12 2.5 a1.5 1.5 0 0 0 -1.5 1.5 v0.6 C7.7 5.4 6 7.7 6 10.5 v4 l-2 2.5 v1 h16 v-1 l-2 -2.5 v-4 c0 -2.8 -1.7 -5.1 -4.5 -5.9 V4 a1.5 1.5 0 0 0 -1.5 -1.5 z M10 19.5 a2 2 0 0 0 4 0 z";

    /// <summary>The fixed glyph of a kind, or null when the user's own icon is used.</summary>
    public static string? Fixed(Domain.MapLayerKind kind) => kind switch
    {
        Domain.MapLayerKind.Explorer => SearchGlyph,
        Domain.MapLayerKind.Monitoring => BellGlyph,
        _ => null
    };
}
