# Runtime Flows

How the app runs, step by step. Use this when debugging: find the step where the behaviour differs from the expectation.

## Start-up
1. `App.OnStartup`: unhandled exception hooks, splash window (`Report(step, total, text)`).
2. Build the DI host (`Host.CreateApplicationBuilder`), register services (see `App.xaml.cs`).
3. `_host.StartAsync`; `ThemeService.Start`; create `MainWindow`; show it; close the splash after at least 1.2 s (`MinSplashTime`).
4. In the background: `MainViewModel.InitializeAsync` reopens the last map (`LoadLastMapAsync`) and loads the connection profiles.
5. Failure at start-up: splash closes, a message box is shown, `Shutdown(1)`.

## Connect
1. `ConnectionsView` selects a profile; `ConnectionViewModel.ConnectProfileAsync`.
2. SQL auth: password from memory, or from the DPAPI vault (no prompt). If missing: the form asks, no popup.
3. `IDatabaseMetadataService.GetDatabasesAsync` (`SqlServerMetadataService`) with a read-only connection string.
4. Success: profile saved (vault first), event `Connected`.
5. `MainViewModel.OnConnected`: the old session is shut down (`EndSession`), a new `ExplorerViewModel` and `MonitoringViewModel` are created, the map result sets are cleared.
6. Errors: `SqlException` becomes `DatabaseAccessException(kind)` in Infrastructure, then a Persian message via `Strings.Describe`.

## Tables
`ExplorerViewModel.LoadTablesAsync` reads `sys.tables` and `sys.partitions` (approximate row counts, no scan). Selecting a table creates a `TableQueryViewModel`.

## Query
1. `TableQueryViewModel.RunAsync` builds a `PageRequest` from the typed filters (`FilterRowViewModel`, operator rules in Domain).
2. `GetPageAsync`: `GetColumnsAsync` validates column names and finds the primary key; the paging capability is probed once per server and database (D27).
3. `SelectQueryBuilder.Build` creates SQL with typed parameters (OFFSET/FETCH or ROW_NUMBER fallback).
4. `ReadOnlySqlGuard.EnsureReadOnly` checks the SQL; `SqlDataReader` reads pageSize + 1 rows.
5. Paging: a stack of cursors. OFFSET below 10,000 rows; keyset above; keyset only when the primary key is usable.

## Export
`ExportExcelAsync` calls `IQueryReportWriter.WriteQueryAsync`: reads up to the workbook limit, writes `.xlsx`; a bigger result streams into split CSV parts (`CsvPartsWriter`). Cancel or error removes every file written.

## Excel coordinate layer
1. `ImportExcelCommand` -> `IFileDialogService.PickExcelFile` -> `ExcelCoordinateImportService` (first sheet; columns Name, Latitude, Longitude; WGS84 degrees).
2. Bad rows are skipped and reported with their row number; a file with no valid row gives no layer.
3. The `MapLayer` is saved as one JSON file (atomic write) and shown in the panel.
4. The view sends GeoJSON to the page (`coordUpsert`); `coordinate-layers.js` draws points, radius rings (ADR-0006) and labels.

## Map send from Explorer or Monitoring
`ShowSelectedOnMapAsync` -> `EnsureMapValuesAvailableAsync` -> `QueryMapPoint`s -> `MainViewModel` event -> `MapViewModel.ShowQueryResultLayer` (duplicate rule, no persistence) -> `QueryResultAdded` -> `MapView.PostQueryResultAdded` -> JS.

## Monitor check
`MonitorScheduler` -> `MonitorCheckService` (watermark read by `SqlServerWatermarkReader`, comparison by `WatermarkComparer`, proximity by `MonitorProximity`) -> report by `XlsxReportWriter` -> alert by `AlertNotifier`.

## Map open and tiles
`MapViewModel.OpenPathAsync` -> `MapSourceFactory` (by extension) -> `IMapSource`. `MapView` answers `https://tiles.local/{z}/{x}/{y}` through `WebResourceRequested` (see `map-formats.md`).

## Pop-out map
`ToggleWindow` -> `PopOutRequested` -> `MapView.OpenDetachedWindow`: this view releases its WebView2 (queued, ADR-0004), the Map section is left, `MapWindow` opens and creates its own page once the gate is free. Closing waits until disposal finished, then the Map section is shown again.
