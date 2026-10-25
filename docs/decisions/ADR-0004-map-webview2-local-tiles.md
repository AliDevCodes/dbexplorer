# ADR-0004 Map rendering with WebView2 and MapLibre, tiles by request interception
Status: accepted
Date: 2026-10

Context: the owner needs vector and raster map files, fully offline, with a modern renderer and labels. Maps must not be served by a local web server or by an internet service.

Options:
1. Local HTTP server inside the app. Works, but opens a port and adds a server lifecycle.
2. Render in WPF directly. Possible for raster, poor for vector tiles and labels.
3. WebView2 with MapLibre GL JS; tiles and page served from virtual hosts; tile requests intercepted in C# and answered from the open file.

Decision: option 3.
- Page at `https://app.local/map.html` from the bundled `Assets/Map`.
- Tiles at `https://tiles.local/{z}/{x}/{y}`, intercepted with `AddWebResourceRequestedFilter` for all request kinds (workers included, D19).
- Gzip vector tiles are unzipped in C#.

Consequences: WebView2 is a native window; WPF cannot draw over it (airspace). Side panels or state screens are used instead. WebView2 lifecycle must be managed explicitly: one at a time, disposal deferred, pop-out window waits for disposal (see change-map). The map view is created once and only hidden or shown.

Re-evaluate when: MapLibre or WebView2 has a breaking change, or when a server-side tile service is needed.
