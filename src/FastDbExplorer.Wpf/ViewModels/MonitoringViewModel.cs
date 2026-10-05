using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;
using FastDbExplorer.Infrastructure.Monitoring;
using FastDbExplorer.Wpf.Localization;
using FastDbExplorer.Wpf.Services;

namespace FastDbExplorer.Wpf.ViewModels;

public sealed record AlertItem(string MonitorName, string TimeText, string Summary, string? ReportPath)
{
    public bool HasReport => ReportPath is not null;
}

/// <summary>
/// Monitor list + alert history + the in-process scheduler for one connected session. It lives as long as the
/// connection (the password stays in memory only) and keeps checking while the user works in other screens.
/// </summary>
public sealed partial class MonitoringViewModel : ObservableObject, IDisposable
{
    private const int MaxAlerts = 100;

    private readonly IDatabaseMetadataService _metadata;
    private readonly IMonitorStore _store;
    private readonly IMonitorCheckService _checks;
    private readonly IFolderPickerService _folders;
    private readonly IAlertNotifier _notifier;
    private readonly ConnectionSettings _settings;
    private readonly IReadOnlyList<DatabaseInfo> _databases;
    private readonly List<MonitorDefinition> _otherServers = []; // monitors of other servers are kept untouched in the file
    private readonly SemaphoreSlim _saveGate = new(1, 1);
    private readonly CancellationTokenSource _lifetime = new();
    private volatile IReadOnlyList<MonitorDefinition> _snapshot = []; // what the scheduler thread reads
    private MonitorScheduler? _scheduler;

