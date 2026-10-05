using CommunityToolkit.Mvvm.ComponentModel;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;

namespace FastDbExplorer.Wpf.ViewModels;

/// <summary>Owns navigation: connection screen -> explorer screen, the map workspace and the monitoring screen.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly ConnectionViewModel _connection;
    private readonly MapViewModel _map;
    private readonly IDatabaseMetadataService _metadata;
    private readonly MonitoringViewModelFactory _monitoringFactory;
    private MonitoringViewModel? _monitoring;
    private object? _returnTo;

    [ObservableProperty]
    private object? _currentView;

    public MainViewModel(
        ConnectionViewModel connection, MapViewModel map, IDatabaseMetadataService metadata,
        MonitoringViewModelFactory monitoringFactory)
    {
        _connection = connection;
        _map = map;
        _metadata = metadata;
        _monitoringFactory = monitoringFactory;
        _connection.Connected += OnConnected;
        _connection.OpenMapRequested += OpenMap;
        _map.BackRequested += () => CurrentView = _returnTo ?? _connection;
        CurrentView = connection;
    }

    public async Task InitializeAsync()
    {
        // The last used map opens in the background at start-up, so it is ready when the user opens the map screen.
        var lastMap = _map.LoadLastMapAsync();
        await _connection.InitializeAsync();
        await lastMap;
    }

    private void OpenMap()
    {
        _returnTo = CurrentView;
        CurrentView = _map;
    }

    private void OnConnected(ConnectionSettings settings, IReadOnlyList<DatabaseInfo> databases)
    {
        _monitoring?.Dispose();

        var explorer = new ExplorerViewModel(_metadata, settings, databases);
        var monitoring = _monitoringFactory.Create(settings, databases);
        _monitoring = monitoring;

        monitoring.BackRequested += () => CurrentView = explorer;
        explorer.OpenMapRequested += OpenMap;
        explorer.OpenMonitoringRequested += () => CurrentView = monitoring;
        explorer.Disconnected += () =>
        {
            monitoring.Dispose(); // disconnecting stops all scheduled checks (the password leaves memory with the session)
            if (ReferenceEquals(_monitoring, monitoring)) _monitoring = null;
            CurrentView = _connection;
        };

        CurrentView = explorer;
        _ = monitoring.InitializeAsync();
    }
}
