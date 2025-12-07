using System.Windows;
using System.Windows.Media.Imaging;

namespace SnapMate.Services;

/// <summary>
/// Provides screenshot capture functionality for different screen regions and windows.
/// </summary>
public interface IScreenshotCaptureService
{
    /// <summary>
    /// Captures a specific rectangular region of the screen.
    /// </summary>
    /// <param name="region">The screen region to capture.</param>
    /// <returns>The captured image, or null if capture fails.</returns>
    Task<BitmapSource?> CaptureRegionAsync(Int32Rect region);

    /// <summary>
    /// Captures the entire primary screen.
    /// </summary>
    /// <returns>The captured screen image, or null if capture fails.</returns>
    Task<BitmapSource?> CaptureFullScreenAsync();

    /// <summary>
    /// Captures a specific window by its handle.
    /// </summary>
    /// <param name="windowHandle">The window handle to capture.</param>
    /// <returns>The captured window image, or null if capture fails.</returns>
    Task<BitmapSource?> CaptureWindowAsync(IntPtr windowHandle);
}

/// <summary>
/// Implementation of screenshot capture service using GDI+ for screen capture operations.
/// </summary>
public class ScreenshotCaptureService : IScreenshotCaptureService
{
    /// <inheritdoc />
    public async Task<BitmapSource?> CaptureRegionAsync(Int32Rect region)
    {
        return await Task.Run(() =>
        {
            try
            {
                var bmp = new System.Drawing.Bitmap(region.Width, region.Height);
                using var graphics = System.Drawing.Graphics.FromImage(bmp);
                graphics.CopyFromScreen(region.X, region.Y, 0, 0,
                    new System.Drawing.Size(region.Width, region.Height));

                return ConvertToBitmapSource(bmp);
            }
            catch
            {
                return null;
            }
        });
    }

    /// <inheritdoc />
    public async Task<BitmapSource?> CaptureFullScreenAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                var screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                var bmp = new System.Drawing.Bitmap(screenWidth, screenHeight);
                using var graphics = System.Drawing.Graphics.FromImage(bmp);
                graphics.CopyFromScreen(0, 0, 0, 0,
                    new System.Drawing.Size(screenWidth, screenHeight));

                return ConvertToBitmapSource(bmp);
            }
            catch
            {
                return null;
            }
        });
    }

    /// <inheritdoc />
    public Task<BitmapSource?> CaptureWindowAsync(IntPtr windowHandle)
    {
        throw new NotImplementedException("Window-specific capture will use Win32 API to capture individual window contents.");
    }

    /// <summary>
    /// Converts a GDI+ bitmap to a WPF BitmapSource.
    /// </summary>
    /// <param name="bitmap">The bitmap to convert.</param>
    /// <returns>A WPF-compatible BitmapSource.</returns>
    private static BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
    {
        var hBitmap = bitmap.GetHbitmap();
        try
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            NativeMethods.DeleteObject(hBitmap);
        }
    }
}

/// <summary>
/// P/Invoke declarations for native GDI operations.
/// </summary>
internal static class NativeMethods
{
    /// <summary>
    /// Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources.
    /// </summary>
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);
}
