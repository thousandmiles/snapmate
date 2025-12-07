using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SnapMate.Services;
using SnapMate.ViewModels;
using SnapMate.Views;

namespace SnapMate;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
  .ConfigureServices((context, services) =>
 {
       services.AddSingleton<IScreenshotCaptureService, ScreenshotCaptureService>();
    services.AddSingleton<IImageEditingService, ImageEditingService>();
      services.AddSingleton<IHistoryService, HistoryService>();
   services.AddSingleton<IOcrService, OcrService>();
       services.AddSingleton<IHotkeyService, HotkeyService>();
     services.AddSingleton<ISettingsService, SettingsService>();
   services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IFileSaveService, FileSaveService>();
    services.AddTransient<MainViewModel>();
  services.AddSingleton<MainWindow>();
  })
    .Build();
    }

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        await _host.StartAsync();

        var settingsService = _host.Services.GetRequiredService<ISettingsService>();
 await settingsService.LoadAsync();

   var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

 base.OnStartup(e);
    }

    protected override async void OnExit(System.Windows.ExitEventArgs e)
    {
        await _host.StopAsync();
     _host.Dispose();
        base.OnExit(e);
    }
}
