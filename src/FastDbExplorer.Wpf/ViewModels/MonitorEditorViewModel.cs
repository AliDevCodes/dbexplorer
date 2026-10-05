using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;
using FastDbExplorer.Infrastructure;
using FastDbExplorer.Wpf.Localization;
using FastDbExplorer.Wpf.Services;

namespace FastDbExplorer.Wpf.ViewModels;

/// <summary>
/// Create / edit one monitor. Everything is chosen dynamically from the real table: database, table, the column that
/// identifies new records, and the condition columns (location, origin, destination ... whatever the table calls them).
/// An existing monitor keeps its table and watermark column (changing them would invalidate its saved position).
/// </summary>
public sealed partial class MonitorEditorViewModel : ObservableObject, IDisposable
{
    private readonly IDatabaseMetadataService _metadata;
    private readonly ConnectionSettings _settings;
    private readonly IFolderPickerService _folders;
    private readonly MonitorDefinition? _existing;
    private readonly CancellationTokenSource _lifetime = new();
    private IReadOnlyList<ColumnInfo> _columns = [];

    public MonitorEditorViewModel(
        IDatabaseMetadataService metadata, ConnectionSettings settings, IReadOnlyList<DatabaseInfo> databases,
        IFolderPickerService folders, MonitorDefinition? existing)
    {
        _metadata = metadata;
        _settings = settings;
        _folders = folders;
        _existing = existing;
        Databases = databases.Select(d => d.Name).ToList();
        _outputFolder = DefaultFolder();

        if (existing is not null)
        {
            _name = existing.Name;
            _selectedDatabase = existing.Database;
            _selectedWatermark = existing.WatermarkColumn;
            _useOr = existing.Logic == FilterLogic.Or;
            _intervalText = existing.IntervalMinutes.ToString(CultureInfo.InvariantCulture);
            _outputFolder = existing.OutputFolder;
            _playSound = existing.PlaySound;
        }
    }

    public event Action<MonitorDefinition>? Saved;
    public event Action? Cancelled;

