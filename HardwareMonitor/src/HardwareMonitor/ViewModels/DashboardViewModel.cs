using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using HardwareMonitor.Core.Models;

namespace HardwareMonitor.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private CpuInfo _cpu = new();

    [ObservableProperty]
    private GpuInfo _gpu = new();

    [ObservableProperty]
    private MemoryInfo _memory = new();

    [ObservableProperty]
    private NetworkInfo _network = new();

    [ObservableProperty]
    private DiskInfo _disk = new();

    [ObservableProperty]
    private FanInfo _fan = new();

    [ObservableProperty]
    private BatteryInfo? _battery;

    [ObservableProperty]
    private bool _hasBattery;

    [ObservableProperty]
    private ObservableCollection<DisplayItemViewModel> _displayItems = new();

    public DashboardViewModel()
    {
        // Initialize with default display items
        DisplayItems.Add(new DisplayItemViewModel { Type = DisplayItemType.Cpu, Title = "CPU", IsVisible = true, Order = 0 });
        DisplayItems.Add(new DisplayItemViewModel { Type = DisplayItemType.Gpu, Title = "GPU", IsVisible = true, Order = 1 });
        DisplayItems.Add(new DisplayItemViewModel { Type = DisplayItemType.Memory, Title = "内存", IsVisible = true, Order = 2 });
        DisplayItems.Add(new DisplayItemViewModel { Type = DisplayItemType.Network, Title = "网络", IsVisible = true, Order = 3 });
        DisplayItems.Add(new DisplayItemViewModel { Type = DisplayItemType.Disk, Title = "硬盘", IsVisible = true, Order = 4 });
        DisplayItems.Add(new DisplayItemViewModel { Type = DisplayItemType.Fan, Title = "风扇", IsVisible = true, Order = 5 });
        DisplayItems.Add(new DisplayItemViewModel { Type = DisplayItemType.Battery, Title = "电池", IsVisible = false, Order = 6 });
    }

    public void UpdateFromSnapshot(HardwareSnapshot snapshot)
    {
        if (snapshot.Cpu != null) Cpu = snapshot.Cpu;
        if (snapshot.Gpu != null) Gpu = snapshot.Gpu;
        if (snapshot.Memory != null) Memory = snapshot.Memory;
        if (snapshot.Network != null) Network = snapshot.Network;
        if (snapshot.Disk != null) Disk = snapshot.Disk;
        if (snapshot.Fan != null) Fan = snapshot.Fan;

        if (snapshot.Battery != null)
        {
            Battery = snapshot.Battery;
            HasBattery = true;
            var batteryItem = DisplayItems.FirstOrDefault(d => d.Type == DisplayItemType.Battery);
            if (batteryItem != null) batteryItem.IsVisible = true;
        }

        // Update display items with latest data
        foreach (var item in DisplayItems)
        {
            item.UpdateFromSnapshot(snapshot);
        }
    }

    public void ApplyDisplayConfig(ObservableCollection<DisplayItemConfig> configs)
    {
        foreach (var config in configs)
        {
            var item = DisplayItems.FirstOrDefault(d => d.Type == config.Type);
            if (item != null)
            {
                item.IsVisible = config.IsVisible;
                item.Order = config.Order;
            }
        }
    }
}

public partial class DisplayItemViewModel : ObservableObject
{
    [ObservableProperty]
    private DisplayItemType _type;

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private int _order;

    [ObservableProperty]
    private string _primaryValue = "";

    [ObservableProperty]
    private string _secondaryValue = "";

    [ObservableProperty]
    private string _tertiaryValue = "";

    [ObservableProperty]
    private float _primaryMetric;

    [ObservableProperty]
    private string _icon = "";

    public void UpdateFromSnapshot(HardwareSnapshot snapshot)
    {
        switch (Type)
        {
            case DisplayItemType.Cpu when snapshot.Cpu != null:
                PrimaryValue = $"{snapshot.Cpu.TotalLoad:F0}%";
                SecondaryValue = $"{snapshot.Cpu.Temperature:F0}°C";
                TertiaryValue = $"{snapshot.Cpu.CoreClock:F0} MHz";
                PrimaryMetric = snapshot.Cpu.TotalLoad;
                break;

            case DisplayItemType.Gpu when snapshot.Gpu != null:
                PrimaryValue = $"{snapshot.Gpu.CoreLoad:F0}%";
                SecondaryValue = $"{snapshot.Gpu.Temperature:F0}°C";
                TertiaryValue = FormatBytes(snapshot.Gpu.MemoryUsed) + " / " + FormatBytes(snapshot.Gpu.MemoryTotal);
                PrimaryMetric = snapshot.Gpu.CoreLoad;
                break;

            case DisplayItemType.Memory when snapshot.Memory != null:
                PrimaryValue = $"{snapshot.Memory.UsagePercent:F0}%";
                SecondaryValue = FormatBytes(snapshot.Memory.UsedPhysical) + " / " + FormatBytes(snapshot.Memory.TotalPhysical);
                TertiaryValue = "";
                PrimaryMetric = snapshot.Memory.UsagePercent;
                break;

            case DisplayItemType.Network when snapshot.Network != null:
                PrimaryValue = "↑ " + FormatSpeed(snapshot.Network.UploadSpeed);
                SecondaryValue = "↓ " + FormatSpeed(snapshot.Network.DownloadSpeed);
                TertiaryValue = "";
                PrimaryMetric = 0;
                break;

            case DisplayItemType.Disk when snapshot.Disk != null:
                PrimaryValue = $"{snapshot.Disk.UsagePercent:F0}%";
                SecondaryValue = $"{snapshot.Disk.Temperature:F0}°C";
                TertiaryValue = FormatBytes(snapshot.Disk.FreeSpace) + " 可用";
                PrimaryMetric = snapshot.Disk.UsagePercent;
                break;

            case DisplayItemType.Fan when snapshot.Fan != null:
                PrimaryValue = $"{snapshot.Fan.CpuFanSpeed:F0} RPM";
                SecondaryValue = snapshot.Fan.IsAutoMode ? "自动" : "手动";
                TertiaryValue = "";
                PrimaryMetric = 0;
                break;

            case DisplayItemType.Battery when snapshot.Battery != null:
                PrimaryValue = $"{snapshot.Battery.ChargeLevel:F0}%";
                SecondaryValue = snapshot.Battery.ChargeStatus;
                TertiaryValue = $"健康度: {snapshot.Battery.Health:F0}%";
                PrimaryMetric = snapshot.Battery.ChargeLevel;
                break;
        }
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1L << 30) return $"{bytes / (double)(1L << 30):F1} GB";
        if (bytes >= 1L << 20) return $"{bytes / (double)(1L << 20):F1} MB";
        if (bytes >= 1L << 10) return $"{bytes / (double)(1L << 10):F1} KB";
        return $"{bytes} B";
    }

    private static string FormatSpeed(float bytesPerSec)
    {
        if (bytesPerSec >= 1 << 20) return $"{bytesPerSec / (1 << 20):F1} MB/s";
        if (bytesPerSec >= 1 << 10) return $"{bytesPerSec / (1 << 10):F1} KB/s";
        return $"{bytesPerSec:F0} B/s";
    }
}
