using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using HardwareMonitor.Core.Models;
using DFont = System.Drawing.Font;
using DFontStyle = System.Drawing.FontStyle;

namespace HardwareMonitor.Services;

public class TrayIconService : IDisposable
{
    private TaskbarIcon? _trayIcon;
    private bool _disposed;
    private TrayDisplayMode _displayMode = TrayDisplayMode.Temperature;

    public event Action? ShowMainWindow;
    public event Action? ToggleOverlay;
    public event Action? ExitApplication;

    public void Initialize()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "硬件监控 - 加载中...",
            Icon = CreateIcon("72"),
            DoubleClickCommand = new DelegateCommand(() => ShowMainWindow?.Invoke())
        };

        var contextMenu = new System.Windows.Controls.ContextMenu();

        var showItem = new System.Windows.Controls.MenuItem { Header = "显示主窗口" };
        showItem.Click += (_, _) => ShowMainWindow?.Invoke();
        contextMenu.Items.Add(showItem);

        var overlayItem = new System.Windows.Controls.MenuItem { Header = "悬浮窗" };
        overlayItem.Click += (_, _) => ToggleOverlay?.Invoke();
        contextMenu.Items.Add(overlayItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "退出" };
        exitItem.Click += (_, _) => ExitApplication?.Invoke();
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenu = contextMenu;
    }

    public void UpdateIcon(HardwareSnapshot? snapshot, TrayDisplayMode mode)
    {
        _displayMode = mode;
        if (_trayIcon == null) return;

        try
        {
            string text = mode switch
            {
                TrayDisplayMode.Temperature when snapshot?.Cpu?.Temperature > 0 =>
                    $"{snapshot.Cpu.Temperature:F0}",
                TrayDisplayMode.Battery when snapshot?.Battery != null =>
                    $"{snapshot.Battery.ChargeLevel:F0}",
                _ => ""
            };

            if (string.IsNullOrEmpty(text))
            {
                _trayIcon.Icon = CreateIcon("H");
                _trayIcon.ToolTipText = "硬件监控";
            }
            else
            {
                _trayIcon.Icon = CreateIcon(text);
                _trayIcon.ToolTipText = mode switch
                {
                    TrayDisplayMode.Temperature => $"CPU: {snapshot?.Cpu?.Temperature:F0}°C | GPU: {snapshot?.Gpu?.Temperature:F0}°C",
                    TrayDisplayMode.Battery => $"电池: {snapshot?.Battery?.ChargeLevel:F0}%",
                    _ => "硬件监控"
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TrayIcon error: {ex.Message}");
        }
    }

    private Icon CreateIcon(string text)
    {
        int size = 64;
        using var bmp = new Bitmap(size, size);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.Transparent);

            // Blue circle background
            using var bgBrush = new SolidBrush(Color.FromArgb(230, 37, 99, 235));
            g.FillEllipse(bgBrush, 2, 2, size - 4, size - 4);

            // White text
            float fontSize = text.Length > 2 ? 16f : text.Length > 1 ? 20f : 24f;
            using var font = new DFont("Segoe UI", fontSize, DFontStyle.Bold, GraphicsUnit.Pixel);
            using var textBrush = new SolidBrush(Color.White);
            using var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(text, font, textBrush, new RectangleF(0, 0, size, size), sf);
        }

        // Convert Bitmap to Icon
        IntPtr hIcon = bmp.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);
        Icon clonedIcon = (Icon)icon.Clone();
        DestroyIcon(hIcon);
        return clonedIcon;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public void ShowNotification(string title, string message, BalloonIcon icon = BalloonIcon.Info)
    {
        _trayIcon?.ShowBalloonTip(title, message, icon);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _trayIcon?.Dispose();
            _disposed = true;
        }
    }
}

public class DelegateCommand : System.Windows.Input.ICommand
{
    private readonly Action _action;
    public DelegateCommand(Action action) => _action = action;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _action();
}
