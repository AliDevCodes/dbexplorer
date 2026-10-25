# Feature: Shared "show on the map" panel (Explorer and Monitoring)

> Moved from `docs/MAP_SEND_UX.md` on 2026-10-25. Content kept. Status: owner reports build and tests pass; the checklist is for the owner to confirm.

## Why
The Explorer results and the Monitoring result preview do the same thing (pick the latitude/longitude columns, select rows, send them to the map) but looked and behaved differently:

| | Explorer (before) | Monitoring (before) |
|---|---|---|
| Column labels | "عرض جغرافیایی" | "ستون عرض جغرافیایی" |
| Button text | "نمایش ردیف‌های انتخاب‌شده روی نقشه" | "ارسال ردیف‌های انتخاب‌شده به نقشه" |
| Button position | above the table, beside the columns | below the table, bottom corner |
| Details of a point | collapsed expander | always visible |
| Selected count | shown | not shown |
| Button disabled | no reason given | no reason given |

## What it is now
One control, `Views/MapSendPanel` (+ `MapSendStrings`), used in both places, directly above the table:
1. **Title + selection chip:** globe badge, "نمایش روی نقشه", and "N ردیف انتخاب‌شده" (or a muted "ردیفی انتخاب نشده").
2. **Two column pickers** ("ستون عرض جغرافیایی", "ستون طول جغرافیایی") with their labels above them.
3. **One primary button** that says what it will do: "ارسال به نقشه" / "ارسال 4 ردیف به نقشه". It is on only when rows are selected, both columns are chosen and the selection holds valid coordinates.
4. **A hint under it when it is off:** no rows selected, no columns chosen, or no valid coordinates in the selection.
5. **Details of each point** in a collapsed expander ("· 2/8", with a one-line explanation) in both screens.

Each screen binds its own members (columns, latitude/longitude, detail columns, selected count, send command); the rules and the wording live in the control, so they cannot drift apart.

## Files
New: `Views/MapSendPanel.xaml(.cs)`, `Localization/MapSendStrings.cs`, `ViewModels/MonitoringViewModel.MapSend.cs`.
Changed: `Views/TableQueryView.xaml` (Explorer), `Views/MonitoringView.xaml` and `.xaml.cs`. The Explorer keeps its own "Excel export" button above the panel (a different action: all rows).

## Manual test checklist (owner)
1. Build + `dotnet test`.
2. Explorer: run a query on a table with coordinates. Nothing selected: muted text, the button is off and the hint explains how to select rows.
3. Select 4 rows: chip shows "4 ردیف انتخاب‌شده", the button reads "ارسال 4 ردیف به نقشه" and is on; click: the Map section opens with the diamonds.
4. Clear one coordinate column: the hint changes.
5. Open the details expander: the checkboxes work and the counter follows; more than 8 is refused.
6. Monitoring: open a check result. The SAME panel, same texts, same behaviour; sending shows the rose squares with a bell.
7. Close the preview and open another one: the selection count starts from zero.

## Observations (not changed)
- The Explorer grid shows dates in the Persian calendar, the Monitoring preview shows ISO dates (decision pending).
- "Excel export" (Explorer button) and the report links of Monitoring alerts are different features and stay separate.
