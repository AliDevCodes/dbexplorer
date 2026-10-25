# Architecture Standards Audit (2026-10-25)

Checked against the senior-engineer operating system v2.0.0 (uploaded package): `architecture-intelligence/`, `engineering/`, `git/`, `ui-ux/`, `ai/`. Each finding has evidence, impact, severity and the smallest correction, as the architecture-review standard asks.

| ID | Standard | Finding | Evidence | Severity | Smallest correction | Status |
|---|---|---|---|---|---|---|
| S1 | architecture-review 4 (business logic in UI) | filter operator rules were in a view model | `FilterRowViewModel` before 5818b9d | high | move rule to Domain | done (5818b9d) |
| S2 | architecture-review 8 (testability) | no tests reach view models | `tests/` listing | high | Domain rules first; decide test target (D38) | open |
| S3 | debugging 9 (regression coverage) | no regression test for N1, N2 | no test references the fixes | high | Domain test for N2 (done); N1 needs its rule moved (see S4) | partly |
| S4 | architecture-review 4 | tab mirror rule (N1) is still in MapViewModel | `MapViewModel.IsMapLayersTab` | medium | move the rule to a Domain function and test it | open |
| S5 | anti-patterns: God Object | MapViewModel (5 partial files), TableQueryViewModel | file sizes | high | split by responsibility (refactor R3) | open |
| S6 | principles: Dependency Direction | Domain holds colours, SVG paths, Persian calendar, JSON attribute | V1 to V3 | medium | move presentation to Wpf; DTO mapping in Infrastructure | open (R2) |
| S7 | architecture-review 3 (circular or wrong direction) | Views use Application helper directly | V4 | low | accept for now; review in R3 | accepted |
| S8 | principles: Composition | App.xaml.cs casts between writer interfaces | V5 | low | one registration per interface | open (R5) |
| S9 | ai-documentation: avoid duplicating truth | legacy docs repeated the same facts | three legacy files | medium | migrate, then delete | done in this change (see change-map) |
| S10 | security: review | no written security review of SQL guard, DPAPI, WebView2 navigation, local paths | none | medium | write `docs/reference/security-review.md` | open |
| S11 | observability: structured signal | crash.log is plain text; no correlation ID per operation | `App.xaml.cs` | medium | structured lines with timestamp, operation, outcome | open |
| S12 | ui-ux: states (loading, empty, error, disabled, permission) | not audited per screen | none | medium | per-screen state table | open |
| S13 | ui-ux: reduced motion | animations ignore reduced motion (master context known issue) | `Explorer` loaded animation in `MonitoringView.xaml` and `MapView.xaml` | low | respect `SystemParameters` setting | open |
| S14 | git: buildable commits | several commits not compiled (N13) | commit messages | medium | owner builds before the next slice; say so in messages | ongoing |
| S15 | git: descriptive commits | some commits titled "WIP" | history | low | from now on: one slice, one descriptive message | in force |
| S16 | engineering: search before create | duplicate tests added (N11) | commits 014b176, reverted by 8a52610 and c66f747 | medium | check folder first | in force |
| S17 | architecture-intelligence: decision with alternatives | ADR-0001 has options but no exit criteria; ADR-0007 and 0006 have both | `docs/decisions/` | low | add exit criteria to ADR-0001 | open |

## What is good and should stay
- Read-only guard, typed parameters and server-side paging (R1 to R3 of the master context).
- Invariants in constructors (MapLayer, CoordinateRules): the model cannot hold invalid data.
- Atomic JSON writes; cleanup on cancel; vault-before-profile order.
- A single persistent map view with explicit release (ADR-0004).

## Verdict
The project follows the standards in its structure (state, maps, decisions, junior guide, change routing) and in its safety rules. It did not follow them fully in three places: business rules in view models (S1, S4, S5), missing regression and view-model tests (S2, S3), and some commit and documentation habits (S14 to S16). The table above is the work list.
