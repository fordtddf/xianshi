using System;
using System.IO;
using System.Text.Json;
using HardwareMonitor.Core;
using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Services;

public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _settingsPath;

    public AppSettings Current { get; private set; }

    public event Action? SettingsChanged;

    public SettingsService()
    {
        var appDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Constants.AppName);
        Directory.CreateDirectory(appDir);
        _settingsPath = Path.Combine(appDir, Constants.SettingsFileName);
        Current = Load();
    }

    private AppSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(Current, JsonOptions);
            File.WriteAllText(_settingsPath, json);
            SettingsChanged?.Invoke();
        }
        catch { }
    }

    public void Update(Action<AppSettings> action)
    {
        action(Current);
        Save();
    }
}
