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
| `guides/` | how to operate the app: runbook (build, test, run, ship, crash log) | "How do I run, build or ship it?" |
| `release/` | release history (`CHANGELOG.md`) | "What changed in each version?" |

## Where to start
1. `junior-guide.md` — orientation for a new developer or AI.
2. `architecture/system-map.md` — the system in one page.
3. `features/index.md` — the feature you need.
4. `changes/change-map.md` — where your change belongs.
5. `changes/doc-update-protocol.md` — which documents to update after your change.

## Status
The reorganisation of 2026-10-25 is complete. The root of `docs/` contains only `README.md` and `junior-guide.md`, and the seven folders above.

Files removed during the reorganisation (their content is in the new location; git keeps the history): `ARCHITECTURE.md`, `CODE_MAP.md`, `PROJECT_MASTER_CONTEXT.md`, `DECISIONS.md`, `KNOWN_ISSUES.md`, `PROJECT_BRAIN.md`, `CURRENT_STATE.md`, `PROGRESS.md`, `FEATURES.md`, the nine feature pages, `RUNBOOK.md`, `CHANGELOG.md`, `REVIEW_2026-10.md`.
