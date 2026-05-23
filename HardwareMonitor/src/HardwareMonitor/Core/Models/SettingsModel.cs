using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HardwareMonitor.Core.Models;

public class AppSettings
{
    public bool AutoStart { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool ShowTemperatureAlert { get; set; } = true;
    public float TemperatureAlertThreshold { get; set; } = 85f;
    public int RefreshIntervalSeconds { get; set; } = 2;
    public TrayDisplayMode TrayDisplayMode { get; set; } = TrayDisplayMode.Temperature;
    public bool ShowOverlay { get; set; }
    public bool OverlayTopMost { get; set; } = true;
    public double OverlayOpacity { get; set; } = 0.85;
    public List<DisplayItemConfig> DisplayItems { get; set; } = DefaultDisplayItems();
    public FanMode FanMode { get; set; } = FanMode.Auto;
    public FanProfile? CustomFanProfile { get; set; }
    public double WindowWidth { get; set; } = 900;
    public double WindowHeight { get; set; } = 650;

    private static List<DisplayItemConfig> DefaultDisplayItems() => new()
    {
        new() { Type = DisplayItemType.Cpu, IsVisible = true, Order = 0 },
        new() { Type = DisplayItemType.Gpu, IsVisible = true, Order = 1 },
        new() { Type = DisplayItemType.Memory, IsVisible = true, Order = 2 },
        new() { Type = DisplayItemType.Network, IsVisible = true, Order = 3 },
        new() { Type = DisplayItemType.Disk, IsVisible = true, Order = 4 },
        new() { Type = DisplayItemType.Fan, IsVisible = true, Order = 5 },
        new() { Type = DisplayItemType.Battery, IsVisible = true, Order = 6 }
    };
}

public class DisplayItemConfig
{
    public DisplayItemType Type { get; set; }
    public bool IsVisible { get; set; } = true;
    public int Order { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisplayItemType
{
    Cpu,
    Gpu,
    Memory,
    Network,
    Disk,
    Fan,
    Battery
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TrayDisplayMode
{
    None,
    Temperature,
    Battery
}
