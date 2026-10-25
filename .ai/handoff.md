# AI Handoff

## Current Goal
Keep FastDbExplorer stable and changeable, following the senior-engineer standards. Domain first: move business rules out of the view models, one slice at a time, and keep the documents in step with each change.

## Repository State
Branch: feature/geosent-live-map. Commit: head of the branch (see `.ai/current-state.md`).

## Completed
- Filter operator rules in Domain (5818b9d), tested by the owner (build and tests passed).
- Documentation structure and index (`docs/README.md`), update procedure (`docs/changes/doc-update-protocol.md`), decision log corrected, known issues consolidated.
- Removed: `docs/ARCHITECTURE.md`, `docs/CODE_MAP.md`, `docs/PROJECT_MASTER_CONTEXT.md`; in this change also `docs/DECISIONS.md`, `docs/KNOWN_ISSUES.md`, `docs/PROJECT_BRAIN.md` (content migrated).

## In Progress
- Documentation reorganisation step 2 (feature pages and guides). See the table in `docs/README.md`.

## Remaining
See `.ai/current-state.md` → Remaining.

## Changed Files (this change)
- Docs: `docs/README.md` (new), `docs/changes/doc-update-protocol.md` (new), `docs/reference/decision-log.md` (corrected), `docs/reference/known-issues.md` (new), `.ai/current-state.md`, `.ai/handoff.md`.
- Removed: `docs/DECISIONS.md`, `docs/KNOWN_ISSUES.md`, `docs/PROJECT_BRAIN.md`.

## Validation
- Owner: build and tests passed (count not sent).
- AI: static check of the new documents against the code (decision numbers, file names).

## Known Issues
See `reference/known-issues.md` and `architecture/standards-audit.md`.

## Decisions
See `reference/decision-log.md` (D1 to D38) and `decisions/` (ADR-0001 to ADR-0007).

## Assumptions
- The feature pages (`CREDENTIALS.md`, `MONITORING.md` and others) still describe the current behaviour; they are checked when they are moved.

## Next Action
Slice 2 (domain first): map-tab rule to Domain with a test. Then the feature pages move, one folder step at a time.
