# Architecture Decision Records

(Index moved to `docs/decisions/README.md`.)

# ADR-0002 Read-only by design
Status: accepted
Date: 2026-10 (the original rule is recorded in the decision log, D1 to D11)

Context: the database is very large and sensitive. A mistake that writes data is unacceptable. The owner needs analysis, not editing.

Options:
1. Trust the user; no protection. Rejected: one wrong click or copied SQL could change data.
2. Read-only database login. Useful, but outside the app's control; the owner may not own the login.
3. Defence in depth inside the app: connection intent ReadOnly, a SQL guard on every command, identifier validation.

Decision: option 3.
- `ApplicationIntent=ReadOnly` in the connection string.
- `ReadOnlySqlGuard` accepts only one SELECT or WITH statement; it ignores bracketed identifiers and string literals.
- Identifiers come from the real column list and are bracket-quoted; values are typed parameters.

Consequences: some legitimate SQL is refused (lesson L4). Every new query path must go through the guard. Monitoring reads watermarks only.

Re-evaluate when: the owner asks for editing, or a new database provider is added (each provider needs its own guard).
