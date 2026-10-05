# Code map (start here)
**Connect flow:** `ConnectionView.xaml` -> `ConnectionViewModel.ConnectAsync` -> `IDatabaseMetadataService.GetDatabasesAsync` (`SqlServerMetadataService`) -> `IConnectionProfileStore.SaveAsync` -> event `Connected` -> `MainViewModel.OnConnected` -> `ExplorerViewModel`.
**Tables flow:** pick database in `ExplorerView.xaml` -> `ExplorerViewModel.LoadTablesAsync` -> `GetTablesAsync` (reads `sys.tables`/`sys.partitions`, no table scan).
**Read-only safety:** every query passes `ReadOnlySqlGuard.EnsureReadOnly`; connection string uses `ApplicationIntent=ReadOnly`.
**Theme:** `ThemeService` swaps `Themes/Light.xaml` <-> `Dark.xaml` (colours); `Themes/Styles.xaml` holds all control styles.
**Texts:** all in `Localization/Strings.cs`. **Saved connections:** `%AppData%\FastDbExplorer\connections.json` (no passwords).
**Errors:** `SqlException` -> `DatabaseAccessException(kind)` in Infrastructure -> Persian message via `Strings.Describe`.

## MVP-02 (query engine)
**Run flow:** `TableQueryView.xaml` -> `TableQueryViewModel.RunAsync` -> `PageRequest` -> `SqlServerMetadataService.GetPageAsync` -> `GetColumnsAsync` (validates names) -> `SelectQueryBuilder.Build` (SQL + typed parameters) -> `ReadOnlySqlGuard` -> reader (page size + 1 rows).
**Paging:** `TableQueryViewModel` keeps a stack of `PageCursor(offset, afterKey)`. OFFSET below 10,000 rows, keyset (`key > last key`) from there on; Previous pops the stack. Keyset needs the primary key (unique); tables without one use the user-chosen order column + OFFSET only.
**Typed values:** `ValueConverter` (text -> int/date/guid/...); LIKE wildcards escaped in `SelectQueryBuilder`.
**Grid columns** are built at run time in `TableQueryView.xaml.cs`; rows are `GridRow` (pre-formatted strings, bound by index).

## Map module
`MapView.xaml(.cs)` (WebView2 host, tile interception) · `MapViewModel` · `MapSourceFactory` → `MbTilesMapSource` / `MvtLayerReader` · `Assets/Map/map.html` (MapLibre page + generic style) · `FileDialogService`. Details: PROJECT_MASTER_CONTEXT §5 and §9.
