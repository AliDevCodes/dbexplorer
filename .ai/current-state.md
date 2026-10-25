# Current State

Branch: feature/geosent-live-map (the branch the owner tests)
Commit: head of this branch (see `git log --oneline -5`).
Task: refactor slice 2 done in code (query filter rules moved to Domain); docs reorganised.

## Verified by the owner
- Build and tests passed before this slice (after slice 1, the filter operator rules).
- Map tab loop fixed (8416448); map pop-out window works.

## Done since the last verified state (NOT compiled, owner to build)
- Domain/FilterRequestBuilder.cs and tests/FilterRequestBuilderTests.cs (new rule, 9 test cases).
- TableQueryViewModel.TryBuildRequest uses FilterRequestBuilder (only the filter section changed; the rest of the file was copied from the branch).
- Docs: reorganised `docs/` into folders; decision log corrected; change map updated.

## Remaining (in order)
1. Owner: `dotnet build FastDbExplorer.slnx` and `dotnet test`. Send the test count and any failure name.
2. Owner: manual check of Explorer filters: incomplete row message, IS NULL, BETWEEN, OR logic.
3. Slice 3: split the rest of `TableQueryViewModel` (paging controller, map-send coordinator) with Domain/Application tests.
4. Owner decision D38 (test target) before any view-model test.
5. Map-tab rule (N1) to Domain (audit S4) when the owner agrees.
6. R2: presentation data out of Domain (S6). R5: one registration per interface (S8). R7: pin NuGet versions.
7. Security review (S10), structured logs (S11), per-screen states (S12), reduced motion (S13).

## Known issues
See `reference/known-issues.md` and `architecture/standards-audit.md`.

## Next recommended action
Owner: build and test; send the result. Then approve slice 3.
