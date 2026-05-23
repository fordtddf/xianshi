using System.ComponentModel;
using System.Linq;
using System.Windows;
using HardwareMonitor.ViewModels;

namespace HardwareMonitor.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Register title update callback
        _viewModel.TitleUpdate = title =>
        {
            Dispatcher.Invoke(() => Title = title);
        };
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Minimize to tray, don't exit
        e.Cancel = true;
        Hide();
        base.OnClosing(e);
    }

    public void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }
}