    public bool IsNew => _existing is null;
    public bool IsExisting => _existing is not null;
    public string Title => IsNew ? MonitoringStrings.EditorTitleNew : MonitoringStrings.EditorTitleEdit;
    public string TargetText => _existing?.TargetText ?? "";
    public IReadOnlyList<string> Databases { get; }
    public ObservableCollection<FilterRowViewModel> Filters { get; } = [];
    public IReadOnlyList<OperatorOption> Operators => OperatorOption.All;

    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string? _selectedDatabase;
    [ObservableProperty] private IReadOnlyList<TableInfo> _tables = [];
    [ObservableProperty] private TableInfo? _selectedTable;
    [ObservableProperty] private IReadOnlyList<string> _watermarkColumns = [];
    [ObservableProperty] private string? _selectedWatermark;
    [ObservableProperty] private IReadOnlyList<string> _filterableColumns = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MatchAll))]
    private bool _useOr;

    [ObservableProperty] private string _intervalText = "60";
    [ObservableProperty] private string _outputFolder = "";
    [ObservableProperty] private bool _playSound = true;
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isLoading;

    public bool MatchAll
    {
        get => !UseOr;
        set => UseOr = !value;
    }

    partial void OnSelectedDatabaseChanged(string? value)
    {
        if (IsNew) _ = LoadTablesAsync(value);
    }

    partial void OnSelectedTableChanged(TableInfo? value)
    {
        if (IsNew) _ = LoadColumnsAsync(value);
    }

    public async Task InitializeAsync()
    {
        if (_existing is null)
        {
            SelectedDatabase = Databases.FirstOrDefault(); // triggers the table list
            return;
        }

        IsLoading = true;
        try
        {
            _columns = await _metadata.GetColumnsAsync(_settings, _existing.Database, _existing.Schema, _existing.Table, _lifetime.Token);
            ApplyColumns();
            foreach (var c in _existing.Conditions)
                Filters.Add(new FilterRowViewModel(c.Column)
                {
                    SelectedOperator = OperatorOption.All.First(o => o.Operator == c.Operator),
                    Value = c.Value ?? "",
                    Value2 = c.Value2 ?? ""
                });
        }
        catch (OperationCanceledException) { }
        catch (DatabaseAccessException ex) { ErrorMessage = Strings.Describe(ex); }
        catch (Exception ex) { ErrorMessage = ex.Message; } // UI boundary
        finally { IsLoading = false; }
    }

    private async Task LoadTablesAsync(string? database)
    {
        Tables = [];
        SelectedTable = null;
        if (database is null) return;

        IsLoading = true;
        ErrorMessage = "";
        try
        {
            var list = await _metadata.GetTablesAsync(_settings, database, _lifetime.Token);
            if (SelectedDatabase != database) return; // a newer selection superseded this one
            Tables = list.ToList();
        }
        catch (OperationCanceledException) { }
        catch (DatabaseAccessException ex) { ErrorMessage = Strings.Describe(ex); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsLoading = false; }
    }

    private async Task LoadColumnsAsync(TableInfo? table)
    {
        _columns = [];
        WatermarkColumns = [];
        FilterableColumns = [];
        SelectedWatermark = null;
        Filters.Clear();
        if (table is null || SelectedDatabase is null) return;

        IsLoading = true;
        ErrorMessage = "";
        try
        {
            var columns = await _metadata.GetColumnsAsync(_settings, SelectedDatabase, table.Schema, table.Name, _lifetime.Token);
            if (SelectedTable != table) return;
            _columns = columns;
            ApplyColumns();
        }
        catch (OperationCanceledException) { }
        catch (DatabaseAccessException ex) { ErrorMessage = Strings.Describe(ex); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsLoading = false; }
    }

    private void ApplyColumns()
    {
        WatermarkColumns = _columns.Where(c => WatermarkTypes.IsSupported(c.TypeName)).Select(c => c.Name).ToList();
        FilterableColumns = _columns.Where(c => c.IsFilterable).Select(c => c.Name).ToList();

        if (IsNew)
        {
            // A sensible default: the first primary-key column when it is a number / date. The user can change it.
            var key = _columns.Where(c => c.IsPrimaryKey).OrderBy(c => c.KeyOrdinal)
                .FirstOrDefault(c => WatermarkTypes.IsSupported(c.TypeName));
            SelectedWatermark = key?.Name ?? WatermarkColumns.FirstOrDefault();
        }
    }

    [RelayCommand]
    private void AddFilter() => Filters.Add(new FilterRowViewModel(FilterableColumns.FirstOrDefault()));

    [RelayCommand]
    private void RemoveFilter(FilterRowViewModel row) => Filters.Remove(row);

    [RelayCommand]
    private void BrowseFolder()
    {
        var picked = _folders.PickFolder(OutputFolder);
        if (picked is not null) OutputFolder = picked;
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();

    [RelayCommand]
    private void Save()
    {
        ErrorMessage = "";

        var name = Name.Trim();
        if (name.Length == 0) { ErrorMessage = MonitoringStrings.ErrName; return; }

        string database, schema, table;
        if (_existing is not null)
        {
            database = _existing.Database;
            schema = _existing.Schema;
            table = _existing.Table;
        }
        else
        {
            if (SelectedDatabase is null || SelectedTable is null) { ErrorMessage = MonitoringStrings.ErrTarget; return; }
            database = SelectedDatabase;
            schema = SelectedTable.Schema;
            table = SelectedTable.Name;
        }

        var watermark = _existing?.WatermarkColumn ?? SelectedWatermark;
        if (watermark is null) { ErrorMessage = MonitoringStrings.ErrWatermark; return; }

        if (!int.TryParse(IntervalText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes)
            || minutes < MonitorDefinition.MinIntervalMinutes || minutes > MonitorDefinition.MaxIntervalMinutes)
        { ErrorMessage = MonitoringStrings.ErrInterval; return; }

        var folder = OutputFolder.Trim();
        if (folder.Length == 0 || !Path.IsPathFullyQualified(folder)) { ErrorMessage = MonitoringStrings.ErrFolder; return; }
        try { Directory.CreateDirectory(folder); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        { ErrorMessage = MonitoringStrings.ErrFolderCreate + ex.Message; return; }

        if (Filters.Count == 0) { ErrorMessage = MonitoringStrings.ErrNoConditions; return; }
        var conditions = new List<FilterCondition>();
        foreach (var row in Filters)
        {
            var incomplete = row.Column is null
                             || (row.NeedsValue && string.IsNullOrWhiteSpace(row.Value))
                             || (row.NeedsSecondValue && string.IsNullOrWhiteSpace(row.Value2));
            if (incomplete) { ErrorMessage = Strings.ErrIncompleteFilter; return; }

            conditions.Add(new FilterCondition(
                row.Column!, row.SelectedOperator.Operator,
                row.NeedsValue ? row.Value : null,
                row.NeedsSecondValue ? row.Value2 : null));
        }

        var logic = UseOr ? FilterLogic.Or : FilterLogic.And;

        // Dry run of the query builder: bad numbers / dates / operators are reported now, not at 3 a.m.
        try
        {
            var probe = new PageRequest(database, schema, table, [watermark], conditions, logic, [watermark], false, 1, 0, null);
            SelectQueryBuilder.Build(probe, _columns);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or NotSupportedException)
        {
            ErrorMessage = Strings.ErrInvalidQuery + ex.Message;
            return;
        }

        var sound = PlaySound;
        var definition = _existing is null
            ? new MonitorDefinition(Guid.NewGuid(), name, _settings.Server, database, schema, table, watermark,
                conditions, logic, minutes, folder, sound, true)
            : _existing with { Name = name, Conditions = conditions, Logic = logic, IntervalMinutes = minutes, OutputFolder = folder, PlaySound = sound };

        Saved?.Invoke(definition);
    }

    public void Dispose() => _lifetime.Cancel();

    private static string DefaultFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FastDbExplorer", "Reports");
}
