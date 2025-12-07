using SnapMate.ViewModels;
using Wpf.Ui.Controls;

namespace SnapMate.Views;

/// <summary>
/// Main application window for SnapMate screenshot tool.
/// Uses WPF-UI FluentWindow for modern design with Mica backdrop.
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the main window with dependency-injected view model.
    /// </summary>
    /// <param name="viewModel">The main view model instance.</param>
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
      DataContext = viewModel;

        // Load screenshot history after window is fully initialized to avoid race conditions
        Loaded += async (s, e) => await _viewModel.LoadHistoryCommand.ExecuteAsync(null);
  }
}
