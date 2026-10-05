using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Localization;

namespace FastDbExplorer.Wpf.ViewModels;

public sealed partial class ConnectionViewModel : ObservableObject
{
    private readonly IDatabaseMetadataService _metadata;
    private readonly IConnectionProfileStore _store;
    private CancellationTokenSource? _cts;

    public ConnectionViewModel(IDatabaseMetadataService metadata, IConnectionProfileStore store)
    {
        _metadata = metadata;
        _store = store;
    }

    public event Action<ConnectionSettings, IReadOnlyList<DatabaseInfo>>? Connected;
    public event Action? OpenMapRequested;

    public ObservableCollection<SavedConnection> Recent { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    private string _server = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    [NotifyPropertyChangedFor(nameof(UseWindowsAuth))]
    private bool _useSqlAuth;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    private string _userName = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    private bool _isBusy;

    [ObservableProperty] private string _password = "";
    [ObservableProperty] private bool _trustServerCertificate = true;
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private string _statusMessage = "";

    public bool UseWindowsAuth
    {
        get => !UseSqlAuth;
        set => UseSqlAuth = !value;
    }

    public async Task InitializeAsync() => await LoadRecentAsync();

    private bool CanRun() =>
        !IsBusy && !string.IsNullOrWhiteSpace(Server) && (!UseSqlAuth || !string.IsNullOrWhiteSpace(UserName));

    [RelayCommand(CanExecute = nameof(CanRun))]
    private Task TestAsync() => RunAsync(async ct =>
    {
        await _metadata.TestConnectionAsync(BuildSettings(), ct);
        StatusMessage = Strings.TestOk;
    });

    [RelayCommand(CanExecute = nameof(CanRun))]
    private Task ConnectAsync() => RunAsync(async ct =>
    {
        var settings = BuildSettings();
        var databases = await _metadata.GetDatabasesAsync(settings, ct);
        await _store.SaveAsync(new SavedConnection(settings.Server, settings.Authentication, settings.UserName), ct);
        await LoadRecentAsync();
        Connected?.Invoke(settings, databases);
    });

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();

    [RelayCommand]
    private void OpenMap() => OpenMapRequested?.Invoke();

    [RelayCommand]
    private void UseRecent(SavedConnection connection)
    {
        Server = connection.Server;
        UseSqlAuth = connection.Authentication == AuthenticationMode.SqlServer;
        UserName = connection.UserName ?? "";
        Password = "";
        ErrorMessage = "";
        StatusMessage = "";
    }

    [RelayCommand]
    private async Task RemoveRecentAsync(SavedConnection connection)
    {
        await _store.RemoveAsync(connection);
        await LoadRecentAsync();
    }

    private async Task LoadRecentAsync()
    {
        var items = await _store.LoadAsync();
        Recent.Clear();
        foreach (var item in items) Recent.Add(item);
    }

    private ConnectionSettings BuildSettings() => new(
        Server.Trim(),
        UseSqlAuth ? AuthenticationMode.SqlServer : AuthenticationMode.Windows,
        UseSqlAuth ? UserName.Trim() : null,
        UseSqlAuth ? Password : null,
        TrustServerCertificate);

    private async Task RunAsync(Func<CancellationToken, Task> work)
    {
        using var cts = new CancellationTokenSource();
        _cts = cts;
        IsBusy = true;
        ErrorMessage = "";
        StatusMessage = Strings.Connecting;
        try
        {
            await work(cts.Token);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = Strings.Cancelled;
        }
        catch (DatabaseAccessException ex)
        {
            StatusMessage = "";
            ErrorMessage = Strings.Describe(ex);
        }
        finally
        {
            IsBusy = false;
            _cts = null;
        }
    }
}
