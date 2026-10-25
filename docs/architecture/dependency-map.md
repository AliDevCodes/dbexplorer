# Dependency Map

## Allowed Dependencies
```text
Wpf (Views, ViewModels, Services, Localization, Assets/Map)
  ↓
Application (contracts)
  ↓
Domain (models, rules)

Infrastructure (adapters) ──implements──► Application
Infrastructure ──uses──► Domain
Wpf ──composition root only──► Infrastructure (App.xaml.cs registrations)
Tests ──► Infrastructure, Domain
```

## Forbidden Dependencies (target rules)
| From | To | Why |
|---|---|---|
| Domain | Application, Infrastructure, Wpf | Domain must be pure rules. |
| Application | Infrastructure, Wpf | Contracts must not know implementations or UI. |
| Infrastructure | Wpf | Adapters must not know the UI. |
| Views (XAML and code-behind) | Infrastructure | UI reaches data only through ViewModels and Application contracts. (Current: `MapView` uses `Application.CoordinateLayers` helpers; accepted for now, see below.) |
| JavaScript map page | network | The page reads tiles and fonts only through the host. |
| Any module | SQL text built from user input | Only typed parameters (R3). |
| Any ViewModel | a View type | ViewModels must be testable without UI. (Current: `MonitorEditorViewModel`, `ConnectPromptViewModel` are views-adjacent; review.) |

## Current Violations (tracked, not yet fixed)
| ID | Violation | Plan |
|---|---|---|
| V1 | Domain contains colours and SVG path data (`MapMarkerCatalog`, `MapSourceStyles`) | Phase R2: move to Wpf presentation. |
| V2 | Domain contains Persian calendar codec (`PersianDateTimeCodec`) | Phase R2. |
| V3 | Domain uses `System.Text.Json.Serialization` (`[JsonIgnore]`) | Phase R2: DTO mapping in Infrastructure. |
| V4 | `MapView` calls `Application.CoordinateLayers` helpers directly | Accept (contract-level helper), review in R3. |
| V5 | `App.xaml.cs` casts `XlsxReportWriter` between two interfaces | Phase R5. |

## Cross-Module Contracts
- Application interfaces are the only contracts between ViewModels and I/O: `IDatabaseMetadataService`, `IConnectionProfileStore`, `ICredentialVault`, `IMapSourceFactory`, `IMapSource`, `ICoordinateLayerStore`, `IExcelImportService`, `IMonitorStore`, `IWatermarkReader`, `IReportWriter`, `IQueryReportWriter`, `IMonitorCheckService`, `IFileDialogService`, `IMapSettingsStore`.
- Map host ↔ page: JSON messages (see `docs/features/index.md` → Map). Any new message must be added on both sides and documented in `docs/changes/change-map.md`.
- Shell ↔ sections: `MainViewModel` owns sessions; a new session disposes the old one (`EndSession`).
- Explorer and Monitoring → Map: `QueryMapPoint` lists via `MainViewModel` events.

## Package Dependencies (external)
| Package | Used by | Status |
|---|---|---|
| Microsoft.Data.SqlClient 6.* | Infrastructure | keep |
| Microsoft.Data.Sqlite 10.* | Infrastructure (maps) | keep |
| ExcelDataReader 3.* | Infrastructure (import) | keep |
| System.Security.Cryptography.ProtectedData 10.* | Infrastructure (DPAPI) | keep |
| Microsoft.Web.WebView2 | Wpf (map) | keep |
| CommunityToolkit.Mvvm 8.* | Wpf, Domain? (check) | keep |
| Microsoft.Extensions.Hosting 10.* | Wpf (DI) | decide after R5 |
| System.Security.Cryptography.Pkcs (transitive) | Infrastructure, Tests | advisory, pin fixed version (R7) |

All versions are floating until pinned in R7.
