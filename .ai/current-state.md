# Current State

Branch: feature/geosent-live-map (the branch the owner tests)
Commit: head of this branch (see `git log --oneline -5`).
Task: refactor. Catalogue split (step 4) in progress: the drawing data moves to WPF, the rules stay in Domain.

## Verified by the owner
- Build and tests after step 4b-2 (before 4b-3): owner reported build and `dotnet test` passing (291 of 291).

## Done (NOT compiled yet; owner to build and test)
- Step 4b-3 part 1: `Domain/MapSourceStyles.cs` no longer holds glyph strings or `FixedGlyph`. Tests now read the WPF glyph data (`MapMarkerGlyphs`, `MapSourceGlyphs`) and the Domain rules (`MapMarkerRules`): `MapMarkerCatalogTests`, `MapMarkerCategoryTests`, `MapSourceStylesTests`, `MapMarkerRulesTests`.
- Consumers already switched in step 4b-2: `LayerStyleChoices`, `MapIconGeometry`, `CoordinateLayerItem`, `MapView.xaml.cs`, `MapViewModel.cs` (import palette).

## Still in Domain (temporary, to delete after the build is confirmed)
- `src/FastDbExplorer.Domain/MapMarkerCatalog.cs`: the old catalogue with the drawing data and a duplicate palette. It is not used by the code anymore, except possibly by partial files of `MapViewModel` that were not read yet (`MapViewModel.Tools.cs`, `MapViewModel.Actions.cs`, `MapViewModel.View.cs`, `MapViewModel.Window.cs`). The build tells whether any reference is left.

## Owner test plan (after build)
1. `dotnet build FastDbExplorer.slnx`: if it fails with CS0103 / CS1061 for `MapMarkerCatalog` or `FixedGlyph`, send the error lines; the file that uses them is then changed.
2. `dotnet test`: expected 291 tests pass (the renamed tests keep their count).
3. Layers: import an Excel layer; choose an icon and colour; restart; the icon and colour are still there.

## Remaining (in order)
1. Confirm the build; delete `Domain/MapMarkerCatalog.cs` (and any reference found).
2. Step 4 closed: the audit finding S6 for markers is done; the Persian calendar codec remains in Domain (R2).
3. Map-tab rule (N1) to Domain (audit S4). Map-send coordinator with I/O (optional).
4. R5 (one registration per interface, S8). R7 (pin NuGet versions).
5. Security review (S10), structured logs (S11), per-screen states (S12), reduced motion (S13).

## Known issues
See `reference/known-issues.md` and `architecture/standards-audit.md`.

## Next recommended action
Owner: build and test; send the result (or the CS lines).
