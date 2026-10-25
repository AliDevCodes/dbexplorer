# ADR-0007 Business rules live in Domain; view models only adapt them
Status: accepted
Date: 2026-10-25 (from the architecture review, findings F1, F2, F4)

Context: rules such as "which filter operator needs which inputs" were inside view models. They could not be tested without the UI and caused the NullReferenceException (lesson N2). Large view models mix state, rules and I/O (F1, F2).

Options:
1. Keep rules in view models and test through the UI. Rejected: no automated test can reach them (lesson N10).
2. Move rules to Domain as pure functions or small classes; view models call them. Chosen.
3. Move rules to Application services with interfaces. Rejected for now: adds indirection before a second caller exists.

Decision: option 2, applied slice by slice (first slice: FilterOperatorRules).
- Domain has no UI, no I/O, no serializer attributes (target; V1 to V3 tracked in docs/architecture/dependency-map.md).
- A view model holds state and commands; it contains no rule that a test would need.

Consequences: more small files in Domain; rules are testable in milliseconds; the view model becomes thinner over time. Cost: discipline in review (change-map checklist).

Re-evaluate when: a rule needs I/O (then it belongs in Application), or when the test project can target Windows and view models can be tested directly (see the pending decision in docs/reference/decision-log.md, D38).
