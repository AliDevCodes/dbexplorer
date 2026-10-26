using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Presentation;
using Xunit;

namespace FastDbExplorer.Tests;

/// <summary>Marker rules (Domain) and their consistency with the glyph data (WPF).</summary>
public class MapMarkerRulesTests
{
    [Theory]
    [InlineData("#4F46E5")]
    [InlineData("#abcdef")]
    [InlineData("#000000")]
    public void Hex_colours_are_valid(string value)
        => Assert.True(MapMarkerRules.IsValidColor(value));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("red")]
    [InlineData("#12345")]
    [InlineData("#GGGGGG")]
    [InlineData("#4F46E5 ")]
    [InlineData("4F46E5")]
    public void Anything_else_is_not_a_colour(string? value)
        => Assert.False(MapMarkerRules.IsValidColor(value));

    [Fact]
    public void A_valid_colour_is_kept_in_upper_case()
        => Assert.Equal("#ABCDEF", MapMarkerRules.EffectiveColor(Guid.NewGuid(), "#abcdef"));

    [Fact]
    public void A_missing_colour_gets_a_palette_colour_that_is_stable_for_the_same_layer()
    {
        var id = Guid.Parse("6d616e75-616c-4c61-7965-722d6d79706f");

        var first = MapMarkerRules.EffectiveColor(id, null);

        Assert.Contains(first, MapMarkerRules.Palette);
        Assert.Equal(first, MapMarkerRules.EffectiveColor(id, "not a colour"));
    }

    [Fact]
    public void The_palette_has_sixteen_valid_colours()
    {
        Assert.Equal(16, MapMarkerRules.Palette.Count);
        Assert.All(MapMarkerRules.Palette, colour => Assert.True(MapMarkerRules.IsValidColor(colour)));
    }

    [Theory]
    [InlineData("pin", true)]
    [InlineData("town_hall", true)]
    [InlineData("PIN", false)]
    [InlineData("unknown_icon", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Icon_keys_are_case_sensitive_and_must_exist(string? key, bool expected)
        => Assert.Equal(expected, MapMarkerRules.IsKnownIconKey(key));

    [Fact]
    public void The_known_keys_are_exactly_the_keys_of_the_glyph_data()
    {
        // Consistency guard: every key the rules accept has a glyph, and no glyph is unknown to the rules.
        var glyphKeys = MapMarkerGlyphs.All.Select(glyph => glyph.Key).ToHashSet(StringComparer.Ordinal);

        Assert.True(MapMarkerRules.KnownIconKeys.SetEquals(glyphKeys));
    }
}
