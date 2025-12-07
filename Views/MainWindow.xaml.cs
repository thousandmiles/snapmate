using SnapMate.ViewModels;
using Wpf.Ui.Controls;

namespace SnapMate.Views;

public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.SetOwnerWindow(() => this);
        Loaded += async (s, e) => await _viewModel.LoadHistoryCommand.ExecuteAsync(null);
    }
}
