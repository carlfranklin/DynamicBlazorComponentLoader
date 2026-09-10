using DynamicBlazorComponentLoader;
using Xunit;

namespace DynamicBlazorComponentLoader.Tests;

public class DllWatcherOptionsTests
{
    [Fact]
    public void WatchPath_RoundTrips()
    {
        var options = new DllWatcherOptions { WatchPath = @"C:\Temp\TempDLLs" };

        Assert.Equal(@"C:\Temp\TempDLLs", options.WatchPath);
    }
}
