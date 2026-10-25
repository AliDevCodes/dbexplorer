# Project Context

## Identity
Name: FastDbExplorer
Purpose: read-only Windows desktop explorer for very large SQL Server databases, with map viewing (MBTiles, GMDB), coordinate layers, map tools, and Monitoring (watch a table for new rows and write reports).
Users: one operator on Windows (the owner). UI is Persian, right-to-left, automatic light and dark.
Core capabilities: connect (Windows or SQL auth) and manage named profiles; browse tables, filter (13 operators, AND/OR), page; export to .xlsx or split CSV; open map files, Excel and manual coordinate layers, measure, go to coordinates; monitor tables for new rows, proximity to a layer, alerts, reports.

## Technology
C# (.NET 10), XAML, JavaScript (map page), PowerShell and cmd (scripts).
WPF, CommunityToolkit.Mvvm 8.*, Microsoft.Extensions.Hosting 10.* (DI only), Microsoft.Web.WebView2, MapLibre GL JS.
SQL Server (read-only, monitored database). SQLite (MBTiles and GMDB, read-only). JSON files for local state.
No server. Runs on the owner's Windows PC. `publish.cmd` builds a self-contained win-x64 ZIP.

## Architecture
Canonical: `docs/architecture/` (system, modules, dependencies, runtime flows) and `docs/decisions/` (ADRs).

## Engineering rules
Canonical: `CLAUDE.md` (hard rules), `docs/reference/lessons-learned.md`, `docs/architecture/standards-audit.md`.
Operating model: senior-engineer-skill v2.0.0 (uploaded package): architecture-intelligence, engineering, git, ui-ux, ai.

## Important constraints
- Read-only: never write data or change database settings (ADR-0002).
- Server-side filtering and paging; no SELECT *, no DataTable, no COUNT(*) on big tables.
- Typed parameters only; identifiers validated and bracket-quoted.
- No plaintext passwords on disk (ADR-0003).
- Texts in Localization; every I/O method takes a CancellationToken.
- The AI sandbox cannot build or run .NET; every AI change is unverified until the owner builds and tests on Windows.
- Floating NuGet versions until pinned (standards audit, R7).

## How to validate
- Build: `dotnet build FastDbExplorer.slnx` (Windows, .NET 10 SDK).
- Tests: `dotnet test` (last full owner run: 204 passed, before the latest slices; new tests since then are not yet run).
- Static checks the AI can run: XML well-formedness, XAML resource keys, `Strings.X` and `x:Name`, code-behind presence, `using` directives of new files.
- Integration: manual, by the owner, with a real SQL Server and real map files (checklists in `docs/features/index.md`).
- Release: `publish.cmd` (tests, self-contained publish, ZIP). Stop the running app first.
