using FastDbExplorer.Domain;
using Xunit;

namespace FastDbExplorer.Tests;

/// <summary>Filter rows to query conditions (Domain rule). Regression coverage for lesson N2 and the incomplete-row message.</summary>
public class FilterRequestBuilderTests
{
    [Fact]
    public void A_complete_row_becomes_one_condition()
    {
        var ok = FilterRequestBuilder.TryBuild(
            new[] { new FilterInput("Name", FilterOperator.EqualTo, "Ali", null) },
            out var conditions, out var index);

        Assert.True(ok);
        Assert.Equal(-1, index);
        var condition = Assert.Single(conditions);
        Assert.Equal("Name", condition.Column);
        Assert.Equal(FilterOperator.EqualTo, condition.Operator);
        Assert.Equal("Ali", condition.Value);
        Assert.Null(condition.Value2);
    }

    [Fact]
    public void A_second_value_is_dropped_unless_the_operator_is_between()
    {
        FilterRequestBuilder.TryBuild(
            new[] { new FilterInput("Id", FilterOperator.EqualTo, "5", "9") },
            out var conditions, out _);

        Assert.Null(Assert.Single(conditions).Value2);
    }

    [Fact]
    public void Between_keeps_both_values()
    {
        FilterRequestBuilder.TryBuild(
            new[] { new FilterInput("Id", FilterOperator.Between, "5", "9") },
            out var conditions, out _);

        var condition = Assert.Single(conditions);
        Assert.Equal("5", condition.Value);
        Assert.Equal("9", condition.Value2);
    }

    [Theory]
    [InlineData(FilterOperator.IsNull)]
    [InlineData(FilterOperator.IsNotNull)]
    public void Null_checks_need_no_value_and_drop_any_typed_value(FilterOperator op)
    {
        FilterRequestBuilder.TryBuild(
            new[] { new FilterInput("Note", op, "ignored", "ignored") },
            out var conditions, out _);

        var condition = Assert.Single(conditions);
        Assert.Null(condition.Value);
        Assert.Null(condition.Value2);
    }

    [Fact]
    public void A_row_without_a_column_is_incomplete()
    {
        var ok = FilterRequestBuilder.TryBuild(
            new[] { new FilterInput("Name", FilterOperator.EqualTo, "x", null), new FilterInput(null, FilterOperator.EqualTo, "y", null) },
            out var conditions, out var index);

        Assert.False(ok);
        Assert.Equal(1, index);
        Assert.Empty(conditions);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void A_value_is_required_for_operators_that_need_one(string? value)
    {
        var ok = FilterRequestBuilder.TryBuild(
            new[] { new FilterInput("Name", FilterOperator.Contains, value, null) },
            out _, out var index);

        Assert.False(ok);
        Assert.Equal(0, index);
    }

    [Fact]
    public void Between_needs_a_second_value()
    {
        var ok = FilterRequestBuilder.TryBuild(
            new[] { new FilterInput("Id", FilterOperator.Between, "5", "") },
            out _, out var index);

        Assert.False(ok);
        Assert.Equal(0, index);
    }

    [Fact]
    public void An_empty_list_gives_no_conditions_and_is_complete()
    {
        var ok = FilterRequestBuilder.TryBuild(Array.Empty<FilterInput>(), out var conditions, out var index);

        Assert.True(ok);
        Assert.Empty(conditions);
        Assert.Equal(-1, index);
    }
}
