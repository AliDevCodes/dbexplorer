# Junior Developer Navigation Guide

Start here. Then follow the numbered steps. You do not need to ask anyone how the project works.

## How to understand the project (in order)
1. Read `docs/architecture/system-map.md` (what the system is, how it starts, what it talks to).
2. Read `docs/architecture/module-map.md` (which folder owns what).
3. Read `docs/architecture/dependency-map.md` (which module may call which; what is forbidden).
4. Find your feature in `docs/features/index.md` (where it starts, where its logic and tests are).
5. Read the matching section of `docs/changes/change-map.md` (which files usually change).
6. Read `docs/decisions/` only for the decisions that touch your change (the README lists them).
7. Read `.ai/current-state.md` to see what is in progress and what the owner must test.

## Your first change: a checklist
1. Write the requirement in three lines: what should happen, what must not change, how you will know it works.
2. Find the smallest scope with `docs/changes/change-map.md`.
3. Read only that code and its tests.
4. Make the smallest change. Do not refactor unrelated code. If you find a problem elsewhere, write it down in `.ai/current-state.md` → Known Issues.
5. Build: `dotnet build FastDbExplorer.slnx`. Run: `dotnet test`.
6. Run the owner checklist for your feature (in `docs/features/index.md`) on Windows.
7. Update: the docs listed in the change map, `.ai/change-manifest.md`, `.ai/current-state.md`.
8. Commit one approved slice on the branch the owner tests. Do not create a new branch unless asked.

## Where does business logic live?
Per feature: `docs/features/index.md`, column "Business logic". In short: query rules in `SelectQueryBuilder`; monitoring rules in `Infrastructure/Monitoring`; map rules in `Infrastructure/Maps` and `Assets/Map`; UI rules in the ViewModels.

## Where are the tests?
`tests/FastDbExplorer.Tests`. ViewModels and Views have no automated tests yet (gap F5). Test the logic you can reach without UI.

## Words used in this project
- Watermark: the value (for example the highest ID or the latest time) that tells Monitoring which rows are new.
- Keyset paging: the next page starts after the last key, not with OFFSET.
- Result set: a group of rows sent from Explorer or Monitoring to the map.
- Layer: a set of map points from one Excel file, or the "my points" layer.
- WebView2: the embedded browser engine that draws the map.

## What should I avoid?
- changing unrelated modules
- bypassing the architecture boundaries in `dependency-map.md`
- adding dependencies without the owner's approval
- building SQL from user input (use typed parameters)
- two-way bindings on derived state (ADR-0005)
- disposing WebView2 inside the event that asked for it (ADR-0004)
- logging passwords or connection strings with passwords

## When you do not know
Write the assumption in `.ai/current-state.md` with the label `Assumption:` and `Needs verification:`, and ask the owner for the smallest piece of evidence you need (for example the output of `git log --oneline -5` or the exact error text).

## Repository discovery (copy the output to the owner or AI when asked)
Windows:
```
git status
git branch -a
git log --oneline --decorate -20
git remote -v
git diff --stat
git ls-files | findstr /R "\.cs$ \.xaml$ \.js$"
```
Then compare with `docs/architecture/module-map.md`.
