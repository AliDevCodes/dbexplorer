# Feature Map

Each feature answers: where it starts, where its business logic lives, where persistence lives, where UI adapters live, where its tests live, what it may depend on, which files usually change.
Test files verified against the folder listing at commit `c66f747`; filter rules verified at `5818b9d`.

| Feature | Entry point | Business logic | Persistence | UI | Tests (existing files) | Docs |
|---|---|---|---|---|---|---|
| Connect and profiles | `ConnectionsView`, `ConnectionViewModel.ConnectProfileAsync` | `Domain/Models.cs` (`SavedConnection`, `ConnectionSettings`), `SqlConnectionStringFactory` | `JsonConnectionProfileStore` -> `%AppData%\FastDbExplorer\connections.json` | `Views/ConnectionsView.*` | `InfrastructureTests` | `docs/CREDENTIALS.md` |
| SQL password (opt-in) | `ConnectionViewModel.SaveProfileCoreAsync` | vault rules in `ConnectionViewModel` (to Domain later, S5) | `DpapiCredentialVault` -> `credentials.json` | locked mask and change flow | `CredentialVaultTests` (ViewModel rules: gap) | `docs/CREDENTIALS.md`, ADR-0003 |
| Shell and sessions | `MainWindow`, `MainViewModel.OnConnected` | session lifecycle in `MainViewModel` | none | `MainWindow.xaml`, `ConnectPromptView`, `HomeView` | none (gap S2) | `docs/architecture/runtime-flows.md` |
| Explorer: tables | `ExplorerView`, `ExplorerViewModel.LoadTablesAsync` | `SqlServerMetadataService.GetTablesAsync` | none | `Views/ExplorerView.*` | none (gap S2) | `docs/FEATURES.md` |
| Explorer: query, filters, paging | `TableQueryView`, `TableQueryViewModel.RunAsync` | `Domain/FilterOperatorRules.cs`, `FilterRowViewModel`, `SelectQueryBuilder`, `ReadOnlySqlGuard`, `ValueConverter` | none | `Views/TableQueryView.*` | `FilterOperatorRulesTests`, `QueryBuilderTests` (ViewModel: gap S2) | `docs/architecture/runtime-flows.md` |
| Explorer: export | `TableQueryViewModel.ExportExcelAsync` | `ExportLimits`, `QueryExportResult` | `.xlsx` or CSV parts | export button in `TableQueryView` | `QueryExportTests` | `docs/EXPORT_AND_LAYOUT.md` |
| Map: open and render | `MapViewModel.OpenPathAsync`, `MapView` | `MapSourceFactory`, `MbTilesMapSource`, `GmdbMapSource`, `MvtLayerReader` | read-only map files | `Views/MapView.*`, `Assets/Map/map.html` | `MapSourceTests`, `MapSettingsStoreTests` | `docs/reference/map-formats.md`, ADR-0004 |
| Map: window (pop-out) | `MapViewModel` pop-out, `MapView.OpenDetachedWindow` | window rules in `MapViewModel.Window.cs` | none | `Views/MapWindow.*` | none (gap S2) | ADR-0004 |
| Map: coordinate layers | `MapViewModel.ImportExcelAsync`, `CoordinateLayerItem` | `Domain/CoordinateLayers.cs`, `Domain/GeoMath.cs` | `JsonCoordinateLayerStore` -> `coordinate-layers\*.json` | layer cards, `coordinate-layers.js` | `CoordinateLayerTests` (`ExcelImportTests`, `MapLayerTests`, `JsonCoordinateLayerStoreTests`), `CoordinateRenderingTests` | `docs/reference/map-formats.md`, ADR-0006 |
| Map: marker styles | `MapMarkerCatalog`, `MapSourceStyles` | marker shapes and colours (presentation data in Domain, S6) | layer JSON (`iconKey`, `color`) | `MapIconGeometry`, `LayerStyleChoices` | `MapMarkerCatalogTests`, `MapMarkerCategoryTests`, `MapSourceStylesTests` | `docs/MAP_UI.md` |
| Map: tools | `map-tools.js`, `MapView.Tools.cs` | coordinate parsing and distance in JS; "my points" in `MapViewModel.Tools.cs` | "my points" layer JSON | overlay in the page | none (JS untested) | `docs/MAP_TOOLS.md` |
| Map: result sets from Explorer and Monitoring | `TableQueryViewModel.ShowSelectedOnMapAsync`, `MainViewModel` events | `MapViewModel.ShowQueryResultLayer` (duplicate rule) | none (not persisted, D24) | chips in `map-tools.js` | none (gap S2) | `docs/MAP_SEND_UX.md`, ADR-0005 |
| Shared "show on map" panel | `Views/MapSendPanel.*` | rules inside the control | none | used by Explorer and Monitoring | none | `docs/MAP_SEND_UX.md` |
| Monitoring: monitors and checks | `MonitoringView`, `MonitoringViewModel`, `MonitorEditorView` | `MonitorCheckService`, `WatermarkComparer`, `Domain/MonitoringModels.cs` | `JsonMonitorStore`; watermark state | `Views/Monitoring*`, `MonitorEditorView.*` | `MonitoringTests` (coverage per rule: to map) | `docs/MONITORING.md` |
| Monitoring: proximity and alerts | `MonitorCheckService` | `MonitorProximity` (haversine) | none | alert list, `AlertNotifier` | `MonitoringTests` (to map) | `docs/MONITORING.md` |
| Monitoring: reports | `MonitorCheckService` -> `XlsxReportWriter` | `ExportLimits` rule | report files | open and show-in-folder links | `QueryExportTests`, `MonitoringTests` | `docs/MONITORING.md` |
| Theme | `ThemeService` | none | Windows registry (read) | `Themes/*.xaml` | none | ADR-0001 notes |
| Localization | `Localization/*.cs` | none | none | `x:Static` in XAML | none (owner checks) | rule R6 |
| Persian date and time | `Domain/PersianDateTimeCodec.cs` | Jalali formatting and parsing (presentation in Domain, S6) | none | `PersianDateTimeValueBox`, filters | `PersianDateTimeCodecTests` | `docs/DATETIME_PICKER.md` |
| Demo login gate | `LoginWindow`, `DemoLoginGate` | demo only, not security | none | `Views/LoginWindow.*` | `InfrastructureTests` (gate) | `docs/DEMO_LOGIN.md` |
| Crash logging | `App.xaml.cs`, `ProcessFailed` in `MapView` | none | `%LocalAppData%\FastDbExplorer\logs\crash.log` | none | none (S11) | `docs/RUNBOOK.md` |

## Manual checklists (owner, on Windows)
- Connect: profile save, connect, switch profile, restart app, connect without a prompt.
- Explorer: query with 3 filters (include an IS NULL and a BETWEEN); page forward and back; export a small and a big result.
- Map: open MBTiles and GMDB; import an Excel layer; go-to, pick, measure; pop out and close; click the Map item while the window is open.
- Monitoring: create a monitor; run a check; check the report; remove the monitor.
- Theme: switch Windows light and dark; check text direction on every screen.
