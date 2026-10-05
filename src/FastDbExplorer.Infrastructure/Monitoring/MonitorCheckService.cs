using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;

namespace FastDbExplorer.Infrastructure.Monitoring;

/// <summary>
/// One check = "which records appeared since the last check, and which of them match the user's conditions?".
/// Steps: read MAX(watermark) once, query only the window (last, max] with the conditions on the server,
/// page through the matches (OFFSET, ordered by watermark + primary key, capped), write the Excel report.
/// The first check only records the starting point, so old data never floods the user with alerts.
/// </summary>
public sealed class MonitorCheckService(IDatabaseMetadataService metadata, IWatermarkReader watermarks, IReportWriter reports)
    : IMonitorCheckService
{
    public const int PageSize = 1000;
    public const int MaxRows = 10_000;

    public async Task<MonitorCheckResult> CheckAsync(ConnectionSettings settings, MonitorDefinition monitor, CancellationToken ct)
    {
        var checkedAt = DateTime.UtcNow;
        var columns = await metadata.GetColumnsAsync(settings, monitor.Database, monitor.Schema, monitor.Table, ct);

        var watermark = columns.FirstOrDefault(c => c.Name == monitor.WatermarkColumn)
                        ?? throw new ArgumentException($"Column '{monitor.WatermarkColumn}' no longer exists in {monitor.TargetText}.");
        if (!WatermarkTypes.IsSupported(watermark.TypeName))
            throw new NotSupportedException($"Column '{watermark.Name}' ({watermark.TypeName}) cannot be used to detect new records.");

        // Only supported types are read (no blobs / xml), always by explicit name: never SELECT *.
        var selected = columns.Where(c => c.IsFilterable).Select(c => c.Name).ToList();
        if (selected.Count == 0) throw new ArgumentException("The table has no readable columns.");

        var upper = await watermarks.GetMaxAsync(settings, monitor.Database, monitor.Schema, monitor.Table, watermark.Name, ct);

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
        for (long offset = 0; ; offset += PageSize)
        {
            ct.ThrowIfCancellationRequested();
            var request = new PageRequest(
                monitor.Database, monitor.Schema, monitor.Table, selected, monitor.Conditions, monitor.Logic,
                order, false, PageSize, offset, null, required);
            var page = await metadata.GetPageAsync(settings, request, ct);
            rows.AddRange(page.Rows);
            if (!page.HasMore) break;
            if (rows.Count >= MaxRows) { truncated = true; break; }
        }

        // Normally the window ends at 'upper'. If we stopped early, continue next time right after the last row we saw.
        var next = upper;
        if (truncated)
        {
            var index = selected.IndexOf(watermark.Name);
            if (rows[^1][index] is { } last) next = WatermarkTypes.Format(last);
        }

        string? reportPath = null;
        if (rows.Count > 0)
            reportPath = await reports.WriteAsync(monitor.Name, checkedAt, selected, rows, monitor.OutputFolder, ct);

        return new MonitorCheckResult(monitor.Id, checkedAt, selected, rows, truncated, next, reportPath, false);
    }
}
