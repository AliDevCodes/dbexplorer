# Architecture Decision Records

Each significant decision has an ADR: problem, options, decision, consequences, re-evaluation triggers. Do not re-open an accepted ADR without new evidence; supersede it with a new ADR.
The decision log for smaller decisions (D1 to D38) is in `docs/reference/decision-log.md`.

| ADR | Title | Status |
|---|---|---|
| 0001 | Layered architecture (Domain, Application, Infrastructure, Wpf) | accepted |
| 0002 | Read-only by design | accepted |
| 0003 | Opt-in encrypted SQL passwords (DPAPI) | accepted; supersedes the old "no passwords on disk" rule for this one case |
| 0004 | Map rendering with WebView2 and MapLibre, tiles by request interception | accepted |
| 0005 | Single source of truth for UI state | accepted |
| 0006 | WGS84 storage and geodesic radius circles | accepted |
| 0007 | Business rules live in Domain; view models only adapt them | accepted |

## Template (senior-engineer decision-making)
```text
# ADR-NNNN Title
Status: proposed | accepted | superseded by ADR-XXXX
Date:
Problem and context: problem, constraints, quality attributes.
Options: 2 to 4 viable options with trade-offs.
Decision: what we chose and why; rejected alternatives.
Consequences: benefits, costs, what becomes harder, failure modes.
Re-evaluate when: concrete triggers.
```
