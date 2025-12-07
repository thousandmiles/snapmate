using System.Windows.Media.Imaging;

namespace SnapMate.Services;

/// <summary>
/// Provides optical character recognition (OCR) functionality for extracting text from images.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Extracts text from the provided image using OCR.
    /// </summary>
    /// <param name="image">The image to extract text from.</param>
    /// <returns>The extracted text, or empty string if no text found.</returns>
    Task<string> ExtractTextAsync(BitmapSource image);

    /// <summary>
    /// Gets whether OCR functionality is available and properly configured.
    /// </summary>
    bool IsAvailable { get; }
}

/// <summary>
/// Implementation of OCR service using Tesseract engine.
/// </summary>
public class OcrService : IOcrService
{
    /// <inheritdoc />
    public bool IsAvailable => false; // Will be true once Tesseract is configured

    /// <inheritdoc />
    public Task<string> ExtractTextAsync(BitmapSource image)
    {
        throw new NotImplementedException("OCR text extraction will be implemented using Tesseract.NET library.");
    }
}
