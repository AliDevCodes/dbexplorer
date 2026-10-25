# Document update protocol (for AI and humans)

After every code change, update the documents that the change makes wrong. This is part of the definition of done. A change is not finished while its documents are stale (senior-engineer `ai-handoff` and `ai-documentation`).

## Step 1 — Read what the change touched
List the files you changed and their module (`architecture/module-map.md`).

## Step 2 — Update the table
| If the change... | Update |
|---|---|
| adds, removes or renames a file | `architecture/module-map.md` (module row and tests column) |
| adds a dependency between modules, or a forbidden edge is crossed | `architecture/dependency-map.md` (and a violation row) |
| changes how a flow runs (start-up, connect, query, export, map, monitor) | `architecture/runtime-flows.md` |
| changes what a feature does, its limits or its tests | `features/index.md` row, and the feature page |
| changes where a kind of change belongs | `changes/change-map.md` |
| adds a message between C# and the map page | `reference/map-formats.md` (page contract) |
| makes a significant decision (structure, dependency, security, persistence format) | a new ADR in `decisions/` and the index in `decisions/README.md` |
| makes a small decision | `reference/decision-log.md` (next free number, never reuse a number) |
| fixes a bug or a mistake that could happen again | `reference/lessons-learned.md` (new N-number, with the rule) |
| closes or opens a finding of the audit | `architecture/standards-audit.md` (status column) |
| adds a known limit or risk | `reference/known-issues.md` |
| is a slice (one approved change) | `.ai/change-manifest.md`, `.ai/current-state.md`, `.ai/handoff.md`, `.ai/state.json` |

## Step 3 — Check claims before writing them
Before writing that something exists, is tested or is missing, check the folder or the code. A document that is wrong is worse than no document (N14).

## Step 4 — Keep one truth
Do not copy a fact into a second file. Link to the owner file. If two files disagree, fix the one that is wrong and mention the conflict in `.ai/current-state.md`.

## Step 5 — Record the state honestly
In `.ai/current-state.md` and `.ai/handoff.md`, write what is done, what is verified (by whom), what is not compiled, and the next action. Never mark work as complete when the handoff does not say so.

## Numbering rules
- Decisions (D-numbers) and lessons (N-numbers) are never reused. A changed decision gets a new number that supersedes the old one.
- ADR numbers are sequential and never reused.

## Checklist before the commit
1. Table above applied. 2. Claims checked against the code. 3. Links to moved or deleted files fixed (search the docs for the old name). 4. `.ai/` state updated. 5. Commit message says what changed in the docs.
