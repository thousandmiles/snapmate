using System.Windows.Media.Imaging;

namespace SnapMate.Services;

/// <summary>
/// Provides clipboard operations for copying images and text.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Copies an image to the system clipboard.
    /// </summary>
    /// <param name="image">The image to copy.</param>
    void CopyImage(BitmapSource image);

    /// <summary>
    /// Copies text to the system clipboard.
    /// </summary>
    /// <param name="text">The text to copy.</param>
    void CopyText(string text);
}

/// <summary>
/// Implementation of clipboard service using Windows clipboard API.
/// </summary>
public class ClipboardService : IClipboardService
{
    /// <inheritdoc />
    public void CopyImage(BitmapSource image)
    {
        try
        {
            System.Windows.Clipboard.SetImage(image);
        }
        catch (Exception)
        {
            // Silently handle clipboard access errors (e.g., when clipboard is locked by another process)
        }
    }

    /// <inheritdoc />
    public void CopyText(string text)
    {
        try
        {
            System.Windows.Clipboard.SetText(text);
        }
        catch (Exception)
        {
            // Silently handle clipboard access errors (e.g., when clipboard is locked by another process)
        }
    }
}
