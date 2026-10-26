# Current State

Branch: feature/geosent-live-map (the branch the owner tests)
Commit: head of this branch (see `git log --oneline -5`).
Task: refactor. Slices 1 to 3 built and tested by the owner (263 of 263 passed). Test project retargeted to net10.0-windows. Catalogue split step 4a in progress.

## Verified by the owner
- Build of the whole solution, including the WPF project: passed.
- `dotnet test`: 263 of 263 passed (before step 4a).
- Test project on net10.0-windows: 263 of 263 passed.

## Done (NOT compiled yet; owner to build and test)
- Step 4a: `Domain/MapMarkerRules.cs` (palette, valid colour, known icon keys) and `tests/MapMarkerRulesTests.cs` (16 new tests, including a consistency test with the catalogue keys).
- Step 4a: `JsonCoordinateLayerStore` uses `MapMarkerRules` instead of the drawing catalogue (same behaviour).

## Owner test plan (after build)
1. `dotnet build FastDbExplorer.slnx` and `dotnet test`. Expected: 263 + 16 = 279 tests pass.
2. Layers: import an Excel layer, choose an icon and colour, restart the app: icon and colour are still there. A layer file with an unknown icon key loads with the default marker.

## Remaining (in order)
1. Step 4b: move the SVG drawing data (`MapMarkerCatalog`) and the two fixed glyphs of `MapSourceStyles` to a WPF class; update `MapView.xaml.cs`, `MapIconGeometry`, `LayerStyleChoices`, `CoordinateLayerItem`, `MapViewModel` (palette). Remove the duplicate palette and colour functions from the catalogue.
2. Step 4c: move the catalogue tests to the WPF side of the tests project; the consistency test keeps running.
3. Decision D38 is resolved in practice (the test project targets net10.0-windows). View-model tests are now possible; plan them after 4c.
4. Map-tab rule (N1) to Domain (audit S4). Map-send coordinator with I/O (optional).
5. R2 (rest of presentation data out of Domain: Persian calendar codec, S6). R5: one registration per interface (S8). R7: pin NuGet versions.
6. Security review (S10), structured logs (S11), per-screen states (S12), reduced motion (S13).

## Known issues
See `reference/known-issues.md` and `architecture/standards-audit.md`.

## Next recommended action
Owner: build and test; send the result. Then step 4b.
