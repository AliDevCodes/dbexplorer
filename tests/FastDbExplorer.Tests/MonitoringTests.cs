using System.IO.Compression;
using System.Xml.Linq;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;
using FastDbExplorer.Infrastructure;
using FastDbExplorer.Infrastructure.Monitoring;
using Xunit;

namespace FastDbExplorer.Tests;

public class MonitoringTests
{
    private static readonly ConnectionSettings Settings = new("srv", AuthenticationMode.Windows, null, null, true);

    private static readonly IReadOnlyList<ColumnInfo> Columns =
    [
        new ColumnInfo("Id", "int", 1),
        new ColumnInfo("Place", "nvarchar", 0)
    ];

    private static MonitorDefinition Monitor(string? last = null, DateTime? checkedAt = null, int minutes = 60, bool enabled = true) =>
        new(Guid.NewGuid(), "m", "srv", "db", "dbo", "T", "Id",
            [new FilterCondition("Place", FilterOperator.EqualTo, "A")], FilterLogic.And, minutes, Path.GetTempPath(), false, enabled,
            last, checkedAt);

    [Fact]
    public void Required_filters_are_and_ed_with_user_filters()
    {
        var request = new PageRequest("db", "dbo", "T", ["Id", "Place"],
            [new FilterCondition("Place", FilterOperator.EqualTo, "A"), new FilterCondition("Place", FilterOperator.EqualTo, "B")],
            FilterLogic.Or, ["Id"], false, 100, 0, null,
            [new FilterCondition("Id", FilterOperator.GreaterThan, "10"), new FilterCondition("Id", FilterOperator.LessOrEqual, "20")]);

        var built = SelectQueryBuilder.Build(request, Columns);

        Assert.Contains("WHERE ([Place] = @p0 OR [Place] = @p1) AND [Id] > @p2 AND [Id] <= @p3 ORDER BY", built.Sql);
        Assert.Equal(10, (int)built.Parameters.Single(p => p.Name == "@p2").Value);
        Assert.Equal(20, (int)built.Parameters.Single(p => p.Name == "@p3").Value);
    }

    [Fact]
    public void Watermark_comparer_compares_numbers_and_dates_not_text()
    {
        Assert.True(WatermarkComparer.Compare("100", "99", "int") > 0); // as text "100" < "99"
        Assert.True(WatermarkComparer.Compare("2026-10-05 08:00:00.0000000", "2026-10-05 07:59:59.0000000", "datetime2") > 0);
        Assert.Equal(0, WatermarkComparer.Compare("5", "5", "bigint"));
    }

    [Fact]
    public void IsDue_follows_enabled_flag_interval_and_last_attempt()
    {
        var now = new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

        Assert.True(MonitorScheduler.IsDue(Monitor(), now, null));
        Assert.False(MonitorScheduler.IsDue(Monitor(enabled: false), now, null));
        Assert.False(MonitorScheduler.IsDue(Monitor(checkedAt: now.UtcDateTime.AddMinutes(-30)), now, null));
        Assert.True(MonitorScheduler.IsDue(Monitor(checkedAt: now.UtcDateTime.AddMinutes(-61)), now, null));
        // a recent attempt (even a failed one) postpones the next run by a full interval
        Assert.False(MonitorScheduler.IsDue(Monitor(checkedAt: now.UtcDateTime.AddHours(-2)), now, now.AddMinutes(-5)));
    }

