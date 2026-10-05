# Monitoring (پایش و هشدار) — branch `feature/monitoring-alerts`

Status: **written, NOT compiled or run by the AI** (no .NET in the AI sandbox). Build + test on Windows first (`dotnet build FastDbExplorer.slnx`, `dotnet test`).

## What it does (phases 1–3 of the plan)
1. **Dynamic conditions** – the user picks database, table, the *watermark* column and any number of condition rows (same 13 operators / typed parameters as the table filters). Column names are never hard-coded: every table can call location / origin / destination differently.
2. **Scheduling** – each monitor has its own interval (1–1440 min). An in-process scheduler (`MonitorScheduler`) ticks every 15 s and runs due monitors one at a time. Runs only while the app is open and connected (password stays in memory).
3. **Alerts + Excel** – when new records match, a corner message appears, an optional sound plays, and an `.xlsx` with the matching records is written to the chosen folder. Alert history is shown in the monitoring screen (open report / show in folder).

## How "new since last check" works
- The user chooses a *watermark column*: increasing number (`int/bigint/smallint/tinyint`) or date (`date/datetime/datetime2/smalldatetime`), e.g. an identity PK or CreatedAt/UpdatedAt.
- Each check: `SELECT MAX(wm)` (one seek if indexed) → `upper`; query only `wm > last AND wm <= upper` plus the user's conditions, server side, ordered by `wm, PK`, OFFSET pages of 1000, cap 10,000 rows per check; then `last = upper`.
- **First check only records the starting point** (no alerts for old data). Table empty at that time → everything that appears later counts as new.
- If a check hits the 10,000-row cap, the watermark moves to the last row seen and the rest is picked up next time (rows sharing exactly the same watermark value at the cap boundary could be skipped; unlikely with ids / datetimes).
- Updated records are only detected when the watermark column changes on update (e.g. `UpdatedAt`).

## Safety rules kept
- Read-only: only `SELECT MAX(...)` and the normal parameterized page query, both through `ReadOnlySqlGuard`. Nothing is written to the monitored database.
- State lives in `%AppData%\FastDbExplorer\monitors.json` (definitions + last watermark; no passwords).
- No new NuGet package: the `.xlsx` is written by `XlsxReportWriter` (System.IO.Compression + XmlWriter). Columns with unsupported types (blobs, xml) are not read.

## Files
- Domain: `MonitoringModels.cs` (`MonitorDefinition`, `MonitorCheckResult`, `WatermarkTypes`); `PageRequest.RequiredFilters` (AND-ed with user filters).
- Application: `Abstractions/MonitoringAbstractions.cs`.
- Infrastructure/Monitoring: `MonitorCheckService`, `MonitorScheduler`, `SqlServerWatermarkReader`, `XlsxReportWriter`, `JsonMonitorStore`, `WatermarkComparer`.
- Wpf: `MonitoringView`, `MonitorEditorView`, `AlertToastWindow`, `MonitoringViewModel`, `MonitorEditorViewModel`, `MonitorItemViewModel`, `MonitoringViewModelFactory`, `Services/AlertNotifier|FolderPickerService|ShellOpen`, `Localization/MonitoringStrings`.
- Tests: `tests/FastDbExplorer.Tests/MonitoringTests.cs`.

## Not done yet (later phases of the plan)
History log sheet appended to reports (source/destination per record), Windows-service/background mode, alert sound file choice, per-monitor column selection for the report, remembered "start from" value.

## Risks to check on Windows
- Segoe glyph `&#xE823;` (clock) on the explorer sidebar and `&#xE7BA;` in the toast must render on Windows 10.
- `OpenFolderDialog` (.NET 8+ WPF) and `PeriodicTimer(TimeSpan, TimeProvider)` are used.
- Open the generated `.xlsx` in Excel once (Persian text, frozen header, right-to-left sheet).
