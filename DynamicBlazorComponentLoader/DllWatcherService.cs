namespace DynamicBlazorComponentLoader;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Watches a folder for new or changed DLL files and raises <see cref="OnDllChanged"/>
/// when one is detected.
/// </summary>
public class DllWatcherService : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger<DllWatcherService> _logger;

    /// <summary>
    /// Raised when a DLL file is created or changed in the watched folder.
    /// </summary>
    public event Action? OnDllChanged;

    /// <summary>
    /// Creates a new <see cref="DllWatcherService"/> that watches the folder
    /// configured in <see cref="DllWatcherOptions.WatchPath"/>.
    /// </summary>
    /// <param name="options">Options providing the folder to watch.</param>
    /// <param name="logger">Logger used to report detected changes.</param>
    public DllWatcherService(IOptions<DllWatcherOptions> options, ILogger<DllWatcherService> logger)
    {
        _logger = logger;

        // Watch the configured folder directly for DLL files.
        string watchPath = options.Value.WatchPath;

        _watcher = new FileSystemWatcher(watchPath)
        {
            Filter = "*.dll",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite
        };
        _watcher.Created += OnDllChangedHandler;
        _watcher.Changed += OnDllChangedHandler;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnDllChangedHandler(object sender, FileSystemEventArgs e)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Detected DLL change: {File}", e.FullPath);
        }

        OnDllChanged?.Invoke();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _watcher.Dispose();
        GC.SuppressFinalize(this);
    }
}
