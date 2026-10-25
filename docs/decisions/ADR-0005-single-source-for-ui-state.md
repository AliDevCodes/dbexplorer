# ADR-0005 Single source of truth for UI state
Status: accepted
Date: 2026-10 (from two production bugs of this phase)

Context: two bugs were caused by UI state design:
- Two TwoWay RadioButtons bound to two properties that notify each other formed an endless loop. The process died with a StackOverflowException, which cannot be caught.
- A ComboBox wrote null into a property that the rest of the code assumed was never null, causing NullReferenceException.

Options:
1. Guards in each setter. Tried twice; each guard stopped only one of several paths.
2. One source of truth; derived properties are read-only or ignore writes.
3. Replace bindings with commands everywhere. Correct but a large change now.

Decision: option 2 now, option 3 where a control is rewritten.
- A derived property (for example the map-layers tab) is read-only or ignores writes.
- Setters that must have a value revert null to a default.
- Never call a setter from a notification of the same pair.

Consequences: a few properties look redundant; the code comment must say why. The rule is checked in review (change-map checklist). Refactor phase R4 audits all TwoWay bindings.

Re-evaluate when: the R4 audit is done, or when a new framework feature (for example a different binding model) changes the trade-off.
