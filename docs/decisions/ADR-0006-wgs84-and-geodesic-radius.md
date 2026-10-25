# ADR-0006 WGS84 storage and geodesic radius circles
Status: accepted
Date: 2026-10 (migrated from docs/ARCHITECTURE.md, section "CRS and radius")

Context: layer points come from Excel in degrees. The map page draws in Web Mercator. A radius typed in metres must be exact on the ground at every latitude.

Options:
1. Send the radius as a number to the page and let MapLibre draw a pixel circle. Rejected: a pixel circle is a different size in metres at each zoom and latitude.
2. Convert metres to degrees with a flat approximation (degrees per metre constant). Rejected: wrong at high latitudes (1 km is about 0.018 degrees of longitude at 60 N, not 0.009).
3. Build each circle as a closed ring of WGS84 vertices using the spherical destination formula; the page only projects the ring.

Decision: option 3.
- Stored and imported coordinates are WGS84 (EPSG:4326, degrees). The map renders Web Mercator (EPSG:3857).
- Radius is metres everywhere except the text box. Parsed in one place only (RadiusInput).
- Circles: 64 segments, sphere radius 6,371,008.8 m (same value as the MapLibre scale bar).
- Geometry lives in Domain/GeoMath.cs; the page receives ready GeoJSON rings.

Consequences: circle error against the WGS84 ellipsoid is below 0.5 percent (chord error about 0.12 percent of the radius). Longitudes are not wrapped, so a circle over the antimeridian stays one shape. Costs: 65 vertices per circle; large layers mean many vertices (out of scope: clustering).

Re-evaluate when: sub-metre accuracy is required (switch to an ellipsoidal solver), or the map engine changes its projection.
