using FastDbExplorer.Domain;

namespace FastDbExplorer.Application.Abstractions;

/// <summary>Where monitor definitions live. Never the monitored database (the app is read-only there).</summary>
public interface IMonitorStore
{
    Task<IReadOnlyList<MonitorDefinition>> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(IReadOnlyList<MonitorDefinition> monitors, CancellationToken ct = default);
}

/// <summary>Reads the current maximum of a watermark column (one cheap, read-only query).</summary>
public interface IWatermarkReader
{
    Task<string?> GetMaxAsync(ConnectionSettings settings, string database, string schema, string table, string column, CancellationToken ct);
}

/// <summary>Writes the matching records to a report file and returns its full path.</summary>
public interface IReportWriter
{
    Task<string> WriteAsync(
        string monitorName, DateTime checkedAtUtc, IReadOnlyList<string> columns,
        IReadOnlyList<object?[]> rows, string folder, CancellationToken ct);
}

/// <summary>Runs one check of one monitor: finds new records that match the conditions and writes the report.</summary>
public interface IMonitorCheckService
{
    Task<MonitorCheckResult> CheckAsync(ConnectionSettings settings, MonitorDefinition monitor, CancellationToken ct);
}
