# Decision Log (D1 to D38)

Small and medium decisions. Significant ones have an ADR (see `docs/decisions/`). Migrated from `PROJECT_MASTER_CONTEXT.md` (D1 to D27), and from the project brain of 2026-10-25 (D28 to D37). D38 is a pending decision.

| # | Decision | Why |
|---|---|---|
| D1 | SQL Server first; WPF; .NET 10; MVVM | initial scope |
| D2 | Auth: Windows and SQL | owner choice |
| D3 | UI in Persian, right-to-left, automatic light and dark, own styles | owner choice |
| D4 | Recent connections saved without passwords (later: ADR-0003 for the opt-in vault) | security |
| D5 | TrustServerCertificate default on (internal self-signed certificates); user can turn it off | usability on internal servers |
| D6 | System databases (id 1 to 4) hidden | the tool is for user data |
| D7 | Row counts from sys.partitions (approximate) | no table scans |
| D8 | Paging hybrid: OFFSET then keyset; no COUNT(*) | owner choice; speed on huge tables |
| D9 | No primary key: user picks the order column, OFFSET only | keyset on a non-unique column skips rows |
| D10 | Default isolation (READ COMMITTED), no NOLOCK | NOLOCK can return wrong data |
| D11 | Filter only common types (numbers, dates, text, guid, bit) | typed parameters keep indexes usable |
| D12 | Floating NuGet versions for now | no network in the AI sandbox (to be pinned, R7) |
| D13 | Map renderer = MapLibre GL JS in WebView2 | best vector-tile support; tiles served from the file (ADR-0004) |
| D14 | Packages Microsoft.Data.Sqlite and Microsoft.Web.WebView2 approved by owner delegation | needed for MBTiles and rendering |
| D15 | MapLibre files downloaded by setup-map-assets.cmd, not committed; publish.cmd calls it if missing | AI has no internet; small repository |
| D16 | GMDB = GMap.NET tile cache: SQLite Tiles and TilesData tables, raster PNG or JPEG, XYZ y (no flip); provider with most tiles is used; MBTiles-layout fallback kept | verified against the owner's sample file |
| D17 | OSM PBF (`*.osm.pbf`) is not rendered directly; the UI explains; a converter is a later slice | raw OSM is not tiles; the country file is huge |
| D18 | Labels for OpenMapTiles-style vector files; glyph PBFs (NotoSans) and RTL plugin downloaded by setup-map-assets.cmd; name priority name:fa, name_en, name:latin, name | owner saw a map without names; no internet at run time |
| D19 | Tile requests from Web Workers intercepted with AddWebResourceRequestedFilter (all request kinds); gzip tiles unzipped in C# | MapLibre fetches tiles inside workers |
| D20 | Excel layer points stored as JSON, one file per layer (atomic write) | simple, inspectable, no database needed |
| D21 | Monitoring replaces the earlier section name in the UI | owner request |
| D22 | Explorer and Monitoring share one "show on map" panel (MapSendPanel) | owner wants identical behaviour |
| D23 | Map view created once and only hidden or shown; pop-out uses one WebView2 at a time | returning to the map took seconds |
| D24 | Result sets from Explorer and Monitoring are independent, removable, not persisted | results must not disappear automatically |
| D25 | Marker styles: Excel circle, my points pin, Explorer amber diamond with magnifier, Monitoring rose square with bell | sources distinguishable at a glance |
| D26 | Proximity to a layer computed in C# (haversine); same geometry rule as radius circles | one geometry rule |
| D27 | Paging detects SQL Server version and compatibility; read-only ROW_NUMBER() fallback when OFFSET/FETCH is unsupported; no database setting is changed | support older compatibility levels without changing the database |
| D28 | Saved SQL password: opt-in per profile, DPAPI CurrentUser (ADR-0003) | owner wants no repeated prompts; encrypted and never shown |
| D29 | No password popup; password typed in the profile form, masked after save | owner: one form, no separate windows |
| D30 | Export: .xlsx up to about 2 M cells, else split CSV (500 k rows or 100 MB per part); never overwrite; cleanup on cancel | big workbooks are slow and heavy |
| D31 | Monitoring section name used everywhere in the UI | owner request |
| D32 | One shared MapSendPanel (same as D22, kept for traceability of the UI decision) | owner wants identical behaviour |
| D33 | Map view created once (same as D23, kept for traceability) | returning to the map took seconds |
| D34 | Result sets independent and not persisted (same as D24) | owner: results must not disappear |
| D35 | Marker styles (same as D25) | sources distinguishable |
| D36 | Excel layers and monitor layers share the coordinate pipeline; proximity in C# | one geometry rule |
| D37 | Map tab state: the Excel tab is the single source of truth (ADR-0005) | fixes the crash loop |
| D38 | PENDING: the test project targets net10.0; Wpf targets net10.0-windows, so view models cannot be tested. Options: (a) retarget the test project to net10.0-windows; (b) keep view-model logic in Domain and Application (ADR-0007) and test there. Recommended: (b) first, (a) only if a rule must stay in the view model | owner decision needed |

Note: D21, D31 and D32 to D35 overlap with earlier entries. Keep one truth: when a decision changes, edit its row and the ADR it points to.
