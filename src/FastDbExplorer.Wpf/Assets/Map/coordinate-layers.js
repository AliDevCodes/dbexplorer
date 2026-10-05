// Excel coordinate layers (Phase 2): a marker (halo + dot + label) and a radius circle for every point, drawn on top of the
// opened map. Independent of the tile layers in map.html; map.html only calls coordAttach(map, dark) when it creates
// a map and forwards "coord*" host messages to coordHandle(m).
//
// The host sends ready-made GeoJSON: points, and radius circles as polygons whose vertices are real WGS84 positions
// computed in C# (GeoMath.CircleRing). This page never converts metres, so circles stay metre-accurate at every
// latitude and zoom (MapLibre projects the polygon to Web Mercator). Layers are cached here, so they are redrawn
// when a different map file is opened and the map is rebuilt.
//
// Interaction: hovering a point shows a tooltip with its name (and posts "pointHover" so the host can show it in the
// status bar); clicking a point highlights it and posts "pointSelect"; clicking empty map clears the highlight.
(function () {
  'use strict';

  const FONT = ['NotoSans'];
  const PARTS = ['fill', 'line', 'halo', 'dot', 'label'];
  const SEL_SRC = 'coord|selected|src', SEL_HALO = 'coord|selected|halo', SEL_RING = 'coord|selected|ring';
  const HIT_RADIUS = 9; // px around the cursor in which a point counts as hit

  const host = window.chrome && window.chrome.webview;
  const post = (m) => host && host.postMessage(m);

  const layers = new Map();      // layer id -> last "coordUpsert" message (name/visible kept up to date)
  let cmap = null, ready = false, dark = true;
  let selected = null;           // { layerId, pointId } of the highlighted point
  let hovered = null;            // "layerId|pointId" of the point under the mouse
  let tip = null;
  let moveEvent = null, moveQueued = false;

  const lid = (id, part) => 'coord|' + id + '|' + part;
  const sid = (id, part) => 'coord|' + id + '|src-' + part;
  const hue = (s) => { let h = 0; for (const c of s) h = (h * 31 + c.charCodeAt(0)) >>> 0; return h % 360; };
  const colorOf = (id) => `hsl(${hue(id)}, 70%, ${dark ? 62 : 42}%)`;
  const accent = () => (dark ? '#8A90FF' : '#4F46E5');
  const empty = () => ({ type: 'FeatureCollection', features: [] });

  // ---- tooltip ------------------------------------------------------------------------------------------------

  function ensureTip() {
    if (tip) return tip;
    const style = document.createElement('style');
    style.textContent =
      '#coord-tip{position:absolute;z-index:5;pointer-events:none;display:none;max-width:260px;padding:6px 10px;' +
      'border-radius:8px;font:600 12px "Segoe UI",Tahoma,sans-serif;box-shadow:0 4px 14px rgba(0,0,0,.28);' +
      'white-space:nowrap;overflow:hidden;text-overflow:ellipsis}' +
      '#coord-tip small{display:block;font-weight:400;font-size:11px;opacity:.7;margin-top:1px}';
    document.head.appendChild(style);
    tip = document.createElement('div');
    tip.id = 'coord-tip';
    document.body.appendChild(tip);
    return tip;
  }

  function showTip(point, name, layerName) {
    const t = ensureTip();
    t.style.background = dark ? '#1F2540' : '#FFFFFF';
    t.style.color = dark ? '#E8EBF5' : '#151B2C';
    t.style.border = '1px solid ' + (dark ? '#2B3252' : '#E0E5EE');
    t.textContent = '';                      // names come from Excel: always textContent, never innerHTML
    const n = document.createElement('div');
    n.dir = 'auto';
    n.textContent = name;
    t.appendChild(n);
    if (layerName) {
      const s = document.createElement('small');
      s.dir = 'auto';
      s.textContent = layerName;
      t.appendChild(s);
    }
    t.style.display = 'block';
    const w = t.offsetWidth, h = t.offsetHeight;
    let x = point.x + 14, y = point.y + 14;
    if (x + w > window.innerWidth - 4) x = point.x - w - 14;
    if (y + h > window.innerHeight - 4) y = point.y - h - 14;
    t.style.left = Math.max(4, x) + 'px';
    t.style.top = Math.max(4, y) + 'px';
  }

  function hideTip() { if (tip) tip.style.display = 'none'; }

  // ---- drawing --------------------------------------------------------------------------------------------------

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
      // Radius area: light fill + crisp dashed outline, so overlapping circles stay readable.
      cmap.addLayer({ id: lid(l.id, 'fill'), type: 'fill', source: circlesId,
        paint: { 'fill-color': c, 'fill-opacity': 0.14 } });
      cmap.addLayer({ id: lid(l.id, 'line'), type: 'line', source: circlesId,
        layout: { 'line-join': 'round' },
        paint: { 'line-color': c, 'line-width': 1.8, 'line-opacity': 0.9, 'line-dasharray': [3, 2] } });
      // Point marker: soft halo + solid dot with a white ring, growing a little with the zoom.
      cmap.addLayer({ id: lid(l.id, 'halo'), type: 'circle', source: pointsId,
        paint: { 'circle-radius': ['interpolate', ['linear'], ['zoom'], 3, 6, 10, 11, 16, 15],
                 'circle-color': c, 'circle-opacity': 0.22, 'circle-blur': 0.4 } });
      cmap.addLayer({ id: lid(l.id, 'dot'), type: 'circle', source: pointsId,
        paint: { 'circle-radius': ['interpolate', ['linear'], ['zoom'], 3, 3.5, 10, 6, 16, 8],
                 'circle-color': c, 'circle-stroke-color': '#FFFFFF', 'circle-stroke-width': 2 } });
      cmap.addLayer({ id: lid(l.id, 'label'), type: 'symbol', source: pointsId, minzoom: 8,
        layout: { 'text-field': ['get', 'name'], 'text-font': FONT, 'text-size': 12, 'text-anchor': 'top',
                  'text-offset': [0, 1.2], 'text-optional': true },
        paint: { 'text-color': dark ? '#E8ECF8' : '#1B2033', 'text-halo-color': dark ? '#0E1220' : '#FFFFFF', 'text-halo-width': 1.8 } });
    }
    setVisible(l.id, l.visible);
    revalidateSelection(l);
    raiseSelection();
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

  // ---- selection highlight ----------------------------------------------------------------------------------------

  function drawSelection(coordinates) {
    const data = { type: 'FeatureCollection',
      features: [{ type: 'Feature', geometry: { type: 'Point', coordinates }, properties: {} }] };
    if (cmap.getSource(SEL_SRC)) {
      cmap.getSource(SEL_SRC).setData(data);
    } else {
      cmap.addSource(SEL_SRC, { type: 'geojson', data });
      cmap.addLayer({ id: SEL_HALO, type: 'circle', source: SEL_SRC,
        paint: { 'circle-radius': 17, 'circle-color': accent(), 'circle-opacity': 0.28 } });
      cmap.addLayer({ id: SEL_RING, type: 'circle', source: SEL_SRC,
        paint: { 'circle-radius': 10, 'circle-color': accent(), 'circle-opacity': 0,
                 'circle-stroke-color': accent(), 'circle-stroke-width': 3 } });
    }
    raiseSelection();
  }

  function raiseSelection() {
    if (!cmap) return;
    if (cmap.getLayer(SEL_HALO)) cmap.moveLayer(SEL_HALO);
    if (cmap.getLayer(SEL_RING)) cmap.moveLayer(SEL_RING);
  }

  function findPoint(layerId, pointId) {
    const l = layers.get(layerId);
    if (!l || !l.points) return null;
    return l.points.features.find((f) => f.properties.id === pointId) || null;
  }

  function select(layerId, pointId) {
    const l = layers.get(layerId), f = findPoint(layerId, pointId);
    if (!l || !f) return;
    selected = { layerId, pointId };
    const [lng, lat] = f.geometry.coordinates;
    drawSelection([lng, lat]);
    post({ type: 'pointSelect', selected: true, name: f.properties.name, layer: l.name, lat, lng });
  }

  function clearSelection(notify) {
    const had = selected !== null;
    selected = null;
    if (cmap && ready && cmap.getSource(SEL_SRC)) cmap.getSource(SEL_SRC).setData(empty());
    if (notify && had) post({ type: 'pointSelect', selected: false });
  }

  // After a layer was redrawn: keep the highlight only while its point still exists (and follow it if it moved).
  function revalidateSelection(l) {
    if (!selected || selected.layerId !== l.id) return;
    const f = findPoint(l.id, selected.pointId);
    if (!f || !l.visible) clearSelection(true);
    else drawSelection(f.geometry.coordinates);
  }

  // ---- hover / click ----------------------------------------------------------------------------------------------

  function hit(point) {
    const ids = [];
    for (const l of layers.values())
      if (l.visible && cmap.getLayer(lid(l.id, 'dot'))) ids.push(lid(l.id, 'dot'));
    if (!ids.length) return null;
    const found = cmap.queryRenderedFeatures(
      [[point.x - HIT_RADIUS, point.y - HIT_RADIUS], [point.x + HIT_RADIUS, point.y + HIT_RADIUS]], { layers: ids });
    if (!found.length) return null;
    const layerId = found[0].layer.id.split('|')[1];
    return { layerId, pointId: found[0].properties.id, name: found[0].properties.name,
             layerName: (layers.get(layerId) || {}).name || '' };
  }

  function clearHover() {
    const had = hovered !== null;
    hovered = null;
    hideTip();
    if (cmap) cmap.getCanvas().style.cursor = '';
    if (had) post({ type: 'pointHover', name: null, layer: null });
  }

  function onMove(e) {
    if (!cmap || !ready) return;
    const h = hit(e.point);
    if (!h) { if (hovered) clearHover(); return; }
    cmap.getCanvas().style.cursor = 'pointer';
    showTip(e.point, h.name, h.layerName);
    const key = h.layerId + '|' + h.pointId;
    if (hovered !== key) {
      hovered = key;
      post({ type: 'pointHover', name: h.name, layer: h.layerName });
    }
  }

  // mousemove can fire faster than frames: handle at most one per animation frame.
  function onMoveThrottled(e) {
    moveEvent = e;
    if (moveQueued) return;
    moveQueued = true;
    requestAnimationFrame(() => { moveQueued = false; const ev = moveEvent; moveEvent = null; if (ev) onMove(ev); });
  }

  function onClick(e) {
    if (!cmap || !ready) return;
    const h = hit(e.point);
    if (h) select(h.layerId, h.pointId);
    else clearSelection(true);
  }

  // ---- map.html entry points ----------------------------------------------------------------------------------------

  // Called by map.html every time it creates a map (the old one is already removed).
  window.coordAttach = function (map, isDark) {
    const hadState = selected !== null || hovered !== null;
    cmap = map; ready = false; dark = isDark;
    selected = null; hovered = null; hideTip();
    if (hadState) { post({ type: 'pointHover', name: null, layer: null }); post({ type: 'pointSelect', selected: false }); }

    map.on('load', () => { ready = true; for (const l of layers.values()) draw(l); });
    map.on('mousemove', onMoveThrottled);
    map.on('mouseout', clearHover);
    map.on('dragstart', () => { hideTip(); });
    map.on('zoomstart', () => { hideTip(); });
    map.on('click', onClick);
  };

  window.coordHandle = function (m) {
    switch (m.type) {
      case 'coordUpsert': layers.set(m.id, m); draw(m); break;
      case 'coordVisible': {
        const l = layers.get(m.id);
        if (l) {
          l.visible = m.visible;
          if (cmap && ready) setVisible(m.id, m.visible);
          if (!m.visible) {
            if (selected && selected.layerId === m.id) clearSelection(true);
            if (hovered && hovered.startsWith(m.id + '|')) clearHover();
          }
        }
        break;
      }
      case 'coordRemove':
        if (selected && selected.layerId === m.id) clearSelection(true);
        if (hovered && hovered.startsWith(m.id + '|')) clearHover();
        remove(m.id); layers.delete(m.id);
        break;
      case 'coordFit': fit(m.id); break;
      case 'coordRename': {
        const l = layers.get(m.id);
        if (l) {
          l.name = m.name;
          if (selected && selected.layerId === m.id) select(selected.layerId, selected.pointId); // refresh the info card
        }
        break;
      }
      case 'coordSelectClear': clearSelection(false); break;
    }
  };
})();
