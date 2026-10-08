using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FastDbExplorer.Domain;
using FastDbExplorer.Wpf.Localization;
using FastDbExplorer.Wpf.Services;
using FastDbExplorer.Wpf.ViewModels;

namespace FastDbExplorer.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.ConnectionManager.SqlPasswordPrompt = PromptForSqlPasswordAsync;
        SourceInitialized += (_, _) => ThemeService.SetDarkTitleBar(this, ThemeService.IsDark());
        StateChanged += (_, _) => UpdateMaximizeState();
        PreviewKeyDown += OnSectionShortcut;
        viewModel.PaletteRequested += OnPaletteRequested;
        Loaded += async (_, _) =>
        {
            UpdateMaximizeState();
            await viewModel.InitializeAsync();
        };
    }

    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnMaximizeRestore(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    /// <summary>Ctrl+K opens the search palette; Ctrl+1..Ctrl+5 jump through the navigation rail. (Not while the map has keyboard focus: WebView2 is a native window.)</summary>
    private void OnSectionShortcut(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control) return;
        if (DataContext is not MainViewModel viewModel) return;

        if (e.Key == Key.K)
        {
            viewModel.OpenPaletteCommand.Execute(null);
            e.Handled = true;
            return;
        }

        AppSection? section = e.Key switch
        {
            Key.D1 or Key.NumPad1 => AppSection.Home,
            Key.D2 or Key.NumPad2 => AppSection.Connections,
            Key.D3 or Key.NumPad3 => AppSection.Explorer,
            Key.D4 or Key.NumPad4 => AppSection.Map,
            Key.D5 or Key.NumPad5 => AppSection.Monitoring,
            _ => null
        };
        if (section is null) return;

        viewModel.NavigateCommand.Execute(section.Value);
        e.Handled = true;
    }

    private async void OnActiveProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox combo || combo.SelectedItem is not SavedConnection profile
            || DataContext is not MainViewModel viewModel || viewModel.ActiveProfile?.Id == profile.Id)
            return;

        var connected = await viewModel.SwitchProfileAsync(profile);
        if (connected) return;

        combo.SelectedItem = viewModel.ActiveProfile;
        if (!string.IsNullOrWhiteSpace(viewModel.ConnectionManager.ErrorMessage))
            MessageBox.Show(this, viewModel.ConnectionManager.ErrorMessage, WindowStrings.AppName,
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }

    private Task<string?> PromptForSqlPasswordAsync(SavedConnection profile)
    {
        var prompt = new Views.SqlPasswordWindow(profile) { Owner = this };
        return Task.FromResult(prompt.ShowDialog() == true ? prompt.Password : null);
    }

    private void OnPaletteRequested(CommandPaletteViewModel palette)
    {
        new Views.CommandPaletteWindow(palette) { Owner = this }.ShowDialog();
        palette.Chosen?.Run(); // runs after the palette window is gone, so focus returns to the main window first
    }

    /// <summary>A maximized chrome window extends past the screen edge by the resize border; pull the content back in.</summary>
    private void UpdateMaximizeState()
    {
        var maximized = WindowState == WindowState.Maximized;
        RootGrid.Margin = maximized ? SystemParameters.WindowResizeBorderThickness : new Thickness(0);
        MaxRestoreButton.Content = maximized ? "\uE923" : "\uE922";
        MaxRestoreButton.ToolTip = maximized ? WindowStrings.Restore : WindowStrings.Maximize;
    }
}
