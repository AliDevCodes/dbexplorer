# Decision Log (small decisions)

Small and medium decisions. Significant ones have an ADR in `docs/decisions/`. Numbers are never reused (see `changes/doc-update-protocol.md`).

Sources: D1 to D19 from the original master context (map decisions D13 to D19 included); D20 to D26 from the former `DECISIONS.md` (Excel coordinate layers); D27 adaptive paging; D28 to D38 from the 2026-10 phase.

## Core (D1 to D12)
| # | Decision | Why |
|---|---|---|
| D1 | SQL Server first; WPF; .NET 10; MVVM | initial scope |
| D2 | Auth: Windows and SQL | owner choice |
| D3 | UI in Persian, right-to-left, automatic light and dark; WPF with custom styles, no UI package | owner choice; modern look without a UI library |
| D4 | Recent connections saved without passwords (later: D28, ADR-0003) | security |
| D5 | `TrustServerCertificate` on by default (internal self-signed certificates); user can turn it off | usability on internal servers |
| D6 | System databases (id 1 to 4) hidden | the tool is for user data |
| D7 | Row counts from `sys.partitions` (approximate); no `COUNT(*)` | no table scans |
| D8 | Paging hybrid: OFFSET first, keyset for depth; "has next" = pageSize + 1 | owner choice; speed on huge tables |
| D9 | No primary key: user picks the order column; OFFSET only; ties not guaranteed stable | keyset on a non-unique column skips rows |
| D10 | Default isolation (READ COMMITTED); no NOLOCK | NOLOCK can return wrong data |
| D11 | Filter only common types (numbers, dates, text, guid, bit) | typed parameters keep indexes usable |
| D12 | Floating NuGet versions until the first restore; pin later (R7) | no network in the AI sandbox |

## Map (D13 to D19)
| # | Decision | Why |
|---|---|---|
| D13 | MapLibre GL JS in WebView2 (ADR-0004) | best vector-tile support; tiles served from the file |
| D14 | Packages Microsoft.Data.Sqlite and Microsoft.Web.WebView2 approved by owner delegation | needed for MBTiles and rendering |
| D15 | MapLibre files downloaded by `setup-map-assets.cmd`, not committed; `publish.cmd` calls it if missing | small repository; no internet in the AI sandbox |
| D16 | GMDB = GMap.NET cache: SQLite `Tiles` and `TilesData`, raster, XYZ y (no flip); provider with most tiles used; MBTiles-layout fallback | verified against the owner's sample file |
| D17 | OSM PBF is not rendered directly; the UI explains; a converter is a later slice | raw OSM is not tiles; country files are huge |
| D18 | Labels for OpenMapTiles-style vector files; glyphs and RTL plugin are local files; name priority `name:fa`, `name_en`, `name:latin`, `name` | owner saw a map without names; no internet at run time |
| D19 | Tile requests from Web Workers intercepted with the all-kinds filter; gzip tiles unzipped in C# | MapLibre fetches tiles inside workers |

## Excel coordinate layers (D20 to D26)
| # | Decision | Why |
|---|---|---|
| D20 | ExcelDataReader 3.* for `.xlsx` and `.xls` (read-only, streaming). Rejected: ClosedXML (no `.xls`), NPOI (heavier, no write needed). Pin the version after the first restore | one package for both formats; owner approved the dependency |
| D21 | Header contract: first non-blank row of the first sheet; exact names Name, Latitude, Longitude, case-insensitive, spaces ignored; any order; extra columns ignored; no auto-detection yet | predictable import; detection is a later phase |
| D22 | Lenient rows, strict files: invalid rows skipped and reported (code, row, column); a file-level problem gives no layer; at most 1,000 issues returned; rows without a name are invalid | the user keeps the valid data; errors are visible |
| D23 | Coordinates: WGS84 decimal degrees only; text may use Persian and Arabic digits, the Arabic decimal separator and one decimal comma; DMS is rejected until its own phase; NaN and Infinity rejected | the owner works in Persian Excel |
| D24 | Radius always in metres, 0 to 20,037,508 (half the Earth's circumference), default 0 = none; unit conversion only at the UI edge | one unit in model, store and geometry |
| D25 | One JSON file per layer (`coordinate-layers\{id}.json`), version field, temp-file and move writes; unreadable, newer or rule-breaking files are skipped and kept, never deleted | small saves; failures isolated |
| D26 | No SQL is involved in layers, so the read-only rules are unaffected; nothing sensitive is stored | scope boundary |

## Paging (D27)
| # | Decision | Why |
|---|---|---|
| D27 | Adaptive read-only paging: detect SQL Server version and database compatibility level with `SELECT`; use `OFFSET/FETCH` when supported, otherwise the `ROW_NUMBER()` query; never alter compatibility or schema; if neither works, show the detected version and the minimum required | support older servers without changing the database |

## 2026-10 phase (D28 to D38)
| # | Decision | Why |
|---|---|---|
| D28 | Saved SQL password: opt-in per profile, DPAPI CurrentUser (ADR-0003); supersedes "no passwords on disk" for this one case | owner wants no repeated prompts; encrypted and never shown |
| D29 | No password popup; the password is typed in the profile form and masked after save | owner: one form, no separate windows |
| D30 | Export: `.xlsx` up to about 2 million cells, else split CSV (500,000 rows or 100 MB per part); never overwrite; cleanup on cancel or error | big workbooks are slow and heavy |
| D31 | Monitoring is the section name everywhere in the UI | owner request |
| D32 | One shared "show on map" panel for Explorer and Monitoring (`MapSendPanel`) | owner wants identical behaviour |
| D33 | The map view is created once and only hidden or shown; pop-out uses one WebView2 at a time (ADR-0004) | returning to the map took seconds |
| D34 | Result sets from Explorer and Monitoring are independent, removable, and not persisted | results must not disappear by themselves |
| D35 | Marker styles: Excel circle; my points pin; Explorer amber diamond with magnifier; Monitoring rose square with bell | sources distinguishable at a glance |
| D36 | Proximity to a layer is computed in C# (haversine) with the same sphere as the radius circles (ADR-0006) | one geometry rule |
| D37 | Map tab state: the Excel tab is the single source of truth (ADR-0005) | fixes the endless loop |
| D38 | PENDING: the test project targets net10.0, Wpf targets net10.0-windows, so view models cannot be tested. Options: (a) retarget the test project to net10.0-windows; (b) move rules to Domain and Application and test there (ADR-0007). Recommended: (b) first | owner decision needed |
