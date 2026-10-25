namespace FastDbExplorer.Domain;

/// <summary>
/// Which inputs a filter operator needs. This is a query rule, so it lives in Domain and not in the view model:
/// it can be tested without UI and reused by any caller that builds filters (Explorer, Monitoring).
/// </summary>
public static class FilterOperatorRules
{
    /// <summary>The operator a new filter row starts with.</summary>
    public const FilterOperator Default = FilterOperator.EqualTo;

    /// <summary>IS NULL and IS NOT NULL compare with nothing; every other operator needs a value.</summary>
    public static bool NeedsValue(FilterOperator op) => op is not (FilterOperator.IsNull or FilterOperator.IsNotNull);

    /// <summary>Only BETWEEN needs a second value (the upper bound).</summary>
    public static bool NeedsSecondValue(FilterOperator op) => op == FilterOperator.Between;
}
