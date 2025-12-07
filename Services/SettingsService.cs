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
        var appData = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
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
            Settings = new AppSettings();
        }

        var wasFixed = ValidateAndFixSettings();
        
        Directory.CreateDirectory(Settings.SaveDirectory);

        // Save corrected settings back to disk
        if (wasFixed)
        {
           await SaveAsync();
        }
    }

    private bool ValidateAndFixSettings()
    {
        bool wasModified = false;

        // Fix common pattern typos
        if (Settings.FileNamePattern.Contains("{yyy}") && !Settings.FileNamePattern.Contains("{yyyy}"))
        {
         Settings.FileNamePattern = Settings.FileNamePattern.Replace("{yyy}", "{yyyy}");
         wasModified = true;
        }

        // Ensure pattern has at least one valid placeholder
        var validPlaceholders = new[] { "{yyyy}", "{MM}", "{dd}", "{HH}", "{mm}", "{ss}", "{fff}" };
        if (!validPlaceholders.Any(p => Settings.FileNamePattern.Contains(p)))
        {
         Settings.FileNamePattern = "Screenshot_{yyyy-MM-dd_HH-mm-ss}";
            wasModified = true;
        }

        // Validate save directory
        if (string.IsNullOrWhiteSpace(Settings.SaveDirectory))
        {
 Settings.SaveDirectory = Path.Combine(
 Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "SnapMate");
            wasModified = true;
        }

        return wasModified;
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
