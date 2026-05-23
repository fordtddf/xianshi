using System;
using System.Threading;
using System.Windows;
using HardwareMonitor.Services;
using HardwareMonitor.ViewModels;
using HardwareMonitor.Views;

namespace HardwareMonitor;

public partial class App : Application
{
    private Mutex? _mutex;
    private MainWindow? _mainWindow;
    private OverlayWindow? _overlayWindow;
    private MainViewModel? _viewModel;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Single instance check
        _mutex = new Mutex(true, Core.Constants.MutexName, out bool isNew);
        if (!isNew)
        {
            MessageBox.Show("程序已在运行中。", Core.Constants.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // Initialize services
        var settingsService = new SettingsService();
        var hardwareMonitor = new HardwareMonitorService();
        var networkMonitor = new NetworkMonitorService();
        var fanControl = new FanControlService(settingsService);
        var batteryService = new BatteryService();
        var trayIcon = new TrayIconService();

        // Start network monitoring
        networkMonitor.Start();

        // Create ViewModel
        _viewModel = new MainViewModel(
            hardwareMonitor,
            networkMonitor,
            fanControl,
            batteryService,
            trayIcon,
            settingsService);

        // Create windows
        _mainWindow = new MainWindow(_viewModel);
        _overlayWindow = new OverlayWindow { DataContext = _viewModel };

        // Setup tray icon
        trayIcon.Initialize();
        trayIcon.ShowMainWindow += () => _mainWindow.ShowFromTray();
        trayIcon.ToggleOverlay += () => _viewModel.ToggleOverlayCommand.Execute(null);
        trayIcon.ExitApplication += () =>
        {
            _viewModel.Cleanup();
            Shutdown();
        };

        // Show main window
        _mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _viewModel?.Cleanup();
        base.OnExit(e);
    }
}
