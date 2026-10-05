# Progress
**MVP-01:** done, owner-verified. **MVP-02:** delivered, owner reports it works.
**Map slice 1:** code complete, NOT compiled/run: MBTiles vector+raster viewer (MapLibre in WebView2), layers panel, cursor coords, zoom, fit; GMDB provisional; OSM PBF explained (converter later).
**Map slice 2 (delivered, not compiled by AI):** real GMDB reader (`GmdbMapSource`), labels with offline fonts + RTL plugin (run `setup-map-assets.cmd` again), tile-request interception fixed for Web Workers.
**Next:** owner builds (`setup-map-assets.cmd`, then `dotnet build`), tests with a real vector .mbtiles, reports errors. Owner to provide a small .gmdb sample. Then: GMDB support, PBF converter, owner-defined map tools.
**Full context:** PROJECT_MASTER_CONTEXT.md.
