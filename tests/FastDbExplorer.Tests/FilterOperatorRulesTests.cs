using FastDbExplorer.Domain;
using Xunit;

namespace FastDbExplorer.Tests;

/// <summary>The filter operator rules (Domain). Regression coverage for lesson N2: the rules must never need a view.</summary>
public class FilterOperatorRulesTests
{
    [Theory]
    [InlineData(FilterOperator.IsNull)]
    [InlineData(FilterOperator.IsNotNull)]
    public void Null_checks_need_no_value(FilterOperator op)
        => Assert.False(FilterOperatorRules.NeedsValue(op));

    [Fact]
    public void Every_other_operator_needs_a_value()
    {
        var withValue = Enum.GetValues<FilterOperator>()
            .Where(op => op is not (FilterOperator.IsNull or FilterOperator.IsNotNull));

        Assert.All(withValue, op => Assert.True(FilterOperatorRules.NeedsValue(op), op.ToString()));
    }

    [Fact]
    public void Only_between_needs_a_second_value()
    {
        foreach (var op in Enum.GetValues<FilterOperator>())
            Assert.Equal(op == FilterOperator.Between, FilterOperatorRules.NeedsSecondValue(op));
    }

    [Fact]
    public void The_default_operator_is_equal_and_needs_one_value()
    {
        Assert.Equal(FilterOperator.EqualTo, FilterOperatorRules.Default);
        Assert.True(FilterOperatorRules.NeedsValue(FilterOperatorRules.Default));
        Assert.False(FilterOperatorRules.NeedsSecondValue(FilterOperatorRules.Default));
    }
}
