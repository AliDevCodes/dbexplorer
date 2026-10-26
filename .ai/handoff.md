# AI Handoff

## Current Goal
Keep FastDbExplorer stable and changeable, following the senior-engineer standards: business rules in Domain, drawing data in the presentation layer, documents kept in step with every change.

## Repository State
Branch: feature/geosent-live-map. Commit: head of the branch (see `.ai/current-state.md`).

## Completed (verified by the owner)
- Refactor slices 1 to 3; step 4a. Tests: 283 of 283 passed.

## Completed (not compiled yet)
- Step 4b-1: WPF copy of the marker glyph data (`Wpf/Presentation/MapMarkerGlyphs.cs`, `MapSourceGlyphs.cs`), tests (`MapMarkerGlyphsTests.cs`), and the test project reference to the WPF project.

## Remaining
See `.ai/current-state.md` → Remaining (step 4b-2: consumers switch to the WPF data).

## Changed Files (step 4b-1)
- Code: `src/FastDbExplorer.Wpf/Presentation/MapMarkerGlyphs.cs` (new), `src/FastDbExplorer.Wpf/Presentation/MapSourceGlyphs.cs` (new), `tests/FastDbExplorer.Tests/MapMarkerGlyphsTests.cs` (new), `tests/FastDbExplorer.Tests/FastDbExplorer.Tests.csproj` (reference added).
- Docs: `.ai/current-state.md`, `.ai/handoff.md`.

## Validation
- Owner: 283 of 283 after step 4a. Step 4b-1 awaits the owner's build.
- AI: static review only.

## Known Issues
See `.ai/current-state.md` and `docs/architecture/standards-audit.md`.

## Decisions
See `docs/reference/decision-log.md` and `docs/decisions/`.

## Assumptions
- The WPF project can be referenced by the test project (a WinExe reference); if not, the test is moved, not the data.
- The glyph data is a copy of the Domain catalogue, verified by eye; the path test checks only that it is not empty (a parse test needs WPF types in the test project).

## Next Action
1. Owner builds and runs tests; reports the count and any error (including MSB messages about the new reference).
2. If green: step 4b-2 (consumers). Each consumer file is read first.
