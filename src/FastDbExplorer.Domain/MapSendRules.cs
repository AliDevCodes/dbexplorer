using System.Globalization;

namespace FastDbExplorer.Domain;

/// <summary>
/// Rules for sending table rows to the map: reading a coordinate from a database value, cutting long texts, and limiting
/// the detail columns of a point. Pure functions (ADR-0007). The view model reads the cell values and calls these rules.
/// </summary>
public static class MapSendRules
{
    public const int MaxDetailColumns = 8;
    public const int MaxDetailKeyLength = 80;
    public const int MaxDetailValueLength = 240;
    public const int MaxDetailTotalLength = 1_200;

    /// <summary>
    /// Reads a coordinate from a database value: a number, or text with ASCII or Persian digits.
    /// Returns false for NULL, non-numeric text, NaN and infinity.
    /// </summary>
    public static bool TryCoordinate(object? value, out double coordinate)
    {
        coordinate = 0;
        if (value is null or DBNull) return false;

        if (value is string text)
        {
            var normalized = string.Concat(text.Select(c =>
            {
                var digit = CharUnicodeInfo.GetDecimalDigitValue(c);
                return digit is >= 0 and <= 9 ? (char)('0' + digit) : c;
            })).Trim();
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out coordinate)
                && double.IsFinite(coordinate);
        }

        try
        {
            coordinate = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            return double.IsFinite(coordinate);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            return false;
        }
    }

    /// <summary>Cuts text to <paramref name="maxLength"/> characters, adds an ellipsis, and never splits a surrogate pair.</summary>
    public static string TruncateText(string value, int maxLength)
    {
        if (value.Length <= maxLength) return value;
        var length = Math.Max(0, maxLength - 1);
        if (length > 0 && char.IsHighSurrogate(value[length - 1]) && char.IsLowSurrogate(value[length]))
            length--;
        return value[..length] + "\u2026";
    }

    /// <summary>
    /// Builds the detail columns of one map point. Only the first <see cref="MaxDetailColumns"/> columns are considered, in order.
    /// A column whose value is absent (null) is skipped. Keys and values are cut to their limits, and the total length is bounded.
    /// </summary>
    public static IReadOnlyDictionary<string, string>? BuildDetails(IReadOnlyList<(string Name, string? FullValue)> columns)
    {
        var details = new Dictionary<string, string>(StringComparer.Ordinal);
        var totalLength = 0;
        foreach (var (name, fullValue) in columns.Take(MaxDetailColumns))
        {
            if (fullValue is null) continue;
            var key = TruncateText(name, MaxDetailKeyLength);
            var remaining = MaxDetailTotalLength - totalLength - key.Length;
            if (remaining <= 0) break;
            var value = TruncateText(fullValue, Math.Min(MaxDetailValueLength, remaining));
            details[key] = value;
            totalLength += key.Length + value.Length;
        }
        return details.Count == 0 ? null : details;
    }
}

/// <summary>Paging rule: keyset paging is used only deep in a result, and only when the query allows it and a key exists.</summary>
public static class KeysetPaging
{
    /// <summary>From this offset on, the next page is read by key instead of OFFSET.</summary>
    public const long Threshold = 10_000;

    public static bool UseKeyset(bool keysetAllowed, long offset, bool hasLastKey) =>
        keysetAllowed && hasLastKey && offset >= Threshold;
}
