# Project Context

## Identity
Name: FastDbExplorer
Purpose: read-only Windows desktop explorer for very large SQL Server databases, with map viewing (MBTiles, GMDB), coordinate layers, map tools, and Monitoring (watch a table for new rows and write reports).
Users: a single operator/analyst on Windows (owner). UI is Persian, right-to-left, auto light/dark.
Core business capabilities:
- Connect to SQL Server (Windows or SQL auth), manage named connection profiles.
- Browse tables, select columns, filter (13 operators, AND/OR), page through results.
- Export query results to .xlsx, or to split CSV when large.
- Open map files, show Excel/manual coordinate layers, measure, go to coordinates.
- Monitor tables for new rows, match conditions, measure proximity to a layer, alert, write reports.

## Technology
Languages: C# (.NET 10), XAML, JavaScript (map pages), PowerShell/cmd (build scripts).
Frameworks: WPF, CommunityToolkit.Mvvm 8.*, Microsoft.Extensions.Hosting 10.* (DI only), Microsoft.Web.WebView2, MapLibre GL JS.
Databases: SQL Server (read-only, monitored database). SQLite (MBTiles and GMDB map files, read-only). JSON files for local app state.
Infrastructure: no server. Everything runs on the user's Windows PC. publish.cmd builds a self-contained win-x64 ZIP.

## Architecture
Canonical architecture documentation: `docs/architecture/system-map.md`, `module-map.md`, `dependency-map.md`.
Legacy (may be stale, read only for history): `docs/ARCHITECTURE.md`, `docs/CODE_MAP.md`, `docs/PROJECT_MASTER_CONTEXT.md`.

## Engineering Rules
Canonical rules: `CLAUDE.md` (short), `docs/PROJECT_BRAIN.md` (pointer), this folder (`.ai/`), and `docs/decisions/` (ADRs).
Skill source of the operating model: senior-engineer-skill v2.0.0 (uploaded package).

## Important Constraints
- Read-only: never write data or change database settings (R1, guard in Infrastructure).
- Server-side filtering and paging; never SELECT *, never DataTable, no COUNT(*) on big tables (R2, R5).
- User values are typed parameters only; identifiers are validated and bracket-quoted (R3).
- No plaintext passwords on disk. SQL password is opt-in and encrypted with DPAPI (CurrentUser) per profile (R4, ADR-0003).
- All user-facing text in Localization/*.cs (R6).
- Every I/O method takes a CancellationToken (R7).
- The AI sandbox cannot build or run .NET; every AI change is unverified until the owner builds and tests on Windows.
- Floating NuGet versions until pinned (see state in dependency-map).

## How to Validate
- Build: `dotnet build FastDbExplorer.slnx` (Windows, .NET 10 SDK).
- Tests: `dotnet test` (last full run: 204 passed, before the latest UI slices).
- Lint: none configured. Static checks the AI can run: XML well-formedness, XAML resource keys, `Strings.X` and `x:Name` usage, code-behind presence, `using` directives of new files.
- Integration: manual, by the owner, with a real SQL Server and real map files. Checklists are in `docs/features/index.md`.
- Release: `publish.cmd` (tests, self-contained publish, ZIP). Stop the running app first.
