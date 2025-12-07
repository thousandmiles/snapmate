using SnapMate.Models;
using System.IO;
using System.Windows.Media.Imaging;

namespace SnapMate.Services;

public interface IFileSaveService
{
    Task<string> SaveImageAsync(BitmapSource image, AppSettings settings);
    Task<string> SaveImageAsync(BitmapSource image, string filePath, ImageFormat format);
}

public class FileSaveService : IFileSaveService
{
    public async Task<string> SaveImageAsync(BitmapSource image, AppSettings settings)
    {
        var fileName = GenerateFileName(settings.DefaultFormat, settings.SaveDirectory);
        var filePath = Path.Combine(settings.SaveDirectory, fileName);
        return await SaveImageAsync(image, filePath, settings.DefaultFormat);
    }

    public async Task<string> SaveImageAsync(BitmapSource image, string filePath, ImageFormat format)
    {
        return await Task.Run(() =>
                {
                    try
                    {
                        var directory = Path.GetDirectoryName(filePath);
                        if (string.IsNullOrEmpty(directory))
                            throw new InvalidOperationException("Invalid file path: directory cannot be determined");

                        Directory.CreateDirectory(directory);

                        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
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
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        throw new InvalidOperationException($"Access denied to save file: {filePath}", ex);
                    }
                    catch (IOException ex)
                    {
                        throw new InvalidOperationException($"Failed to save file: {ex.Message}", ex);
                    }
                });
    }

    private static string GenerateFileName(ImageFormat format, string saveDirectory)
    {
        var now = DateTime.Now;
        var baseFileName = $"Screenshot_{now:yyyy-MM-dd_HH-mm-ss-fff}";

        var extension = format switch
        {
            ImageFormat.Png => ".png",
            ImageFormat.Jpg => ".jpg",
            ImageFormat.Bmp => ".bmp",
            _ => ".png"
        };

        var fileName = baseFileName + extension;
        var fullPath = Path.Combine(saveDirectory, fileName);

        if (!File.Exists(fullPath))
            return fileName;

        int counter = 1;
        while (File.Exists(fullPath))
        {
            fileName = $"{baseFileName}_{counter}{extension}";
            fullPath = Path.Combine(saveDirectory, fileName);
            counter++;

            if (counter > 9999)
                throw new InvalidOperationException("Too many files with the same name pattern");
        }

        return fileName;
    }
}
