# FastDbExplorer — Project Brain

> **Single entry point for humans and AI agents.** Read this file first, then `CLAUDE.md`. Everything here reflects the code on this branch (`feature/geosent-live-map`) as reviewed on 2026-10-25. When code and this file disagree, the code wins and this file must be fixed in the same commit.
>
> Owner language: Persian (UI is Persian RTL). Docs: English, with Persian summaries where useful.

---

## 1. What the product is
A **read-only** WPF desktop explorer for very large SQL Server databases, plus map viewing and coordinate tools, plus Monitoring (watch a table for new rows and write reports).

Non-negotiable: the tool **never changes data** and **never changes database settings**.

## 2. Hard rules (never break; reviewed against code)
| # | Rule | Enforced by |
|---|---|---|
| R1 | Read-only SQL only (single SELECT/WITH) | `ReadOnlySqlGuard` on every command; `ApplicationIntent=ReadOnly` |
| R2 | Filtering on the server; never `SELECT *`, never `DataTable`, always paged, cancellable, 30 s timeout | `SelectQueryBuilder`, `SqlServerMetadataService` |
| R3 | User values only as typed parameters; identifiers come from the real column list and are bracket-quoted | `ValueConverter`, `SelectQueryBuilder` |
| R4 | No plaintext passwords on disk. SQL password optional, opt-in, encrypted with Windows DPAPI (CurrentUser), per profile | `DpapiCredentialVault`, `docs/CREDENTIALS.md` |
| R5 | No `COUNT(*)` on big tables; approximate rows from `sys.partitions`; "has next" = pageSize+1 | `SqlServerMetadataService` |
| R6 | Every user-facing text lives in `Localization/*.cs` (no literals in XAML except technical labels) | review checklist |
| R7 | Every I/O method takes a `CancellationToken` | convention, reviewed |

## 3. Stack
.NET 10 · WPF · CommunityToolkit.Mvvm 8.* (source generators) · Microsoft.Data.SqlClient 6.* · Microsoft.Extensions.Hosting 10.* (DI only) · Microsoft.Data.Sqlite 10.* (MBTiles/GMDB) · ExcelDataReader 3.* (Excel import) · System.Security.Cryptography.ProtectedData 10.* (DPAPI) · Microsoft.Web.WebView2 (map page) · MapLibre GL JS (bundled by `setup-map-assets.cmd`) · xUnit 2.* (tests). NuGet versions are floating; pin after the first restore.

**Open warning:** NU1902/NU1903 on `System.Security.Cryptography.Pkcs 9.0.11` (transitive, from SqlClient). Track and pin a fixed version.

## 4. Layers and dependency direction
```
FastDbExplorer.Wpf            (composition root, views, view models, WebView2 host, JS map page)
    │  uses
    ▼
FastDbExplorer.Infrastructure (SqlClient, SQLite, JSON stores, DPAPI, Excel I/O, CSV/XLSX writers)
    │  implements
    ▼
FastDbExplorer.Application    (interfaces + request/result records: the contracts)
    │  uses
    ▼
FastDbExplorer.Domain         (models, rules, geometry; target: no UI, no I/O, no serializer attributes)

tests/FastDbExplorer.Tests    → Infrastructure, Domain (no ViewModel tests yet)
```
Rule: a project may only reference the layer directly below it. Current violations are listed in `docs/REVIEW_2026-10.md` (Domain holds presentation data and a JSON attribute).

## 5. Module inventory (what exists today)

### 5.1 Application layer (contracts)
- `Abstractions.cs`: `IDatabaseMetadataService`, `IConnectionProfileStore`, `IFileDialogService`, paging contracts.
- `MapAbstractions.cs`, `MapSettingsAbstractions.cs`: `IMapSource`, `IMapSourceFactory`, `IMapSettingsStore`.
- `CoordinateLayerAbstractions.cs`: `ICoordinateLayerStore`, `IExcelImportService`.
- `CredentialAbstractions.cs`: `ICredentialVault`.
- `MonitoringAbstractions.cs`: `IMonitorStore`, `IWatermarkReader`, `IReportWriter`, `IQueryReportWriter`, `IMonitorCheckService`.
- `ExportAbstractions.cs`: `ExportLimits` (2 M cells per workbook, 500 k rows / 100 MB per CSV part), `QueryExportResult`.

