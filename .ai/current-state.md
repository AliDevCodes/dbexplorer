# Current State

Branch: feature/geosent-live-map (the branch the owner tests)
Commit: head of this branch at the time of writing (see `git log --oneline -5`). Last code commits: 15038c3 (filter row null fix), 8416448 (map tab loop fix), 1d1a132 (map tab loop fix, first attempt), 626660a (brain docs v1).
Task: adopt the project-brain operating model (documentation only).
Status: documentation in progress. No code changed in this task.

## Completed
- Map tab state loop (StackOverflow) fixed: the map-layers tab is a read-only mirror of the Excel tab (commit 8416448). Owner confirmed the bug is solved.
- Filter row NullReference fixed: null operator reverts to the first operator (commit 15038c3). Owner has not yet confirmed.
- Map window crashes addressed earlier: deferred WebView2 disposal, window waits for disposal, one WebView2 at a time (commits 4f427d8 and earlier). Owner to verify.
- Shared "show on map" panel for Explorer and Monitoring.
- Smart export (xlsx up to ~2 M cells, else split CSV) with tests.
- Monitoring renamed in the UI.
- Project-brain v1 (docs/PROJECT_BRAIN.md, docs/REVIEW_2026-10.md) committed in 626660a. This task supersedes it (see handoff).

## Remaining
- Owner tests the latest slices (filter, tabs, map pop-out, export, monitoring) and reports results.
- Phase R1 of the refactor plan: ViewModel tests (waiting for owner approval).
- Decisions pending from the owner: date format (Persian or Gregorian), CSV delimiter, demo login gate (keep or remove).
- Pin NuGet versions and resolve the Pkcs advisory (R7).
- OSM PBF converter (not started).

## Known Issues
- ViewModels and Views have no automated tests (see docs/REVIEW_2026-10.md F5).
- MapViewModel and TableQueryViewModel are large (F1, F2).
- Domain contains presentation data and a JSON attribute (F6, F7, F8).
- Pkcs 9.0.11 advisory (transitive).
- CSV from Excel may open as one column with a comma delimiter on some regional settings.
- Explorer shows Persian dates, Monitoring shows ISO dates (decision pending).

## Last Validation
- Owner full run of tests: 204 passed, 0 failed (before the latest UI slices).
- Latest UI slices: owner reported the tab crash is solved; the filter crash fix is awaiting owner test.
- AI validation: static review only. No build run by the AI.

## Next Recommended Action
1. Owner confirms the status of the filter fix and the pop-out/export slices.
2. Owner approves Phase R1 (ViewModel tests) or asks for another fix first.
