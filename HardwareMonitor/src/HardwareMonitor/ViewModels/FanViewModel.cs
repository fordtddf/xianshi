using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HardwareMonitor.Core.Models;
using HardwareMonitor.Services;

namespace HardwareMonitor.ViewModels;

public partial class FanViewModel : ObservableObject
{
    private readonly FanControlService _fanControl;
    private readonly SettingsService _settings;

    [ObservableProperty]
    private FanMode _selectedMode = FanMode.Auto;

    [ObservableProperty]
    private ObservableCollection<FanCurvePoint> _customCurvePoints = new();

    [ObservableProperty]
    private FanCurvePoint? _selectedPoint;

    [ObservableProperty]
    private bool _isCustomMode;

    [ObservableProperty]
    private float _targetFanSpeed;

    public ObservableCollection<FanModeOption> ModeOptions { get; } = new()
    {
        new FanModeOption(FanMode.Auto, "自动", "BIOS 控制风扇"),
        new FanModeOption(FanMode.Silent, "静音", "低噪音优先"),
        new FanModeOption(FanMode.Balance, "平衡", "均衡温度与噪音"),
        new FanModeOption(FanMode.Performance, "性能", "散热优先"),
        new FanModeOption(FanMode.Custom, "自定义", "自定义风扇曲线")
    };

    public FanViewModel(FanControlService fanControl, SettingsService settings)
    {
        _fanControl = fanControl;
        _settings = settings;
        _selectedMode = settings.Current.FanMode;
        _isCustomMode = _selectedMode == FanMode.Custom;

        LoadCustomCurve();
    }

    private void LoadCustomCurve()
    {
        var profile = _settings.Current.CustomFanProfile;
        if (profile != null && profile.CurvePoints.Count > 0)
        {
            foreach (var point in profile.CurvePoints)
                CustomCurvePoints.Add(point);
        }
        else
        {
            // Default curve
            var defaults = FanPresets.Balance.CurvePoints;
            foreach (var point in defaults)
                CustomCurvePoints.Add(new FanCurvePoint
                {
                    Temperature = point.Temperature,
                    FanSpeedPercent = point.FanSpeedPercent
                });
        }
    }

    [RelayCommand]
    private void SetMode(FanMode mode)
    {
        SelectedMode = mode;
        IsCustomMode = mode == FanMode.Custom;
        _fanControl.SetMode(mode);
    }

    [RelayCommand]
    private void ApplyCustomCurve()
    {
        var profile = new FanProfile
        {
            Name = "自定义",
            Mode = FanMode.Custom,
            CurvePoints = CustomCurvePoints.OrderBy(p => p.Temperature).ToList()
        };
        _fanControl.SetCustomProfile(profile);
    }

    [RelayCommand]
    private void AddCurvePoint()
    {
        var lastPoint = CustomCurvePoints.LastOrDefault();
        var newTemp = lastPoint != null ? lastPoint.Temperature + 10 : 50;
        var newSpeed = lastPoint != null
            ? System.Math.Min(100, lastPoint.FanSpeedPercent + 15)
            : 50;

        CustomCurvePoints.Add(new FanCurvePoint
        {
            Temperature = System.Math.Min(100, newTemp),
            FanSpeedPercent = System.Math.Min(100, newSpeed)
        });
    }

    [RelayCommand]
    private void RemoveCurvePoint()
    {
        if (SelectedPoint != null && CustomCurvePoints.Count > 2)
        {
            CustomCurvePoints.Remove(SelectedPoint);
            SelectedPoint = null;
        }
    }

    [RelayCommand]
    private void ResetToPreset(string preset)
    {
        var profile = preset switch
        {
            "Silent" => FanPresets.Silent,
            "Balance" => FanPresets.Balance,
            "Performance" => FanPresets.Performance,
            _ => FanPresets.Balance
        };

        CustomCurvePoints.Clear();
        foreach (var point in profile.CurvePoints)
            CustomCurvePoints.Add(point);
    }
}

public record FanModeOption(FanMode Mode, string Name, string Description);
