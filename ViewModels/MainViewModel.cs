using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SnapMate.Models;
using SnapMate.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SnapMate.ViewModels;

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
    private Func<Window?>? _getOwnerWindow;

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

    public void SetOwnerWindow(Func<Window?> getWindow)
    {
        _getOwnerWindow = getWindow;
    }

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

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        await _historyService.ClearAllAsync();
        Screenshots.Clear();
        CurrentImage = null;
        StatusMessage = "History cleared";
    }

    [RelayCommand]
    private async Task CaptureRegionAsync()
    {
        try
        {
            IsCapturing = true;
            StatusMessage = "Select a region...";

            var regionSelector = new Views.RegionSelectorWindow();
            var result = regionSelector.ShowDialog();

            if (result == true && regionSelector.HasSelection)
            {
                var region = regionSelector.SelectedRegion;
                StatusMessage = "Capturing selected region...";

                CurrentImage = await _captureService.CaptureRegionAsync(region);

                if (CurrentImage != null)
                {
                    await ProcessCapturedImageAsync(CurrentImage, ScreenshotType.Regional);
                    StatusMessage = $"Region captured: {region.Width}x{region.Height}";
                }
                else
                {
                    StatusMessage = "Failed to capture region";
                }
            }
            else
            {
                StatusMessage = "Region selection cancelled";
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

    [RelayCommand]
    private async Task CaptureFullScreenAsync()
    {
        try
        {
            IsCapturing = true;
            StatusMessage = "Capturing full screen...";

            var ownerWindow = _getOwnerWindow?.Invoke();

            if (ownerWindow != null)
            {
                ownerWindow.Hide();
                await Task.Delay(250);
            }

            try
            {
                CurrentImage = await _captureService.CaptureFullScreenAsync(ownerWindow);

                if (CurrentImage != null)
                {
                    await ProcessCapturedImageAsync(CurrentImage, ScreenshotType.FullScreen);
                    StatusMessage = "Full screen captured successfully";
                }
            }
            finally
            {
                if (ownerWindow != null)
                {
                    ownerWindow.Show();
                    ownerWindow.Activate();
                }
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

    [RelayCommand]
    private async Task CaptureWindowAsync()
    {
        try
        {
            IsCapturing = true;
            StatusMessage = "Select a window...";

            var windowSelector = new Views.WindowSelectorWindow();
            var result = windowSelector.ShowDialog();

            if (result == true && windowSelector.HasSelection)
            {
                StatusMessage = "Capturing window...";

                var ownerWindow = _getOwnerWindow?.Invoke();
                if (ownerWindow != null)
                {
                    ownerWindow.Hide();
                }

                await Task.Delay(150);

                try
                {
                    CurrentImage = await _captureService.CaptureWindowAsync(windowSelector.SelectedWindowHandle);

                    if (CurrentImage != null)
                    {
                        await ProcessCapturedImageAsync(CurrentImage, ScreenshotType.Window);
                        StatusMessage = "Window captured successfully";
                    }
                    else
                    {
                        StatusMessage = "Failed to capture window";
                    }
                }
                finally
                {
                    if (ownerWindow != null)
                    {
                        ownerWindow.Show();
                        ownerWindow.Activate();
                    }
                }
            }
            else
            {
                StatusMessage = "Window capture cancelled";
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

    [RelayCommand]
    private void CopyToClipboard()
    {
        if (CurrentImage != null)
        {
            _clipboardService.CopyImage(CurrentImage);
            StatusMessage = "Copied to clipboard";
        }
    }

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

        if (_settingsService.Settings.AutoSave)
        {
            screenshot.FilePath = await _fileSaveService.SaveImageAsync(image, _settingsService.Settings);
            screenshot.FileSize = new FileInfo(screenshot.FilePath).Length;
        }

        if (_settingsService.Settings.CopyToClipboard)
        {
            _clipboardService.CopyImage(image);
        }

        if (_settingsService.Settings.EnableOcr && _ocrService.IsAvailable)
        {
            screenshot.OcrText = await _ocrService.ExtractTextAsync(image);
        }

        await _historyService.AddAsync(screenshot);
        await LoadHistoryAsync();
    }
}
