using FastDbExplorer.Domain;
using Xunit;

namespace FastDbExplorer.Tests;

public class MapSourceStylesTests
{
    [Fact]
    public void Each_source_has_its_own_marker_shape()
    {
        var shapes = new[] { MapLayerKind.Excel, MapLayerKind.Manual, MapLayerKind.Query }.Select(MapSourceStyles.Shape).ToList();

        Assert.Equal(["circle", "pin", "diamond"], shapes);
        Assert.Equal(shapes.Count, shapes.Distinct().Count()); // never two sources with the same shape
    }

    [Fact]
    public void Source_keys_are_unique()
    {
        var keys = Enum.GetValues<MapLayerKind>().Select(MapSourceStyles.Key).ToList();
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void Query_results_have_a_fixed_colour_and_glyph_that_the_user_cannot_mistake_for_an_excel_layer()
    {
        Assert.True(MapMarkerCatalog.IsValidColor(MapSourceStyles.QueryColor));
        Assert.True(MapMarkerCatalog.IsValidColor(MapSourceStyles.ManualColor));
        Assert.Equal(MapSourceStyles.QueryColor, MapSourceStyles.FixedColor(MapLayerKind.Query));
        Assert.Null(MapSourceStyles.FixedColor(MapLayerKind.Excel));
        Assert.Null(MapSourceStyles.FixedGlyph(MapLayerKind.Excel));

        var glyph = MapSourceStyles.FixedGlyph(MapLayerKind.Query);
        Assert.NotNull(glyph);
        Assert.Matches(@"^M[MmLlHhVvCcSsAaZz0-9eE.,\s+\-]+$", glyph!);
        Assert.EndsWith("z", glyph.TrimEnd());
    }

    [Fact]
    public void The_dot_glyph_used_by_pin_and_diamond_markers_exists_in_the_icon_catalog()
        => Assert.NotNull(MapMarkerCatalog.Find("dot"));
}
