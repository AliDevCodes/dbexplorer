# Current State

Branch: feature/geosent-live-map (the branch the owner tests)
Commit: head of this branch (see `git log --oneline -5`).
Task: reorganise `docs/`, make the update procedure explicit, correct the decision numbering; first refactor slice (filter rules to Domain) built and tested by the owner.

## Verified by the owner (this phase)
- `dotnet build FastDbExplorer.slnx`: passed, no problem reported.
- `dotnet test`: passed, no problem reported (exact count not sent; the new `FilterOperatorRulesTests` are included).
- Map tab loop fixed (8416448); owner confirmed the crash is solved.
- Map pop-out window works (owner confirmed).

## Completed in this phase
- Filter operator rules moved to Domain (5818b9d) with tests.
- Standards read and applied; audit in `architecture/standards-audit.md`.
- Legacy content migrated; ADR-0006 and ADR-0007 written.
- Documentation reorganised into folders with an index (`docs/README.md`) and an update procedure (`changes/doc-update-protocol.md`).
- Decision numbering corrected: D20 to D26 are the Excel decisions from the former `DECISIONS.md`; the earlier duplicates were removed.

## Remaining (in order)
1. Slice 2 (domain first): move the map-tab rule (N1) from `MapViewModel` into Domain, with a test (audit S4).
2. Slice 3: split `TableQueryViewModel`, starting with a Domain query-filter builder (S5).
3. Owner decision D38 (test target) before any view-model test.
4. Documentation: move the feature pages into `features/`, `REVIEW_2026-10.md` into `architecture/`, `RUNBOOK.md` into `guides/`, `CHANGELOG.md` into `release/`; read and delete `CURRENT_STATE.md`, `PROGRESS.md`, `FEATURES.md`.
5. R2: move presentation data out of Domain (S6). R5: one registration per interface (S8). R7: pin NuGet versions.
6. Security review (S10), structured logs (S11), per-screen states (S12), reduced motion (S13).

## Known issues
See `reference/known-issues.md` and `architecture/standards-audit.md`.

## Next recommended action
Slice 2 (domain first), after the owner confirms the build and test count if needed.
