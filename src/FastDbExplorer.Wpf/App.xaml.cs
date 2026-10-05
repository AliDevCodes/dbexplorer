using System.Windows;
using FastDbExplorer.Application.Abstractions;
using FastDbExplorer.Infrastructure;
using FastDbExplorer.Infrastructure.CoordinateLayers;
using FastDbExplorer.Infrastructure.Maps;
using FastDbExplorer.Wpf.Services;
using FastDbExplorer.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FastDbExplorer.Wpf;

// Note: "Application" would clash with the FastDbExplorer.Application namespace, so the base type is fully qualified.
public partial class App : System.Windows.Application
{
    private IHost? _host;
    private ThemeService? _theme;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.Message, "FastDbExplorer", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IDatabaseMetadataService, SqlServerMetadataService>();
        builder.Services.AddSingleton<IConnectionProfileStore>(_ => new JsonConnectionProfileStore());
        builder.Services.AddSingleton<IMapSourceFactory, MapSourceFactory>();
        builder.Services.AddSingleton<IExcelImportService, ExcelCoordinateImportService>();
        builder.Services.AddSingleton<ICoordinateLayerStore>(_ => new JsonCoordinateLayerStore());
        builder.Services.AddSingleton<IFileDialogService, FileDialogService>();
        builder.Services.AddSingleton<ConnectionViewModel>();
        builder.Services.AddSingleton<MapViewModel>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainWindow>();
        _host = builder.Build();
        await _host.StartAsync();

        _theme = new ThemeService();
        _theme.Start();

        _host.Services.GetRequiredService<MainWindow>().Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _theme?.Dispose();
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
