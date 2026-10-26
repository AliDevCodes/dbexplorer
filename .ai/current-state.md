# Current State

Branch: feature/geosent-live-map (the branch the owner tests)
Commit: head of this branch (see `git log --oneline -5`).
Task: refactor. Slices 1 to 3 and step 4a verified by the owner (283 of 283 tests passed). Catalogue split step 4b-1 in progress.

## Verified by the owner
- Whole solution builds (including WPF): passed (before step 4a).
- `dotnet test` after step 4a: 283 of 283 passed.

## Done (NOT compiled yet; owner to build and test)
- Step 4b-1: `Wpf/Presentation/MapMarkerGlyphs.cs` (68 glyphs in 11 folders, WPF copy of the drawing data), `Wpf/Presentation/MapSourceGlyphs.cs` (fixed glyphs of Explorer and Monitoring), `tests/MapMarkerGlyphsTests.cs` (4 test methods, several cases).
- The test project now references the WPF project. This is a NEW reference: if the build reports a problem with it (for example the WinExe reference), tell me the exact error; the reference can be removed and the tests moved.

## Owner test plan (after build)
1. `dotnet build FastDbExplorer.slnx` then `dotnet test`. Expected: 283 + new WPF glyph tests pass.
2. If the WinExe reference causes an error, send the MSB or CS lines.

## Remaining (in order)
1. Step 4b-2: switch the consumers to the WPF glyph data: `Wpf/Services/MapIconGeometry.cs`, `Wpf/ViewModels/LayerStyleChoices.cs`, `Wpf/ViewModels/CoordinateLayerItem.cs`, `Wpf/Views/MapView.xaml.cs` (PostCoordinateLayer), `Wpf/ViewModels/MapViewModel.cs` (palette). Each file is read before it is changed.
2. Step 4b-3: delete the drawing data and the duplicate palette/colour functions from `Domain/MapMarkerCatalog.cs` and `Domain/MapSourceStyles.cs` (the glyph strings); move `MapMarkerCatalogTests` and `MapMarkerCategoryTests` to use the WPF data.
3. Map-tab rule (N1) to Domain (audit S4). Map-send coordinator with I/O (optional).
4. R2 (remaining presentation data out of Domain: Persian calendar codec, S6). R5 and R7.
5. Security review (S10), structured logs (S11), per-screen states (S12), reduced motion (S13).

## Known issues
See `reference/known-issues.md` and `architecture/standards-audit.md`.

## Next recommended action
Owner: build and test; send the result. Then step 4b-2.
