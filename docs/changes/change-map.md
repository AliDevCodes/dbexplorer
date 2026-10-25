# Change Map

Purpose: route a change to the smallest relevant scope. Read this before editing.

## Feature / Area: Explorer query
### If changing business rules (filter meaning, order, paging rule)
Location: `Infrastructure/SelectQueryBuilder.cs`, `Application` filter records, `TableQueryViewModel.TryBuildRequest`.
Tests: `QueryBuilderTests`; add a ViewModel test when R1 exists.
Docs: `docs/features/index.md`, master context §5 (paging).

### If changing API (contracts between layers)
Location: `src/FastDbExplorer.Application/Abstractions/*.cs`.
Contract: all implementations in `Infrastructure` must change in the same commit.
Tests: `InfrastructureTests`.
Docs: `docs/architecture/dependency-map.md` (contracts list).

### If changing persistence (JSON files, profiles, layers, monitors)
Location: the relevant `Json*Store` in `Infrastructure`.
Compatibility: old files must still load (optional fields; unknown values dropped, never fatal). Note the change in the file's header comment and in `docs/features/index.md`.
Tests: round-trip tests in `InfrastructureTests`, `MapMarkerCatalogTests`.

### If changing UI (view, layout, texts)
Location: `Views/*.xaml` and code-behind; texts in `Localization/*.cs`; colours only through `Themes/*`.
States: loading, empty, error, success. Check light and dark and RTL.
Tests: none automated; owner checklist in `docs/features/index.md`.
Visual checks: owner screenshot in the change report.

### Must Not Change (protected boundaries)
- Read-only guard and its call sites.
- Parameter-only value handling.
- DPAPI scope (CurrentUser) and the rule that passwords never go to JSON.
- Single WebView2 at a time (`WebGate`) and deferred disposal (ADR-0004).
- The map page's network rule (no fetch at run time).

## Feature / Area: Explorer export
### If changing the format rule (when xlsx vs csv)
Location: `Application/Abstractions/ExportAbstractions.cs` (`ExportLimits`), `Infrastructure/Monitoring/XlsxReportWriter.cs`, `CsvPartsWriter.cs`.
Tests: `QueryExportTests` (update the expected numbers with the limits).
Docs: `docs/EXPORT_AND_LAYOUT.md`.

### If changing the CSV delimiter or encoding
Location: `CsvPartsWriter.cs` only. Decision pending (see `.ai/current-state.md`).
Tests: `QueryExportTests` (BOM and quoting test).

## Feature / Area: Map
### If adding a new file format
Location: `Infrastructure/Maps/MapSourceFactory.cs` (detection), a new `IMapSource` implementation, `Domain/MapModels.cs` for kinds and errors.
Tests: `MapSourceTests` with a small sample file.
Docs: `docs/MAP_UI.md`, master context §9 (format table).

### If adding a new message between C# and the page
Location: C# side in `Views/MapView.xaml.cs` (`OnMessage`) or `Views/MapView.Tools.cs`; page side in `Assets/Map/*.js`; host push in `MapView.Post*`.
Rule: name the message in both places; add a `default` branch or a log line for unknown messages; document the message in `docs/features/index.md` and `docs/dependency-map` (cross-module contracts).
Tests: none automated (owner checklist). Static check: every `type` sent from JS has a case in C#.

### If changing map tools (go-to, pick, measure, my points)
Location: `Assets/Map/map-tools.js` (behaviour), `MapViewModel.Tools.cs` (saving "my points"), `MapToolStrings.cs` (texts).
Docs: `docs/MAP_TOOLS.md`.

### If changing the pop-out window or WebView2 lifecycle
Location: `Views/MapView.xaml.cs` (`ReleaseWebView`, `OpenDetachedWindow`), `Views/MapWindow.xaml.cs`, `MainWindow.xaml.cs` (`SyncMapHost`).
Rules: never dispose inside the event that asked for it; one WebView2 at a time; the Map section copy shows the note while the window is open.
Docs: ADR-0004, `docs/MAP_UI.md`.
Tests: owner checklist (close with dock button; click Map in the rail while the window is open).

### Must Not Change
- Coordinate rules in `CoordinateRules` (WGS84, metres for radius).
- Map page must not fetch from the internet.

## Feature / Area: Monitoring
### If changing the check rule (new rows, watermark)
Location: `Infrastructure/Monitoring/WatermarkComparer.cs`, `SqlServerWatermarkReader.cs`, `MonitorCheckService.cs`.
Tests: add unit tests in `Tests` for the comparer before changing it.
Docs: `docs/MONITORING.md`.

### If changing a monitor's fields (new setting)
Location: `Domain/MonitoringModels.cs` (`MonitorDefinition`), `JsonMonitorStore` (optional field with default), `MonitorEditorViewModel` and `Views/MonitorEditorView.*`, `MonitoringStrings.cs`.
Compatibility: old monitor files must load with the default.

### If changing proximity or alerts
Location: `MonitorProximity` (Domain), `MonitorCheckService`, `AlertNotifier`.
Tests: distance tests (add them first).

## Feature / Area: Connections and passwords
### If changing the password flow
Location: `ConnectionViewModel` (state), `Views/ConnectionsView.xaml` (fields), `DpapiCredentialVault`.
Rules: no password in JSON; vault write before profile save; failed login clears the stored password; server/user change clears it.
Tests: `CredentialVaultTests`; add ViewModel tests in R1.
Docs: `docs/CREDENTIALS.md`, ADR-0003.

## Feature / Area: Shared UI rules (any screen)
### If adding a ViewModel property used by two controls
Rule: one source of truth (ADR-0005). Derived state is read-only. No TwoWay on derived properties.

### If adding a ComboBox bound to a value that must not be null
Rule: null-safe setter that reverts to a default (ADR-0005, lesson N2).

## Before every commit (checklist)
1. Usings of new files checked.
2. XAML resource keys and `Strings.X` checked.
3. `x:Name` used in code-behind exists.
4. Docs listed in this map updated.
5. `.ai/change-manifest.md` and `.ai/current-state.md` updated.
