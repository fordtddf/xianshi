using System;
using System.Linq;
using HardwareMonitor.Core.Models;
using LibreHardwareMonitor.Hardware;

namespace HardwareMonitor.Services;

public class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer;
    private bool _disposed;

    public HardwareMonitorService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsStorageEnabled = true,
            IsBatteryEnabled = true,
            IsNetworkEnabled = false
        };
        _computer.Open();
    }

    public HardwareSnapshot TakeSnapshot()
    {
        var snapshot = new HardwareSnapshot();

        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();

            foreach (var sub in hardware.SubHardware)
                sub.Update();

            switch (hardware.HardwareType)
            {
                case HardwareType.Cpu:
                    snapshot.Cpu = ReadCpu(hardware);
                    break;
                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    snapshot.Gpu = ReadGpu(hardware);
                    break;
                case HardwareType.Memory:
                    snapshot.Memory = ReadMemory(hardware);
                    break;
                case HardwareType.Storage:
                    if (snapshot.Disk == null)
                        snapshot.Disk = ReadDisk(hardware);
                    break;
                case HardwareType.Battery:
                    snapshot.Battery = ReadBattery(hardware);
                    break;
                case HardwareType.Motherboard:
                    ReadMotherboardFans(hardware, snapshot);
                    break;
            }
        }

        snapshot.Timestamp = DateTime.Now;
        return snapshot;
    }

    private CpuInfo ReadCpu(IHardware hw)
    {
        var info = new CpuInfo { Name = hw.Name };

        foreach (var sensor in hw.Sensors)
        {
            if (sensor.SensorType == SensorType.Temperature && sensor.Name == "CPU Package")
                info.Temperature = sensor.Value ?? 0;
            else if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total")
                info.TotalLoad = sensor.Value ?? 0;
            else if (sensor.SensorType == SensorType.Clock && sensor.Name == "CPU Core #1")
                info.CoreClock = sensor.Value ?? 0;
        }

        info.CoreLoads = hw.Sensors
            .Where(s => s.SensorType == SensorType.Load && s.Name.StartsWith("CPU Core"))
            .Select(s => s.Value ?? 0)
            .ToArray();

        info.CoreTemperatures = hw.Sensors
            .Where(s => s.SensorType == SensorType.Temperature && s.Name.StartsWith("CPU Core"))
            .Select(s => s.Value ?? 0)
            .ToArray();

        if (info.Temperature == 0)
        {
            info.Temperature = hw.Sensors
                .Where(s => s.SensorType == SensorType.Temperature)
                .Select(s => s.Value ?? 0)
                .DefaultIfEmpty(0)
                .Max();
        }

        return info;
    }

    private GpuInfo ReadGpu(IHardware hw)
    {
        var info = new GpuInfo { Name = hw.Name };

        foreach (var sensor in hw.Sensors)
        {
            var val = sensor.Value ?? 0;
            switch (sensor.SensorType)
            {
                case SensorType.Temperature when sensor.Name.Contains("Core") || sensor.Name.Contains("GPU"):
                    if (info.Temperature == 0) info.Temperature = val;
                    break;
                case SensorType.Load when sensor.Name == "GPU Core":
                    info.CoreLoad = val;
                    break;
                case SensorType.Load when sensor.Name.Contains("Memory"):
                    info.MemoryLoad = val;
                    break;
                case SensorType.Fan:
                    info.FanSpeed = val;
                    break;
                case SensorType.Clock when sensor.Name == "GPU Core":
                    info.CoreClock = val;
                    break;
                case SensorType.Clock when sensor.Name.Contains("Memory"):
                    info.MemoryClock = val;
                    break;
                case SensorType.SmallData when sensor.Name.Contains("Used"):
                    info.MemoryUsed = (long)val;
                    break;
                case SensorType.SmallData when sensor.Name.Contains("Total") || sensor.Name.Contains("Free"):
                    if (info.MemoryTotal == 0) info.MemoryTotal = (long)val + info.MemoryUsed;
                    break;
                case SensorType.Power:
                    info.Power = val;
                    break;
            }
        }

        if (info.Temperature == 0)
        {
            info.Temperature = hw.Sensors
                .Where(s => s.SensorType == SensorType.Temperature)
                .Select(s => s.Value ?? 0)
                .DefaultIfEmpty(0)
                .Max();
        }

        return info;
    }

    private MemoryInfo ReadMemory(IHardware hw)
    {
        var info = new MemoryInfo();

        foreach (var sensor in hw.Sensors)
        {
            var val = sensor.Value ?? 0;
            switch (sensor.SensorType)
            {
                case SensorType.Data when sensor.Name == "Memory Used":
                    info.UsedPhysical = (long)(val * 1024 * 1024 * 1024);
                    break;
                case SensorType.Data when sensor.Name == "Memory Available":
                    info.AvailablePhysical = (long)(val * 1024 * 1024 * 1024);
                    break;
                case SensorType.Load when sensor.Name == "Memory":
                    info.UsagePercent = val;
                    break;
            }
        }

        info.TotalPhysical = info.UsedPhysical + info.AvailablePhysical;
        if (info.TotalPhysical > 0 && info.UsagePercent == 0)
            info.UsagePercent = (float)(info.UsedPhysical * 100.0 / info.TotalPhysical);

        return info;
    }

    private DiskInfo ReadDisk(IHardware hw)
    {
        var info = new DiskInfo { Name = hw.Name };

        foreach (var sensor in hw.Sensors)
        {
            var val = sensor.Value ?? 0;
            switch (sensor.SensorType)
            {
                case SensorType.Temperature:
                    if (info.Temperature == 0) info.Temperature = val;
                    break;
                case SensorType.Load when sensor.Name == "Used Space":
                    info.UsagePercent = val;
                    break;
            }
        }

        return info;
    }

    private BatteryInfo ReadBattery(IHardware hw)
    {
        var info = new BatteryInfo();

        foreach (var sensor in hw.Sensors)
        {
            var val = sensor.Value ?? 0;
            switch (sensor.SensorType)
            {
                case SensorType.Level when sensor.Name == "Charge Level":
                    info.ChargeLevel = val;
                    break;
                case SensorType.Level when sensor.Name == "Wear Level":
                    info.WearLevel = val;
                    info.Health = 100 - val;
                    break;
                case SensorType.Voltage:
                    info.Voltage = val;
                    break;
                case SensorType.Power:
                    info.ChargeRate = (int)val;
                    break;
            }
        }

        info.IsCharging = info.ChargeRate > 0;
        info.ChargeStatus = info.IsCharging ? "充电中" : "放电中";
        if (info.ChargeLevel >= 99) info.ChargeStatus = "已充满";

        return info;
    }

    private void ReadMotherboardFans(IHardware hw, HardwareSnapshot snapshot)
    {
        var fanInfo = snapshot.Fan ?? new FanInfo();

        foreach (var sensor in hw.Sensors)
        {
            if (sensor.SensorType == SensorType.Fan)
            {
                var val = sensor.Value ?? 0;
                var name = sensor.Name.ToLowerInvariant();
                if (name.Contains("cpu"))
                    fanInfo.CpuFanSpeed = val;
                else if (fanInfo.SystemFanSpeeds.Length < 4)
                {
                    var list = fanInfo.SystemFanSpeeds.ToList();
                    list.Add(val);
                    fanInfo.SystemFanSpeeds = list.ToArray();
                }
            }
        }

        snapshot.Fan = fanInfo;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _computer.Close();
            _disposed = true;
        }
    }
}