### 5.2 Domain (models and rules)
- `Models.cs`: `ConnectionSettings`, `SavedConnection` (has `RememberPassword` flag, never the password), `DatabaseInfo`, `TableInfo`, `DatabaseAccessException` + `DatabaseErrorKind`.
- `QueryModels.cs`: `PageRequest`, `PageResult`, `FilterCondition`, `FilterLogic`, `FilterOperator`, `ColumnInfo`.
- `MapModels.cs`: map metadata, `MapTileKind`, `UnsupportedMapFormatException`.
- `CoordinateLayers.cs`: `MapPoint`, `MapLayer` (invariants enforced in the constructor), `CoordinateRules` (WGS84, radius in metres), import result/issue types.
- `MonitoringModels.cs`: `MonitorDefinition`, `MonitorCoordinateTypes`, `MonitorProximity` (haversine), `WatermarkTypes`, `MonitorCheckResult`.
- `GeoMath.cs`: circle rings in WGS84.
- `DemoLoginGate.cs`: demo-only UI gate (not security).
- `PersianDateTimeCodec.cs`: Jalali formatting/parsing (**presentation concern — should move, see review**).
- `MapMarkerCatalog.cs`, `MapSourceStyles.cs`: marker shapes, colours, SVG path data (**presentation concern — should move, see review**).

### 5.3 Infrastructure (I/O)
- SQL: `SqlServerMetadataService`, `SelectQueryBuilder`, `ValueConverter`, `ReadOnlySqlGuard`, `SqlConnectionStringFactory`.
- Profiles and secrets: `JsonConnectionProfileStore`, `DpapiCredentialVault`.
- Maps: `Maps/MapSourceFactory` (by extension), `MbTilesMapSource`, `GmdbMapSource`, `MvtLayerReader`.
- Coordinate layers: `CoordinateLayers/JsonCoordinateLayerStore` (one JSON per layer, atomic writes), `ExcelCoordinateImportService`.
- Monitoring: `JsonMonitorStore`, `MonitorCheckService`, `MonitorScheduler`, `SqlServerWatermarkReader`, `WatermarkComparer`, `XlsxReportWriter` (also the Explorer export writer), `CsvPartsWriter` (split CSV with cleanup on cancel).

### 5.4 Wpf (composition root and UI)
- `App.xaml.cs`: DI registrations (`Host.CreateApplicationBuilder`), splash, global exception logging to `%LocalAppData%\FastDbExplorer\logs\crash.log`.
- `MainWindow.xaml(.cs)`: custom title bar, navigation rail, top bar (active profile, database, search palette), **one persistent map host** (`MapHost`), Ctrl+1..5 and Ctrl+K.
- `ViewModels`: `MainViewModel` (sections, sessions), `ConnectionViewModel` (profiles, password flows), `ExplorerViewModel` (table list), `TableQueryViewModel` (grid, filters, paging, map send, export), `FilterRowViewModel` (one filter row), `MonitoringViewModel` (+ `.MapSend`, editor, items, factory), `MapViewModel` (+ `.Actions`, `.Tools`, `.View`, `.Window`), `CoordinateLayerItem`, `LayerStyleChoices`, `ConnectPromptViewModel`, `HomeViewModel`, `CommandPaletteViewModel`, `NavItemViewModel`.
- `Views`: `ConnectionsView`, `TableQueryView`, `ExplorerView`, `MonitoringView`, `MonitorEditorView`, `MapView` (+ `.Tools`), `MapWindow` (detached map), `MapSendPanel` (shared "show on map" control), `CommandPaletteWindow`, `SplashWindow`, `LoginWindow`, `ConnectPromptView`, `HomeView`, `AlertToastWindow`, `CellValueWindow`.
- `Services`: `ThemeService`, `FileDialogService`, `FolderPickerService`, `AlertNotifier`, `MapIconGeometry`.
- `Localization/*.cs`: `Strings`, `ShellStrings`, `MonitoringStrings`, `CoordinateStrings`, `MapUiStrings`, `MapToolStrings`, `MapSendStrings`, `MapEntryStrings`, `ExportStrings`, `CredentialStrings`, `WindowStrings`, `SplashStrings`, `LoginStrings`, `PaletteStrings`.
- `Themes/`: `Styles.xaml`, `Light.xaml`, `Dark.xaml` (merged-dictionary index 0 is the theme; see lessons).
- `Assets/Map/`: `map.html` (MapLibre host page), `coordinate-layers.js` (layers, markers, selection), `map-tools.js` (go-to, pick, measure, result chips, "my points" cards), `maplibre-gl.*`, `fonts/`.

