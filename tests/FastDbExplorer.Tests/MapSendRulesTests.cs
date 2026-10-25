using FastDbExplorer.Domain;
using Xunit;

namespace FastDbExplorer.Tests;

/// <summary>Rules for sending table rows to the map (Domain). Pure logic, no UI.</summary>
public class MapSendRulesTests
{
    [Fact]
    public void A_number_is_read_as_a_coordinate()
    {
        Assert.True(MapSendRules.TryCoordinate(35.7219, out var value));
        Assert.Equal(35.7219, value, 6);
    }

    [Fact]
    public void A_decimal_is_read_as_a_coordinate()
    {
        Assert.True(MapSendRules.TryCoordinate(51.3347m, out var value));
        Assert.Equal(51.3347, value, 6);
    }

    [Fact]
    public void Text_with_ascii_digits_is_read()
    {
        Assert.True(MapSendRules.TryCoordinate("35.7219", out var value));
        Assert.Equal(35.7219, value, 6);
    }

    [Fact]
    public void Text_with_persian_digits_is_read()
    {
        // "۳۵.۷۲۱۹" = 35.7219
        Assert.True(MapSendRules.TryCoordinate("\u06f3\u06f5.\u06f7\u06f2\u06f1\u06f9", out var value));
        Assert.Equal(35.7219, value, 6);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("NaN")]
    [InlineData("")]
    public void Null_non_numeric_and_nan_text_are_rejected(string? text)
    {
        Assert.False(MapSendRules.TryCoordinate(text, out _));
    }

    [Fact]
    public void Database_null_is_rejected()
    {
        Assert.False(MapSendRules.TryCoordinate(DBNull.Value, out _));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Non_finite_numbers_are_rejected(double value)
    {
        Assert.False(MapSendRules.TryCoordinate(value, out _));
    }

    [Fact]
    public void Short_text_is_not_changed()
    {
        Assert.Equal("abc", MapSendRules.TruncateText("abc", 5));
    }

    [Fact]
    public void Long_text_is_cut_with_an_ellipsis_within_the_limit()
    {
        var result = MapSendRules.TruncateText("abcdefghij", 5);

        Assert.Equal("abcd\u2026", result);
        Assert.True(result.Length <= 5);
    }

    [Fact]
    public void Text_is_never_cut_inside_a_surrogate_pair()
    {
        // "ab" + U+1F600 (surrogate pair) + "cd"; a cut at 4 must not split the pair
        var result = MapSendRules.TruncateText("ab\uD83D\uDE00cd", 4);

        Assert.Equal("ab\u2026", result);
    }

    [Fact]
    public void Columns_without_a_value_are_skipped_and_order_is_kept()
    {
        var details = MapSendRules.BuildDetails(new (string, string?)[]
        {
            ("Name", "A"), ("Phone", null), ("City", "Tehran")
        });

        Assert.NotNull(details);
        Assert.Equal(2, details!.Count);
        Assert.Equal("A", details["Name"]);
        Assert.Equal("Tehran", details["City"]);
    }

    [Fact]
    public void At_most_eight_detail_columns_are_used()
    {
        var columns = Enumerable.Range(1, 10).Select(i => ($"C{i}", (string?)"x")).ToArray();

        var details = MapSendRules.BuildDetails(columns);

        Assert.NotNull(details);
        Assert.Equal(8, details!.Count);
        Assert.False(details.ContainsKey("C9"));
    }

    [Fact]
    public void The_total_length_of_the_details_is_bounded()
    {
        var longValue = new string('x', 240);
        var columns = Enumerable.Range(1, 8).Select(i => ($"C{i}", (string?)longValue)).ToArray();

        var details = MapSendRules.BuildDetails(columns);

        Assert.NotNull(details);
        Assert.True(details!.Sum(pair => pair.Key.Length + pair.Value.Length) <= MapSendRules.MaxDetailTotalLength);
        Assert.True(details.Count < 8);
    }

    [Fact]
    public void No_present_value_gives_no_details()
    {
        var details = MapSendRules.BuildDetails(new (string, string?)[] { ("Name", null) });

        Assert.Null(details);
    }
}
