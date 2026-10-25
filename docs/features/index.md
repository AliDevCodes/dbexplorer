# Feature map

Each feature answers: where it starts, where its business logic lives, where persistence lives, where UI adapters live, where its tests live, what it may depend on, which files usually change.
Test files verified at commit `c66f747`; filter rules at `5818b9d`; documents moved in the reorganisation of 2026-10-25.

## Feature pages (one per feature, in this folder)
| Page | Feature |
|---|---|
| `credentials.md` | saved connections and the encrypted SQL password (ADR-0003) |
| `monitoring.md` | Monitoring: watched tables, schedule, watermark, alerts, reports |
| `explorer-export-and-layout.md` | Explorer result layout, smart export (xlsx or split CSV) |
| `map-ui.md` | map screen layout, side panel, marker icons |
| `map-tools.md` | map tools: coordinates, distance, my points, result sets |
| `map-send-panel.md` | shared "show on the map" panel (Explorer and Monitoring) |
| `coordinate-layers.md` | Excel coordinate layers and radius circles |
| `persian-date-time-picker.md` | Persian date-time picker and Jalali display |
| `navigation-and-shell.md` | navigation rail, home, top bar, command palette |
| `demo-login.md` | demo login gate (not security) |

| Feature | Entry point | Business logic | Persistence | UI | Tests (existing files) | Page |
|---|---|---|---|---|---|---|
| Connect and profiles | `ConnectionsView`, `ConnectionViewModel.ConnectProfileAsync` | `Domain/Models.cs` (`SavedConnection`, `ConnectionSettings`), `SqlConnectionStringFactory` | `JsonConnectionProfileStore` -> `%AppData%\FastDbExplorer\connections.json` | `Views/ConnectionsView.*` | `InfrastructureTests` | `credentials.md` |
| SQL password (opt-in) | `ConnectionViewModel.SaveProfileCoreAsync` | vault rules in `ConnectionViewModel` (to Domain later, S5) | `DpapiCredentialVault` -> `credentials.json` | locked mask and change flow | `CredentialVaultTests` (ViewModel rules: gap) | `credentials.md`, ADR-0003 |
| Shell and sessions | `MainWindow`, `MainViewModel.OnConnected` | session lifecycle in `MainViewModel` | none | `MainWindow.xaml`, `ConnectPromptView`, `HomeView` | none (gap S2) | `navigation-and-shell.md`, `architecture/runtime-flows.md` |
| Explorer: tables | `ExplorerView`, `ExplorerViewModel.LoadTablesAsync` | `SqlServerMetadataService.GetTablesAsync` | none | `Views/ExplorerView.*` | none (gap S2) | `navigation-and-shell.md` |
| Explorer: query, filters, paging | `TableQueryView`, `TableQueryViewModel.RunAsync` | `Domain/FilterOperatorRules.cs`, `FilterRowViewModel`, `SelectQueryBuilder`, `ReadOnlySqlGuard`, `ValueConverter` | none | `Views/TableQueryView.*` | `FilterOperatorRulesTests`, `QueryBuilderTests` (ViewModel: gap S2) | `explorer-export-and-layout.md`, `architecture/runtime-flows.md` |
| Explorer: date filters | `FilterRowViewModel.IsDateTimeField` | Jalali codec (presentation in Domain, S6) | none | `PersianDateTimeValueBox` | `PersianDateTimeCodecTests` | `persian-date-time-picker.md` |
| Explorer: export | `TableQueryViewModel.ExportExcelAsync` | `ExportLimits`, `QueryExportResult` | `.xlsx` or CSV parts | export button in `TableQueryView` | `QueryExportTests` | `explorer-export-and-layout.md` |
| Map: open and render | `MapViewModel.OpenPathAsync`, `MapView` | `MapSourceFactory`, `MbTilesMapSource`, `GmdbMapSource`, `MvtLayerReader` | read-only map files | `Views/MapView.*`, `Assets/Map/map.html` | `MapSourceTests`, `MapSettingsStoreTests` | `map-ui.md`, `reference/map-formats.md`, ADR-0004 |
| Map: window (pop-out) | `MapViewModel` pop-out, `MapView.OpenDetachedWindow` | window rules in `MapViewModel.Window.cs` | none | `Views/MapWindow.*` | none (gap S2) | ADR-0004 |
| Map: coordinate layers | `MapViewModel.ImportExcelAsync`, `CoordinateLayerItem` | `Domain/CoordinateLayers.cs`, `Domain/GeoMath.cs` | `JsonCoordinateLayerStore` -> `coordinate-layers\*.json` | layer cards, `coordinate-layers.js` | `CoordinateLayerTests` (`ExcelImportTests`, `MapLayerTests`, `JsonCoordinateLayerStoreTests`), `CoordinateRenderingTests` | `coordinate-layers.md`, ADR-0006 |
| Map: marker styles | `MapMarkerCatalog`, `MapSourceStyles` | marker shapes and colours (presentation data in Domain, S6) | layer JSON (`iconKey`, `color`) | `MapIconGeometry`, `LayerStyleChoices` | `MapMarkerCatalogTests`, `MapMarkerCategoryTests`, `MapSourceStylesTests` | `map-ui.md` |
| Map: tools | `map-tools.js`, `MapView.Tools.cs` | coordinate parsing and distance in JS; "my points" in `MapViewModel.Tools.cs` | "my points" layer JSON | overlay in the page | none (JS untested) | `map-tools.md` |
| Map: result sets from Explorer and Monitoring | `TableQueryViewModel.ShowSelectedOnMapAsync`, `MainViewModel` events | `MapViewModel.ShowQueryResultLayer` (duplicate rule) | none (not persisted, D34) | chips in `map-tools.js` | none (gap S2) | `map-tools.md`, ADR-0005 |
| Shared "show on map" panel | `Views/MapSendPanel.*` | rules inside the control | none | used by Explorer and Monitoring | none | `map-send-panel.md` |
| Monitoring: monitors and checks | `MonitoringView`, `MonitoringViewModel`, `MonitorEditorView` | `MonitorCheckService`, `WatermarkComparer`, `Domain/MonitoringModels.cs` | `JsonMonitorStore`; watermark state | `Views/Monitoring*`, `MonitorEditorView.*` | `MonitoringTests` (coverage per rule: to map) | `monitoring.md` |
| Monitoring: proximity and alerts | `MonitorCheckService` | `MonitorProximity` (haversine) | none | alert list, `AlertNotifier` | `MonitoringTests` (to map) | `monitoring.md` |
| Monitoring: reports | `MonitorCheckService` -> `XlsxReportWriter` | `ExportLimits` rule | report files | open and show-in-folder links | `QueryExportTests`, `MonitoringTests` | `monitoring.md` |
| Theme | `ThemeService` | none | Windows registry (read) | `Themes/*.xaml` | none | ADR-0001 notes |
| Localization | `Localization/*.cs` | none | none | `x:Static` in XAML | none (owner checks) | rule R6 |
| Demo login gate | `LoginWindow`, `DemoLoginGate` | demo only, not security | none | `Views/LoginWindow.*` | `InfrastructureTests` (gate) | `demo-login.md` |
| Crash logging | `App.xaml.cs`, `ProcessFailed` in `MapView` | none | `%LocalAppData%\FastDbExplorer\logs\crash.log` | none | none (S11) | `guides/runbook.md` (to move) |

## Manual checklists
Each page has its own owner checklist. Summary of the main paths:
- Connect: save a profile, connect, switch profile, restart app, connect without a prompt.
- Explorer: query with 3 filters (include an IS NULL and a BETWEEN); page forward and back; export a small and a big result.
- Map: open MBTiles and GMDB; import an Excel layer; go-to, pick, measure; pop out and close; click the Map item while the window is open.
- Monitoring: create a monitor; run a check; check the report; remove the monitor.
- Theme: switch Windows light and dark; check text direction on every screen.
