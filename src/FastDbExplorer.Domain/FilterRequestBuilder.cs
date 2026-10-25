namespace FastDbExplorer.Domain;

/// <summary>One filter row as the user typed it. UI-independent: the view model converts its rows into this.</summary>
public sealed record FilterInput(string? Column, FilterOperator Operator, string? Value, string? Value2);

/// <summary>
/// Turns filter rows into query conditions. This is a query rule, so it lives in Domain (ADR-0007):
/// a column is required; a value is required when the operator needs one (<see cref="FilterOperatorRules"/>);
/// a second value is required only for BETWEEN; values that an operator does not use are dropped.
/// </summary>
public static class FilterRequestBuilder
{
    /// <summary>Returns false and the index of the first incomplete row when a row cannot be used.</summary>
    public static bool TryBuild(
        IReadOnlyList<FilterInput> inputs,
        out IReadOnlyList<FilterCondition> conditions,
        out int incompleteIndex)
    {
        var result = new List<FilterCondition>(inputs.Count);
        for (var index = 0; index < inputs.Count; index++)
        {
            var input = inputs[index];
            var needsValue = FilterOperatorRules.NeedsValue(input.Operator);
            var needsSecondValue = FilterOperatorRules.NeedsSecondValue(input.Operator);

            var incomplete = input.Column is null
                || (needsValue && string.IsNullOrWhiteSpace(input.Value))
                || (needsSecondValue && string.IsNullOrWhiteSpace(input.Value2));
            if (incomplete)
            {
                conditions = Array.Empty<FilterCondition>();
                incompleteIndex = index;
                return false;
            }

            result.Add(new FilterCondition(
                input.Column!,
                input.Operator,
                needsValue ? input.Value : null,
                needsSecondValue ? input.Value2 : null));
        }

        conditions = result;
        incompleteIndex = -1;
        return true;
    }
}
