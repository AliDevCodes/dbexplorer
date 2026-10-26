# AI Handoff

## Current Goal
Keep FastDbExplorer stable and changeable, following the senior-engineer standards: business rules in Domain, drawing data in the presentation layer, documents kept in step with every change.

## Repository State
Branch: feature/geosent-live-map. Commit: head of the branch (see `.ai/current-state.md`).

## Completed (verified by the owner)
- Refactor slices 1 to 3 (filter rules, query request builder, map-send and paging rules): build and 263 of 263 tests passed.
- Test project on net10.0-windows: 263 of 263 passed.

## Completed (not compiled yet)
- Step 4a of the catalogue split: `Domain/MapMarkerRules.cs`, `tests/MapMarkerRulesTests.cs`, and the store uses the rules.

## Remaining
See `.ai/current-state.md` → Remaining (step 4b next: drawing data to WPF).

## Changed Files (step 4a)
- Code: `src/FastDbExplorer.Domain/MapMarkerRules.cs` (new), `tests/FastDbExplorer.Tests/MapMarkerRulesTests.cs` (new), `src/FastDbExplorer.Infrastructure/CoordinateLayers/JsonCoordinateLayerStore.cs` (ToLayer).
- Docs: `docs/changes/change-map.md`, `.ai/current-state.md`, `.ai/handoff.md`.

## Validation
- Owner: build and tests passed before step 4a. Step 4a awaits the owner's build.
- AI: static review only.

## Known Issues
See `.ai/current-state.md` and `docs/architecture/standards-audit.md`.

## Decisions
See `docs/reference/decision-log.md` and `docs/decisions/`. Open: the duplicate palette in the catalogue is temporary (removed in 4b).

## Assumptions
- The 68 icon keys in `MapMarkerRules` equal the catalogue keys (checked by a test that the owner runs).
- The `JsonCoordinateLayerStore` change is limited to `ToLayer` (the file was copied from the branch version).

## Next Action
1. Owner builds and runs tests; reports the count (expected 279) and any failure.
2. If green: step 4b (drawing data to WPF).
