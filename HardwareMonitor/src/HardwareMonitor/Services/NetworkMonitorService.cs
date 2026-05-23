using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Services;

public class NetworkMonitorService : IDisposable
{
    private PerformanceCounter? _sendCounter;
    private PerformanceCounter? _recvCounter;
    private string? _activeAdapter;
    private long _totalSent;
    private long _totalReceived;
    private float _lastSendSpeed;
    private float _lastRecvSpeed;
    private Timer? _sampleTimer;
    private bool _disposed;

    public NetworkInfo CurrentInfo { get; private set; } = new();

    public void Start()
    {
        DetectActiveAdapter();
        _sampleTimer = new Timer(TimerCallback, null, 1000, 1000);
    }

    private void TimerCallback(object? state)
    {
        try
        {
            if (_sendCounter == null || _recvCounter == null)
            {
                DetectActiveAdapter();
                if (_sendCounter == null) return;
            }

            var sent = (long)_sendCounter!.NextValue();
            var recv = (long)_recvCounter!.NextValue();

            // Use raw values for speed calculation
            _lastSendSpeed = Math.Max(0, sent);
            _lastRecvSpeed = Math.Max(0, recv);
            _totalSent += (long)_lastSendSpeed;
            _totalReceived += (long)_lastRecvSpeed;

            CurrentInfo = new NetworkInfo
            {
                AdapterName = _activeAdapter ?? "Unknown",
                UploadSpeed = _lastSendSpeed,
                DownloadSpeed = _lastRecvSpeed,
                TotalBytesSent = _totalSent,
                TotalBytesReceived = _totalReceived
            };
        }
        catch
        {
            // Adapter may have changed, re-detect on next cycle
            _sendCounter?.Dispose();
            _recvCounter?.Dispose();
            _sendCounter = null;
            _recvCounter = null;
        }
    }

    private void DetectActiveAdapter()
    {
        try
        {
            var category = new PerformanceCounterCategory("Network Interface");
            var instances = category.GetInstanceNames();

            if (instances.Length == 0) return;

            // Pick the first non-loopback adapter
            var adapter = instances.FirstOrDefault(i =>
                !i.Contains("Loopback", StringComparison.OrdinalIgnoreCase) &&
                !i.Contains("isatap", StringComparison.OrdinalIgnoreCase) &&
                !i.Contains("Teredo", StringComparison.OrdinalIgnoreCase))
                ?? instances[0];

            _activeAdapter = adapter;
            _sendCounter?.Dispose();
            _recvCounter?.Dispose();
            _sendCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", adapter);
            _recvCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", adapter);
        }
        catch
        {
            // Performance counters not available
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _sampleTimer?.Dispose();
            _sendCounter?.Dispose();
            _recvCounter?.Dispose();
            _disposed = true;
        }
    }
}
