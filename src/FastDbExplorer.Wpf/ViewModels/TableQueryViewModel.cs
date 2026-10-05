using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Localization;

namespace FastDbExplorer.Wpf.ViewModels;

/// <summary>One result row, already formatted for display. The grid binds to it by index: [0], [1], ...</summary>
public sealed class GridRow(string[] cells)
{
    public string this[int index] => cells[index];
}

public sealed partial class ColumnItem(ColumnInfo info) : ObservableObject
{
    public string Name => info.Name;
    public string TypeName => info.TypeName;
    public bool IsKey => info.IsPrimaryKey;

    [ObservableProperty] private bool _isSelected;
}

/// <summary>
/// Column selection + filters + paging for one table.
/// Paging is hybrid: OFFSET for the first pages, keyset ("rows after the last key") from
/// <see cref="KeysetThreshold"/> rows onwards, so deep pages stay fast on huge tables.
/// </summary>
public sealed partial class TableQueryViewModel : ObservableObject
{
    private const long KeysetThreshold = 10_000;
    private const int DefaultSelectedColumns = 20;

    private sealed record PageCursor(long Offset, object?[]? AfterKey);

    private readonly IDatabaseMetadataService _metadata;
    private readonly ConnectionSettings _settings;
    private readonly string _database;
    private readonly TableInfo _table;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Stack<PageCursor> _history = new();
    private PageCursor _current = new(0, null);
    private PageRequest? _base;
    private PageResult? _lastResult;
    private CancellationTokenSource? _running;
    private List<ColumnInfo> _keyColumns = [];

    public TableQueryViewModel(IDatabaseMetadataService metadata, ConnectionSettings settings, string database, TableInfo table)
    {
        _metadata = metadata;
        _settings = settings;
        _database = database;
        _table = table;
    }

    public event Action<IReadOnlyList<string>>? ResultColumnsChanged;

    public string TableName => _table.FullName;
    public long ApproxRows => _table.ApproxRows;
    public ObservableCollection<ColumnItem> Columns { get; } = [];
    public ObservableCollection<FilterRowViewModel> Filters { get; } = [];
    public IReadOnlyList<OperatorOption> Operators => OperatorOption.All;
    public IReadOnlyList<int> PageSizes { get; } = [50, 100, 250, 500];
    public IReadOnlyList<string> ResultColumns { get; private set; } = [];
    public bool NeedsOrderColumn => !UsesPrimaryKey;

