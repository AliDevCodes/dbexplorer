# AI Handoff

## Current Goal
Keep FastDbExplorer stable and safely changeable. Current focus: owner testing of the latest UI slices, then the approval of refactor phase R1 (ViewModel tests).

## Repository State
Branch: feature/geosent-live-map
Commit: see `.ai/current-state.md`
Git status: the documentation commit of this task (docs only).

## Completed
- Project brain structure created (`.ai/`, `docs/architecture`, `docs/features`, `docs/changes`, `docs/decisions`, `docs/junior-guide.md`).
- Map tab loop fixed, filter null fixed, map window crash fixes, shared map send panel, smart export, Monitoring rename.

## In Progress
- Owner testing of: filter row fix, map pop-out, export, Monitoring.

## Remaining
- Phase R1 (ViewModel tests), then R2 to R7 (see `docs/REVIEW_2026-10.md`), each after approval.
- Owner decisions: date format, CSV delimiter, demo login gate.
- Pin NuGet versions; resolve Pkcs advisory.

## Changed Files
See `.ai/change-manifest.md` (documentation only in this task).

## Validation
- AI: static review of the documentation against the review and the branch listing.
- Owner: tests 204/204 at the last full run; latest UI slices pending.

## Known Issues
See `.ai/current-state.md`.

## Decisions
See `docs/decisions/` (ADR-0001 to ADR-0005).

## Assumptions
- The branch file listing used for the module map is accurate at the time of writing (Needs verification with `git ls-files`).
- Legacy docs are not authoritative where they contradict the code.

## Next Action
1. Read `docs/junior-guide.md`, then `.ai/current-state.md`.
2. Ask the owner for the status of the latest test results before changing code.
3. Do not start R1 without owner approval.
