# Map Formats and Facts

Facts about the map files and the page contract. Migrated from `PROJECT_MASTER_CONTEXT.md` section 9 and `ARCHITECTURE.md`.

## Formats
| Format | Status | Notes |
|---|---|---|
| `.mbtiles` vector (pbf) | implemented | layers from metadata `json` / `vector_layers`, or sampled from tiles (`MvtLayerReader`) |
| `.mbtiles` raster (png, jpg, webp) | implemented | |
| `.gmdb` (GMap.NET cache) | implemented (raster); MBTiles-layout fallback; otherwise the UI lists its tables | verified against the owner's sample (D16); tables `Tiles(id, X, Y, Zoom, Type, CacheTime)` and `TilesData(id, Tile)`; XYZ y, no flip |
| `.pbf` = OSM raw extract | not renderable directly | not tiles, and the country files are huge (D17). Plan: converter slice (external tilemaker or Planetiler, bounding box, progress, cancel) producing MBTiles |
| `.pbf` = MVT tile folder | not implemented | the owner's PBF files are OSM |

## Facts
- MBTiles = SQLite `metadata(name, value)` + `tiles(zoom_level, tile_column, tile_row, tile_data)`. `tile_row` is TMS: `xyz_y = 2^z - 1 - row`. Vector tiles are gzip MVT protobuf.
- OSM PBF = zlib blobs of nodes, ways and relations; the header block type is `OSMHeader`.
- Labels (D18): name priority `name:fa` then `name_en` then `name:latin` then `name`; glyphs `NotoSans` and the RTL plugin are local files.
- Style is generic (JS, one fill, line and circle layer per vector layer; colour hashed from the layer name), so any vector file renders.

## Limits
- WebView2 runtime is required (present on Windows 11, usually on 10).
- WebView2 is a native window: WPF elements cannot overlay the map.
- Drag-and-drop of a file works only on the empty state.

## Page contract (host to page messages, JSON)
- Host to page: `load`, `layer`, `fit`, `coordUpsert`, `coordVisible`, `coordRemove`, `coordFit`, `coordRename`, `coordSelectClear`, `toolsInit`, `toolResults`.
- Page to host: `ready`, `cursor`, `zoom`, `view`, `pointHover`, `pointSelect`, `toolAddPoint`, `toolDeletePoint`, `toolZoomResult`, `toolClearResult`.
- `coordUpsert` carries: id, name, visible, radiusMeters, kind, shape, color, iconKey, iconPath, points, circles, bounds. The page keeps the last `coordUpsert` per id and redraws after every map (re)load, so opening another map keeps the layers.
- Radius circles are GeoJSON rings computed in C# (ADR-0006). The page never converts metres.
- Coordinates are WGS84; the map draws Web Mercator.
