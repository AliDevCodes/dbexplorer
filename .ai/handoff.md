# AI Handoff

## Current Goal
Keep FastDbExplorer stable and changeable, following the senior-engineer standards. Business rules out of view models, one slice at a time; documents updated with every change.

## Repository State
Branch: feature/geosent-live-map. Commit: head of the branch (see `.ai/current-state.md`).

## Completed (verified by the owner)
- Slice 1: filter operator rules in Domain; build and tests passed.
- Documentation reorganised into folders; index and update procedure in place.

## Completed (not compiled yet)
- Slice 2: filter rows to conditions in Domain (`FilterRequestBuilder`), used by `TableQueryViewModel`.
- Slice 3: map-send rules and keyset paging rule in Domain (`MapSendRules`, `KeysetPaging`), used by `TableQueryViewModel`; tests added.

## Remaining
See `.ai/current-state.md` → Remaining.

## Changed Files (slices 2 and 3)
- Code: `Domain/FilterRequestBuilder.cs`, `Domain/MapSendRules.cs` (new); `Wpf/ViewModels/TableQueryViewModel.cs` (rules delegated; full file re-pushed from the branch version); tests `FilterRequestBuilderTests.cs`, `MapSendRulesTests.cs`, `KeysetPagingTests.cs` (new).
- Docs: `docs/changes/change-map.md`, `.ai/current-state.md`, `.ai/handoff.md`.

## Validation
- Owner: build and tests passed for slice 1; slices 2 and 3 await the owner's build.
- AI: static review only. Risk: the whole `TableQueryViewModel.cs` was re-written; a copy error would appear only in the build.

## Known Issues
See `.ai/current-state.md` and `docs/architecture/standards-audit.md`.

## Decisions
See `docs/reference/decision-log.md` (D1 to D38) and `docs/decisions/` (ADR-0001 to ADR-0007).

## Assumptions
- New type names (`FilterInput`, `FilterRequestBuilder`, `MapSendRules`, `KeysetPaging`) do not clash with existing names (search done before writing; the build confirms).
- The rest of `TableQueryViewModel.cs` matches the branch (copied from the version read earlier).

## Next Action
1. Owner builds and runs tests; reports the count and failures.
2. If green: decide whether to continue with the separate map-send coordinator, or move to D38.
