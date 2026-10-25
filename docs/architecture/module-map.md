# Module Map

Rules: keep responsibilities explicit; record dependency direction; record ownership, not only references.
Inventory verified on branch `feature/geosent-live-map` at commit `c66f747` (test file names checked against the folder listing).

| Module | Responsibility | Location | Depends On | Owned Data | Tests (existing files) |
|---|---|---|---|---|---|
| Domain | Models, rules, geometry, Persian calendar codec, marker catalogue | `src/FastDbExplorer.Domain` | none (target) | none | `CoordinateLayerTests` (MapLayer), `CoordinateRenderingTests` (GeoMath, RadiusInput), `PersianDateTimeCodecTests`, `MapMarkerCatalogTests`, `MapMarkerCategoryTests`, `MapSourceStylesTests` |
| Application | Interfaces and request/result records | `src/FastDbExplorer.Application` | Domain | none | indirect (through Infrastructure tests) |
| Infrastructure: SQL | Metadata, paging SQL, guard, typed values, connection strings | `src/FastDbExplorer.Infrastructure` (root files) | Application, Domain, SqlClient | none (reads the monitored DB) | `QueryBuilderTests`, `InfrastructureTests` |
| Infrastructure: profiles and secrets | Profile JSON, DPAPI vault | `JsonConnectionProfileStore.cs`, `DpapiCredentialVault.cs` | Application, Domain | `%AppData%\FastDbExplorer\connections.json`, `credentials.json` | `InfrastructureTests` (profile store), `CredentialVaultTests` |
| Infrastructure: maps | File-type detection, MBTiles, GMDB, vector layer reader | `Maps/*` | Application, Domain, Sqlite | read-only map files | `MapSourceTests`, `MapSettingsStoreTests` |
| Infrastructure: coordinate layers | Excel import, JSON layer store | `CoordinateLayers/*` | Application, Domain, ExcelDataReader | `%AppData%\FastDbExplorer\coordinate-layers\` | `CoordinateLayerTests` (`ExcelImportTests`, `JsonCoordinateLayerStoreTests`) |
| Infrastructure: monitoring and export | Monitor store, scheduler, check service, watermark, XLSX and CSV writers | `Monitoring/*` | Application, Domain | monitor definitions, report folder | `MonitoringTests`, `QueryExportTests` |
| Wpf: composition | DI, splash, crash log, startup | `App.xaml.cs` | everything | none | none |
| Wpf: shell | Main window, navigation, sessions, palette | `MainWindow.*`, `ViewModels/MainViewModel.cs` | ViewModels, Views | session state | none (gap F5) |
| Wpf: connections | Profiles, password flows | `ConnectionViewModel.cs`, `Views/ConnectionsView.*` | Application | profile form state | none (gap F5) |
| Wpf: explorer | Tables, query, filters, paging, export, map send | `ExplorerViewModel.cs`, `TableQueryViewModel.cs`, `FilterRowViewModel.cs`, `Views/Explorer*`, `TableQueryView.*` | Application | grid state | none (gap F5) |
| Wpf: map | Map document, layers, results, tools, pop-out window | `MapViewModel*.cs`, `Views/MapView*.cs`, `Views/MapWindow.*`, `Assets/Map/*` | Application, Domain, WebView2 | map session | none (gap F5); JS untested |
| Wpf: monitoring | Monitors, editor, results preview, alerts | `MonitoringViewModel*.cs`, `MonitorEditorViewModel.cs`, `MonitorItemViewModel.cs`, `Views/Monitoring*`, `MonitorEditorView.*` | Application, Domain | monitor UI state | none (gap F5) |
| Wpf: shared map send | Shared panel for Explorer and Monitoring | `Views/MapSendPanel.*`, `Localization/MapSendStrings.cs` | none | none | none |
| Wpf: services | Theme, file dialogs, folder picker, alerts, icon geometry | `Services/*` | Windows APIs | none | none |
| Wpf: localization | All user-facing texts | `Localization/*.cs` | none | none | none |
| Wpf: themes | Light and dark resources | `Themes/*.xaml` | none | none | none |
| Tests | xUnit tests | `tests/FastDbExplorer.Tests` | Infrastructure, Domain | none | itself |

## Verified corrections to the previous version
- Coordinate layers and geometry ARE tested (`CoordinateLayerTests.cs`, `CoordinateRenderingTests.cs`). The old docs that said so were right; the first version of this map said otherwise.
- Monitoring IS tested (`MonitoringTests.cs`, about 32 KB). Coverage per rule is not mapped yet; it is listed as a next step in `.ai/current-state.md`.
- Test files that existed but were not listed before: `PersianDateTimeCodecTests`, `MapSettingsStoreTests`, `MapMarkerCategoryTests`, `MapSourceStylesTests`.

## Known ownership gaps
No test owns the ViewModel and View modules (`docs/REVIEW_2026-10.md` F5). That is the only structural test gap.
