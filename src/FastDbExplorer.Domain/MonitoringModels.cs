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
    int SettleSeconds = 30,
    Guid? CoordinateLayerId = null,
    string? LatitudeColumn = null,
    string? LongitudeColumn = null,
    double? DistanceMeters = null)
{
    public const int MinIntervalMinutes = 1;
    public const int MaxIntervalMinutes = 1440;

    [JsonIgnore]
    public string TargetText => $"{Database} · {Schema}.{Table}";

    [JsonIgnore]
    public bool HasProximityRule => CoordinateLayerId is not null || LatitudeColumn is not null
                                    || LongitudeColumn is not null || DistanceMeters is not null;

    /// <summary>Copy of this monitor with the state of a finished check applied.</summary>
    public MonitorDefinition Apply(MonitorCheckResult result) =>
        this with { LastWatermark = result.NewWatermark ?? LastWatermark, LastCheckedUtc = result.CheckedAtUtc };
}

/// <summary>SQL types whose values can be read as numeric WGS84 latitude/longitude inputs by a monitor.</summary>
public static class MonitorCoordinateTypes
{
    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        "char", "varchar", "nchar", "nvarchar", "bigint", "int", "smallint", "tinyint",
        "decimal", "numeric", "money", "smallmoney", "float", "real"
    };

    public static bool IsSupported(string typeName) => Supported.Contains(typeName);
}

/// <summary>WGS84 great-circle distance calculations used by monitor proximity rules.</summary>
public static class MonitorProximity
{
    private const double EarthMeanRadiusMeters = 6_371_008.8;

    public static double HaversineMeters(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        if (!CoordinateRules.IsValidLatitude(latitude1) || !CoordinateRules.IsValidLongitude(longitude1)
            || !CoordinateRules.IsValidLatitude(latitude2) || !CoordinateRules.IsValidLongitude(longitude2))
            throw new ArgumentOutOfRangeException(nameof(latitude1), "Coordinates must be finite WGS84 latitude/longitude values.");

        var lat1 = DegreesToRadians(latitude1);
        var lat2 = DegreesToRadians(latitude2);
        var deltaLat = lat2 - lat1;
        var deltaLon = DegreesToRadians(longitude2 - longitude1);
        var sinLat = Math.Sin(deltaLat / 2);
        var sinLon = Math.Sin(deltaLon / 2);
        var a = sinLat * sinLat + Math.Cos(lat1) * Math.Cos(lat2) * sinLon * sinLon;
        var centralAngle = 2 * Math.Atan2(Math.Sqrt(Math.Min(1, a)), Math.Sqrt(Math.Max(0, 1 - a)));
        return EarthMeanRadiusMeters * centralAngle;
    }

    public static bool IsWithinDistance(double latitude1, double longitude1, double latitude2, double longitude2, double distanceMeters)
    {
        if (!double.IsFinite(distanceMeters) || distanceMeters <= 0 || distanceMeters > CoordinateRules.MaxRadiusMeters)
            throw new ArgumentOutOfRangeException(nameof(distanceMeters), distanceMeters,
                $"The monitor distance must be greater than 0 and at most {CoordinateRules.MaxRadiusMeters} metres.");

        return HaversineMeters(latitude1, longitude1, latitude2, longitude2) <= distanceMeters;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);
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
