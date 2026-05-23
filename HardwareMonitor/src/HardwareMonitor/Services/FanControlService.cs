using System;
using System.Collections.Generic;
using System.Linq;
using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Services;

public class FanControlService : IDisposable
{
    private readonly SettingsService _settings;
    private FanMode _currentMode;
    private FanProfile? _customProfile;
    private bool _disposed;

    public FanMode CurrentMode
    {
        get => _currentMode;
        private set
        {
            if (_currentMode != value)
            {
                _currentMode = value;
                ModeChanged?.Invoke(value);
            }
        }
    }

    public event Action<FanMode>? ModeChanged;

    public FanControlService(SettingsService settings)
    {
        _settings = settings;
        _currentMode = settings.Current.FanMode;
        _customProfile = settings.Current.CustomFanProfile;
    }

    public void SetMode(FanMode mode)
    {
        CurrentMode = mode;
        _settings.Update(s => s.FanMode = mode);

        if (mode == FanMode.Auto)
        {
            ResetToBiosDefault();
        }
    }

    public void SetCustomProfile(FanProfile profile)
    {
        _customProfile = profile;
        _settings.Update(s =>
        {
            s.FanMode = FanMode.Custom;
            s.CustomFanProfile = profile;
        });
        CurrentMode = FanMode.Custom;
    }

    public FanProfile? GetActiveProfile()
    {
        return CurrentMode switch
        {
            FanMode.Auto => null,
            FanMode.Custom => _customProfile,
            _ => FanPresets.GetDefault(CurrentMode)
        };
    }

    /// <summary>
    /// Calculate target fan speed based on current temperature and active profile.
    /// Returns null if in auto mode (BIOS handles fans).
    /// </summary>
    public float? CalculateTargetFanSpeed(float temperature)
    {
        if (CurrentMode == FanMode.Auto)
            return null;

        var profile = GetActiveProfile();
        if (profile == null || profile.CurvePoints.Count == 0)
            return null;

        var points = profile.CurvePoints.OrderBy(p => p.Temperature).ToList();

        // Below lowest point
        if (temperature <= points[0].Temperature)
            return points[0].FanSpeedPercent;

        // Above highest point
        if (temperature >= points[^1].Temperature)
            return points[^1].FanSpeedPercent;

        // Interpolate between points
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (temperature >= points[i].Temperature && temperature <= points[i + 1].Temperature)
            {
                var t = (temperature - points[i].Temperature) /
                        (points[i + 1].Temperature - points[i].Temperature);
                return points[i].FanSpeedPercent +
                       t * (points[i + 1].FanSpeedPercent - points[i].FanSpeedPercent);
            }
        }

        return points[^1].FanSpeedPercent;
    }

    private void ResetToBiosDefault()
    {
        // On most systems, setting EC fan control to auto restores BIOS control.
        // This is a best-effort operation - actual implementation depends on hardware.
        try
        {
            // Attempt to reset via WMI if available (some OEMs support this)
            // For now, this is a no-op that signals intent.
            // Real implementation would use manufacturer-specific APIs:
            // - Lenovo: WMI ACPI
            // - Dell: WMI BIOS API
            // - HP: WMI BIOS API
            // - ASUS: ACPI EC
        }
        catch
        {
            // Silently fail - BIOS will eventually take back control
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ResetToBiosDefault();
            _disposed = true;
        }
    }
}
