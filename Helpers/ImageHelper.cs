using System.IO;
using System.Windows.Media.Imaging;

namespace SnapMate.Helpers;

/// <summary>
/// Provides utility methods for image operations including thumbnail generation and format conversion.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Creates a thumbnail from a source image while maintaining aspect ratio.
    /// </summary>
    /// <param name="source">The source image.</param>
    /// <param name="maxWidth">Maximum width of the thumbnail.</param>
    /// <param name="maxHeight">Maximum height of the thumbnail.</param>
    /// <returns>A thumbnail bitmap scaled to fit within the specified dimensions.</returns>
    public static BitmapSource CreateThumbnail(BitmapSource source, int maxWidth, int maxHeight)
    {
        var scale = Math.Min((double)maxWidth / source.PixelWidth, (double)maxHeight / source.PixelHeight);

        var newWidth = (int)(source.PixelWidth * scale);
        var newHeight = (int)(source.PixelHeight * scale);

        var thumbnail = new TransformedBitmap(source, new System.Windows.Media.ScaleTransform(scale, scale));
        return thumbnail;
    }

    /// <summary>
    /// Converts a <see cref="BitmapSource"/> to a byte array in PNG format.
    /// Useful for storing images in databases.
    /// </summary>
    /// <param name="bitmapSource">The bitmap to convert.</param>
    /// <returns>PNG-encoded byte array.</returns>
    public static byte[] BitmapSourceToByteArray(BitmapSource bitmapSource)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Converts a byte array to a <see cref="BitmapSource"/>.
    /// Useful for loading images from databases.
    /// </summary>
    /// <param name="data">The PNG-encoded byte array.</param>
    /// <returns>A WPF-compatible bitmap source.</returns>
    public static BitmapSource ByteArrayToBitmapSource(byte[] data)
    {
        using var stream = new MemoryStream(data);
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        return decoder.Frames[0];
    }
}