    [ObservableProperty] private IReadOnlyList<string> _filterableColumns = [];
    [ObservableProperty] private IReadOnlyList<GridRow> _rows = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MatchAll))]
    private bool _useOr;

    public bool MatchAll
    {
        get => !UseOr;
        set => UseOr = !value;
    }
    [ObservableProperty] private int _pageSize = 100;
    [ObservableProperty] private string? _orderColumn;
    [ObservableProperty] private string _keyDescription = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private string _pageInfo = "";
    [ObservableProperty] private bool _hasResult;
    [ObservableProperty] private bool _isConfigVisible = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsOrderColumn))]
    private bool _usesPrimaryKey;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    private bool _isLoadingColumns;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private bool _hasNext;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    private bool _hasPrevious;

    public async Task InitializeAsync()
    {
        IsLoadingColumns = true;
        try
        {
            var meta = await _metadata.GetColumnsAsync(_settings, _database, _table.Schema, _table.Name, _lifetime.Token);
            foreach (var column in meta)
                Columns.Add(new ColumnItem(column) { IsSelected = Columns.Count < DefaultSelectedColumns });

            _keyColumns = meta.Where(c => c.IsPrimaryKey).OrderBy(c => c.KeyOrdinal).ToList();
            UsesPrimaryKey = _keyColumns.Count > 0 && _keyColumns.All(c => c.IsFilterable);
            KeyDescription = string.Join(", ", _keyColumns.Select(c => c.Name));
            FilterableColumns = meta.Where(c => c.IsFilterable).Select(c => c.Name).ToList();
        }
        catch (OperationCanceledException) { }
        catch (DatabaseAccessException ex) { ErrorMessage = Strings.Describe(ex); }
        finally { IsLoadingColumns = false; }
    }

    /// <summary>Called when the user leaves this table: cancels anything still running.</summary>
    public void Shutdown() => _lifetime.Cancel();

    private bool CanRun() => !IsBusy && !IsLoadingColumns && Columns.Count > 0;
    private bool CanGoNext() => HasNext && !IsBusy;
    private bool CanGoPrevious() => HasPrevious && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunAsync()
    {
        if (!TryBuildRequest(out var request)) return;

        _base = request;
        var first = new PageCursor(0, null);
        var fetched = await FetchAsync(first);
        if (fetched is null) return;

        _history.Clear();
        _current = first;
        Show(fetched.Value.Result, fetched.Value.Ms);
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        if (_base is null || _lastResult is null) return;

        var offset = _current.Offset + _base.PageSize;
        var useKey = _base.UseKeyset && _lastResult.LastKey is not null && offset >= KeysetThreshold;
        var next = new PageCursor(offset, useKey ? _lastResult.LastKey : null);

        var fetched = await FetchAsync(next);
        if (fetched is null) return;

        _history.Push(_current);
        _current = next;
        Show(fetched.Value.Result, fetched.Value.Ms);
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private async Task PreviousAsync()
    {
        var previous = _history.Peek();
        var fetched = await FetchAsync(previous);
        if (fetched is null) return;

        _history.Pop();
        _current = previous;
        Show(fetched.Value.Result, fetched.Value.Ms);
    }

    [RelayCommand]
    private void Cancel() => _running?.Cancel();

    [RelayCommand]
    private void ToggleConfig() => IsConfigVisible = !IsConfigVisible;

    [RelayCommand]
    private void AddFilter() => Filters.Add(new FilterRowViewModel(FilterableColumns.FirstOrDefault()));

    [RelayCommand]
    private void RemoveFilter(FilterRowViewModel row) => Filters.Remove(row);

    [RelayCommand]
    private void SelectAllColumns() { foreach (var c in Columns) c.IsSelected = true; }

    [RelayCommand]
    private void SelectNoColumns() { foreach (var c in Columns) c.IsSelected = false; }

    private bool TryBuildRequest(out PageRequest request)
    {
        request = null!;
        ErrorMessage = "";

        var selected = Columns.Where(c => c.IsSelected).Select(c => c.Name).ToList();
        if (selected.Count == 0) { ErrorMessage = Strings.ErrNoColumns; return false; }

        var filters = new List<FilterCondition>();
        foreach (var row in Filters)
        {
            var incomplete = row.Column is null
                             || (row.NeedsValue && string.IsNullOrWhiteSpace(row.Value))
                             || (row.NeedsSecondValue && string.IsNullOrWhiteSpace(row.Value2));
            if (incomplete) { ErrorMessage = Strings.ErrIncompleteFilter; return false; }

            filters.Add(new FilterCondition(
                row.Column!, row.SelectedOperator.Operator,
                row.NeedsValue ? row.Value : null,
                row.NeedsSecondValue ? row.Value2 : null));
        }

        IReadOnlyList<string> order;
        if (UsesPrimaryKey) order = _keyColumns.Select(c => c.Name).ToList();
        else if (OrderColumn is not null) order = [OrderColumn];
        else { ErrorMessage = Strings.ErrPickOrder; return false; }

        request = new PageRequest(
            _database, _table.Schema, _table.Name, selected, filters,
            UseOr ? FilterLogic.Or : FilterLogic.And, order, UsesPrimaryKey, PageSize, 0, null);
        return true;
    }

    private async Task<(PageResult Result, long Ms)?> FetchAsync(PageCursor cursor)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _running = cts;
        IsBusy = true;
        ErrorMessage = "";
        var watch = Stopwatch.StartNew();
        try
        {
            var request = _base! with { Offset = cursor.Offset, AfterKey = cursor.AfterKey };
            var result = await _metadata.GetPageAsync(_settings, request, cts.Token);
            return (result, watch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            PageInfo = Strings.Cancelled;
            return null;
        }
        catch (DatabaseAccessException ex)
        {
            ErrorMessage = Strings.Describe(ex);
            return null;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or NotSupportedException)
        {
            ErrorMessage = Strings.ErrInvalidQuery + ex.Message;
            return null;
        }
        finally
        {
            IsBusy = false;
            _running = null;
        }
    }

    private void Show(PageResult result, long milliseconds)
    {
        _lastResult = result;
        ResultColumns = result.Columns;
        ResultColumnsChanged?.Invoke(result.Columns);
        Rows = result.Rows.Select(r => new GridRow(r.Select(FormatCell).ToArray())).ToList();

        HasResult = true;
        HasNext = result.HasMore;
        HasPrevious = _history.Count > 0;
        PageInfo = result.Rows.Count == 0
            ? Strings.NoResults
            : $"{Strings.RowsRange(_current.Offset + 1, _current.Offset + result.Rows.Count)} · {milliseconds:N0} ms";
    }

    private static string FormatCell(object? value) => value switch
    {
        null => "NULL",
        string s => s.Length > 300 ? s[..300] + "…" : s,
        DateTime d => d.TimeOfDay == TimeSpan.Zero
            ? d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : d.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        byte[] b => "0x" + Convert.ToHexString(b.AsSpan(0, Math.Min(b.Length, 16))) + (b.Length > 16 ? "…" : ""),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? ""
    };
}
