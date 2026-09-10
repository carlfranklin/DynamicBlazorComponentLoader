using DynamicBlazorComponentLoader;
using Microsoft.Extensions.Options;
using Xunit;

namespace DynamicBlazorComponentLoader.Tests;

public class DllWatcherServiceTests : IDisposable
{
    private readonly List<string> _tempRoots = new();

    private string CreateWatchPath()
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        _tempRoots.Add(root);
        var watchPath = Path.Combine(root, "TempDLLs");
        Directory.CreateDirectory(watchPath);
        return watchPath;
    }

    [Fact]
    public void Constructor_WatchesConfiguredPath_WithoutThrowing()
    {
        var watchPath = CreateWatchPath();

        using var service = new DllWatcherService(
            new DynamicComponentLoader(),
            Options.Create(new DllWatcherOptions { WatchPath = watchPath }));

        // Reaching here without an exception is the assertion.
    }

    [Fact]
    public async Task OnDllChangedAction_Fires_WhenFileCopiedToWatchedFolder()
    {
        var watchPath = CreateWatchPath();
        var service = new DllWatcherService(
            new DynamicComponentLoader(),
            Options.Create(new DllWatcherOptions { WatchPath = watchPath }));

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        service.OnDllChangedAction = () => tcs.TrySetResult();

        File.Copy(Path.Combine(AppContext.BaseDirectory, "TestRCL.dll"), Path.Combine(watchPath, "TestRCL.dll"));

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));

        service.Dispose();
        Assert.Same(tcs.Task, completed);
    }

    public void Dispose()
    {
        foreach (var root in _tempRoots)
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }
}