### 5.5 Tests
- `InfrastructureTests.cs` (connection strings, profile store, guards), `QueryBuilderTests.cs`, `MapSourceTests.cs`, `CredentialVaultTests.cs`, `MapMarkerCatalogTests.cs`, `MapSourceStylesTests.cs`, `QueryExportTests.cs`.
- Last full run (owner, Windows): **204 / 204 passed.**
- **Gap:** no tests for ViewModels, Views, or the JS map pages. Most runtime bugs of this phase were there (see §9).

## 6. Feature status (as of this review)
| Area | Status | Owner-verified? |
|---|---|---|
| Connect (Windows / SQL), profiles, shell switch | done | yes (MVP-01) |
| Saved SQL password (DPAPI, opt-in), locked field, change flow | done | build+tests yes; UI flow partly |
| Explorer: tables, columns, filters (13 operators, AND/OR), hybrid paging | done | MVP-02 reported working |
| Explorer: Excel/CSV export (smart, split when big) | done | pending owner test of big export |
| Map: MBTiles vector/raster, GMDB, pop-out window, persistent map host | done | rendering confirmed; crashes fixed in latest commits |
| Map: Excel coordinate layers (icons, colours, radius, rename, refresh) | done | confirmed |
| Map: tools (go to, pick, measure, my points, result chips) | done | confirmed by owner in tests |
| Map: "show selected rows" from Explorer/Monitoring (shared `MapSendPanel`) | done | confirmed |
| Monitoring: monitors, schedule, watermark, conditions, proximity to layer, alerts, reports | done | partly |
| Map: OSM PBF converter | not started | — |
| Demo login gate | done | demo only |

