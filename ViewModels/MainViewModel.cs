using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SnapMate.Models;
using SnapMate.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SnapMate.ViewModels;

/// <summary>
/// Main view model for the SnapMate application.
/// Handles screenshot capture, editing, history management, and all user interactions.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IScreenshotCaptureService _captureService;
    private readonly IImageEditingService _editingService;
    private readonly IHistoryService _historyService;
    private readonly IOcrService _ocrService;
    private readonly IHotkeyService _hotkeyService;
    private readonly ISettingsService _settingsService;
    private readonly IClipboardService _clipboardService;
    private readonly IFileSaveService _fileSaveService;

    [ObservableProperty]
    private BitmapSource? _currentImage;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isCapturing;

    [ObservableProperty]
    private ObservableCollection<Screenshot> _screenshots = new();

    [ObservableProperty]
    private Screenshot? _selectedScreenshot;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    /// <summary>
    /// Collection of annotations applied to the current image.
    /// </summary>
    public ObservableCollection<Annotation> Annotations { get; } = new();

    public MainViewModel(
        IScreenshotCaptureService captureService,
        IImageEditingService editingService,
        IHistoryService historyService,
        IOcrService ocrService,
        IHotkeyService hotkeyService,
        ISettingsService settingsService,
        IClipboardService clipboardService,
        IFileSaveService fileSaveService)
    {
        _captureService = captureService;
        _editingService = editingService;
        _historyService = historyService;
        _ocrService = ocrService;
        _hotkeyService = hotkeyService;
        _settingsService = settingsService;
        _clipboardService = clipboardService;
        _fileSaveService = fileSaveService;
    }

    /// <summary>
    /// Called when a screenshot is selected from the history list.
    /// Loads the image file and displays it in the editor.
    /// </summary>
    partial void OnSelectedScreenshotChanged(Screenshot? value)
    {
        if (value != null && File.Exists(value.FilePath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(value.FilePath);
                bitmap.EndInit();
                CurrentImage = bitmap;
                StatusMessage = $"Loaded: {Path.GetFileName(value.FilePath)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading image: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Loads all screenshots from the history database.
    /// </summary>
    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        var items = await _historyService.GetAllAsync();
        Screenshots.Clear();
        foreach (var item in items)
        {
            Screenshots.Add(item);
        }
    }

    /// <summary>
    /// Clears all screenshots from history and database.
    /// </summary>
    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        await _historyService.ClearAllAsync();
        Screenshots.Clear();
        CurrentImage = null;
        StatusMessage = "History cleared";
    }

    /// <summary>
    /// Initiates region-based screenshot capture.
    /// Currently uses placeholder coordinates - will show region selector in future implementation.
    /// </summary>
    [RelayCommand]
    private async Task CaptureRegionAsync()
    {
        try
        {
            IsCapturing = true;
            StatusMessage = "Select a region...";

            // Placeholder region - will be replaced with interactive region selector
            var region = new Int32Rect(0, 0, 800, 600);

            CurrentImage = await _captureService.CaptureRegionAsync(region);

            if (CurrentImage != null)
            {
                await ProcessCapturedImageAsync(CurrentImage, ScreenshotType.Regional);
                StatusMessage = "Region captured successfully";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsCapturing = false;
        }
    }

    /// <summary>
    /// Captures the entire primary screen.
    /// </summary>
    [RelayCommand]
    private async Task CaptureFullScreenAsync()
    {
        try
        {
            IsCapturing = true;
            StatusMessage = "Capturing full screen...";

            CurrentImage = await _captureService.CaptureFullScreenAsync();

            if (CurrentImage != null)
            {
                await ProcessCapturedImageAsync(CurrentImage, ScreenshotType.FullScreen);
                StatusMessage = "Full screen captured successfully";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsCapturing = false;
        }
    }

    /// <summary>
    /// Copies the current image to the system clipboard.
    /// </summary>
    [RelayCommand]
    private void CopyToClipboard()
    {
        if (CurrentImage != null)
        {
            _clipboardService.CopyImage(CurrentImage);
            StatusMessage = "Copied to clipboard";
        }
    }

    /// <summary>
    /// Saves the current image to disk using configured settings.
    /// </summary>
    [RelayCommand]
    private async Task SaveImageAsync()
    {
        if (CurrentImage != null)
        {
            try
            {
                var filePath = await _fileSaveService.SaveImageAsync(CurrentImage, _settingsService.Settings);
                StatusMessage = $"Saved to {filePath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Save error: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Extracts text from the current image using OCR and copies it to clipboard.
    /// </summary>
    [RelayCommand]
    private async Task ExtractTextAsync()
    {
        if (CurrentImage != null && _ocrService.IsAvailable)
        {
            try
            {
                StatusMessage = "Extracting text...";
                var text = await _ocrService.ExtractTextAsync(CurrentImage);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    _clipboardService.CopyText(text);
                    StatusMessage = "Text extracted and copied to clipboard";
                }
                else
                {
                    StatusMessage = "No text found";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"OCR error: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Processes a captured screenshot: saves to disk, adds to history, performs OCR if enabled.
    /// </summary>
    /// <param name="image">The captured image.</param>
    /// <param name="type">The type of screenshot capture.</param>
    private async Task ProcessCapturedImageAsync(BitmapSource image, ScreenshotType type)
    {
        var screenshot = new Screenshot
        {
            FilePath = string.Empty,
            CapturedAt = DateTime.Now,
            Type = type,
            Width = image.PixelWidth,
            Height = image.PixelHeight
        };

        // Auto-save if enabled
        if (_settingsService.Settings.AutoSave)
        {
            screenshot.FilePath = await _fileSaveService.SaveImageAsync(image, _settingsService.Settings);
            screenshot.FileSize = new FileInfo(screenshot.FilePath).Length;
        }

        // Copy to clipboard if enabled
        if (_settingsService.Settings.CopyToClipboard)
        {
            _clipboardService.CopyImage(image);
        }

        // OCR if enabled
        if (_settingsService.Settings.EnableOcr && _ocrService.IsAvailable)
        {
            screenshot.OcrText = await _ocrService.ExtractTextAsync(image);
        }

        // Save to history
        await _historyService.AddAsync(screenshot);

        // Reload history
        await LoadHistoryAsync();
    }
}
