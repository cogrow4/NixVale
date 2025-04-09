using Nixvale.Core.Debug;
using System.Collections.ObjectModel;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NixvaleNew.Maui.Services;

public class MauiDebugLogger : IDebug
{
    private readonly ILogger _logger;
    private readonly ObservableCollection<LogEntry> _recentLogs;
    private readonly int _maxLogEntries;

    public IReadOnlyCollection<LogEntry> RecentLogs => _recentLogs;

    public MauiDebugLogger(ILogger logger, int maxLogEntries = 1000)
    {
        _logger = logger;
        _maxLogEntries = maxLogEntries;
        _recentLogs = new ObservableCollection<LogEntry>();
    }

    public void Write(string message)
    {
        Write(LogLevel.Information, message);
    }

    public void Write(LogLevel level, string message)
    {
        var entry = new LogEntry(DateTime.UtcNow, level, message);
        
        // Convert Nixvale log level to MAUI log level
        var mauiLevel = level switch
        {
            LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.None
        };

        _logger.Log(mauiLevel, message);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _recentLogs.Add(entry);
            while (_recentLogs.Count > _maxLogEntries)
            {
                _recentLogs.RemoveAt(0);
            }
        });
    }

    public void WriteException(Exception ex, string? message = null)
    {
        var logMessage = message == null
            ? $"Exception: {ex.Message}"
            : $"{message} - Exception: {ex.Message}";

        Write(LogLevel.Error, logMessage);
        Write(LogLevel.Debug, ex.StackTrace ?? "No stack trace available");
    }

    public void Clear()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _recentLogs.Clear();
        });
    }
} 