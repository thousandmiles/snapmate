using SnapMate.Models;
using System.IO;
using System.Text.Json;

namespace SnapMate.Services;

/// <summary>
/// Provides application settings management with persistence to JSON.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    AppSettings Settings { get; }

    /// <summary>
    /// Loads settings from disk. If no settings file exists, defaults are used.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Saves current settings to disk.
    /// </summary>
    Task SaveAsync();
}

/// <summary>
/// Implementation of settings service using JSON file storage in AppData.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;

    /// <inheritdoc />
    public AppSettings Settings { get; private set; } = new();

    public SettingsService()
    {
        var appData = Path.Combine(
  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SnapMate");
        Directory.CreateDirectory(appData);
 _settingsPath = Path.Combine(appData, "settings.json");
    }

    /// <inheritdoc />
    public async Task LoadAsync()
    {
        try
        {
      if (File.Exists(_settingsPath))
            {
   var json = await File.ReadAllTextAsync(_settingsPath);
    Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
       }
    }
   catch (Exception)
        {
  // If settings fail to load, use defaults
   Settings = new AppSettings();
     }

        // Ensure save directory exists
        Directory.CreateDirectory(Settings.SaveDirectory);
    }

    /// <inheritdoc />
    public async Task SaveAsync()
    {
        try
        {
     var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions
            {
WriteIndented = true
     });
            await File.WriteAllTextAsync(_settingsPath, json);
      }
        catch (Exception)
        {
            // Silently handle save errors
    }
    }
}
