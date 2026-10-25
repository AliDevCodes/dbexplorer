# Architecture Decision Records

Each significant decision has an ADR. An ADR records the problem, the options, the choice, the cost, and when to re-evaluate. Do not re-open an accepted ADR without new evidence; write a new ADR that supersedes it.

| ADR | Title | Status |
|---|---|---|
| 0001 | Layered architecture (Domain, Application, Infrastructure, Wpf) | accepted |
| 0002 | Read-only by design | accepted |
| 0003 | Opt-in encrypted SQL passwords (DPAPI) | accepted (supersedes "no passwords on disk" for this one case) |
| 0004 | Map rendering with WebView2 and MapLibre, tiles by request interception | accepted |
| 0005 | Single source of truth for UI state | accepted |

## Template
```text
# ADR-NNNN Title
Status: proposed | accepted | superseded by ADR-XXXX
Date:
Context: the problem, constraints, quality attributes.
Options: 2 to 4 viable options with trade-offs.
Decision: what we chose and why.
Consequences: benefits, costs, what becomes harder.
Re-evaluate when: concrete triggers.
```