    public MonitoringViewModel(
        IDatabaseMetadataService metadata, IMonitorStore store, IMonitorCheckService checks, IFolderPickerService folders,
        IAlertNotifier notifier, ConnectionSettings settings, IReadOnlyList<DatabaseInfo> databases)
    {
        _metadata = metadata;
        _store = store;
        _checks = checks;
        _folders = folders;
        _notifier = notifier;
        _settings = settings;
        _databases = databases;
        Monitors.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoMonitors));
        Alerts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoAlerts));
    }

    public event Action? BackRequested;

    public ObservableCollection<MonitorItemViewModel> Monitors { get; } = [];
    public ObservableCollection<AlertItem> Alerts { get; } = [];
    public bool HasNoMonitors => Monitors.Count == 0;
    public bool HasNoAlerts => Alerts.Count == 0;

    [ObservableProperty] private MonitorEditorViewModel? _editor;
    [ObservableProperty] private string _errorMessage = "";

    public async Task InitializeAsync()
    {
        try
        {
            var all = await _store.LoadAsync(_lifetime.Token);
            foreach (var monitor in all)
            {
                if (string.Equals(monitor.Server, _settings.Server, StringComparison.OrdinalIgnoreCase))
                    Monitors.Add(new MonitorItemViewModel(monitor));
                else
                    _otherServers.Add(monitor);
            }
            RefreshSnapshot();

            // Created here (UI thread) so its events come back on the UI thread.
            _scheduler = new MonitorScheduler(_checks, _settings, () => _snapshot);
            _scheduler.Started += OnStarted;
            _scheduler.Completed += OnCompleted;
            _scheduler.Start();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ErrorMessage = ex.Message; } // UI boundary
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke();

    [RelayCommand]
    private void NewMonitor() => OpenEditor(null);

    [RelayCommand]
    private void EditMonitor(MonitorItemViewModel item) => OpenEditor(item.Definition);

    [RelayCommand]
    private void DeleteMonitor(MonitorItemViewModel item)
    {
        Monitors.Remove(item);
        RefreshSnapshot();
        _ = SaveAsync();
    }

    [RelayCommand]
    private void ToggleEnabled(MonitorItemViewModel item)
    {
        item.Definition = item.Definition with { Enabled = !item.Definition.Enabled };
        RefreshSnapshot();
        _ = SaveAsync();
    }

    [RelayCommand]
    private Task CheckNowAsync(MonitorItemViewModel item) =>
        _scheduler?.CheckNowAsync(item.Definition.Id) ?? Task.CompletedTask;

    [RelayCommand]
    private void OpenReport(AlertItem alert)
    {
        if (alert.ReportPath is null) return;
        var error = ShellOpen.Open(alert.ReportPath);
        ErrorMessage = error is null ? "" : MonitoringStrings.ErrOpenFile + error;
    }

    [RelayCommand]
    private void ShowReportInFolder(AlertItem alert)
    {
        if (alert.ReportPath is null) return;
        var error = ShellOpen.Reveal(alert.ReportPath);
        ErrorMessage = error is null ? "" : MonitoringStrings.ErrOpenFile + error;
    }

    private void OpenEditor(MonitorDefinition? existing)
    {
        Editor?.Dispose();
        var editor = new MonitorEditorViewModel(_metadata, _settings, _databases, _folders, existing);
        editor.Saved += OnEditorSaved;
        editor.Cancelled += CloseEditor;
        Editor = editor;
        _ = editor.InitializeAsync();
    }

    private void CloseEditor()
    {
        Editor?.Dispose();
        Editor = null;
    }

    private void OnEditorSaved(MonitorDefinition definition)
    {
        var item = Monitors.FirstOrDefault(m => m.Definition.Id == definition.Id);
        if (item is null) Monitors.Add(new MonitorItemViewModel(definition));
        else item.Definition = definition;

        RefreshSnapshot();
        CloseEditor();
        _ = SaveAsync();
    }

    private void OnStarted(Guid id)
    {
        var item = Monitors.FirstOrDefault(m => m.Definition.Id == id);
        if (item is null) return;
        item.IsChecking = true;
        item.HasProblem = false;
        item.StatusText = MonitoringStrings.Checking;
    }

    private void OnCompleted(MonitorRunOutcome outcome)
    {
        var item = Monitors.FirstOrDefault(m => m.Definition.Id == outcome.Monitor.Id);
        if (item is null) return; // deleted while it was running
        item.IsChecking = false;

        if (outcome.Error is not null)
        {
            item.HasProblem = true;
            item.StatusText = MonitoringStrings.CheckFailed(Describe(outcome.Error));
            return;
        }

        var result = outcome.Result!;
        // Copy only the check state: the user may have edited the monitor while the check was running.
        item.Definition = item.Definition with
        {
            LastWatermark = outcome.Monitor.LastWatermark,
            LastCheckedUtc = outcome.Monitor.LastCheckedUtc
        };
        RefreshSnapshot();

        if (result.IsBaseline)
        {
            item.StatusText = MonitoringStrings.BaselineSet;
        }
        else if (result.Rows.Count == 0)
        {
            item.StatusText = MonitoringStrings.NoNewMatches;
        }
        else
        {
            item.StatusText = MonitoringStrings.Matched(result.Rows.Count);
            var summary = MonitoringStrings.AlertSummary(result.Rows.Count, result.Truncated);
            Alerts.Insert(0, new AlertItem(item.Name, MonitoringStrings.TimeText(result.CheckedAtUtc), summary, result.ReportPath));
            while (Alerts.Count > MaxAlerts) Alerts.RemoveAt(Alerts.Count - 1);
            _notifier.Notify(MonitoringStrings.ToastTitle(item.Name), summary, result.ReportPath, item.Definition.PlaySound);
        }

        _ = SaveAsync();
    }

    private void RefreshSnapshot() => _snapshot = Monitors.Select(m => m.Definition).ToList();

    private async Task SaveAsync()
    {
        await _saveGate.WaitAsync();
        try
        {
            var all = _otherServers.Concat(Monitors.Select(m => m.Definition)).ToList();
            await _store.SaveAsync(all);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; } // UI boundary
        finally { _saveGate.Release(); }
    }

    private static string Describe(Exception ex) => ex switch
    {
        DatabaseAccessException db => Strings.Describe(db),
        ArgumentException or FormatException or NotSupportedException => Strings.ErrInvalidQuery + ex.Message,
        _ => ex.Message
    };

    public void Dispose()
    {
        _lifetime.Cancel();
        _scheduler?.Dispose();
        Editor?.Dispose();
    }
}
