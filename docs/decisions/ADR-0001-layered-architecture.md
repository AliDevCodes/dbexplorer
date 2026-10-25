# ADR-0001 Layered architecture
Status: accepted
Date: 2026-10 (reconstructed from the code; see docs/DECISIONS.md for the early history)

Context: a Windows desktop tool with SQL access, file stores, a map renderer and a UI. Testability of the core logic and safe replacement of I/O are required. The owner works alone; the design must stay understandable.

Options:
1. One project with folders. Simple, but no compile-time boundary; UI and SQL mix easily.
2. Layered projects (Domain, Application, Infrastructure, Wpf). Compile-time boundaries, small test surface for logic.
3. Clean Architecture with many more projects (feature slices). More boundaries, more ceremony than one owner needs.

Decision: option 2. Direction: Wpf → Application → Domain; Infrastructure implements Application.

Consequences: contracts live in Application; tests cover Infrastructure and Domain easily. Costs: some presentation data leaked into Domain (violations V1 to V3) and ViewModels are large (F1, F2); both are tracked in the review.

Re-evaluate when: a second UI or a second database provider is requested, or when the ViewModel refactor (R3) shows that the layer split is not enough.
