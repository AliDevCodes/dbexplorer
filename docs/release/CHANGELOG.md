# Changelog

Format: newest first. Dates are ISO. Everything below is unverified by a build until the owner confirms (see `.ai/current-state.md`).
Moved from `docs/CHANGELOG.md` on 2026-10-25; the entries are unchanged.

## [Unreleased] — 2026-10-16 — Detachable map window and categorized marker icons (branch `feature/saved-connection-credentials`)
### Added
- "باز کردن در پنجره جدا" button on the map toolbar: the map opens in its own `MapWindow` (for a second monitor). It shares the same `MapViewModel` (layers, file, camera); the Map section shows a note with "بازگرداندن به برنامه" while it is detached. The WebView is collapsed in the section while detached (native window, no WPF layering).
- Marker icon library grouped in 11 folders: 56 icons, up from 24. Icons drawn in-house as SVG path data, as before.
- `MapMarkerCategory` in the domain catalog; `MapMarkerCategoryTests.cs` (every icon in exactly one folder, the 24 earlier keys still exist, at least 50 icons).

### Changed
- The layer's icon picker is one button that shows the current icon; the folders and their icons open in a popup.

### Not verified
Not compiled or run at the time: XAML bindings (`IsPoppedOut`, `ToggleWindowCommand`, `IconCategories`, `DisplayIconShape`) and the new icon paths needed an owner build and a visual check of the icon sheet.

## [Unreleased] — 2026-10-06 — Navigation shell and UI overhaul (branch `feature/nav-shell`)
### Added
- Persistent navigation rail (Home, Explorer, Map, Monitoring) with Ctrl+1..4; sections keep their state, no Back buttons.
- Shared top bar: connection status, active-database picker, disconnect, search button.
- Home dashboard when connected; connect form is the Home page when disconnected; Explorer and Monitoring show a "connect first" invitation.
- Unread-alert badge on the Monitoring item.
- Ctrl+K palette (`CommandPaletteWindow`): sections, tables of the active database, monitors, disconnect.
- `ShellStrings.cs`, `NavItemViewModel`, `HomeViewModel`, `ConnectPromptViewModel`, `CommandPaletteViewModel`, `HomeView`, `ConnectPromptView`.

### Changed
- `MainViewModel` is now the shell (sections instead of one swapped `CurrentView`); `MainWindow.xaml` hosts rail and top bar.
- Explorer sidebar 340 -> 300 px, tables only (database picker moved to the top bar).
- Connect screen: brand text describes the whole app; map entry card removed; tagline updated.
- Map and Monitoring "Back" buttons removed.

### Not included
Map layers in the Ctrl+K palette; Ctrl+1..4 and Ctrl+K do not fire while the WebView2 map has keyboard focus. See `features/navigation-and-shell.md`.

## [Unreleased] — 2026-10-06 — Start-up splash screen (branch `feature/monitoring-alerts`)
### Added
- `Views/SplashWindow.xaml(.cs)`: borderless, transparent, rounded window with a fixed dark design: radial glow, contour lines, three ripples, database logo, name and Persian tagline, progress bar with shimmer, status line, footer (read-only note and version).
- `Localization/SplashStrings.cs`: tagline, footer and the three step texts.

### Changed
- `App.xaml.cs`: `OnStartup` shows the splash first and reports the real steps (services and DI, host start, theme and main window). The main window is shown before the splash closes; the splash stays at least 1.2 s (`MinSplashTime`). A failure during start-up closes the splash, shows the message and shuts the app down. DI registrations and their order are unchanged.

### Not included (by design)
No images or fonts downloaded from the web; no per-theme variant; the bar steps are start-up stages, not a percentage of real work.

## [Unreleased] — 2026-10-05 — Excel Coordinate Layer Import, Phase 1 (branch `feature/excel-coordinate-layers-phase1`)
### Added
- `MapPoint`, `MapLayer`, `CoordinateRules`, `ImportIssue` / `ImportIssueCode`, `CoordinateImportResult` (Domain).
- `IExcelImportService`, `ICoordinateLayerStore` (Application).
- `ExcelCoordinateImportService`: `.xlsx` and `.xls`, required columns Name, Latitude, Longitude, WGS84 validation, row-level and file-level errors with codes, streaming read, cancellation.
- `JsonCoordinateLayerStore`: one JSON file per layer, atomic write, tolerant load.
- Tests: `CoordinateLayerTests.cs` (import, model rules, persistence) with a built-in minimal `.xlsx` writer.

### Changed
- `FastDbExplorer.Infrastructure.csproj`: new package `ExcelDataReader 3.*`.
- `App.xaml.cs`: registers the two new services in DI (no behaviour change).

### Not included (by design)
Map rendering, UI panel, column detection, DDM and DMS conversion, database spatial queries.

### Phase 2 (done later; see `features/coordinate-layers.md`)
Import command, saved layers at start-up, layers panel, map rendering with metre-accurate circles. Open items from this list are in `features/coordinate-layers.md` and `reference/known-issues.md`.

## Earlier work
MVP-01, MVP-02 and map slices 1–2 were recorded in `PROGRESS.md` and `PROJECT_MASTER_CONTEXT.md`. Both files were removed on 2026-10-25 after their content was migrated; their history is in git (commit `e04bdd0` and earlier).
