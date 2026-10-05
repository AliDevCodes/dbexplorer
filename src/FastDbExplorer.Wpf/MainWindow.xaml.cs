using System.Windows;
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
        SourceInitialized += (_, _) => ThemeService.SetDarkTitleBar(this, ThemeService.IsDark());
        StateChanged += (_, _) => UpdateMaximizeState();
        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }

    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnMaximizeRestore(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    /// <summary>A maximized chrome window extends past the screen edge by the resize border; pull the content back in.</summary>
    private void UpdateMaximizeState()
    {
        var maximized = WindowState == WindowState.Maximized;
        RootGrid.Margin = maximized ? SystemParameters.WindowResizeBorderThickness : new Thickness(0);
        MaxRestoreButton.Content = maximized ? "\uE923" : "\uE922";
        MaxRestoreButton.ToolTip = maximized ? WindowStrings.Restore : WindowStrings.Maximize;
    }
}