    [Fact]
    public async Task Xlsx_is_a_valid_package_with_expected_cells()
    {
        var dir = Path.Combine(Path.GetTempPath(), "fdbx-" + Guid.NewGuid().ToString("N"));
        try
        {
            var path = await new XlsxReportWriter().WriteAsync(
                "پایش/تست", new DateTime(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc), ["Id", "Name", "When"],
                [[1, "علی & <x>", new DateTime(2026, 1, 2, 3, 4, 5)], [2, null, null]], dir, CancellationToken.None);

            Assert.True(File.Exists(path));
            Assert.EndsWith(".xlsx", path);
            Assert.DoesNotContain('/', Path.GetFileName(path));

            using var zip = ZipFile.OpenRead(path);
            foreach (var name in new[] { "[Content_Types].xml", "_rels/.rels", "xl/workbook.xml", "xl/_rels/workbook.xml.rels", "xl/styles.xml", "xl/worksheets/sheet1.xml" })
                Assert.NotNull(zip.GetEntry(name));

            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            using var sheet = zip.GetEntry("xl/worksheets/sheet1.xml")!.Open();
            var cells = XDocument.Load(sheet).Descendants(ns + "c").ToList();

            Assert.Contains(cells, c => (string?)c.Attribute("r") == "A1" && c.Value == "Id");
            Assert.Contains(cells, c => (string?)c.Attribute("r") == "A2" && c.Value == "1");
            Assert.Contains(cells, c => (string?)c.Attribute("r") == "B2" && c.Value == "علی & <x>");
            Assert.Contains(cells, c => (string?)c.Attribute("r") == "C2" && c.Value == "2026-01-02 03:04:05");
            Assert.DoesNotContain(cells, c => (string?)c.Attribute("r") == "B3"); // NULL = empty cell
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public async Task Store_round_trips_a_monitor()
    {
        var path = Path.Combine(Path.GetTempPath(), "fdbx-" + Guid.NewGuid().ToString("N"), "monitors.json");
        try
        {
            var store = new JsonMonitorStore(path);
            var original = Monitor("42", new DateTime(2026, 10, 5, 8, 0, 0, DateTimeKind.Utc), 120);
            await store.SaveAsync([original]);

            var loaded = (await store.LoadAsync()).Single();
            Assert.Equal(original.Id, loaded.Id);
            Assert.Equal(120, loaded.IntervalMinutes);
            Assert.Equal("42", loaded.LastWatermark);
            Assert.Equal(FilterOperator.EqualTo, loaded.Conditions.Single().Operator);
            Assert.Equal(FilterLogic.And, loaded.Logic);
            Assert.DoesNotContain("password", await File.ReadAllTextAsync(path), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            var dir = Path.GetDirectoryName(path)!;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public async Task First_check_only_records_the_starting_point()
    {
        var metadata = new FakeMetadata(Columns, new PageResult(["Id", "Place"], [], false, null));
        var service = new MonitorCheckService(metadata, new FakeWatermark("500"), new FakeReports());

        var result = await service.CheckAsync(Settings, Monitor(), CancellationToken.None);

        Assert.True(result.IsBaseline);
        Assert.Equal("500", result.NewWatermark);
        Assert.Empty(result.Rows);
        Assert.Equal(0, metadata.PageCalls);
    }

    [Fact]
    public async Task New_matching_rows_are_reported_and_the_watermark_moves_to_the_window_end()
    {
        var rows = new List<object?[]> { new object?[] { 11, "A" }, new object?[] { 12, "A" } };
        var metadata = new FakeMetadata(Columns, new PageResult(["Id", "Place"], rows, false, null));
        var reports = new FakeReports();
        var service = new MonitorCheckService(metadata, new FakeWatermark("12"), reports);

        var result = await service.CheckAsync(Settings, Monitor("10", DateTime.UtcNow.AddHours(-2)), CancellationToken.None);

        Assert.False(result.IsBaseline);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("12", result.NewWatermark);
        Assert.Equal("fake.xlsx", result.ReportPath);
        Assert.Equal(2, reports.RowCount);

        var required = metadata.LastRequest!.RequiredFilters!;
        Assert.Equal(FilterOperator.GreaterThan, required[0].Operator);
        Assert.Equal("10", required[0].Value);
        Assert.Equal(FilterOperator.LessOrEqual, required[1].Operator);
        Assert.Equal("12", required[1].Value);
    }

    [Fact]
    public async Task Nothing_new_means_no_query_and_no_report()
    {
        var metadata = new FakeMetadata(Columns, new PageResult(["Id", "Place"], [], false, null));
        var reports = new FakeReports();
        var service = new MonitorCheckService(metadata, new FakeWatermark("10"), reports);

        var result = await service.CheckAsync(Settings, Monitor("10", DateTime.UtcNow.AddHours(-2)), CancellationToken.None);

        Assert.Empty(result.Rows);
        Assert.Null(result.ReportPath);
        Assert.Equal("10", result.NewWatermark);
        Assert.Equal(0, metadata.PageCalls);
        Assert.Equal(0, reports.Calls);
    }

    private sealed class FakeMetadata(IReadOnlyList<ColumnInfo> columns, PageResult page) : IDatabaseMetadataService
    {
        public int PageCalls { get; private set; }
        public PageRequest? LastRequest { get; private set; }

        public Task TestConnectionAsync(ConnectionSettings settings, CancellationToken ct) => Task.CompletedTask;
        public Task<IReadOnlyList<DatabaseInfo>> GetDatabasesAsync(ConnectionSettings settings, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<DatabaseInfo>>([]);
        public Task<IReadOnlyList<TableInfo>> GetTablesAsync(ConnectionSettings settings, string database, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<TableInfo>>([]);
        public Task<IReadOnlyList<ColumnInfo>> GetColumnsAsync(ConnectionSettings settings, string database, string schema, string table, CancellationToken ct) =>
            Task.FromResult(columns);
        public Task<PageResult> GetPageAsync(ConnectionSettings settings, PageRequest request, CancellationToken ct)
        {
            PageCalls++;
            LastRequest = request;
            return Task.FromResult(page);
        }
    }

    private sealed class FakeWatermark(string? max) : IWatermarkReader
    {
        public Task<string?> GetMaxAsync(ConnectionSettings settings, string database, string schema, string table, string column, CancellationToken ct) =>
            Task.FromResult(max);
    }

    private sealed class FakeReports : IReportWriter
    {
        public int Calls { get; private set; }
        public int RowCount { get; private set; }

        public Task<string> WriteAsync(string monitorName, DateTime checkedAtUtc, IReadOnlyList<string> columns, IReadOnlyList<object?[]> rows, string folder, CancellationToken ct)
        {
            Calls++;
            RowCount = rows.Count;
            return Task.FromResult("fake.xlsx");
        }
    }
}
