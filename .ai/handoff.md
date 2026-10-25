# AI Handoff

## Current Goal
Keep FastDbExplorer stable and changeable, following the senior-engineer standards. Domain first: business rules out of view models, one slice at a time; documents updated with every change.

## Repository State
Branch: feature/geosent-live-map. Commit: head of the branch (see `.ai/current-state.md`).

## Completed (verified by the owner)
- Filter operator rules in Domain (slice 1), build and tests passed.
- Documentation reorganised into folders; index and update procedure in place.

## Completed (not compiled yet)
- Slice 2: query filter rows to conditions in Domain (`FilterRequestBuilder`), used by `TableQueryViewModel.TryBuildRequest`; tests added.

## Remaining
See `.ai/current-state.md` → Remaining.

## Changed Files (slice 2)
- Code: `src/FastDbExplorer.Domain/FilterRequestBuilder.cs` (new), `src/FastDbExplorer.Wpf/ViewModels/TableQueryViewModel.cs` (filter section only), `tests/FastDbExplorer.Tests/FilterRequestBuilderTests.cs` (new).
- Docs: `docs/changes/change-map.md`, `.ai/current-state.md`, `.ai/handoff.md`.

## Validation
- Owner: build and tests passed for slice 1; slice 2 awaits the owner's build.
- AI: static review only.

## Known Issues
See `.ai/current-state.md` and `docs/architecture/standards-audit.md`.

## Decisions
See `docs/reference/decision-log.md` (D1 to D38) and `docs/decisions/` (ADR-0001 to ADR-0007).

## Assumptions
- The `TableQueryViewModel` copy matches the branch except the filter section (checked by the diff of the pushed file against the read version; the owner's build confirms).
- `FilterInput` and `FilterRequestBuilder` names do not clash with existing types (search done before writing).

## Next Action
1. Owner builds and runs tests; reports the count and failures.
2. If green: approve slice 3 (paging controller and map-send coordinator, with Domain/Application tests).
3. Owner decides D38 before any view-model test.
