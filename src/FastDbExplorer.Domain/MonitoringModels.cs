using System.Globalization;
using System.Text.Json.Serialization;

namespace FastDbExplorer.Domain;

/// <summary>Column types usable to detect "new since the last check": increasing numbers or dates.</summary>
public static class WatermarkTypes
{
    private static readonly HashSet<string> Supported =
        ["bigint", "int", "smallint", "tinyint", "date", "datetime", "datetime2", "smalldatetime"];

    private static readonly HashSet<string> DateTypes = ["date", "datetime", "datetime2", "smalldatetime"];

    public static bool IsSupported(string typeName) => Supported.Contains(typeName);
    public static bool IsDate(string typeName) => DateTypes.Contains(typeName);

    /// <summary>Culture-independent text of a watermark value (stored in the monitor file, parsed back by ValueConverter).</summary>
    public static string Format(object value) => value switch
    {
        DateTime d => d.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? ""
    };
}

/// <summary>MAX of the watermark column and the SQL Server clock, read in the same query.</summary>
public sealed record WatermarkReading(string? Max, DateTime? ServerNow);

/// <summary>
/// One user-defined monitor: which table, which conditions, how often, where the Excel report goes.
/// It never contains a password. <see cref="LastWatermark"/> / <see cref="LastCheckedUtc"/> are the only state.
/// <see cref="SettleSeconds"/>: for date watermarks the newest N seconds are held back, so rows committed a moment late are not skipped.
/// </summary>
public sealed record MonitorDefinition(
    Guid Id,
    string Name,
    string Server,
    string Database,
    string Schema,
    string Table,
    string WatermarkColumn,
    IReadOnlyList<FilterCondition> Conditions,
    FilterLogic Logic,
    int IntervalMinutes,
    string OutputFolder,
    bool PlaySound,
    bool Enabled,
    string? LastWatermark = null,
    DateTime? LastCheckedUtc = null,
    int SettleSeconds = 30)
{
    public const int MinIntervalMinutes = 1;
    public const int MaxIntervalMinutes = 1440;

    [JsonIgnore]
    public string TargetText => $"{Database} · {Schema}.{Table}";

    /// <summary>Copy of this monitor with the state of a finished check applied.</summary>
    public MonitorDefinition Apply(MonitorCheckResult result) =>
        this with { LastWatermark = result.NewWatermark ?? LastWatermark, LastCheckedUtc = result.CheckedAtUtc };
}

/// <summary>Outcome of one check. <see cref="IsBaseline"/> = first run: only the starting point was recorded.</summary>
public sealed record MonitorCheckResult(
    Guid MonitorId,
    DateTime CheckedAtUtc,
    IReadOnlyList<string> Columns,
    IReadOnlyList<object?[]> Rows,
    bool Truncated,
    string? NewWatermark,
    string? ReportPath,
    bool IsBaseline);
