using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Presentation;
using Xunit;

namespace FastDbExplorer.Tests;

/// <summary>
/// The drawing data of the markers (WPF layer) must match the rules of Domain. These tests run because the test project
/// references the WPF project (step 4b-1 of the catalogue split).
/// </summary>
public class MapMarkerGlyphsTests
{
    [Fact]
    public void Every_rule_key_has_exactly_one_glyph_and_no_glyph_is_unknown()
    {
        var glyphKeys = MapMarkerGlyphs.All.Select(glyph => glyph.Key).ToList();

        Assert.Equal(glyphKeys.Count, glyphKeys.Distinct(StringComparer.Ordinal).Count());
        Assert.True(MapMarkerRules.KnownIconKeys.SetEquals(glyphKeys));
    }

    [Fact]
    public void Every_glyph_has_path_data()
    {
        Assert.All(MapMarkerGlyphs.All, glyph => Assert.False(string.IsNullOrWhiteSpace(glyph.PathData), glyph.Key));
    }

    [Fact]
    public void The_default_category_holds_the_dot_marker()
    {
        var category = MapMarkerGlyphs.Categories.Single(c => c.Key == MapMarkerGlyphs.DefaultCategory);

        Assert.Contains(category.Glyphs, glyph => glyph.Key == "dot");
    }

    [Theory]
    [InlineData("pin", true)]
    [InlineData("PIN", false)]
    [InlineData("unknown", false)]
    [InlineData(null, false)]
    public void Path_lookup_is_case_sensitive(string? key, bool found)
        => Assert.Equal(found, MapMarkerGlyphs.PathFor(key) is not null);

    [Fact]
    public void Explorer_and_monitoring_have_fixed_glyphs_and_the_other_kinds_do_not()
    {
        Assert.NotNull(MapSourceGlyphs.Fixed(MapLayerKind.Explorer));
        Assert.NotNull(MapSourceGlyphs.Fixed(MapLayerKind.Monitoring));
        Assert.Null(MapSourceGlyphs.Fixed(MapLayerKind.Excel));
        Assert.Null(MapSourceGlyphs.Fixed(MapLayerKind.Manual));
    }
}
