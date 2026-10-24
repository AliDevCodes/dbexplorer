using System.Windows;
using FastDbExplorer.Wpf.Localization;
using FastDbExplorer.Wpf.Services;
using FastDbExplorer.Wpf.ViewModels;

namespace FastDbExplorer.Wpf.Views;

/// <summary>
/// The map in its own window, so it can be moved to another monitor. It shares the one <see cref="MapViewModel"/> with the
/// Map section: layers, the open file and the camera stay the same, only the WebView2 page is a second instance.
/// </summary>
public partial class MapWindow : Window
{
    private readonly MapViewModel _viewModel;

    public MapWindow(MapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Title = MapUiStrings.MapWindowTitle(viewModel.FileName);
        // The window is left-to-right (title bar buttons on the right, like the main window); the map UI inside stays right-to-left.
        Content = new MapView { IsDetached = true, FlowDirection = FlowDirection.RightToLeft };

        SourceInitialized += (_, _) => ThemeService.SetDarkTitleBar(this, ThemeService.IsDark());
        _viewModel.DockRequested += OnDockRequested;
        _viewModel.SetPoppedOut(true);
        // Closing (not Closed): the WebView2 must be released while this window still has its handle.
        Closing += (_, _) =>
        {
            _viewModel.DockRequested -= OnDockRequested;
            (Content as MapView)?.ReleaseWebView();
            _viewModel.SetPoppedOut(false);
        };
    }

    private void OnDockRequested() => Close();
}
