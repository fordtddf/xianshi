using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HardwareMonitor.Core.Models;
using HardwareMonitor.Services;

namespace HardwareMonitor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly AutoStartService _autoStartService = new();

    [ObservableProperty]
    private bool _autoStart;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private bool _showTemperatureAlert = true;

    [ObservableProperty]
    private float _temperatureAlertThreshold = 85;

    [ObservableProperty]
    private int _refreshInterval = 2;

    [ObservableProperty]
    private TrayDisplayMode _trayDisplayMode = TrayDisplayMode.Temperature;

    [ObservableProperty]
    private bool _showOverlay;

    [ObservableProperty]
    private ObservableCollection<DisplayItemConfig> _displayItems = new();

    public Array TrayDisplayModes => Enum.GetValues(typeof(TrayDisplayMode));

    public SettingsViewModel(SettingsService settings)
    {
        _settings = settings;
        LoadFromSettings();
    }

    private void LoadFromSettings()
    {
        var s = _settings.Current;
        AutoStart = _autoStartService.IsAutoStartEnabled();
        MinimizeToTray = s.MinimizeToTray;
        ShowTemperatureAlert = s.ShowTemperatureAlert;
        TemperatureAlertThreshold = s.TemperatureAlertThreshold;
        RefreshInterval = s.RefreshIntervalSeconds;
        TrayDisplayMode = s.TrayDisplayMode;
        ShowOverlay = s.ShowOverlay;

        DisplayItems.Clear();
        foreach (var item in s.DisplayItems)
            DisplayItems.Add(item);
    }

    [RelayCommand]
    private void Save()
    {
        _autoStartService.SetAutoStart(AutoStart);

        _settings.Update(s =>
        {
            s.MinimizeToTray = MinimizeToTray;
            s.ShowTemperatureAlert = ShowTemperatureAlert;
            s.TemperatureAlertThreshold = TemperatureAlertThreshold;
            s.RefreshIntervalSeconds = Math.Clamp(RefreshInterval, 1, 30);
            s.TrayDisplayMode = TrayDisplayMode;
            s.ShowOverlay = ShowOverlay;
        });
    }

    [RelayCommand]
    private void ToggleItemVisibility(DisplayItemConfig? item)
    {
        if (item != null)
        {
            item.IsVisible = !item.IsVisible;
            Save();
        }
    }
}
