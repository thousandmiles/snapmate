using SnapMate.Models;
using System.IO;
using System.Windows.Media.Imaging;

namespace SnapMate.Services;

/// <summary>
/// Provides file saving operations for screenshots with automatic naming patterns.
/// </summary>
public interface IFileSaveService
{
    /// <summary>
    /// Saves an image using the settings-defined directory and naming pattern.
    /// </summary>
    /// <param name="image">The image to save.</param>
    /// <param name="settings">Application settings containing save preferences.</param>
    /// <returns>The full path of the saved file.</returns>
    Task<string> SaveImageAsync(BitmapSource image, AppSettings settings);

    /// <summary>
    /// Saves an image to a specific path with the specified format.
    /// </summary>
    /// <param name="image">The image to save.</param>
    /// <param name="filePath">The destination file path.</param>
    /// <param name="format">The image format to use.</param>
    /// <returns>The full path of the saved file.</returns>
  Task<string> SaveImageAsync(BitmapSource image, string filePath, ImageFormat format);
}

/// <summary>
/// Implementation of file save service supporting multiple image formats.
/// </summary>
public class FileSaveService : IFileSaveService
{
    /// <inheritdoc />
    public async Task<string> SaveImageAsync(BitmapSource image, AppSettings settings)
    {
  var fileName = GenerateFileName(settings.FileNamePattern, settings.DefaultFormat);
    var filePath = Path.Combine(settings.SaveDirectory, fileName);

      return await SaveImageAsync(image, filePath, settings.DefaultFormat);
    }

 /// <inheritdoc />
    public async Task<string> SaveImageAsync(BitmapSource image, string filePath, ImageFormat format)
    {
return await Task.Run(() =>
{
       Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

using var fileStream = new FileStream(filePath, FileMode.Create);
            BitmapEncoder encoder = format switch
 {
       ImageFormat.Png => new PngBitmapEncoder(),
  ImageFormat.Jpg => new JpegBitmapEncoder { QualityLevel = 95 },
    ImageFormat.Bmp => new BmpBitmapEncoder(),
    _ => new PngBitmapEncoder()
            };

       encoder.Frames.Add(BitmapFrame.Create(image));
  encoder.Save(fileStream);

    return filePath;
      });
    }

    /// <summary>
    /// Generates a filename using the configured pattern with date/time placeholders.
    /// </summary>
    /// <param name="pattern">The naming pattern (e.g., "Screenshot_{yyyy-MM-dd_HH-mm-ss}").</param>
    /// <param name="format">The image format for the file extension.</param>
    /// <returns>A generated filename with appropriate extension.</returns>
    private static string GenerateFileName(string pattern, ImageFormat format)
    {
  var now = DateTime.Now;
 var fileName = pattern
  .Replace("{yyyy}", now.Year.ToString("D4"))
         .Replace("{MM}", now.Month.ToString("D2"))
            .Replace("{dd}", now.Day.ToString("D2"))
     .Replace("{HH}", now.Hour.ToString("D2"))
     .Replace("{mm}", now.Minute.ToString("D2"))
       .Replace("{ss}", now.Second.ToString("D2"));

var extension = format switch
    {
   ImageFormat.Png => ".png",
            ImageFormat.Jpg => ".jpg",
       ImageFormat.Bmp => ".bmp",
        _ => ".png"
        };

        return fileName + extension;
    }
}
