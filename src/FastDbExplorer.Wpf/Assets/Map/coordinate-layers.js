// Excel coordinate layers (Phase 2): a marker (dot + label) and a radius circle for every point, drawn on top of the
// opened map. Independent of the tile layers in map.html; map.html only calls coordAttach(map, dark) when it creates
// a map and forwards "coord*" host messages to coordHandle(m).
//
// The host sends ready-made GeoJSON: points, and radius circles as polygons whose vertices are real WGS84 positions
// computed in C# (GeoMath.CircleRing). This page never converts metres, so circles stay metre-accurate at every
// latitude and zoom (MapLibre projects the polygon to Web Mercator). Layers are cached here, so they are redrawn
// when a different map file is opened and the map is rebuilt.
(function () {
  'use strict';

  const FONT = ['NotoSans'];
  const PARTS = ['fill', 'line', 'dot', 'label'];
  const layers = new Map();      // layer id -> last "coordUpsert" message
  let cmap = null, ready = false, dark = true;

  const lid = (id, part) => 'coord|' + id + '|' + part;
  const sid = (id, part) => 'coord|' + id + '|src-' + part;
  const hue = (s) => { let h = 0; for (const c of s) h = (h * 31 + c.charCodeAt(0)) >>> 0; return h % 360; };
  const colorOf = (id) => `hsl(${hue(id)}, 70%, ${dark ? 62 : 42}%)`;

  function setVisible(id, visible) {
    for (const p of PARTS)
      if (cmap.getLayer(lid(id, p))) cmap.setLayoutProperty(lid(id, p), 'visibility', visible ? 'visible' : 'none');
  }

  function draw(l) {
    if (!cmap || !ready) return;
    const pointsId = sid(l.id, 'points'), circlesId = sid(l.id, 'circles');

    if (cmap.getSource(pointsId)) {
      cmap.getSource(pointsId).setData(l.points);
      cmap.getSource(circlesId).setData(l.circles);
    } else {
      const c = colorOf(l.id);
      cmap.addSource(circlesId, { type: 'geojson', data: l.circles });
      cmap.addSource(pointsId, { type: 'geojson', data: l.points });
      cmap.addLayer({ id: lid(l.id, 'fill'), type: 'fill', source: circlesId,
        paint: { 'fill-color': c, 'fill-opacity': 0.18 } });
      cmap.addLayer({ id: lid(l.id, 'line'), type: 'line', source: circlesId,
        paint: { 'line-color': c, 'line-width': 2 } });
      cmap.addLayer({ id: lid(l.id, 'dot'), type: 'circle', source: pointsId,
        paint: { 'circle-radius': 6, 'circle-color': c, 'circle-stroke-color': '#FFFFFF', 'circle-stroke-width': 2 } });
      cmap.addLayer({ id: lid(l.id, 'label'), type: 'symbol', source: pointsId, minzoom: 9,
        layout: { 'text-field': ['get', 'name'], 'text-font': FONT, 'text-size': 11, 'text-anchor': 'top',
                  'text-offset': [0, 1.1], 'text-optional': true },
        paint: { 'text-color': dark ? '#E8ECF8' : '#1B2033', 'text-halo-color': dark ? '#0E1220' : '#FFFFFF', 'text-halo-width': 1.5 } });
    }
    setVisible(l.id, l.visible);
  }

  function remove(id) {
    if (!cmap || !ready) return;
    for (const p of PARTS) if (cmap.getLayer(lid(id, p))) cmap.removeLayer(lid(id, p));
    for (const p of ['points', 'circles']) if (cmap.getSource(sid(id, p))) cmap.removeSource(sid(id, p));
  }

  function fit(id) {
    const l = layers.get(id);
    if (!cmap || !l || !l.bounds) return;
    const [w, s, e, n] = l.bounds;
    if (w === e && s === n) cmap.easeTo({ center: [w, s], zoom: Math.max(cmap.getZoom(), 14), duration: 600 });
    else cmap.fitBounds([[w, s], [e, n]], { padding: 60, maxZoom: 17, duration: 600 });
  }

  // Called by map.html every time it creates a map (the old one is already removed).
  window.coordAttach = function (map, isDark) {
    cmap = map; ready = false; dark = isDark;
    map.on('load', () => { ready = true; for (const l of layers.values()) draw(l); });
  };

  window.coordHandle = function (m) {
    switch (m.type) {
      case 'coordUpsert': layers.set(m.id, m); draw(m); break;
      case 'coordVisible': {
        const l = layers.get(m.id);
        if (l) { l.visible = m.visible; if (cmap && ready) setVisible(m.id, m.visible); }
        break;
      }
      case 'coordRemove': remove(m.id); layers.delete(m.id); break;
      case 'coordFit': fit(m.id); break;
    }
  };
})();
