# Current State

Branch: feature/geosent-live-map (the branch the owner tests)
Commit: head of this branch (see `git log --oneline -5`).
Task: refactor slices 1 to 3 (rules moved from view models to Domain); documents kept in step.

## Verified by the owner
- Build and tests passed after slice 1 (filter operator rules).
- Map tab loop fixed (8416448); map pop-out window works.

## Done (NOT compiled yet; owner to build and test)
- Slice 2: `Domain/FilterRequestBuilder.cs` + `FilterRequestBuilderTests` (9 cases); `TableQueryViewModel.TryBuildRequest` uses it.
- Slice 3: `Domain/MapSendRules.cs` (`TryCoordinate`, `TruncateText`, `BuildDetails`, `KeysetPaging`) + `MapSendRulesTests`, `KeysetPagingTests`; `TableQueryViewModel` delegates to them.
- Note: slice 3 moved the RULES to Domain. The separate map-send coordinator class with I/O is NOT done; it is the next step if the owner wants it.

## Owner test plan (after build)
1. `dotnet build FastDbExplorer.slnx`, then `dotnet test`. Send the count and any failing test names.
2. Explorer filters: incomplete row shows the usual message; IS NULL ignores a typed value; BETWEEN uses both values; OR logic.
3. Long tables: next page after 10,000 rows still works (keyset).
4. Map: send selected rows; send all rows; the detail columns and names appear as before; a row with bad coordinates is skipped.

## Remaining (in order)
1. Owner build and test results; fix any compile error reported.
2. Decision D38 (test target) before any view-model test.
3. Optional next slice: the map-send coordinator (I/O) and the paging controller as classes with tests.
4. Map-tab rule (N1) to Domain (audit S4), when the owner agrees.
5. R2: presentation data out of Domain (S6). R5: one registration per interface (S8). R7: pin NuGet versions.
6. Security review (S10), structured logs (S11), per-screen states (S12), reduced motion (S13).

## Known issues
See `reference/known-issues.md` and `architecture/standards-audit.md`.

## Next recommended action
Owner: build and test; send the result.
