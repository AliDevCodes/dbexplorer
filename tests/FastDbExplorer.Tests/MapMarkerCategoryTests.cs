using FastDbExplorer.Wpf.Presentation;
using Xunit;

namespace FastDbExplorer.Tests;

/// <summary>The icon folders and the glyph library (WPF drawing data).</summary>
public sealed class MapMarkerCategoryTests
{
    [Fact]
    public void Every_icon_belongs_to_exactly_one_category()
    {
        var all = MapMarkerGlyphs.Categories.SelectMany(c => c.Glyphs.Select(i => i.Key)).ToList();

        Assert.Equal(all.Count, all.Distinct().Count());
        Assert.Equal(MapMarkerGlyphs.All.Count, all.Count);
        Assert.Equal(all.OrderBy(k => k), MapMarkerGlyphs.All.Select(i => i.Key).OrderBy(k => k));
    }

    [Fact]
    public void Every_category_has_at_least_one_icon_and_the_default_category_exists()
    {
        Assert.All(MapMarkerGlyphs.Categories, c => Assert.NotEmpty(c.Glyphs));
        Assert.Contains(MapMarkerGlyphs.Categories, c => c.Key == MapMarkerGlyphs.DefaultCategory);
    }

    [Fact]
    public void Category_keys_are_unique()
    {
        var keys = MapMarkerGlyphs.Categories.Select(c => c.Key).ToList();
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Theory]
    [InlineData("pin")]
    [InlineData("dot")]
    [InlineData("hospital")]
    [InlineData("school")]
    [InlineData("mosque")]
    [InlineData("bolt")]
    [InlineData("drop")]
    [InlineData("warning")]
    [InlineData("shield")]
    [InlineData("camera")]
    [InlineData("tower")]
    [InlineData("plane")]
    [InlineData("ship")]
    [InlineData("bus")]
    [InlineData("shop")]
    [InlineData("person")]
    [InlineData("building")]
    [InlineData("bank")]
    [InlineData("factory")]
    [InlineData("home")]
    [InlineData("star")]
    [InlineData("heart")]
    [InlineData("flag")]
    [InlineData("target")]
    public void Keys_saved_by_earlier_versions_still_exist(string key) =>
        Assert.NotNull(MapMarkerGlyphs.PathFor(key));

    [Fact]
    public void Every_glyph_path_is_plain_svg_path_data_ending_with_a_close()
    {
        foreach (var icon in MapMarkerGlyphs.All)
        {
            Assert.Matches(@"^M[MmLlHhVvCcSsAaZz0-9eE.,\s+\-]+$", icon.PathData);
            Assert.EndsWith("z", icon.PathData.TrimEnd());
        }
    }

    [Fact]
    public void The_icon_library_is_large_enough_to_be_useful()
    {
        Assert.True(MapMarkerGlyphs.All.Count >= 50, $"only {MapMarkerGlyphs.All.Count} icons");
    }
}
