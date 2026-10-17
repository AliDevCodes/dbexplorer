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
        Content = new MapView { IsDetached = true };

        SourceInitialized += (_, _) => ThemeService.SetDarkTitleBar(this, ThemeService.IsDark());
        _viewModel.DockRequested += OnDockRequested;
        _viewModel.SetPoppedOut(true);
        // Closing (not Closed): the Map section learns the map is back before this window's WebView is torn down.
        Closing += (_, _) =>
        {
            _viewModel.DockRequested -= OnDockRequested;
            _viewModel.SetPoppedOut(false);
        };
    }

    private void OnDockRequested() => Close();
}
