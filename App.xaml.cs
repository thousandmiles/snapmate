using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SnapMate.Services;
using SnapMate.ViewModels;
using SnapMate.Views;

namespace SnapMate;

/// <summary>
/// Main application class for SnapMate.
/// Configures dependency injection container and manages application lifecycle.
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
      {
     // Register all service implementations
    services.AddSingleton<IScreenshotCaptureService, ScreenshotCaptureService>();
   services.AddSingleton<IImageEditingService, ImageEditingService>();
         services.AddSingleton<IHistoryService, HistoryService>();
   services.AddSingleton<IOcrService, OcrService>();
     services.AddSingleton<IHotkeyService, HotkeyService>();
    services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IClipboardService, ClipboardService>();
         services.AddSingleton<IFileSaveService, FileSaveService>();

       // Register view models (transient to allow multiple instances if needed)
     services.AddTransient<MainViewModel>();

          // Register views (singleton for main window)
            services.AddSingleton<MainWindow>();
     })
     .Build();
    }

    /// <summary>
    /// Called when the application starts.
    /// Initializes services, loads settings, and displays the main window.
    /// </summary>
    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        await _host.StartAsync();

        // Load application settings before showing UI
        var settingsService = _host.Services.GetRequiredService<ISettingsService>();
        await settingsService.LoadAsync();

        // Create and show main window with dependency injection
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

    base.OnStartup(e);
    }

    /// <summary>
    /// Called when the application is shutting down.
    /// Performs cleanup of the hosting infrastructure.
    /// </summary>
    protected override async void OnExit(System.Windows.ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
    base.OnExit(e);
    }
}
