# Module Map

Rules: keep responsibilities explicit; record dependency direction; record ownership, not only references. Inventory based on the branch listing at documentation time (Needs verification with `git ls-files`).

| Module | Responsibility | Location | Depends On | Owned Data | Tests |
|---|---|---|---|---|---|
| Domain | Models, rules, geometry, monitoring rules, Persian calendar codec | `src/FastDbExplorer.Domain` | none (target) | none | Domain logic via Infrastructure tests (no dedicated project) |
| Application | Interfaces and request/result records | `src/FastDbExplorer.Application` | Domain | none | indirect |
| Infrastructure: SQL | Metadata, paging SQL, guard, typed values, connection strings | `src/FastDbExplorer.Infrastructure` (root files) | Application, Domain, SqlClient | none (reads the monitored DB) | `QueryBuilderTests`, `InfrastructureTests` |
| Infrastructure: profiles and secrets | Profile JSON, DPAPI vault | `JsonConnectionProfileStore.cs`, `DpapiCredentialVault.cs` | Application, Domain | `%AppData%\FastDbExplorer\connections.json`, `credentials.json` | `InfrastructureTests`, `CredentialVaultTests` |
| Infrastructure: maps | File-type detection, MBTiles, GMDB, vector layer reader | `Maps/*` | Application, Domain, Sqlite | read-only map files | `MapSourceTests` |
| Infrastructure: coordinate layers | Excel import, JSON layer store | `CoordinateLayers/*` | Application, Domain, ExcelDataReader | `%AppData%\FastDbExplorer\coordinate-layers\` | `MapMarkerCatalogTests` (store part) |
| Infrastructure: monitoring and export | Monitor store, scheduler, check service, watermark, XLSX and CSV writers | `Monitoring/*` | Application, Domain | monitor definitions, reports folder | `QueryExportTests` |
| Wpf: composition | DI, splash, crash log, startup | `App.xaml.cs` | everything | none | none |
| Wpf: shell | Main window, navigation, sessions, palette | `MainWindow.*`, `ViewModels/MainViewModel.cs` | ViewModels, Views | session state | none |
| Wpf: connections | Profiles, password flows | `ConnectionViewModel.cs`, `Views/ConnectionsView.*` | Application | profile form state | none (gap) |
| Wpf: explorer | Tables, query, filters, paging, export, map send | `ExplorerViewModel.cs`, `TableQueryViewModel.cs`, `FilterRowViewModel.cs`, `Views/Explorer*`, `TableQueryView.*` | Application | grid state | none (gap) |
| Wpf: map | Map document, layers, results, tools, pop-out window | `MapViewModel*.cs`, `Views/MapView*.cs`, `Views/MapWindow.*`, `Assets/Map/*` | Application, Domain, WebView2 | map session | none (gap) |
| Wpf: monitoring | Monitors, editor, results preview, alerts | `MonitoringViewModel*.cs`, `MonitorEditorViewModel.cs`, `MonitorItemViewModel.cs`, `Views/Monitoring*`, `MonitorEditorView.*` | Application, Domain | monitor UI state | none (gap) |
| Wpf: shared map send | Shared panel for Explorer and Monitoring | `Views/MapSendPanel.*`, `Localization/MapSendStrings.cs` | none | none | none |
| Wpf: services | Theme, file dialogs, folder picker, alerts, icon geometry | `Services/*` | Windows APIs | none | none |
| Wpf: localization | All user-facing texts | `Localization/*.cs` | none | none | none |
| Wpf: themes | Light and dark resources | `Themes/*.xaml` | none | none | none |
| Tests | xUnit tests | `tests/FastDbExplorer.Tests` | Infrastructure, Domain | none | itself |

Known ownership gaps: no test owns the ViewModel and View modules (see `docs/REVIEW_2026-10.md` F5).
