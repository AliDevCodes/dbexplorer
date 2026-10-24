# CLAUDE.md — AI entry point for FastDbExplorer

**Read in this order:**
1. `docs/PROJECT_BRAIN.md` — the project brain: layers, modules, feature status, flows, lessons, decisions, working agreement.
2. `docs/PROJECT_MASTER_CONTEXT.md` — original handoff and the SQL-side hard rules (R1–R5 are still valid).
3. `docs/REVIEW_2026-10.md` — latest architecture review and the refactor plan.

## Hard rules (short form)
- Read-only SQL only; typed parameters only; server-side filtering and paging; no `SELECT *`; no `COUNT(*)` on big tables.
- No plaintext passwords on disk. SQL passwords only encrypted (DPAPI) and only when the owner opted in.
- Texts in `Localization/*.cs`; RTL aware; numbers isolated LTR in Persian sentences.
- No two-way mirror properties between view-model members (lesson N1).
- Never dispose WebView2 inside the event that asked for it (N3).
- Every I/O method takes a `CancellationToken`.

## Working agreement (short form)
- Restate scope, ask at most five blocking questions, get approval for new dependencies and for anything bigger than one slice.
- One approved slice = one commit on the branch the owner tests. No new branch unless asked.
- Before pushing: check `using`s, XAML resource keys, `Strings.X`, `x:Name`, `InitializeComponent()`.
- Report after each slice: changed files, reason, tests run, build result, what the human must test, risks.
- The AI cannot build on Windows. Label unverified work. Never claim "done" without verification.

## Commands
- Build: `dotnet build FastDbExplorer.slnx` · Tests: `dotnet test` · Run: `dotnet run --project src/FastDbExplorer.Wpf` · Release: `publish.cmd`.
- Map assets (once): `setup-map-assets.cmd`.
