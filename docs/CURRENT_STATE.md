# Current state
_Last updated: 2026-10-05 · Branch: `feature/excel-coordinate-layers-phase1` · Phase: Excel Coordinate Layer Import, **Phase 1 (foundation)**_

## Verification status (read this first)
Nothing in this branch has been **compiled or run** by the AI (its sandbox has no .NET). Treat all of it as *unverified* until the owner runs `dotnet build FastDbExplorer.slnx` and `dotnet test` on Windows and reports the result. The most likely build risks are listed under *Known risks*.

## What exists after Phase 1
A headless foundation for importing coordinate layers from Excel. There is **no UI, no map rendering and no startup loading yet**.

| Piece | Where |
|---|---|
| Models `MapPoint`, `MapLayer`, `CoordinateRules`, `ImportIssue(Code)`, `CoordinateImportResult` | `src/FastDbExplorer.Domain/CoordinateLayers.cs` |
| Interfaces `IExcelImportService`, `ICoordinateLayerStore` | `src/FastDbExplorer.Application/Abstractions/CoordinateLayerAbstractions.cs` |
| Excel import (`ExcelCoordinateImportService`, ExcelDataReader) | `src/FastDbExplorer.Infrastructure/CoordinateLayers/` |
| JSON persistence (`JsonCoordinateLayerStore`) | `src/FastDbExplorer.Infrastructure/CoordinateLayers/` |
| DI registration (2 lines, nothing resolves them yet) | `src/FastDbExplorer.Wpf/App.xaml.cs` |
| Tests (import, models, store) | `tests/FastDbExplorer.Tests/CoordinateLayerTests.cs` |

### Import rules
- Extensions: `.xlsx` and `.xls` only (anything else → `UnsupportedFileType`). The actual format is detected from the file content.
- Reads the **first worksheet**. The first non-blank row is the header and must contain `Name`, `Latitude`, `Longitude` (case-insensitive, spaces ignored, any order, extra columns ignored). No column auto-detection.
- Coordinates: WGS84 (EPSG:4326) decimal degrees. Latitude −90..90, longitude −180..180, finite. Numeric cells, or text cells (Persian/Arabic digits, `٫` and a single decimal comma are accepted). Degrees/minutes/seconds text is rejected (`InvalidNumber`).
- A bad row is **skipped and reported**; the other rows are still imported. A file-level problem (missing/duplicate column, empty sheet, no valid row, unreadable/corrupt/password-protected file) gives **no layer**. At most 1,000 issues are returned (`IssuesTruncated`).
- Blank rows are ignored. Rows without a name are invalid.

### Model rules
- `RadiusMeters` is always metres: 0 ≤ r ≤ 20,037,508, finite. Default 0 (= no radius).
- A `MapLayer` cannot be built with an invalid point, a point of another layer, an empty name/file name or an invalid radius.
- `Visibility` (bool) and `RadiusMeters` are mutable; call `ICoordinateLayerStore.SaveAsync` after changing them.

### Persistence
`%AppData%\FastDbExplorer\coordinate-layers\{layerId}.json`, one file per layer, format version 1, written via temp file + move. Damaged / newer-version / rule-breaking files are skipped and left on disk. No passwords or database data are involved; the SQL read-only rules are untouched.

## Not changed
SQL Server explorer (MVP-01/02), the map viewer (MBTiles/GMDB, WebView2, MapLibre), themes, localisation, all existing views and view models.

## Known risks / limitations
1. **Not compiled.** Possible trouble spots: the `ExcelDataReader` API names (`ExcelDataReader.Exceptions.InvalidPasswordException`), the hand-built `.xlsx` used by the tests (`XlsxBuilder`), JSON binding of the private on-disk records.
2. **New dependency `ExcelDataReader 3.*`** (floating, like the other packages — pin it after the first restore). Needs the owner's approval per Master Context §12 (decision D20 in `DECISIONS.md`).
3. **Real `.xls` (BIFF) is not covered by an automated test**: the sandbox cannot create one. Test it manually with a real Excel-saved `.xls`.
4. Row numbers in issues are counted by the reader (header = first non-blank row). If a sheet starts with blank rows, numbers may differ from Excel's row labels.
5. Only the first worksheet is read. `.xlsm`/`.xlsb` are rejected.
6. The whole layer file is rewritten on every save (fine for tens of thousands of points; revisit for very large layers).
7. A skipped (damaged) layer file is silent for now; Phase 2 should surface it.

## Next
Phase 2 backlog is in `CHANGELOG.md` ("Remaining for Phase 2").
