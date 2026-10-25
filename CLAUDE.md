# CLAUDE.md — AI entry point for FastDbExplorer

Read in this order:
1. `.ai/current-state.md` and `.ai/handoff.md` — where the work stands and what to do next.
2. `docs/junior-guide.md` — how the project is organised.
3. `docs/architecture/dependency-map.md` and `docs/changes/change-map.md` — where a change may go.
4. `docs/features/index.md` — the feature you are touching.
5. `docs/decisions/` — only the ADRs that touch your change.
6. `docs/REVIEW_2026-10.md` — refactor plan (do not start a phase without approval).

Legacy (history only): `docs/PROJECT_MASTER_CONTEXT.md`, `docs/ARCHITECTURE.md`, `docs/CODE_MAP.md`.

## Hard rules
- Read-only SQL only; typed parameters only; server-side filtering and paging; no `SELECT *`; no `COUNT(*)` on big tables (ADR-0002).
- No plaintext passwords on disk; SQL password only encrypted with DPAPI, only when opted in (ADR-0003).
- Texts in `Localization/*.cs`; RTL aware; numbers isolated LTR in Persian sentences.
- No two-way bindings on derived state; null-safe setters (ADR-0005).
- Never dispose WebView2 inside the event that asked for it (ADR-0004).
- Every I/O method takes a `CancellationToken`.
- The map page must not fetch from the network at run time.

## Working agreement
- Restate the scope in three lines; ask at most five blocking questions; get approval for any new dependency or any change bigger than one slice.
- One approved slice = one commit on the branch the owner tests. No new branch unless asked.
- Before pushing: check `using`s, XAML resource keys, `Strings.X`, `x:Name`, `InitializeComponent()`.
- Report after each slice: changed files, reason, tests run, build result, what the owner must test, risks.
- The AI cannot build on Windows. Label unverified work. Never claim "done" without verification; update `.ai/current-state.md` and `.ai/handoff.md` in the same commit.

## Commands
- Build: `dotnet build FastDbExplorer.slnx` · Tests: `dotnet test` · Run: `dotnet run --project src/FastDbExplorer.Wpf` · Release: `publish.cmd`.
- Map assets (once): `setup-map-assets.cmd`.
