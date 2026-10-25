# ADR-0002 Read-only by design
Status: accepted
Date: 2026-10 (see docs/PROJECT_MASTER_CONTEXT.md §2)

Context: the database is very large and sensitive. A mistake that writes data is unacceptable. The owner needs analysis, not editing.

Options:
1. Trust the user; no protection. Rejected: one wrong click or a copied SQL could change data.
2. Read-only database login. Useful but outside the app's control; the owner may not own the login.
3. Defence in depth inside the app: connection intent ReadOnly, a SQL guard on every command, and identifier validation.

Decision: option 3.
- `ApplicationIntent=ReadOnly` in the connection string.
- `ReadOnlySqlGuard` accepts only one SELECT or WITH statement; it ignores bracketed identifiers and string literals.
- Identifiers come from the real column list and are bracket-quoted; values are typed parameters.

Consequences: some legitimate SQL is refused (for example a column named `[Update]` was once a false positive, fixed). Every new query path must go through the guard. Monitoring reads watermarks only.

Re-evaluate when: the owner asks for editing, or a new database provider is added (each provider needs its own guard).
