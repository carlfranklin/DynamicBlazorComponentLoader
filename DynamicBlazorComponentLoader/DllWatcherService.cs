namespace DynamicBlazorComponentLoader;

using Microsoft.Extensions.Options;

public class DllWatcherService : IDisposable
{
    private FileSystemWatcher _watcher;
    private DynamicComponentLoader _loader;
    public Action OnDllChangedAction { get; set; }

    public DllWatcherService(DynamicComponentLoader loader, IOptions<DllWatcherOptions> options)
    {
        _loader = loader;

        // Watch the TempDLLs folder
        string watchPath = options.Value.WatchPath;

        _watcher = new FileSystemWatcher(Path.GetDirectoryName(watchPath))
        {
            Filter = Path.GetFileName(watchPath),
            NotifyFilter = NotifyFilters.LastWrite
        };
        _watcher.Changed += OnDllChanged;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnDllChanged(object sender, FileSystemEventArgs e)
    {
        OnDllChangedAction?.Invoke();
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}
