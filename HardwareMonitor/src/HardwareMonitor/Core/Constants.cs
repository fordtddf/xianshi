namespace HardwareMonitor.Core;

public static class Constants
{
    public const string AppName = "HardwareMonitor";
    public const string AppTitle = "硬件监控";
    public const string SettingsFileName = "settings.json";
    public const string MutexName = "HardwareMonitor_SingleInstance_Mutex";

    public const int DefaultRefreshIntervalMs = 2000;
    public const int MinRefreshIntervalMs = 500;
    public const int MaxRefreshIntervalMs = 10000;

    public const float DefaultTempAlertThreshold = 85f;
    public const float MaxTempAlertThreshold = 105f;
    public const float MinTempAlertThreshold = 60f;

    public const int NetworkSampleIntervalMs = 1000;

    public const string AutoStartRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    public const string AutoStartRegistryValue = "HardwareMonitor";
}
