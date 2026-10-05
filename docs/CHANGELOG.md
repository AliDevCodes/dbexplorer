# Changelog
Format: newest first. Dates are ISO. Everything below is unverified by a build until the owner confirms (see `CURRENT_STATE.md`).

## [Unreleased] — 2026-10-05 — Excel Coordinate Layer Import, Phase 1 (branch `feature/excel-coordinate-layers-phase1`)
### Added
- `MapPoint`, `MapLayer`, `CoordinateRules`, `ImportIssue` / `ImportIssueCode`, `CoordinateImportResult` (Domain).
- `IExcelImportService`, `ICoordinateLayerStore` (Application).
- `ExcelCoordinateImportService`: `.xlsx`/`.xls`, required columns Name/Latitude/Longitude, WGS84 validation, row-level and file-level errors with codes, streaming read, cancellation.
- `JsonCoordinateLayerStore`: one JSON file per layer, atomic write, tolerant load.
- Tests: `CoordinateLayerTests.cs` (import, model rules, persistence) with a built-in minimal `.xlsx` writer.
- Docs: `CURRENT_STATE.md`, `FEATURES.md`, `CHANGELOG.md`; notes in `CODE_MAP.md` and `DECISIONS.md` (D20–D26).

### Changed
- `FastDbExplorer.Infrastructure.csproj`: new package `ExcelDataReader 3.*`.
- `App.xaml.cs`: registers the two new services in DI (no behaviour change).

### Not included (by design)
Map rendering, UI panel, column detection, DDM/DMS conversion, database spatial queries.

### Remaining for Phase 2
1. `IFileDialogService.PickExcelFile` + a view model that calls `IExcelImportService`, shows the result, and saves with `ICoordinateLayerStore`.
2. Persian texts for every `ImportIssueCode` in `Strings.cs` (same pattern as `Strings.Describe` for database errors); show row/column.
3. Load saved layers at startup (`LoadAllAsync`) and expose them as an observable collection; also surface skipped/damaged layer files.
4. Layers panel: visibility toggle, radius input (convert user units to metres at the edge), rename, delete; call `SaveAsync` on change.
5. Map rendering: send layers to the WebView2 page (`layer`-style host↔page messages), draw points and metre-accurate radius circles (geodesic polygon or zoom-scaled circles), fit-to-layer. Remember WebView2 airspace (panels beside the map only).
6. Performance plan for large layers (clustering / GeoJSON size), and a cheaper save for visibility/radius changes.
7. Decide open questions: multiple worksheets, `.xlsm`, rows without a name, accurate Excel row labels.
8. Pin the `ExcelDataReader` version after the first restore; add a manual-test checklist for a real `.xls`.

## Earlier work
See `PROGRESS.md` and `PROJECT_MASTER_CONTEXT.md` §10 (MVP-01, MVP-02, Map slices 1–2).
