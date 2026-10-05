# Features
_Last updated: 2026-10-05. Status: ✅ done & owner-verified · 🟡 delivered, not verified by AI · 🧱 foundation only (no UI) · ⏳ planned_

## SQL Server explorer
| Feature | Status |
|---|---|
| Connect (Windows / SQL auth), database list, table list + search, recent connections (no passwords) | ✅ MVP-01 |
| Column picker, 13-operator parameterised filters, hybrid OFFSET→keyset paging, cancel, timeout | 🟡 MVP-02 (owner reports it works) |
| Virtualised viewer, header sorting, column management | ⏳ MVP-03 |

## Map viewer
| Feature | Status |
|---|---|
| MBTiles vector + raster (MapLibre in WebView2), layers panel, cursor coordinates, zoom, fit | 🟡 |
| GMDB (GMap.NET raster cache), labels with offline fonts | 🟡 |
| OSM PBF → MBTiles converter | ⏳ |

## Excel coordinate layers
| Feature | Status | Notes |
|---|---|---|
| Import `.xlsx` / `.xls` with columns Name, Latitude, Longitude (WGS84 decimal degrees) | 🧱 Phase 1 | `ExcelCoordinateImportService`; invalid rows skipped and reported with codes |
| Models `MapPoint`, `MapLayer` (Visibility, RadiusMeters in metres) | 🧱 Phase 1 | validated in the constructor |
| Layers survive restart (JSON per layer in `%AppData%\FastDbExplorer\coordinate-layers`) | 🧱 Phase 1 | store exists; nothing loads it at startup yet |
| Import UI (file picker, report, layer list, visibility, radius, delete) | ⏳ Phase 2 | |
| Render points + radius circles on the map | ⏳ Phase 2/3 | |
| Column detection / mapping | ⏳ later | |
| DDM / DMS conversion | ⏳ later | |
| Database spatial queries against layers | ⏳ later | |
