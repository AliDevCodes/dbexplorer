# docs/ — where to find things

Start with `junior-guide.md`. This page says what each folder and file is for, and where a new document must go.

## Folders (the rule for each)
| Folder | What goes here | Question it answers |
|---|---|---|
| `architecture/` | how the system is built: system, modules, dependencies, runtime flows, reviews, standards audit | "How is it built and where does X live?" |
| `decisions/` | Architecture Decision Records (ADR-0001 onward) | "Why is it built this way?" |
| `features/` | one page per feature: behaviour, limits, owner test procedure; `index.md` is the feature map | "What does feature Y do and what can go wrong?" |
| `changes/` | change routing (`change-map.md`) and the document update procedure (`doc-update-protocol.md`) | "Where do I change Z, and which docs must I update?" |
| `reference/` | facts and logs: decision log, lessons learned, map formats, known issues | "What is the rule or the fact about W?" |
| `guides/` | how to operate the app: runbook, build and release | "How do I run, build or ship it?" |
| `release/` | release history (`CHANGELOG.md`) | "What changed in each version?" |

## Where to start
1. `junior-guide.md` — orientation for a new developer or AI.
2. `architecture/system-map.md` — the system in one page.
3. `features/index.md` — the feature you need.
4. `changes/change-map.md` — where your change belongs.
5. `changes/doc-update-protocol.md` — which documents to update after your change.

## Current files (status of the reorganisation)
| File | Status | Target |
|---|---|---|
| `junior-guide.md` | active | stays (entry point) |
| `architecture/*` | active | in place |
| `decisions/*` | active | in place |
| `features/index.md`, `features/coordinate-layers.md` | active | in place |
| `changes/*` | active | in place |
| `reference/*` | active | in place |
| `REVIEW_2026-10.md` | active (dated review) | move to `architecture/review-2026-10.md` |
| `CREDENTIALS.md`, `DATETIME_PICKER.md`, `DEMO_LOGIN.md`, `EXPORT_AND_LAYOUT.md`, `MAP_UI.md`, `MAP_TOOLS.md`, `MAP_SEND_UX.md`, `MONITORING.md`, `NAV_REDESIGN.md` | active feature pages | move to `features/` (next step) |
| `RUNBOOK.md` | active | move to `guides/runbook.md` |
| `CHANGELOG.md` | active history | move to `release/CHANGELOG.md` |
| `CURRENT_STATE.md`, `PROGRESS.md`, `FEATURES.md` | superseded; content migrated (see `features/coordinate-layers.md` and `features/index.md`) | deleted in this change |

The reorganisation proceeds in steps so that no content is lost and no link breaks. Each step names the files it moves.
