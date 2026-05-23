using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HardwareMonitor.Core.Models;
using HardwareMonitor.Services;

namespace HardwareMonitor.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HardwareMonitorService _hardwareMonitor;
    private readonly NetworkMonitorService _networkMonitor;
    private readonly FanControlService _fanControl;
    private readonly BatteryService _batteryService;
    private readonly TrayIconService _trayIcon;
    private readonly SettingsService _settings;
    private readonly DispatcherTimer _refreshTimer;
    private readonly DispatcherTimer _alertTimer;

    public Action<string>? TitleUpdate;

    [ObservableProperty]
    private HardwareSnapshot _snapshot = new();

    [ObservableProperty]
    private DashboardViewModel _dashboard;

    [ObservableProperty]
    private FanViewModel _fan;

    [ObservableProperty]
    private SettingsViewModel _settingsVm;

    [ObservableProperty]
    private object _currentPage;

    [ObservableProperty]
    private bool _isOverlayVisible;

    [ObservableProperty]
    private string _alertMessage = "";

    [ObservableProperty]
    private bool _isAlertVisible;

    public MainViewModel(
        HardwareMonitorService hardwareMonitor,
        NetworkMonitorService networkMonitor,
        FanControlService fanControl,
        BatteryService batteryService,
        TrayIconService trayIcon,
        SettingsService settings)
    {
        _hardwareMonitor = hardwareMonitor;
        _networkMonitor = networkMonitor;
        _fanControl = fanControl;
        _batteryService = batteryService;
        _trayIcon = trayIcon;
        _settings = settings;

        _dashboard = new DashboardViewModel();
        _fan = new FanViewModel(fanControl, settings);
        _settingsVm = new SettingsViewModel(settings);
        _currentPage = _dashboard;

        // Hardware refresh timer
        var interval = TimeSpan.FromSeconds(Math.Max(1, settings.Current.RefreshIntervalSeconds));
        _refreshTimer = new DispatcherTimer { Interval = interval };
        _refreshTimer.Tick += RefreshTimer_Tick;
        _refreshTimer.Start();

        // Alert check timer (less frequent)
        _alertTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _alertTimer.Tick += AlertTimer_Tick;
        _alertTimer.Start();

        settings.SettingsChanged += OnSettingsChanged;
    }

    private void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            var snapshot = _hardwareMonitor.TakeSnapshot();
            snapshot.Network = _networkMonitor.CurrentInfo;

            // Merge battery info from dedicated service if LibreHardwareMonitor didn't get it
            if (snapshot.Battery == null || snapshot.Battery.ChargeLevel == 0)
            {
                snapshot.Battery = _batteryService.GetBatteryInfo();
            }

            Snapshot = snapshot;
            Dashboard.UpdateFromSnapshot(snapshot);

            // Update tray icon
            _trayIcon.UpdateIcon(snapshot, _settings.Current.TrayDisplayMode);

            // Update taskbar title
            var cpu = snapshot.Cpu != null ? $"CPU {snapshot.Cpu.Temperature:F0}°C {snapshot.Cpu.TotalLoad:F0}%" : "";
            var gpu = snapshot.Gpu != null ? $"GPU {snapshot.Gpu.Temperature:F0}°C {snapshot.Gpu.CoreLoad:F0}%" : "";
            var mem = snapshot.Memory != null ? $"RAM {snapshot.Memory.UsagePercent:F0}%" : "";
            var title = string.Join("  |  ", new[] { cpu, gpu, mem }.Where(x => !string.IsNullOrEmpty(x)));
            if (!string.IsNullOrEmpty(title))
                TitleUpdate?.Invoke(title);
        }
        catch { }
    }

    private void AlertTimer_Tick(object? sender, EventArgs e)
    {
        if (!_settings.Current.ShowTemperatureAlert) return;

        var temp = Snapshot.Cpu?.Temperature ?? 0;
        var threshold = _settings.Current.TemperatureAlertThreshold;

        if (temp >= threshold)
        {
            AlertMessage = $"CPU 温度过高: {temp:F0}°C (阈值: {threshold:F0}°C)";
            IsAlertVisible = true;

            _trayIcon.ShowNotification("温度警告", AlertMessage, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
        }
        else
        {
            IsAlertVisible = false;
        }
    }

    private void OnSettingsChanged()
    {
        _refreshTimer.Interval = TimeSpan.FromSeconds(
            Math.Max(1, _settings.Current.RefreshIntervalSeconds));
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        CurrentPage = page switch
        {
            "Dashboard" => Dashboard,
            "Fan" => Fan,
            "Settings" => SettingsVm,
            _ => Dashboard
        };
    }

    [RelayCommand]
    private void ToggleOverlay()
    {
        IsOverlayVisible = !IsOverlayVisible;
    }

    public void Cleanup()
    {
        _refreshTimer.Stop();
        _alertTimer.Stop();
        _hardwareMonitor.Dispose();
        _networkMonitor.Dispose();
        _fanControl.Dispose();
        _trayIcon.Dispose();
    }
}
