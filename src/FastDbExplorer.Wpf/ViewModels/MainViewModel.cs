using CommunityToolkit.Mvvm.ComponentModel;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;

namespace FastDbExplorer.Wpf.ViewModels;

/// <summary>Owns navigation: connection screen -> explorer screen, and the map workspace reachable from both.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly ConnectionViewModel _connection;
    private readonly MapViewModel _map;
    private readonly IDatabaseMetadataService _metadata;
    private object? _returnTo;

    [ObservableProperty]
    private object? _currentView;

    public MainViewModel(ConnectionViewModel connection, MapViewModel map, IDatabaseMetadataService metadata)
    {
        _connection = connection;
        _map = map;
        _metadata = metadata;
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
        var explorer = new ExplorerViewModel(_metadata, settings, databases);
        explorer.Disconnected += () => CurrentView = _connection;
        explorer.OpenMapRequested += OpenMap;
        CurrentView = explorer;
    }
}
