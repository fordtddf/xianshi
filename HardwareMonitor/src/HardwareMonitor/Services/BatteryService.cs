using System;
using System.Management;
using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Services;

public class BatteryService : IDisposable
{
    public BatteryInfo? GetBatteryInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("root\\cimv2",
                "SELECT * FROM Win32_Battery");

            foreach (ManagementObject obj in searcher.Get())
            {
                var info = new BatteryInfo();

                if (obj["EstimatedChargeRemaining"] is ushort charge)
                    info.ChargeLevel = charge;

                if (obj["BatteryStatus"] is ushort status)
                {
                    info.IsCharging = status == 2 || status == 6;
                    info.ChargeStatus = status switch
                    {
                        1 => "放电中",
                        2 => "充电中",
                        3 => "已充满",
                        4 => "电量低",
                        5 => "电量危急",
                        6 => "充电中",
                        7 => "充电中",
                        8 => "正在涓流充电",
                        9 => "正在初始化",
                        10 => "未初始化",
                        11 => "部分充电",
                        _ => "未知"
                    };
                }

                if (obj["EstimatedRunTime"] is uint runTime && runTime != 71582788)
                    info.EstimatedTimeRemaining = TimeSpan.FromMinutes(runTime);

                if (obj["DesignVoltage"] is uint voltage)
                    info.Voltage = voltage / 1000f;

                // Try to get health from WMI
                try
                {
                    using var capSearcher = new ManagementObjectSearcher("root\\WMI",
                        "SELECT * FROM BatteryStaticData");
                    foreach (ManagementObject cap in capSearcher.Get())
                    {
                        if (cap["DesignedCapacity"] is uint designed && designed > 0)
                        {
                            using var fullSearcher = new ManagementObjectSearcher("root\\WMI",
                                "SELECT * FROM BatteryFullChargedCapacity");
                            foreach (ManagementObject full in fullSearcher.Get())
                            {
                                if (full["FullChargedCapacity"] is uint fullCap)
                                {
                                    info.Health = (float)fullCap / designed * 100;
                                    info.WearLevel = 100 - info.Health;
                                }
                            }
                        }
                    }
                }
                catch { }

                return info;
            }
        }
        catch
        {
            // WMI not available or no battery
        }

        return null;
    }

    public void Dispose() { }
}
