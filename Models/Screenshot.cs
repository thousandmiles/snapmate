namespace SnapMate.Models;

/// <summary>
/// Represents a captured screenshot with associated metadata.
/// </summary>
public class Screenshot
{
    /// <summary>
    /// Gets or sets the unique identifier for this screenshot.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the file path where the screenshot is stored.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the screenshot was captured.
    /// </summary>
    public DateTime CapturedAt { get; set; }

    /// <summary>
    /// Gets or sets the optional user-defined title for this screenshot.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets comma-separated tags for categorizing the screenshot.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the capture method used (Regional, FullScreen, Window, or Stitched).
    /// </summary>
    public ScreenshotType Type { get; set; }

    /// <summary>
    /// Gets or sets the width of the screenshot in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the screenshot in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the text extracted from the screenshot via OCR, if enabled.
    /// </summary>
    public string? OcrText { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail image data for quick preview in history list.
    /// </summary>
    public byte[]? ThumbnailData { get; set; }
}

/// <summary>
/// Defines the method used to capture a screenshot.
/// </summary>
public enum ScreenshotType
{
    /// <summary>
    /// User-selected rectangular region of the screen.
    /// </summary>
    Regional,

    /// <summary>
    /// Entire primary screen.
    /// </summary>
    FullScreen,

    /// <summary>
    /// Specific application window.
    /// </summary>
    Window,

    /// <summary>
    /// Multiple images combined into one.
    /// </summary>
    Stitched
}
