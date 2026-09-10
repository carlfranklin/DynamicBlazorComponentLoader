namespace DynamicBlazorComponentLoader;

/// <summary>
/// Options for <see cref="DllWatcherService"/>.
/// </summary>
public class DllWatcherOptions
{
    /// <summary>
    /// Gets or sets the folder to watch for new or changed DLL files.
    /// </summary>
    public string WatchPath { get; set; } = string.Empty;
}