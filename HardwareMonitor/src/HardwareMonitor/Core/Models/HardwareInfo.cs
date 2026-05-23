using System;
using System.Collections.ObjectModel;

namespace HardwareMonitor.Core.Models;

public class HardwareSnapshot
{
    public CpuInfo? Cpu { get; set; }
    public GpuInfo? Gpu { get; set; }
    public MemoryInfo? Memory { get; set; }
    public NetworkInfo? Network { get; set; }
    public DiskInfo? Disk { get; set; }
    public FanInfo? Fan { get; set; }
    public BatteryInfo? Battery { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class CpuInfo
{
    public string Name { get; set; } = "";
    public float Temperature { get; set; }
    public float TotalLoad { get; set; }
    public float CoreClock { get; set; }
    public float[] CoreLoads { get; set; } = Array.Empty<float>();
    public float[] CoreTemperatures { get; set; } = Array.Empty<float>();
}

public class GpuInfo
{
    public string Name { get; set; } = "";
    public float Temperature { get; set; }
    public float CoreLoad { get; set; }
    public float MemoryLoad { get; set; }
    public float FanSpeed { get; set; }
    public float CoreClock { get; set; }
    public float MemoryClock { get; set; }
    public long MemoryUsed { get; set; }
    public long MemoryTotal { get; set; }
    public float Power { get; set; }
}

public class MemoryInfo
{
    public long TotalPhysical { get; set; }
    public long UsedPhysical { get; set; }
    public long AvailablePhysical { get; set; }
    public float UsagePercent { get; set; }
    public long TotalVirtual { get; set; }
    public long UsedVirtual { get; set; }
}

public class NetworkInfo
{
    public string AdapterName { get; set; } = "";
    public float UploadSpeed { get; set; }
    public float DownloadSpeed { get; set; }
    public long TotalBytesSent { get; set; }
    public long TotalBytesReceived { get; set; }
}

public class DiskInfo
{
    public string Name { get; set; } = "";
    public float Temperature { get; set; }
    public float UsagePercent { get; set; }
    public long TotalSpace { get; set; }
    public long UsedSpace { get; set; }
    public long FreeSpace { get; set; }
    public float ReadRate { get; set; }
    public float WriteRate { get; set; }
    public float Health { get; set; }
}

public class FanInfo
{
    public float CpuFanSpeed { get; set; }
    public float GpuFanSpeed { get; set; }
    public float[] SystemFanSpeeds { get; set; } = Array.Empty<float>();
    public bool IsAutoMode { get; set; } = true;
    public FanMode CurrentMode { get; set; } = FanMode.Auto;
}

public class BatteryInfo
{
    public float ChargeLevel { get; set; }
    public float Health { get; set; }
    public string ChargeStatus { get; set; } = "";
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public float Voltage { get; set; }
    public float WearLevel { get; set; }
    public bool IsCharging { get; set; }
    public int ChargeRate { get; set; }
}

public enum FanMode
{
    Auto,
    Silent,
    Balance,
    Performance,
    Custom
}
