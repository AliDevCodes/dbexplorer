# Change Manifest

Task: adopt the project-brain operating model (documentation only).

## Modified Files
| File | Reason |
|---|---|
| CLAUDE.md | Entry point now points to `.ai/` and `docs/architecture`, keeps the short hard rules. |
| docs/PROJECT_BRAIN.md | Reduced to a pointer, so there is one source of truth. Content moved to the structure below. |

## Added Files
| File | Reason |
|---|---|
| .ai/project-context.md | Project identity, technology, constraints, validation commands. |
| .ai/current-state.md | Where the work stands and what the owner must test. |
| .ai/active-task.md | This task, its acceptance criteria and non-goals. |
| .ai/change-manifest.md | This file: what changed and why. |
| .ai/repo-snapshot.md | Routing summary of the repository for a new AI. |
| .ai/handoff.md | Cross-session handoff. |
| .ai/state.json | Machine-readable state. |
| docs/architecture/system-map.md | System, entry points, external systems, flows. |
| docs/architecture/module-map.md | Modules, responsibilities, locations, owned data, tests. |
| docs/architecture/dependency-map.md | Allowed and forbidden dependency edges, with current violations. |
| docs/features/index.md | Feature map: entry point, logic, persistence, UI, tests, docs; plus manual checklists. |
| docs/changes/change-map.md | Routing for common changes; protected boundaries. |
| docs/decisions/README.md | ADR index and template. |
| docs/decisions/ADR-0001-layered-architecture.md | Layers and dependency direction. |
| docs/decisions/ADR-0002-read-only-by-design.md | Read-only rule and its enforcement. |
| docs/decisions/ADR-0003-dpapi-optional-sql-passwords.md | Opt-in encrypted SQL password storage. |
| docs/decisions/ADR-0004-map-webview2-local-tiles.md | WebView2 + MapLibre, tiles by request interception. |
| docs/decisions/ADR-0005-single-source-for-ui-state.md | Two-way mirror properties forbidden (StackOverflow lesson). |
| docs/junior-guide.md | Start here for a new developer or AI. |

## Deleted Files
None.

## Not Changed Intentionally
- All code under `src/` and `tests/`.
- `docs/REVIEW_2026-10.md` (its findings and phases remain the refactor plan; referenced from this structure).
- Legacy docs `docs/ARCHITECTURE.md`, `docs/CODE_MAP.md`, `docs/PROJECT_MASTER_CONTEXT.md`.

## Validation
- Documentation review by the AI: consistency with the review and the current state.
- Owner review: pending.

## Commit
Branch: feature/geosent-live-map
Commit: this commit (documentation only).
