# Active Task

## Goal
Create the complete project brain for FastDbExplorer, in the structure of the senior-engineer operating system (v2.0.0): `.ai/` for cross-session state and `docs/architecture`, `docs/features`, `docs/changes`, `docs/decisions` for navigation and decisions. Make the repository understandable and safely changeable by a new AI or a junior developer without a verbal briefing.

## Acceptance Criteria
- [x] `.ai/` contains project-context, current-state, active-task, change-manifest, repo-snapshot, handoff, state.json.
- [x] `docs/architecture/` contains system-map, module-map, dependency-map.
- [x] `docs/features/index.md` maps every feature to entry point, business logic, persistence, UI, tests, docs.
- [x] `docs/changes/change-map.md` routes the common kinds of change to the smallest scope.
- [x] `docs/decisions/` contains an index and ADRs for the decisions that constrain future work.
- [x] `docs/junior-guide.md` lets a junior developer start without help.
- [x] `CLAUDE.md` and `docs/PROJECT_BRAIN.md` point to this structure (no second source of truth).
- [x] Every claim that was not verified against the code is marked `Needs verification`.

## Non-Goals
- No code change, no refactor, no dependency change (those are phases R1 to R7, each needing approval).
- No rewrite of legacy docs (`docs/ARCHITECTURE.md`, `docs/CODE_MAP.md`, `docs/PROJECT_MASTER_CONTEXT.md`). They are marked legacy instead.
- No ZIP, no publish.

## Relevant Feature
All features; see `docs/features/index.md`.

## Relevant Change Map
`docs/changes/change-map.md` (documentation change: no code path).

## Planned Files
See `.ai/change-manifest.md`.

## Risks / Assumptions
- Known: the project-brain v1 and the review already exist and are partly stale; this task reconciles them.
- Assumption: the file inventory in `docs/architecture/module-map.md` reflects the branch at the time of writing. The owner can confirm with the discovery commands in `docs/junior-guide.md`.
- Risk: documentation can drift from the code again. Mitigation: the rule in `docs/changes/change-map.md` ("if you change X, update Y") and the checklist in each slice report.

## Validation Plan
- Read each new file once for contradictions with `.ai/current-state.md` and the review.
- Check that every link path exists in the repository (paths listed in the docs).
- Owner reads `docs/junior-guide.md` and reports where he got lost.
