# Change Map

Purpose: route a change to the smallest relevant scope. Read this before editing.

## Explorer query
### If changing business rules (filter meaning, order, paging)
Location: `Domain/FilterOperatorRules.cs` (which operator needs which inputs), `Domain/FilterRequestBuilder.cs` (filter rows to conditions), `Domain/MapSendRules.cs` (`KeysetPaging`: when keyset paging starts), `Domain/QueryModels.cs` (`FilterOperator`, `ColumnInfo`), `Infrastructure/SelectQueryBuilder.cs` (SQL). `TableQueryViewModel` only converts screen rows and calls these rules.
Tests: `FilterOperatorRulesTests`, `FilterRequestBuilderTests`, `KeysetPagingTests`, `QueryBuilderTests`.
Docs: `features/explorer-export-and-layout.md`, `architecture/runtime-flows.md`.

### If changing how rows are sent to the map (coordinates, names, detail columns, limits)
Location: `Domain/MapSendRules.cs` (`TryCoordinate`, `TruncateText`, `BuildDetails`, limits). `TableQueryViewModel` reads cell values and calls these rules.
Tests: `MapSendRulesTests`.
Docs: `features/map-send-panel.md`, `features/map-tools.md`.

### If changing the API (contracts between layers)
Location: `src/FastDbExplorer.Application/Abstractions/*.cs`.
Rule: every implementation in Infrastructure changes in the same commit.
Tests: `InfrastructureTests`. Docs: `architecture/dependency-map.md`.

### If changing persistence (JSON files, profiles, layers, monitors)
Location: the relevant `Json*Store` in Infrastructure.
Compatibility: old files must still load (optional fields with defaults; unknown values dropped, never fatal).
Tests: round-trip tests (`JsonCoordinateLayerStoreTests`, `InfrastructureTests`).

### If changing UI (view, layout, texts)
Location: `Views/*.xaml` and code-behind; texts in `Localization/*.cs`; colours only through `Themes/*`.
States: loading, empty, error, success, disabled. Check light, dark and right-to-left.
Tests: none automated; owner checklist on the feature page.

## Explorer export
### If changing the format rule (xlsx or csv)
Location: `Application/Abstractions/ExportAbstractions.cs` (`ExportLimits`), `Infrastructure/Monitoring/XlsxReportWriter.cs`, `CsvPartsWriter.cs`.
Tests: `QueryExportTests` (update the expected numbers).
Docs: `features/explorer-export-and-layout.md`.

### If changing the CSV delimiter or encoding
Location: `CsvPartsWriter.cs` only. Decision pending (see `.ai/current-state.md`).

## Map
### If adding a file format
Location: `Infrastructure/Maps/MapSourceFactory.cs`, a new `IMapSource`, `Domain/MapModels.cs` for kinds and errors.
Tests: `MapSourceTests` with a small sample file. Docs: `reference/map-formats.md`.

### If adding a message between C# and the page
Location: C# in `Views/MapView.xaml.cs` (`OnMessage`) or `Views/MapView.Tools.cs`; page in `Assets/Map/*.js`.
Rule: name the message on both sides; log unknown messages; document it in `reference/map-formats.md` (page contract).

### If changing map tools (go to, pick, measure, my points)
Location: `Assets/Map/map-tools.js`, `MapViewModel.Tools.cs`, `MapToolStrings.cs`. Docs: `features/map-tools.md`.

### If changing geometry (radius circles, bounds, coordinates)
Location: `Domain/GeoMath.cs`, `Domain/CoordinateLayers.cs`.
Tests: `CoordinateRenderingTests`, `CoordinateLayerTests`. Decision: ADR-0006 (do not change the sphere radius or the segment count without a new ADR).
Docs: `features/coordinate-layers.md`.

### If changing the pop-out window or WebView2 lifecycle
Location: `Views/MapView.xaml.cs` (`ReleaseWebView`, `OpenDetachedWindow`), `Views/MapWindow.xaml.cs`, `MainWindow.xaml.cs` (`SyncMapHost`).
Rules: no disposal inside the event that asked for it; one WebView2 at a time; the Map section copy shows the note while the window is open. ADR-0004.
Tests: owner checklist (close with dock button; click Map in the rail while the window is open).

## Monitoring
### If changing the check rule (new rows, watermark)
Location: `Infrastructure/Monitoring/WatermarkComparer.cs`, `SqlServerWatermarkReader.cs`, `MonitorCheckService.cs`.
Tests: `MonitoringTests` (map the rule's coverage first).
Docs: `features/monitoring.md`.

### If changing a monitor field
Location: `Domain/MonitoringModels.cs` (`MonitorDefinition`), `JsonMonitorStore` (optional field with default), `MonitorEditorViewModel`, `Views/MonitorEditorView.*`, `MonitoringStrings.cs`.
Compatibility: old monitor files must load with the default.

## Connections and passwords
### If changing the password flow
Location: `ConnectionViewModel`, `Views/ConnectionsView.xaml`, `DpapiCredentialVault`.
Rules: no password in JSON; vault write before profile save; a failed login clears the stored password; a server or user change clears it.
Tests: `CredentialVaultTests`. Docs: `features/credentials.md`, ADR-0003.

## Shared UI rules (any screen)
- ViewModel property used by two controls: one source of truth (ADR-0005). Derived state is read-only.
- ComboBox that must not be null: null-safe setter that reverts to a default (lesson N2).
- A rule that a test should cover: move it to Domain first (ADR-0007).

## Must not change (protected boundaries)
- Read-only guard and its call sites.
- Parameter-only value handling.
- DPAPI scope (CurrentUser) and the rule that passwords never go to JSON.
- Single WebView2 at a time and deferred disposal (ADR-0004).
- The map page's network rule (no fetch at run time).
- Coordinate rules and the sphere radius of ADR-0006.

## Before every commit (checklist)
1. Usings of new files checked. 2. XAML resource keys and `Strings.X` checked. 3. `x:Name` used in code-behind exists. 4. Search before create done (lesson N11). 5. Docs in this map updated, following `doc-update-protocol.md`. 6. `.ai/change-manifest.md` and `.ai/current-state.md` updated. 7. Commit message says "not compiled" if the owner has not built it.