## 7. Key flows (follow these when debugging)
1. **Connect:** `ConnectionsView` → `ConnectionViewModel.SaveProfileCoreAsync` (vault first, then profile) → `ConnectProfileAsync` (reads vault, no popup) → `MainViewModel.OnConnected` (new `ExplorerViewModel` + `MonitoringViewModel`, old session disposed).
2. **Query:** `TableQueryViewModel.RunAsync` → `TryBuildRequest` (typed filters) → `GetPageAsync` → `SelectQueryBuilder` → `ReadOnlySqlGuard` → `SqlDataReader` (pageSize+1).
3. **Paging:** `PageCursor` stack. OFFSET below 10 000 rows; keyset above; keyset only with a usable PK.
4. **Export:** `ExportExcelAsync` → `IQueryReportWriter.WriteQueryAsync` → reads up to the workbook limit, then `.xlsx`, or streams `CsvPartsWriter`.
5. **Send rows to map:** `ShowSelectedOnMapAsync` → `EnsureMapValuesAvailableAsync` → `QueryMapPoint`s → `MainViewModel` → `MapViewModel.ShowQueryResultLayer` → `QueryResultAdded` → `MapView.PostQueryResultAdded` → JS `coordUpsert`.
6. **Monitor check:** `MonitorScheduler` → `MonitorCheckService` (`SqlServerWatermarkReader`, watermark, proximity) → `XlsxReportWriter` → alerts (`AlertNotifier`).
7. **Map open:** `MapViewModel.OpenPathAsync` → `MapSourceFactory` → `IMapSource`; `MapView` serves tiles through `https://tiles.local/{z}/{x}/{y}` (WebResourceRequested in C#).
8. **Map messages:** page → host: `ready`, `cursor`, `zoom`, `view`, `pointHover`, `pointSelect`, `toolAddPoint`, `toolDeletePoint`, `toolZoomResult`, `toolClearResult`. Host → page: `load`, `layer`, `fit`, `coordUpsert`, `coordVisible`, `coordRemove`, `coordFit`, `coordRename`, `coordSelectClear`, `toolsInit`, `toolResults`.
9. **Pop-out map:** `ToggleWindow` → `PopOutRequested` → `MapView.OpenDetachedWindow` → one WebView2 at a time (`WebGate`); disposal is queued (never inside its own event); `MapWindow` waits for `Disposed` before it closes.

## 8. Data on disk
| Path | Content | Secret? |
|---|---|---|
| `%AppData%\FastDbExplorer\connections.json` | profiles (name, server, auth mode, user, flags) | no password |
| `%AppData%\FastDbExplorer\credentials.json` | Base64 DPAPI blobs keyed by profile ID | yes (encrypted, current user only) |
| `%AppData%\FastDbExplorer\coordinate-layers\{id}.json` | one Excel/manual layer each | no |
| `%AppData%\FastDbExplorer\monitors.json` (JsonMonitorStore) | monitor definitions and watermark state | no password |
| `%LocalAppData%\FastDbExplorer\logs\crash.log` | unhandled exceptions, WebView2 process failures | may contain messages |
| `%LocalAppData%\FastDbExplorer\WebView2\` | WebView2 profile | cache |

## 9. Lessons learned (bugs already hit; do not repeat)
**Inherited (L1–L9):** a View without `InitializeComponent()` renders empty; `FastDbExplorer.Application` shadows `Application` in WPF code; `localhost` vs `127.0.0.1` needs TCP/IP; guard false positives on `[Update]` identifiers; the AI sandbox has no brace expansion; ComboBox/DataGrid need dark templates; WebView2 is a native window (nothing WPF can overlay); no implicit Style in `Border.Resources`; check XAML resource keys and `Strings.X` before pushing.

**New in this phase (N1–N10):**
- **N1 Two-way mirror properties loop.** Two `RadioButton`s bound TwoWay to two properties that notify each other caused an endless loop and `StackOverflowException` (process dies; cannot be caught). Rule: one source of truth. Derived properties are read-only (or ignore writes). Never call a setter from a notification handler of the same pair.
- **N2 ComboBox gives `null`.** `SelectedItem` can become null when the item list changes. Rule: a setter that must have a value reverts `null` to a default; getters use `?.`.
- **N3 Never dispose WebView2 inside the event that asked for it** (click, Closing, Unloaded). Queue the disposal; expose a `Disposed` task; close the window only after it.
- **N4 Persian text with numbers.** Wrap each number in an isolated LTR run so "32.22 کیلومتر" reads correctly in RTL.
- **N5 Missing `using` directives** broke the build three times. Before pushing a new file, check every namespace it uses.
- **N6 Generator errors** (`CS0759`, `CS0103` for `[ObservableProperty]` members) mean the build failed earlier; delete `bin` and `obj` and rebuild, do not edit the code.
- **N7 Stale copies.** Downloaded ZIP folders are not git repositories: `git pull` fails and the old code runs. Always verify the commit (`git log --oneline -1`) before testing.
- **N8 Disk full** (`C:` full) makes Visual Studio fail with copy errors. Free space before debugging.
- **N9 Blank pages after returning to the map** came from rebuilding the WebView2 each visit. The map view is created once and only hidden/shown.
- **N10 Untested view models.** Every failure above appeared only in the UI, because no test covers a ViewModel. Add ViewModel tests before refactoring them.

## 10. Decision log (continues `PROJECT_MASTER_CONTEXT.md` §7)
| # | Decision | Why |
|---|---|---|
| D1–D27 | see `PROJECT_MASTER_CONTEXT.md` §7 | — |
| D28 | Saved SQL password: opt-in per profile, DPAPI CurrentUser, supersedes "no passwords on disk" for that one case | owner wants no repeated prompts; encrypted and never shown |
| D29 | No password popup; the password is typed in the profile form, masked after save | owner: one form, no separate windows |
| D30 | Export: `.xlsx` up to ~2 M cells, else split CSV (500 k rows / 100 MB per part), never overwrite, cleanup on cancel | big workbooks are slow and heavy |
| D31 | Monitoring replaces the old "Pāyesh" name everywhere in the UI | owner request |
| D32 | One shared `MapSendPanel` for Explorer and Monitoring | owner wants identical behaviour |
| D33 | Map view created once, hidden/shown; pop-out uses one WebView2 at a time | returning to the map was taking seconds |
| D34 | Result sets from Explorer/Monitoring are independent, removable, not persisted | owner: results must not disappear automatically |
| D35 | Marker styles: Excel = circle, my points = pin, Explorer = amber diamond + magnifier, Monitoring = rose square + bell | owner: sources must be distinguishable at a glance |
| D36 | Excel layer and monitor layers share the coordinate pipeline; proximity is computed in C# (haversine) | one geometry rule |
| D37 | Map tab state: the Excel tab is the single source of truth (N1) | fixes the crash loop |

## 11. Working agreement (vibe coding, for AI and humans)
1. **Before coding:** read this file and `CLAUDE.md`; restate scope in three lines; ask at most five blocking questions; get approval for any new dependency or any change bigger than one vertical slice.
2. **Slice order:** Domain → Application → Infrastructure → ViewModel → View → Tests. No premature abstractions.
3. **One approved slice = one commit** on the branch the owner tests. Never create a new branch unless the owner asks.
4. **Before pushing:** list every new file's `using`s; check XAML resource keys and `Strings.X`; check every `x:Name` used in code-behind exists; check that code-behind calls `InitializeComponent()`.
5. **Checklist for a new View:** code-behind present; brushes are `DynamicResource`; light and dark checked; RTL checked; loading, empty and error states; keyboard path (Enter, Esc, F5 where relevant).
6. **Checklist for a new ViewModel:** no two-way mirror properties (N1); null-safe setters (N2); commands for actions; `CancellationToken` on I/O; at least one unit test for the logic.
7. **Checklist for a new query feature:** typed parameters; identifiers validated; guard passes; paging; cancel; timeout; test added.
8. **Report after each slice:** changed files · reason · tests run · build result · what the human must test · risks.
9. **Never claim "done" without verification.** Label unverified work clearly. The AI cannot build on Windows.

## 12. Build, test, publish
- Get a fresh copy: `git clone -b <branch> https://github.com/AliDevCodes/dbexplorer.git` (ZIP copies are not repositories).
- Map assets once: `setup-map-assets.cmd` (needs internet).
- Build: `dotnet build FastDbExplorer.slnx` · Tests: `dotnet test` · Run: `dotnet run --project src/FastDbExplorer.Wpf`.
- Release: `publish.cmd` (tests → self-contained win-x64 → ZIP in `publish\`). Stop the running app first.
- Visual Studio: Debug configuration; for a stack overflow, copy the first 40 lines of the Call Stack.

## 13. Known issues and risks
- Most UI code of the last phases was never compiled by the AI; the owner tests each slice.
- ViewModels and Views have no automated tests (N10).
- `MapViewModel` (~560 lines across 5 partial files) and `TableQueryViewModel` (~600 lines) are large; their logic is hard to reason about and to test.
- Presentation data (colours, SVG paths, Jalali formatting) sits in Domain.
- Domain has a `System.Text.Json` attribute (`[JsonIgnore]` in `MonitoringModels.cs`, `Models.cs`).
- `XlsxReportWriter` implements two interfaces (`IReportWriter`, `IQueryReportWriter`); the composition root casts between them.
- Floating NuGet versions; Pkcs warnings (see §3).
- CSV from Excel: comma separator; Excel with a regional list separator may show one column.
- Monitoring and Explorer share one result style (rose vs amber now differs by source; query-result chips still share one code path).
- Dates: Explorer shows Persian dates; Monitoring preview shows ISO dates. Not yet unified (owner decision pending).
- Deep OFFSET paging on tables without a usable PK gets slower with depth.
- Huge text cells are read fully before truncation.
- OSM PBF cannot be rendered directly (needs the converter).

## 14. Roadmap (short)
1. **Now:** stabilise (owner tests current slices; fix only what fails).
2. **Safety net:** ViewModel unit tests for tabs, filter rows, map send, monitor editor, connection flows.
3. **Refactor (approval needed per phase):** see `docs/REVIEW_2026-10.md` §4.
4. **Features:** OSM PBF converter; owner-defined map tools; date format decision; packaging/versioning.

## 15. Change log of this document
- 2026-10-25: rewritten from the code on `feature/geosent-live-map` (replaces the 2026-10-08 master context for day-to-day work; the master context remains as history).
