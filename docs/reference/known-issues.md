# Known Issues and Risks

Known limits that are not bugs in the code but must be remembered. Migrated from `docs/KNOWN_ISSUES.md` (superseded). Each item says what to do about it.

## Build and packages
- NuGet versions are floating (`8.*`, `6.*`, ...). Pin exact versions after the first successful restore (R7). Pkcs 9.0.11 has an advisory (transitive, from SqlClient).
- Some code was never compiled by the AI. The owner builds and tests each slice (lesson N13).

## SQL and performance
- `LIKE '%text%'` (Contains, EndsWith) cannot use indexes; on huge tables it can hit the 30 s timeout. Prefer "Starts with", "=" or ranges.
- Deep OFFSET paging (only for tables without a usable primary key) gets slower with depth.
- Huge text or binary cells are read fully before they are cut for display (cell text is cut at 300 characters).
- A page-size change applies on the next Run; the current paging keeps its snapshot.
- Filter values use invariant culture: dates like 2025-01-31, decimals with a dot.
- Very large table lists load item by item; virtualisation is on but not benchmarked.
- Connecting: `localhost` uses Shared Memory; `127.0.0.1` needs TCP/IP enabled in SQL Server Configuration Manager (restart the SQL service). Named instance with IP: `127.0.0.1\SQLEXPRESS`; with port: `127.0.0.1,1433` (lesson L3).

## Monitoring (details in `features/MONITORING.md` once moved)
- Monitors run only while the app is open and connected.
- `SELECT MAX(column)` on an un-indexed column of a huge table scans it (30 s timeout). Choose an indexed watermark column.
- Numeric (identity) watermarks can miss rows committed out of order; date watermarks are protected by a 30 s settle time.
- Updated rows are detected only if the watermark column changes on update.
- Alert history is kept in memory only.

## UI and Windows
- Icons use Segoe Fluent Icons and MDL2 glyph codes. Check each glyph on Windows 10, including the monitoring glyphs `E823` (sidebar) and `E7BA` (toast).
- Animations ignore the Windows reduce-motion setting (audit S13).
- Saved connections are plain JSON with no secrets (SQL passwords are in the DPAPI vault, ADR-0003).

## Map
- WebView2 runtime is required.
- OSM PBF is not rendered directly (converter planned, D17).
- GMDB support was verified only against the owner's sample file.

## Lessons that are also in `reference/lessons-learned.md`
- A view (`x:Class`) without a code-behind that calls `InitializeComponent()` renders blank (L1).
