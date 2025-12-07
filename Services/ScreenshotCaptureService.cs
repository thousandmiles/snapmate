using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace SnapMate.Services;

public interface IScreenshotCaptureService
{
    Task<BitmapSource?> CaptureRegionAsync(Int32Rect region);
    Task<BitmapSource?> CaptureFullScreenAsync(Window? sourceWindow = null);
    Task<BitmapSource?> CaptureWindowAsync(IntPtr windowHandle);
}

public class ScreenshotCaptureService : IScreenshotCaptureService
{
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

    public async Task<BitmapSource?> CaptureFullScreenAsync(Window? sourceWindow = null)
    {
        System.Drawing.Rectangle screenBounds;

        if (sourceWindow != null)
        {
            var windowRect = new System.Drawing.Rectangle(
          (int)sourceWindow.Left,
                (int)sourceWindow.Top,
    (int)sourceWindow.Width,
     (int)sourceWindow.Height);

            var targetScreen = System.Windows.Forms.Screen.FromRectangle(windowRect);
            screenBounds = targetScreen.Bounds;
        }
        else
        {
            if (System.Windows.Forms.Screen.PrimaryScreen is { } primaryScreen)
            {
                screenBounds = primaryScreen.Bounds;
            }
            else
            {
                throw new InvalidOperationException("No primary screen detected.");
            }
        }

        return await Task.Run(() =>
          {
              try
              {
                  var bmp = new System.Drawing.Bitmap(screenBounds.Width, screenBounds.Height);
                  using var graphics = System.Drawing.Graphics.FromImage(bmp);
                  graphics.CopyFromScreen(
            screenBounds.X,
                  screenBounds.Y,
                0,
                0,
                   new System.Drawing.Size(screenBounds.Width, screenBounds.Height));

                  return ConvertToBitmapSource(bmp);
              }
              catch
              {
                  return null;
              }
          });
    }

    public async Task<BitmapSource?> CaptureWindowAsync(IntPtr windowHandle)
    {
        return await Task.Run(async () =>
    {
        try
        {
            if (!NativeMethods.IsWindow(windowHandle) || !NativeMethods.IsWindowVisible(windowHandle))
                return null;

            if (!NativeMethods.GetWindowRect(windowHandle, out var rect))
                return null;

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (width <= 0 || height <= 0)
                return null;

            NativeMethods.SetForegroundWindow(windowHandle);
            NativeMethods.BringWindowToTop(windowHandle);

            await Task.Delay(300);

            var bmp = new System.Drawing.Bitmap(width, height);
            using var graphics = System.Drawing.Graphics.FromImage(bmp);

            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0,
                        new System.Drawing.Size(width, height),
                     System.Drawing.CopyPixelOperation.SourceCopy);

            return ConvertToBitmapSource(bmp);
        }
        catch
        {
            return null;
        }
    });
    }

    private static BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
    {
        var hBitmap = bitmap.GetHbitmap();
        try
        {
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                         hBitmap,
                         IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            bitmapSource.Freeze();
            return bitmapSource;
        }
        finally
        {
            NativeMethods.DeleteObject(hBitmap);
        }
    }
}

internal static class NativeMethods
{
    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
