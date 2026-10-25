# CLAUDE.md — AI entry point for FastDbExplorer

Read in this order:
1. `.ai/current-state.md` and `.ai/handoff.md` — where the work stands, what to do next.
2. `docs/junior-guide.md` — how the project is organised.
3. `docs/architecture/dependency-map.md` and `docs/changes/change-map.md` — where a change may go.
4. `docs/features/index.md` — the feature you touch.
5. `docs/reference/` — decision log, lessons learned, map formats (read the part you need).
6. `docs/decisions/` — only the ADRs that touch your change.
7. `docs/architecture/standards-audit.md` — open findings to work on, one slice at a time.

## Hard rules
- Read-only SQL only; typed parameters only; server-side filtering and paging; no `SELECT *`; no `COUNT(*)` on big tables (ADR-0002).
- No plaintext passwords on disk; SQL password only encrypted (DPAPI) and only when opted in (ADR-0003).
- Business rules live in Domain, view models only adapt them (ADR-0007).
- Texts in `Localization/*.cs`; RTL aware; numbers isolated LTR in Persian sentences.
- No two-way bindings on derived state; null-safe setters (ADR-0005, lesson N2).
- Never dispose WebView2 inside the event that asked for it (ADR-0004).
- Every I/O method takes a `CancellationToken`.
- The map page must not fetch from the network at run time.

## Working agreement
- Restate the scope in three lines; ask at most five blocking questions; get approval for any new dependency or any change bigger than one slice.
- One approved slice = one commit on the branch the owner tests. No new branch unless asked.
- Search before create: list the folder and grep the names before adding a file, a test or a utility (lesson N11).
- Before pushing: check `using`s, XAML resource keys, `Strings.X`, `x:Name`, `InitializeComponent()`.
- Say "not compiled" in the commit message until the owner builds.
- Report after each slice: changed files, reason, tests run, build result, what the owner must test, risks.
- The AI cannot build on Windows. Label unverified work. Never claim "done" without verification; update `.ai/current-state.md` and `.ai/handoff.md` in the same commit.

## Commands
- Build: `dotnet build FastDbExplorer.slnx` · Tests: `dotnet test` · Run: `dotnet run --project src/FastDbExplorer.Wpf` · Release: `publish.cmd`.
- Map assets (once): `setup-map-assets.cmd`.
