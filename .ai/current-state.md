# Current State

Branch: feature/geosent-live-map (the branch the owner tests)
Commit: head of this branch (see `git log --oneline -5`).
Task: standards audit and migration of the legacy documentation; first refactor slice (filter rules to Domain).
Status: documentation migrated; legacy deletion in progress; code slice 1 done (not compiled).

## Completed (this phase)
- Map tab loop fixed (8416448): the map-layers tab is a read-only mirror of the Excel tab. Owner confirmed.
- Filter null fixed (15038c3); rule moved to Domain (5818b9d, FilterOperatorRules + tests). Owner test pending.
- Map window crash fixes (4f427d8 and earlier). Owner confirmed the pop-out window works.
- Shared "show on map" panel; smart export; Monitoring renamed in the UI.
- Project brain structure; ADR-0001 to ADR-0007; reference docs; standards audit (`docs/architecture/standards-audit.md`).
- Duplicate test files reverted (8a52610, c66f747); test inventory corrected (7f2e1fd).

## Remaining (in order)
1. Owner builds and runs the tests; reports the result of `FilterOperatorRulesTests` and the existing suite.
2. Slice 2: move the map-tab rule (N1) from `MapViewModel` to Domain with a test (S4).
3. Owner decision D38: test target for view models (recommended: Domain first, as ADR-0007).
4. Slice 3: split `TableQueryViewModel` (S5) starting with `QueryFilterBuilder` (testable).
5. Refactor R2: move presentation data out of Domain (S6).
6. R5: one registration per interface (S8).
7. R7: pin NuGet versions; resolve the Pkcs advisory.
8. Security review written (S10); structured logs (S11); per-screen states (S12); reduced motion (S13).
9. OSM PBF converter (not started).

## Known issues (see the standards audit for the full list)
- View models and views have no automated tests (S2).
- `MapViewModel` and `TableQueryViewModel` are large (S5).
- Domain holds presentation data and a JSON attribute (S6).
- Pkcs 9.0.11 advisory (transitive).
- CSV from Excel may open as one column with a comma delimiter on some regional settings.
- Explorer shows Persian dates, Monitoring shows ISO dates (decision pending).
- `docs/REVIEW_2026-10.md` names the three legacy files in its scope; it is a dated snapshot and was not rewritten.

## Last validation
- Owner full run: 204 passed, 0 failed, before the latest slices.
- Since then, not run: `FilterOperatorRulesTests` (new), and the changes of 15038c3 and 8416448 (owner reports them as fixed, tests not re-run).
- AI validation: static review only; no build.

## Next recommended action
Owner: build and run `dotnet test`; send the summary (count, failures with names). Then approve slice 2.
