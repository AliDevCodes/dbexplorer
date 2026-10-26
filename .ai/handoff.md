# AI Handoff

## Current Goal
Keep FastDbExplorer stable and changeable, following the senior-engineer standards: business rules in Domain, drawing data in the presentation layer, documents kept in step with every change.

## Repository State
Branch: feature/geosent-live-map. Commit: head of the branch (see `.ai/current-state.md`).

## Completed (verified by the owner)
- Refactor slices 1 to 3; catalogue split step 4a and 4b-1; step 4b-2 (build and 291 tests passed).

## Completed (not compiled yet)
- Step 4b-3 part 1: Domain keeps only rules; the drawing data is tested in WPF terms; four test files changed.

## Remaining
See `.ai/current-state.md` → Remaining. Next: confirm the build, then delete `Domain/MapMarkerCatalog.cs`.

## Changed Files (step 4b-3 part 1)
- Code: `src/FastDbExplorer.Domain/MapSourceStyles.cs`; `tests/FastDbExplorer.Tests/MapMarkerCatalogTests.cs`, `MapMarkerCategoryTests.cs`, `MapSourceStylesTests.cs`, `MapMarkerRulesTests.cs`.
- Docs: `.ai/current-state.md`, `.ai/handoff.md`.

## Validation
- Owner: build and tests passed after step 4b-2 (291 of 291). Step 4b-3 awaits the owner's build.
- AI: static review only; the partial files of MapViewModel (Tools, Actions, View, Window) were not read and may still reference the old catalogue.

## Known Issues
See `.ai/current-state.md` and `docs/architecture/standards-audit.md`.

## Decisions
See `docs/reference/decision-log.md` and `docs/decisions/`.

## Assumptions
- The catalogue file is not deleted until the build confirms no reference is left (a deletion before the build could break the owner's build).

## Next Action
1. Owner builds and runs tests; sends the result or the CS lines.
2. If green: delete `Domain/MapMarkerCatalog.cs` and update the change map (marker section).
