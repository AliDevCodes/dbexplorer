# Map tools: coordinates, distance, my points, result markers

> Branch: `feature/map-tools-and-markers` (from `feature/geosent-source-upload` @ `c667e69`). **Written, NOT compiled or run by the AI.**

## What the user gets
All tools live **on the map itself** (a small HTML overlay in the map page), so they also work in the detached map window.

**Top-left toolbar:** a coordinates box + three icon buttons.
1. **Go to coordinates:** type `35.7219, 51.3347` (also `35.7219 51.3347`, `35.72N 51.33E`, `35°43'19"N 51°20'05"E`, Persian digits) and press Enter. The map flies there, drops a red temporary pin and shows a card with latitude/longitude. Wrong input shows a short red message.
2. **Get coordinates:** turn the crosshair on and click anywhere (or simply **right-click** the map at any time). A card shows latitude and longitude with a copy button for each and for "lat, lng".
3. **Measure distance** (like Google Maps): click points; a live total follows the mouse, every vertex shows its running distance, the card has undo / finish / clear. Double-click, Enter or right-click finishes; Backspace undoes; Esc leaves the tool. Distances are great-circle (haversine).

**Card actions:** *Add to my points* (optional name) saves the point permanently in the layer **«نقاط من»** (it appears in the side panel like any layer, with a **pin** marker); *Measure from here* starts a measurement at that point. Clicking a saved "my points" point shows a card with copy and **delete**.

## Explorer / Monitoring results on the map
- The temporary result layer now has its own look: **amber diamond with a magnifier**. Excel layers stay **round** (their own icon and colour), "my points" are **pins**, so the source is clear at a glance.
- A chip at the top-left of the map shows `layer name · point count` with two buttons: **zoom to the results** and **remove them from the map**. Sending new rows replaces the old results; disconnecting also removes them.

## How it works
- `Domain/MapSourceStyles.cs`: `MapLayerKind` (Excel / Manual / Query), marker shape, fixed colour/glyph for query results.
- `map-tools.js` (new): the overlay; `coordinate-layers.js`: marker shapes (circle / pin / diamond) + `coordselect` events; `map.html`: loads the tools and forwards `tool*` messages.
- `MapView.xaml.cs` + `MapView.Tools.cs`: send texts, layer kind/shape and the result-chip state; handle `toolAddPoint`, `toolDeletePoint`, `toolZoomResult`, `toolClearResult`.
- `MapViewModel.Tools.cs`: persistent "my points" layer (fixed id), add/remove point, zoom to results. `MapToolStrings.cs`: all Persian texts.
- No changes to `MapView.xaml`, `MapUiStrings`, `CoordinateLayerItem`, `MapMarkerCatalog`, `MapWindow`.

## Manual test checklist
1. Build + `dotnet test` (new `MapSourceStylesTests`).
2. Type `35.7219, 51.3347` + Enter: map flies there, red pin + card. Try `abc`: red error. Try `35°43'19"N 51°20'05"E` and Persian digits.
3. Card: copy buttons turn green "کپی شد"; paste somewhere to check the value.
4. Crosshair tool: click several places; each shows its coordinates. Right-click works without the tool.
5. Ruler tool: click 3-4 points; labels at vertices, live total in the card; undo, finish, clear; double-click finishes; Esc exits.
6. "Add to my points" with and without a name: pin appears, layer «نقاط من» in the Excel tab; restart the app: still there.
7. Click a saved pin: card with copy + delete; delete removes it (last point removes the layer).
8. Explorer: select rows with coordinates, send to map: amber diamonds + chip. Zoom button, remove button (diamonds disappear, chip disappears). Send again: replaced.
9. Excel layer markers still round; hover/click/selection ring work for pins and diamonds; detached map window shows the same tools.
10. Dark and light theme.

## Known risks / not verified
- Uncompiled. Likely trouble: canvas `roundRect` (needs the current WebView2 runtime), `circle-translate` hit-testing for pins, glyph shapes.
- Monitoring and Explorer share the one result layer and style (both call the same method); they are not told apart from each other.
- Result points are never saved (as before). Only "my points" are persistent; their per-point details are not stored (the store keeps name + coordinates).
- The "selected point" card in the side panel still shows for every point; for "my points" the delete button is in the card on the map.
