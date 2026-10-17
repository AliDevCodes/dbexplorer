using System.Globalization;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;

namespace FastDbExplorer.Infrastructure.Monitoring;

/// <summary>
/// One check = "which records appeared since the last check, and which of them match the user's conditions?".
/// Steps: read MAX(watermark) once, query only the window (last, upper] with the conditions on the server,
/// page through scalar matches (OFFSET, ordered by watermark + primary key); proximity rules are then checked on each
/// explicit-column page before applying the report cap.
/// The first check only records the starting point, so old data never floods the user with alerts.
/// For date watermarks the newest <see cref="MonitorDefinition.SettleSeconds"/> are held back (see <see cref="SettleUpper"/>).
/// </summary>
public sealed class MonitorCheckService(
    IDatabaseMetadataService metadata, IWatermarkReader watermarks, IReportWriter reports, ICoordinateLayerStore coordinateLayers)
    : IMonitorCheckService
{
    public const int PageSize = 1000;
    public const int MaxRows = 10_000;
    private const string NearestPointColumn = "Nearest point name";
    private const string CoordinateLayerColumn = "Coordinate layer";
    private const string DistanceColumn = "Distance (m)";

    public async Task<MonitorCheckResult> CheckAsync(ConnectionSettings settings, MonitorDefinition monitor, CancellationToken ct)
    {
        var checkedAt = DateTime.UtcNow;
        var layer = await LoadProximityLayerAsync(monitor, ct);
        var columns = await metadata.GetColumnsAsync(settings, monitor.Database, monitor.Schema, monitor.Table, ct);

        var watermark = columns.FirstOrDefault(c => c.Name == monitor.WatermarkColumn)
                        ?? throw new ArgumentException($"Column '{monitor.WatermarkColumn}' no longer exists in {monitor.TargetText}.");
        if (!WatermarkTypes.IsSupported(watermark.TypeName))
            throw new NotSupportedException($"Column '{watermark.Name}' ({watermark.TypeName}) cannot be used to detect new records.");

        // Only supported types are read (no blobs / xml), always by explicit name: never SELECT *.
        var selected = columns.Where(c => c.IsFilterable).Select(c => c.Name).ToList();
        var latitudeIndex = -1;
        var longitudeIndex = -1;
        if (layer is not null)
        {
            latitudeIndex = AddCoordinateColumn(monitor.LatitudeColumn!, columns, selected);
            longitudeIndex = AddCoordinateColumn(monitor.LongitudeColumn!, columns, selected);
        }
        if (selected.Count == 0) throw new ArgumentException("The table has no readable columns.");

        var reading = await watermarks.ReadAsync(settings, monitor.Database, monitor.Schema, monitor.Table, watermark.Name, ct);
        var upper = SettleUpper(reading, watermark.TypeName, monitor.SettleSeconds);

        // First successful check: remember where we are, do not alert for existing data.
        if (monitor.LastWatermark is null && monitor.LastCheckedUtc is null)
            return new MonitorCheckResult(monitor.Id, checkedAt, selected, [], false, upper, null, true);

        var nothingNew = upper is null
                         || (monitor.LastWatermark is not null
                             && WatermarkComparer.Compare(upper, monitor.LastWatermark, watermark.TypeName) <= 0);
        if (nothingNew)
            return new MonitorCheckResult(monitor.Id, checkedAt, selected, [], false, monitor.LastWatermark, null, false);

        // (last, upper]. No lower bound only when the table was empty at the first check.
        var required = new List<FilterCondition>();
        if (monitor.LastWatermark is not null)
            required.Add(new FilterCondition(watermark.Name, FilterOperator.GreaterThan, monitor.LastWatermark));
        required.Add(new FilterCondition(watermark.Name, FilterOperator.LessOrEqual, upper));

        var order = new List<string> { watermark.Name };
        order.AddRange(columns.Where(c => c.IsPrimaryKey && c.IsFilterable && c.Name != watermark.Name)
            .OrderBy(c => c.KeyOrdinal).Select(c => c.Name));

        var rows = new List<object?[]>();
        var truncated = false;
        var stopPaging = false;
        var watermarkIndex = selected.IndexOf(watermark.Name);
        var capReached = false;
        string? capWatermark = null;
        for (long offset = 0; ; offset += PageSize)
        {
            ct.ThrowIfCancellationRequested();
            var request = new PageRequest(
                monitor.Database, monitor.Schema, monitor.Table, selected, monitor.Conditions, monitor.Logic,
                order, false, PageSize, offset, null, required);
            var page = await metadata.GetPageAsync(settings, request, ct);

            for (var i = 0; i < page.Rows.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var candidate = page.Rows[i];
                var rowNumber = offset + i + 1;
                var candidateWatermark = candidate[watermarkIndex] is null or DBNull
                    ? throw new InvalidOperationException($"Monitor '{monitor.Name}' found a candidate without a watermark at row {rowNumber}.")
                    : WatermarkTypes.Format(candidate[watermarkIndex]!);

                if (capReached)
                {
                    var comparison = WatermarkComparer.Compare(candidateWatermark, capWatermark!, watermark.TypeName);
                    if (comparison > 0)
                    {
                        // Resume after a complete watermark group; a strict > lower bound must not split ties.
                        truncated = true;
                        stopPaging = true;
                        break;
                    }
                    if (comparison < 0)
                        throw new InvalidOperationException($"Monitor '{monitor.Name}' received rows out of watermark order while checking a capped result.");
                }

                object?[]? matched = null;
                if (layer is null)
                {
                    matched = candidate;
                }
                else
                {
                    var latitude = ReadCoordinate(candidate[latitudeIndex], monitor.LatitudeColumn!, "latitude", rowNumber, monitor.Name);
                    var longitude = ReadCoordinate(candidate[longitudeIndex], monitor.LongitudeColumn!, "longitude", rowNumber, monitor.Name);
                    var nearest = FindNearest(layer, latitude, longitude, ct);
                    if (nearest.DistanceMeters <= monitor.DistanceMeters!.Value)
                    {
                        matched = new object?[candidate.Length + 3];
                        Array.Copy(candidate, matched, candidate.Length);
                        matched[^3] = nearest.Point.Name;
                        matched[^2] = layer.Name;
                        matched[^1] = nearest.DistanceMeters;
                    }
                }

                if (matched is null) continue;
                if (capReached)
                {
                    throw new InvalidOperationException($"Monitor '{monitor.Name}' matched more than {MaxRows:N0} rows with the same watermark value. It cannot safely resume without skipping records; choose a watermark column with fewer ties or split the monitor conditions.");
                }

                rows.Add(matched);
                if (rows.Count == MaxRows)
                {
                    capReached = true;
                    capWatermark = candidateWatermark;
                }
            }

            if (stopPaging || !page.HasMore) break;
        }

        var next = truncated ? capWatermark : upper;

        var reportColumns = layer is null
            ? selected
            : selected.Concat([NearestPointColumn, CoordinateLayerColumn, DistanceColumn]).ToList();
        string? reportPath = null;
        if (rows.Count > 0)
            reportPath = await reports.WriteAsync(monitor.Name, checkedAt, reportColumns, rows, monitor.OutputFolder, ct);

        return new MonitorCheckResult(monitor.Id, checkedAt, reportColumns, rows, truncated, next, reportPath, false);
    }

    private async Task<MapLayer?> LoadProximityLayerAsync(MonitorDefinition monitor, CancellationToken ct)
    {
        if (!monitor.HasProximityRule) return null;

        if (monitor.CoordinateLayerId is not { } layerId || layerId == Guid.Empty
            || string.IsNullOrWhiteSpace(monitor.LatitudeColumn) || string.IsNullOrWhiteSpace(monitor.LongitudeColumn)
            || string.Equals(monitor.LatitudeColumn, monitor.LongitudeColumn, StringComparison.OrdinalIgnoreCase)
            || monitor.DistanceMeters is not { } distance || !double.IsFinite(distance) || distance <= 0
            || distance > CoordinateRules.MaxRadiusMeters)
            throw new InvalidOperationException($"Monitor '{monitor.Name}' has an incomplete or invalid proximity rule. Reopen it and select a layer, distinct numeric coordinate columns, and a distance greater than 0 and no greater than {CoordinateRules.MaxRadiusMeters} metres.");

        var layer = (await coordinateLayers.LoadAllAsync(ct)).FirstOrDefault(item => item.Id == layerId);
        if (layer is null)
            throw new InvalidOperationException($"Monitor '{monitor.Name}' refers to a missing or unreadable imported coordinate layer ({layerId}). Choose an available layer in the monitor editor.");
        if (layer.Points.Count == 0)
            throw new InvalidOperationException($"The selected coordinate layer '{layer.Name}' contains no points and cannot be used by monitor '{monitor.Name}'.");
        if (layer.Points.Any(point => !CoordinateRules.IsValidLatitude(point.Latitude) || !CoordinateRules.IsValidLongitude(point.Longitude)))
            throw new InvalidOperationException($"The selected coordinate layer '{layer.Name}' contains invalid WGS84 coordinates.");

        return layer;
    }

    private static int AddCoordinateColumn(string name, IReadOnlyList<ColumnInfo> columns, List<string> selected)
    {
        var column = columns.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        if (column is null || !MonitorCoordinateTypes.IsSupported(column.TypeName))
            throw new InvalidOperationException($"Monitor coordinate column '{name}' is missing or cannot contain numeric coordinate values.");

        var index = selected.FindIndex(c => string.Equals(c, column.Name, StringComparison.Ordinal));
        if (index < 0)
        {
            selected.Add(column.Name);
            index = selected.Count - 1;
        }
        return index;
    }

    private static double ReadCoordinate(object? value, string column, string coordinate, long rowNumber, string monitorName)
    {
        if (value is null or DBNull)
            throw new InvalidOperationException($"Monitor '{monitorName}' found a missing {coordinate} in database column '{column}' at candidate row {rowNumber}.");

        double parsed;
        try
        {
            parsed = Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or ArgumentException)
        {
            throw new InvalidOperationException($"Monitor '{monitorName}' found a missing or invalid {coordinate} in database column '{column}' at candidate row {rowNumber}.", ex);
        }

        var valid = coordinate == "latitude"
            ? CoordinateRules.IsValidLatitude(parsed)
            : CoordinateRules.IsValidLongitude(parsed);
        if (!valid)
            throw new InvalidOperationException($"Monitor '{monitorName}' found a missing or out-of-range {coordinate} in database column '{column}' at candidate row {rowNumber}; expected finite WGS84 coordinates.");
        return parsed;
    }

    private static (MapPoint Point, double DistanceMeters) FindNearest(
        MapLayer layer, double latitude, double longitude, CancellationToken ct)
    {
        MapPoint? nearest = null;
        var distance = double.PositiveInfinity;
        for (var i = 0; i < layer.Points.Count; i++)
        {
            if ((i & 0x3FF) == 0) ct.ThrowIfCancellationRequested();
            var point = layer.Points[i];
            var candidate = MonitorProximity.HaversineMeters(latitude, longitude, point.Latitude, point.Longitude);
            if (candidate >= distance) continue;
            nearest = point;
            distance = candidate;
        }
        return (nearest!, distance);
    }

    /// <summary>
    /// Upper end of the checked window. A row with a slightly older date can be committed a moment after a newer one
    /// (parallel transactions); if the window ended exactly at MAX such a row would be skipped forever. So for date
    /// columns the end is min(MAX, server time - settle). It can only delay a row (never lose it): an idle table is
    /// still reported once the settle time has passed, and a column in another time zone just waits longer.
    /// Number columns (identity) have no clock, so they are used as they are.
    /// </summary>
    public static string? SettleUpper(WatermarkReading reading, string typeName, int settleSeconds)
    {
        if (reading.Max is null || settleSeconds <= 0 || reading.ServerNow is not { } now || !WatermarkTypes.IsDate(typeName))
            return reading.Max;

        var limit = WatermarkTypes.Format(now.AddSeconds(-settleSeconds));
        return WatermarkComparer.Compare(reading.Max, limit, typeName) > 0 ? limit : reading.Max;
    }
}
