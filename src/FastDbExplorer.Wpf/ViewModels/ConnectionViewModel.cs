using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Localization;

namespace FastDbExplorer.Wpf.ViewModels;

/// <summary>
/// Named profile management and connection creation. SQL passwords are requested at connect time and kept in memory,
/// unless the profile opted in to the encrypted credential vault (RememberPassword, off by default).
/// </summary>
public sealed partial class ConnectionViewModel : ObservableObject
{
    private readonly IDatabaseMetadataService _metadata;
    private readonly IConnectionProfileStore _store;
    private readonly ICredentialVault _vault;
    private CancellationTokenSource? _connectCts;

    public ConnectionViewModel(IDatabaseMetadataService metadata, IConnectionProfileStore store, ICredentialVault vault)
    {
        _metadata = metadata;
        _store = store;
        _vault = vault;
    }

    public event Action<ConnectionSettings, IReadOnlyList<DatabaseInfo>, SavedConnection>? Connected;
    public event Action<SavedConnection>? ProfileDeleted;

    public Func<SavedConnection, Task<string?>>? SqlPasswordPrompt { get; set; }
    public ObservableCollection<SavedConnection> Profiles { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    private string _profileName = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    private string _server = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    [NotifyPropertyChangedFor(nameof(UseWindowsAuth))]
    private bool _useSqlAuth;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    private string _userName = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedProfile))]
    [NotifyCanExecuteChangedFor(nameof(ConnectSelectedCommand))]
    private SavedConnection? _selectedProfile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(ConnectSelectedCommand))]
    private bool _isBusy;

    [ObservableProperty] private bool _trustServerCertificate = true;
    [ObservableProperty] private bool _rememberPassword;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string _errorMessage = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    private string _statusMessage = "";

    public bool UseWindowsAuth
    {
        get => !UseSqlAuth;
        set => UseSqlAuth = !value;
    }

    public bool HasSelectedProfile => SelectedProfile is not null;
    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage)
        && StatusMessage != Strings.Cancelled && StatusMessage != Strings.Connecting;

    public async Task InitializeAsync() => await LoadProfilesAsync();

    private bool CanSaveProfile() => !IsBusy
        && !string.IsNullOrWhiteSpace(ProfileName)
        && !string.IsNullOrWhiteSpace(Server)
        && (!UseSqlAuth || !string.IsNullOrWhiteSpace(UserName));

    private bool CanConnectSelected() => !IsBusy && SelectedProfile is not null;

    [RelayCommand]
    private void NewProfile()
    {
        SelectedProfile = null;
        ProfileName = "";
        Server = "";
        UseSqlAuth = false;
        UserName = "";
        TrustServerCertificate = true;
        RememberPassword = false;
        ErrorMessage = "";
        StatusMessage = "";
    }

    [RelayCommand(CanExecute = nameof(CanSaveProfile))]
    private Task SaveProfileAsync() => SaveProfileCoreAsync();

    [RelayCommand]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile is not { } profile || IsBusy) return;
        try
        {
            await _store.RemoveAsync(profile);
            await ForgetStoredPasswordAsync(profile.Id);
            Profiles.Remove(profile);
            ProfileDeleted?.Invoke(profile);
            NewProfile();
            StatusMessage = ShellStrings.ProfileDeleted;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanConnectSelected))]
    private async Task ConnectSelectedAsync()
    {
        var profile = await SaveProfileCoreAsync();
        if (profile is not null) await ConnectProfileAsync(profile);
    }

    [RelayCommand]
    private void Cancel() => _connectCts?.Cancel();

    public async Task<bool> ConnectProfileAsync(SavedConnection profile)
    {
        var saved = Profiles.FirstOrDefault(item => item.Id == profile.Id);
        if (saved is null)
        {
            ErrorMessage = ShellStrings.ProfileUnavailable;
            return false;
        }

        SelectedProfile = saved;
        _connectCts?.Cancel();
        using var cts = new CancellationTokenSource();
        _connectCts = cts;
        IsBusy = true;
        ErrorMessage = "";
        StatusMessage = Strings.Connecting;
        var usedStoredPassword = false;
        try
        {
            string? password = null;
            if (saved.Authentication == AuthenticationMode.SqlServer)
            {
                if (saved.RememberPassword)
                {
                    password = await ReadStoredPasswordAsync(saved.Id, cts.Token);
                    usedStoredPassword = password is not null;
                }

                if (password is null)
                {
                    if (SqlPasswordPrompt is null)
                    {
                        ErrorMessage = ShellStrings.PasswordPromptUnavailable;
                        StatusMessage = "";
                        return false;
                    }

                    password = await SqlPasswordPrompt(saved);
                    cts.Token.ThrowIfCancellationRequested();
                    if (password is null)
                    {
                        StatusMessage = Strings.Cancelled;
                        return false;
                    }
                }
            }

            var settings = new ConnectionSettings(
                saved.Server,
                saved.Authentication,
                saved.UserName,
                password,
                saved.TrustServerCertificate);
            var databases = await _metadata.GetDatabasesAsync(settings, cts.Token);
            cts.Token.ThrowIfCancellationRequested();

            // Keep a typed password only after the server accepted it.
            if (saved.RememberPassword && password is not null && !usedStoredPassword)
                await StorePasswordAsync(saved.Id, password);

            StatusMessage = Strings.TestOk;
            Connected?.Invoke(settings, databases, saved);
            return true;
        }
        catch (OperationCanceledException)
        {
            if (_connectCts == cts) StatusMessage = Strings.Cancelled;
            return false;
        }
        catch (DatabaseAccessException ex)
        {
            var storedRejected = usedStoredPassword && ex.Kind == DatabaseErrorKind.LoginFailed;
            if (storedRejected) await ForgetStoredPasswordAsync(saved.Id);
            if (_connectCts != cts) return false;
            StatusMessage = "";
            ErrorMessage = storedRejected
                ? $"{Strings.Describe(ex)} {CredentialStrings.StoredPasswordRejected}"
                : Strings.Describe(ex);
            return false;
        }
        catch (Exception ex)
        {
            if (_connectCts != cts) return false;
            StatusMessage = "";
            ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            if (_connectCts == cts)
            {
                _connectCts = null;
                IsBusy = false;
            }
        }
    }

    private async Task<SavedConnection?> SaveProfileCoreAsync()
    {
        if (!CanSaveProfile())
        {
            ErrorMessage = ShellStrings.ProfileFieldsRequired;
            return null;
        }

        var previous = SelectedProfile;
        var profileId = previous?.Id ?? Guid.NewGuid();
        if (Profiles.Any(item => item.Id != profileId
            && string.Equals(item.Name, ProfileName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ErrorMessage = ShellStrings.ProfileNameTaken;
            return null;
        }

        var profile = new SavedConnection(
            Server.Trim(),
            UseSqlAuth ? AuthenticationMode.SqlServer : AuthenticationMode.Windows,
            UseSqlAuth ? UserName.Trim() : null,
            ProfileName.Trim(),
            TrustServerCertificate,
            profileId,
            UseSqlAuth && RememberPassword);

        try
        {
            await _store.SaveAsync(profile);

            // A stored password belongs to one server + account: drop it when the option is off or the target changed.
            if (!profile.RememberPassword || IdentityChanged(previous, profile))
                await ForgetStoredPasswordAsync(profile.Id);

            await LoadProfilesAsync(profile.Id);
            StatusMessage = ShellStrings.ProfileSaved;
            ErrorMessage = "";
            return Profiles.First(item => item.Id == profile.Id);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return null;
        }
    }

    private static bool IdentityChanged(SavedConnection? previous, SavedConnection current) =>
        previous is not null
        && (!string.Equals(previous.Server, current.Server, StringComparison.OrdinalIgnoreCase)
            || previous.Authentication != current.Authentication
            || !string.Equals(previous.UserName, current.UserName, StringComparison.OrdinalIgnoreCase));

    private async Task<string?> ReadStoredPasswordAsync(Guid profileId, CancellationToken ct)
    {
        try { return await _vault.GetPasswordAsync(profileId, ct); }
        catch (OperationCanceledException) { throw; }
        catch (Exception) { return null; } // an unreadable vault only means "ask again"
    }

    private async Task StorePasswordAsync(Guid profileId, string password)
    {
        try { await _vault.SavePasswordAsync(profileId, password); }
        catch (Exception) { /* connecting still works; the password is simply asked again next time */ }
    }

    private async Task ForgetStoredPasswordAsync(Guid profileId)
    {
        try { await _vault.RemoveAsync(profileId); }
        catch (Exception) { /* nothing sensitive can be left behind by a failed delete of an already unreadable entry */ }
    }

    private async Task LoadProfilesAsync(Guid? selectId = null)
    {
        var requestedId = selectId ?? SelectedProfile?.Id;
        var items = await _store.LoadAsync();
        SelectedProfile = null;
        Profiles.Clear();
        foreach (var item in items) Profiles.Add(item);

        SelectedProfile = requestedId is null
            ? Profiles.FirstOrDefault()
            : Profiles.FirstOrDefault(item => item.Id == requestedId.Value) ?? Profiles.FirstOrDefault();
    }

    partial void OnSelectedProfileChanged(SavedConnection? value)
    {
        if (value is null) return;
        ProfileName = value.Name;
        Server = value.Server;
        UseSqlAuth = value.Authentication == AuthenticationMode.SqlServer;
        UserName = value.UserName ?? "";
        TrustServerCertificate = value.TrustServerCertificate;
        RememberPassword = value.RememberPassword;
        ErrorMessage = "";
        StatusMessage = "";
    }
}
