# AI Handoff

## Current Goal
Keep FastDbExplorer stable and safely changeable, following the senior-engineer standards. Current focus: owner test of the latest slices, then slice 2 (map-tab rule to Domain).

## Repository State
Branch: feature/geosent-live-map
Commit: head of the branch (see `.ai/current-state.md`).
Git status: documentation and the first code slice are pushed; the three legacy files are being deleted in this same series.

## Completed
- Standards read in full (architecture-intelligence, engineering, git, ui-ux, ai) and applied: see `docs/architecture/standards-audit.md`.
- Legacy content migrated: decision log (D1-D38), lessons (L1-L9, N1-N14), map formats and page contract, runtime flows, ADR-0006 and ADR-0007.
- First refactor slice: filter operator rules in Domain (`FilterOperatorRules`) with tests.
- Two duplicate test files found and reverted before they reached the owner's build.

## In Progress
- Owner test of the latest slices.
- Deletion of the legacy files and their references (done in this series).

## Remaining
See `.ai/current-state.md` → Remaining (slices 2 to 4, then R2, R5, R7, security and logs).

## Changed Files (this phase)
- Code: `Domain/FilterOperatorRules.cs`, `Wpf/ViewModels/FilterRowViewModel.cs`, `tests/FilterOperatorRulesTests.cs`.
- Docs: `.ai/*`, `CLAUDE.md`, `docs/PROJECT_BRAIN.md`, `docs/architecture/*`, `docs/features/index.md`, `docs/changes/change-map.md`, `docs/decisions/*`, `docs/reference/*`, `docs/junior-guide.md`.
- Removed: `docs/ARCHITECTURE.md`, `docs/CODE_MAP.md`, `docs/PROJECT_MASTER_CONTEXT.md` (content migrated; see the decision log and `docs/reference/`).

## Validation
- Owner: 204/204 at the last full run (before the latest slices).
- AI: static review only.

## Known Issues
See `.ai/current-state.md` and `docs/architecture/standards-audit.md`.

## Decisions
See `docs/reference/decision-log.md` (D1 to D38) and `docs/decisions/` (ADR-0001 to ADR-0007).

## Assumptions
- Module and feature inventory matches the branch (verify with `git ls-files`, see `docs/junior-guide.md`).
- `FilterOperator` names used in the new Domain file match `Domain/QueryModels.cs` (checked at the time of writing).

## Next Action
1. Owner builds and runs `dotnet test`; reports results.
2. If green: approve slice 2 (map-tab rule to Domain).
3. Owner decides D38 (test target) when slice 3 starts.
