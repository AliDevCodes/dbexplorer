# FastDbExplorer — AI entry point
**Read first: `docs/PROJECT_MASTER_CONTEXT.md`** (full handoff: rules, architecture, decisions, lessons, next feature).
Read-only WPF explorer for huge SQL Server databases. UI: Persian RTL, auto light/dark.
Commands: `dotnet build FastDbExplorer.slnx` · `dotnet test` · `publish.cmd` (offline portable build).
Hard rules: never run write SQL (`ReadOnlySqlGuard`); typed parameters only; no passwords on disk; no SELECT * / DataTable / COUNT(*) on big tables; every View needs a code-behind with InitializeComponent.
Current: MVP-01/02 done; map slice 2 (MBTiles, GMDB raster, labels) delivered but unverified. Map UX polish (branch `feature/map-ux-polish-last-map`) and Monitoring phases 1-3 (branch `feature/monitoring-alerts`, see `docs/MONITORING.md` for status + work plan) written but not compiled. Next: owner builds/tests monitoring, then PBF converter, owner map tools (Master Context §9).
Other docs: `docs/PROGRESS.md`, `CODE_MAP.md`, `DECISIONS.md`, `RUNBOOK.md`, `KNOWN_ISSUES.md`, `docs/MONITORING.md`.
