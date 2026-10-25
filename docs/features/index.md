# Feature Map

Each feature must answer: where it starts, where its business logic lives, where persistence lives, where UI/API adapters live, where its tests live, what it may depend on, what must not depend on it, which files usually change.
Test files verified against the folder listing at commit `c66f747`.

| Feature | Entry point | Business logic | Persistence | UI | Tests (existing files) | Docs |
|---|---|---|---|---|---|---|
| Connect and profiles | `ConnectionsView`, `ConnectionViewModel.ConnectProfileAsync` | `Domain/Models.cs` (`SavedConnection`, `ConnectionSettings`), `SqlConnectionStringFactory` | `JsonConnectionProfileStore` -> `%AppData%\FastDbExplorer\connections.json` | `Views/ConnectionsView.*` | `InfrastructureTests` (connection string, profile store) | `docs/CREDENTIALS.md` |
| SQL password (opt-in) | `ConnectionViewModel.SaveProfileCoreAsync` | vault rules in `ConnectionViewModel` | `DpapiCredentialVault` -> `credentials.json` | locked mask and change flow in `ConnectionsView` | `CredentialVaultTests` (ViewModel rules: gap) | `docs/CREDENTIALS.md`, ADR-0003 |
| Shell and sessions | `MainWindow`, `MainViewModel.OnConnected` | session lifecycle in `MainViewModel` | none | `MainWindow.xaml`, `ConnectPromptView`, `HomeView` | none (gap F5) | `docs/NAV_REDESIGN.md` |
| Explorer: tables | `ExplorerView`, `ExplorerViewModel.LoadTablesAsync` | `SqlServerMetadataService.GetTablesAsync` (sys.tables + sys.partitions) | none | `Views/ExplorerView.*` | none (gap F5); owner integration | `docs/FEATURES.md` |
| Explorer: query, filters, paging | `TableQueryView`, `TableQueryViewModel.RunAsync` | `FilterRowViewModel`, `SelectQueryBuilder`, `ReadOnlySqlGuard`, `ValueConverter` | none | `Views/TableQueryView.*` | `QueryBuilderTests` (ViewModel: gap F5) | `docs/MONITORING.md` (paging notes), `PROJECT_MASTER_CONTEXT` history |
| Explorer: export | `TableQueryViewModel.ExportExcelAsync` | `ExportLimits`, `QueryExportResult` | writes `.xlsx` or CSV parts | export button in `TableQueryView` | `QueryExportTests` | `docs/EXPORT_AND_LAYOUT.md` |
| Map: open and render | `MapViewModel.OpenPathAsync`, `MapView` | `MapSourceFactory`, `MbTilesMapSource`, `GmdbMapSource`, `MvtLayerReader` | read-only map files | `Views/MapView.*`, `Assets/Map/map.html` | `MapSourceTests`, `MapSettingsStoreTests` | `docs/MAP_UI.md`, ADR-0004 |
| Map: window (pop-out) | `MapViewModel` pop-out, `MapView.OpenDetachedWindow` | window rules in `MapViewModel.Window.cs` | none | `Views/MapWindow.*` | none (gap F5) | ADR-0004 |
| Map: coordinate layers | `MapViewModel.ImportExcelAsync`, `CoordinateLayerItem` | `Domain/CoordinateLayers.cs` (`MapPoint`, `MapLayer`, `CoordinateRules`), `Domain/GeoMath.cs` | `JsonCoordinateLayerStore` -> `coordinate-layers\*.json` | layer cards in `MapView.xaml`, `coordinate-layers.js` | `CoordinateLayerTests` (`ExcelImportTests`, `MapLayerTests`, `JsonCoordinateLayerStoreTests`), `CoordinateRenderingTests` | `docs/MAP_UI.md`, ADR-0006 (to write) |
| Map: marker styles | `MapMarkerCatalog`, `MapSourceStyles` | marker shapes and colours (presentation data in Domain, V1) | layer JSON (`iconKey`, `color`) | `MapIconGeometry`, `LayerStyleChoices` | `MapMarkerCatalogTests`, `MapMarkerCategoryTests`, `MapSourceStylesTests` | `docs/MAP_UI.md` |
| Map: tools | `map-tools.js`, `MapView.Tools.cs` | coordinate parsing and distance in JS; "my points" in `MapViewModel.Tools.cs` | "my points" layer JSON | overlay in the page | none (JS untested) | `docs/MAP_TOOLS.md` |
| Map: result sets from Explorer and Monitoring | `TableQueryViewModel.ShowSelectedOnMapAsync`, `MainViewModel` events | `MapViewModel.ShowQueryResultLayer`, duplicate rule | none (not persisted, by design) | chips in `map-tools.js` | none (gap F5) | `docs/MAP_SEND_UX.md`, ADR-0005 |
| Shared "show on map" panel | `Views/MapSendPanel.*` | rules inside the control | none | used by Explorer and Monitoring | none | `docs/MAP_SEND_UX.md` |
| Monitoring: monitors and checks | `MonitoringView`, `MonitoringViewModel`, `MonitorEditorView` | `MonitorCheckService`, `WatermarkComparer`, `Domain/MonitoringModels.cs` | `JsonMonitorStore` -> monitors file; watermark state | `Views/Monitoring*`, `MonitorEditorView.*` | `MonitoringTests` (coverage per rule: to map) | `docs/MONITORING.md` |
| Monitoring: proximity and alerts | `MonitorCheckService` | `MonitorProximity` (haversine) | none | alert list in `MonitoringView`, `AlertNotifier` | `MonitoringTests` (to map) | `docs/MONITORING.md` |
| Monitoring: reports | `MonitorCheckService` -> `XlsxReportWriter` | `ExportLimits` rule | report files in the configured folder | open and show-in-folder links | `QueryExportTests` (monitoring report rule), `MonitoringTests` | `docs/MONITORING.md` |
| Theme | `ThemeService` | none | Windows registry (read) | `Themes/*.xaml` | none | ADR-0001 notes; `docs/DATETIME_PICKER.md` |
| Localization | `Localization/*.cs` | none | none | `x:Static` in XAML | none (owner checks) | rule R6 |
| Persian date and time | `Domain/PersianDateTimeCodec.cs` | Jalali formatting and parsing | none | `ctrl:PersianDateTimeValueBox`, filters | `PersianDateTimeCodecTests` | `docs/DATETIME_PICKER.md` |
| Demo login gate | `LoginWindow`, `DemoLoginGate` | demo only, not security | none | `Views/LoginWindow.*` | `InfrastructureTests` (gate) | `docs/DEMO_LOGIN.md` |
| Crash logging | `App.xaml.cs` (exception handlers), `ProcessFailed` in `MapView` | none | `%LocalAppData%\FastDbExplorer\logs\crash.log` | none | none | `docs/RUNBOOK.md` |

## Manual checklists (owner, on Windows)
- Connect: profile save, connect, switch profile, restart app, connect without a prompt.
- Explorer: run a query with 3 filters; page forward and back; export a small and a big result.
- Map: open MBTiles and GMDB; import an Excel layer; use go-to, pick, measure; pop out and close the window; click the Map item while the window is open.
- Monitoring: create a monitor; run a check; check the report; remove the monitor.
- Theme: switch Windows light and dark; check text direction in every screen.
