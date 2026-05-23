using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HardwareMonitor.Core.Models;

public class FanProfile
{
    public string Name { get; set; } = "";
    public FanMode Mode { get; set; }
    public List<FanCurvePoint> CurvePoints { get; set; } = new();
}

public class FanCurvePoint
{
    public float Temperature { get; set; }
    public float FanSpeedPercent { get; set; }
}

public static class FanPresets
{
    public static FanProfile Silent => new()
    {
        Name = "静音",
        Mode = FanMode.Silent,
        CurvePoints = new()
        {
            new() { Temperature = 30, FanSpeedPercent = 20 },
            new() { Temperature = 50, FanSpeedPercent = 30 },
            new() { Temperature = 70, FanSpeedPercent = 50 },
            new() { Temperature = 85, FanSpeedPercent = 80 },
            new() { Temperature = 95, FanSpeedPercent = 100 }
        }
    };

    public static FanProfile Balance => new()
    {
        Name = "平衡",
        Mode = FanMode.Balance,
        CurvePoints = new()
        {
            new() { Temperature = 30, FanSpeedPercent = 30 },
            new() { Temperature = 50, FanSpeedPercent = 50 },
            new() { Temperature = 65, FanSpeedPercent = 70 },
            new() { Temperature = 80, FanSpeedPercent = 90 },
            new() { Temperature = 90, FanSpeedPercent = 100 }
        }
    };

    public static FanProfile Performance => new()
    {
        Name = "性能",
        Mode = FanMode.Performance,
        CurvePoints = new()
        {
            new() { Temperature = 30, FanSpeedPercent = 50 },
            new() { Temperature = 45, FanSpeedPercent = 70 },
            new() { Temperature = 60, FanSpeedPercent = 85 },
            new() { Temperature = 75, FanSpeedPercent = 100 },
            new() { Temperature = 90, FanSpeedPercent = 100 }
        }
    };

    public static FanProfile GetDefault(FanMode mode) => mode switch
    {
        FanMode.Silent => Silent,
        FanMode.Balance => Balance,
        FanMode.Performance => Performance,
        _ => Balance
    };
}
