# System Map

## System
Name: FastDbExplorer
Type: Windows desktop application (WPF, .NET 10), single user, local.
Primary technologies: C#, WPF, MVVM (CommunityToolkit.Mvvm), SQL Server client, SQLite, WebView2 + MapLibre (JavaScript), ClosedXML-free OpenXML writing (custom), DPAPI.
Deployment model: `publish.cmd` produces a self-contained win-x64 folder/ZIP. No server, no installer. Optional: `setup-map-assets.cmd` downloads MapLibre and fonts once.

## Entry Points
- Desktop: `src/FastDbExplorer.Wpf/App.xaml.cs` (`OnStartup`: splash, DI, theme, main window).
- Main window: `src/FastDbExplorer.Wpf/MainWindow.xaml(.cs)` (navigation rail, top bar, persistent map host).
- Map page: `src/FastDbExplorer.Wpf/Assets/Map/map.html` loaded at `https://app.local/map.html`.
- Web/API: none.
- CLI: none (build scripts only).
- Workers: background monitor scheduling inside the app (`MonitorScheduler`), no separate worker process.

## Major Modules
- Domain: models and rules. Location: `src/FastDbExplorer.Domain`.
- Application: contracts. Location: `src/FastDbExplorer.Application`.
- Infrastructure: I/O adapters. Location: `src/FastDbExplorer.Infrastructure`.
- Wpf: UI and composition root. Location: `src/FastDbExplorer.Wpf`.
- Tests. Location: `tests/FastDbExplorer.Tests`.

## High-Level Flow
1. Start: splash, DI registration, theme, last map reopened in the background, last connection profiles loaded.
2. Connect: profile chosen, SQL password from memory or DPAPI vault, databases listed, a new session (Explorer and Monitoring) is created.
3. Browse and query: table selected, columns and filters built, typed parameters, server-side paging, results in a grid.
4. Export: all matching rows streamed to .xlsx or split CSV.
5. Map: file opened by extension; tiles served to WebView2 through request interception; JSON messages between C# and the page for layers, hover, selection and tools.
6. Monitoring: scheduled check reads a watermark, finds new rows, applies conditions and proximity to a layer, writes a report, raises an alert.

## External Systems
- SQL Server: purpose: the database to explore (read-only). Integration: `Infrastructure/SqlServerMetadataService`, `SqlConnectionStringFactory`, `SqlServerWatermarkReader`.
- Local map files: MBTiles and GMDB (SQLite, read-only), OSM PBF (not renderable yet). Integration: `Infrastructure/Maps/*`.
- Excel files: coordinate import (read) and report export (write). Integration: `ExcelCoordinateImportService`, `XlsxReportWriter`.
- Local file system: JSON stores in `%AppData%` and `%LocalAppData%`. Integration: `Json*Store` classes.
- Windows DPAPI: encrypt SQL passwords per user. Integration: `DpapiCredentialVault`.
- Windows registry (theme setting): integration: `Services/ThemeService`.
- WebView2 runtime: renders the map page. Integration: `Views/MapView.xaml.cs`.
- MapLibre GL JS and fonts: bundled locally by `setup-map-assets.cmd`. Integration: `Assets/Map`.
- Internet: not used at run time (tiles and fonts are local).

## Key Constraints
- Read-only against every database and every map file.
- No plaintext passwords on disk.
- The map page must not fetch from the network at run time.
- WebView2 is a native window: WPF cannot draw over it.
- Only one WebView2 is alive at a time (see ADR-0004).

Update this map when architecture materially changes (new external system, new process, new entry point).
