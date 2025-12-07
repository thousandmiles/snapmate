using SnapMate.ViewModels;
using Wpf.Ui.Controls;
using System.Windows.Input;
using System.Windows;

namespace SnapMate.Views;

public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;
    private double _currentZoom = 1.0;
    private const double MinZoom = 0.1;
    private const double MaxZoom = 10.0;
    private const double ZoomStep = 0.1;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.SetOwnerWindow(() => this);
        Loaded += async (s, e) => await _viewModel.LoadHistoryCommand.ExecuteAsync(null);
    
        // Register Ctrl+0 keyboard shortcut for reset zoom
        KeyDown += MainWindow_KeyDown;
    }

    private void ImageScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
if (Keyboard.Modifiers != ModifierKeys.Control)
 return;

        e.Handled = true;

        // Calculate new zoom level
     double zoomChange = e.Delta > 0 ? ZoomStep : -ZoomStep;
        double newZoom = _currentZoom + zoomChange;

  // Clamp zoom level
        newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, newZoom));

        if (Math.Abs(newZoom - _currentZoom) < 0.001)
    return;

    // Get mouse position relative to the image
 var mousePos = e.GetPosition(DisplayImage);
   var scrollViewer = ImageScrollViewer;

   // Calculate the point to zoom into
        double offsetX = mousePos.X / DisplayImage.ActualWidth;
  double offsetY = mousePos.Y / DisplayImage.ActualHeight;

        // Apply zoom
        _currentZoom = newZoom;
        ImageScaleTransform.ScaleX = _currentZoom;
        ImageScaleTransform.ScaleY = _currentZoom;

        // Update zoom display
        UpdateZoomDisplay();

        // Adjust scroll position to zoom towards mouse cursor
        scrollViewer.UpdateLayout();
        scrollViewer.ScrollToHorizontalOffset(offsetX * scrollViewer.ScrollableWidth);
  scrollViewer.ScrollToVerticalOffset(offsetY * scrollViewer.ScrollableHeight);
 }

    private void ResetZoom_Click(object sender, RoutedEventArgs e)
    {
        ResetZoom();
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.D0 && Keyboard.Modifiers == ModifierKeys.Control)
      {
         e.Handled = true;
          ResetZoom();
    }
    }

    private void ResetZoom()
{
        _currentZoom = 1.0;
  ImageScaleTransform.ScaleX = _currentZoom;
        ImageScaleTransform.ScaleY = _currentZoom;
        UpdateZoomDisplay();
    }

    private void UpdateZoomDisplay()
  {
        ZoomLevelText.Text = $"Zoom: {_currentZoom * 100:F0}%";
    }
}
