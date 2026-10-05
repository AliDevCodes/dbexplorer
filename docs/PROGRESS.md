# Progress
**MVP-01:** done, owner-verified. **MVP-02:** delivered, owner reports it works.
**Map slice 1:** code complete, NOT compiled/run: MBTiles vector+raster viewer (MapLibre in WebView2), layers panel, cursor coords, zoom, fit; GMDB provisional; OSM PBF explained (converter later).
**Map slice 2 (delivered, not compiled by AI):** real GMDB reader (`GmdbMapSource`), labels with offline fonts + RTL plugin (run `setup-map-assets.cmd` again), tile-request interception fixed for Web Workers.
**Map UX (branch feature/map-ux-polish-last-map):** zoom controls top-right, big map card on the connect screen, labelled map pill + vector map icon in the explorer sidebar. Not compiled.
**Monitoring (branch feature/monitoring-alerts, details in MONITORING.md):** phases 1-3 written (dynamic conditions, per-monitor schedule, corner alert + sound + xlsx report); step 3 hardening done (late-committed rows, failed-check alerts, editor race). NOT compiled/run by the AI.
**Next:** owner builds (`setup-map-assets.cmd`, then `dotnet build`, `dotnet test`) and reports errors; manual monitoring test on a real table (MONITORING.md, work plan steps 1-2). Then: remaining docs, persistent alert history, GMDB/PBF items.
**Full context:** PROJECT_MASTER_CONTEXT.md.
