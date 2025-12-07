using System.IO;

namespace SnapMate.Models;

/// <summary>
/// Application configuration settings persisted to JSON.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the directory where screenshots are automatically saved.
    /// Defaults to a SnapMate folder in the user's Pictures directory.
    /// </summary>
    public string SaveDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "SnapMate");

    /// <summary>
    /// Gets or sets the file naming pattern using date/time placeholders.
    /// Supported placeholders: {yyyy}, {MM}, {dd}, {HH}, {mm}, {ss}, {fff}
    /// </summary>
  public string FileNamePattern { get; set; } = "Screenshot_{yyyy-MM-dd_HH-mm-ss}";

    /// <summary>
    /// Gets or sets the default image format for saving screenshots.
    /// </summary>
    public ImageFormat DefaultFormat { get; set; } = ImageFormat.Png;

    /// <summary>
    /// Gets or sets whether screenshots should be automatically copied to the clipboard.
    /// </summary>
    public bool CopyToClipboard { get; set; } = true;

    /// <summary>
    /// Gets or sets whether OCR text extraction should run automatically on captured screenshots.
    /// </summary>
    public bool EnableOcr { get; set; } = false;

    /// <summary>
    /// Gets or sets the global hotkey configuration.
    /// </summary>
    public HotkeyConfig Hotkeys { get; set; } = new();

    /// <summary>
    /// Gets or sets the application theme preference.
    /// </summary>
    public ThemeMode Theme { get; set; } = ThemeMode.Dark;
}

/// <summary>
/// Configuration for global hotkeys.
/// </summary>
public class HotkeyConfig
{
    /// <summary>
    /// Gets or sets the hotkey for regional screenshot capture.
    /// </summary>
    public string RegionalCapture { get; set; } = "Ctrl+Shift+A";

    /// <summary>
    /// Gets or sets the hotkey for full-screen capture.
    /// </summary>
    public string FullScreenCapture { get; set; } = "Ctrl+Shift+S";

    /// <summary>
    /// Gets or sets the hotkey for window capture.
    /// </summary>
    public string WindowCapture { get; set; } = "Ctrl+Shift+W";

    /// <summary>
    /// Gets or sets the hotkey for showing the history panel.
    /// </summary>
    public string ShowHistory { get; set; } = "Ctrl+Shift+H";
}

/// <summary>
/// Supported image file formats for screenshot saving.
/// </summary>
public enum ImageFormat
{
    /// <summary>
    /// Portable Network Graphics - lossless compression.
    /// </summary>
    Png,

 /// <summary>
    /// JPEG - lossy compression, smaller file size.
    /// </summary>
    Jpg,

    /// <summary>
    /// Bitmap - uncompressed, largest file size.
    /// </summary>
    Bmp
}

/// <summary>
/// Application theme options.
/// </summary>
public enum ThemeMode
{
/// <summary>
    /// Light theme with bright colors.
    /// </summary>
    Light,

    /// <summary>
    /// Dark theme with dark colors (default).
    /// </summary>
    Dark,

    /// <summary>
    /// Follow Windows system theme preference.
    /// </summary>
    System
}
