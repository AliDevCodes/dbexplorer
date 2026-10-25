# Feature: Excel Coordinate Layers

Status: implemented (Phase 1 model and store, Phase 2 map connection). Not yet compiled by the AI; the owner builds and tests (see the test procedure below).
Sources: migrated from the former `CURRENT_STATE.md` (2026-10-05) and `FEATURES.md`. Decisions: D20 to D26 and ADR-0006.

## What it does
The user imports an Excel file (`.xlsx` or `.xls`) with the columns Name, Latitude, Longitude (WGS84 decimal degrees). The points appear on the open map as markers with labels. Each point can have a radius circle in metres. Layers survive restarts.

## Flow
Excel import -> `MapLayer` model -> `CoordinateLayerGeoJson` -> `coordinate-layers.js` on the map page.

| Piece | Location |
|---|---|
| Models and rules | `Domain/CoordinateLayers.cs` |
| Geometry (circle ring, bounds, radius parsing) | `Domain/GeoMath.cs` |
| GeoJSON for the page | `Application/CoordinateLayers/CoordinateLayerGeoJson.cs` |
| Excel import and JSON store | `Infrastructure/CoordinateLayers/` |
| Commands, list, visibility, radius, refresh, zoom, remove | `Wpf/ViewModels/MapViewModel.cs`, `CoordinateLayerItem.cs` |
| Panel and messages to the page | `Wpf/Views/MapView.xaml(.cs)` |
| Drawing | `Assets/Map/coordinate-layers.js` (three additive hooks in `map.html`) |
| Excel file dialog | `Wpf/Services/FileDialogService.cs` (`PickExcelFile`) |
| Persian texts | `Wpf/Localization/CoordinateStrings.cs` |
| Tests | `CoordinateRenderingTests.cs`, `CoordinateLayerTests.cs` |

## Behaviour
- Import button on the map toolbar (visible when a map is open). A file-level problem shows a Persian message and no layer. Bad rows are skipped and counted (lenient rows, strict files, D22).
- Radius is typed in metres and applied with the button or Enter. Invalid text (negative, comma, above 20,037,508) shows an error and changes nothing. Persian digits and the Arabic decimal separator are accepted. 0 = no circle (D24).
- Circles are polygons of real WGS84 vertices (64 segments, sphere radius 6,371,008.8 m), so their size in metres is correct at any latitude and zoom (ADR-0006).
- Visibility and radius changes are saved. Refresh re-reads the layer from the store and redraws it; if the store no longer has it, the current copy is redrawn and a message is shown.
- Saved layers load once when the map page first appears and are sent again every time the page is created. Opening another map file keeps the layers.
- Each layer is one JSON file in `%AppData%\FastDbExplorer\coordinate-layers\` (D25).

## Not included (by design, for now)
Spatial database queries against layers; clustering and large-layer optimisation; column detection or mapping; DMS and DDM coordinates; several worksheets and `.xlsm`; renaming a layer; a basemap-less mode (layers need an opened map file); rename.

## Known limits
1. Circles that cross the antimeridian keep longitudes above 180 (not wrapped). Circles that contain a pole are not handled. Near the maximum radius the sphere model is used.
2. Spherical distances differ from the WGS84 ellipsoid by less than 0.5 percent.
3. A layer file is rewritten completely on every change. Damaged layer files are skipped silently by the store.
4. Refresh cannot re-import the Excel file (only the file name is stored, not its path); it reloads from the store.
5. Labels need the offline font files from `setup-map-assets.cmd`; without them markers show but labels do not.
6. `.xls` has no automated test; only the first worksheet is read.
7. `ExcelDataReader 3.*` is unpinned (R7).

## Owner test procedure (Windows)
1. `dotnet build FastDbExplorer.slnx`, then `dotnet test` (includes `CoordinateRenderingTests`).
2. Run `setup-map-assets.cmd` if not done; start the app; open any map file (MBTiles or GMDB).
3. Click the Excel import button; choose a file with Name, Latitude, Longitude. The panel shows the layer; markers appear.
4. Type a radius (for example 1000) and press Enter. Circles appear. At zoom about 14, one circle should be about 2 km across, at any latitude.
5. Uncheck the layer: markers and circles disappear. Check it again: they return. Restart the app: the layer, its visibility and radius are back.
6. Zoom button: the map fits all points and circles. Refresh: the layer redraws and a message appears. Remove: the layer disappears from the map and the panel.
7. Open a different map file: the layers are still drawn.
8. Try a file with a bad row, a wrong header, and a `.csv` renamed to `.xlsx`: Persian messages, no crash.

## Not compiled: likely trouble spots
- Generated command names of CommunityToolkit.Mvvm (`ZoomCommand`, `RefreshCommand`, `RemoveCommand`, `ApplyRadiusCommand`).
- The `DangerBrush` resource for error text (a missing key only logs a binding warning).
- Segoe MDL2 glyphs `E8A3`, `E72C`, `E74D`.
