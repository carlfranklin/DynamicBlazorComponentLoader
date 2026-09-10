using DynamicBlazorComponentLoader;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace DynamicBlazorComponentLoader.Tests;

public class DynamicComponentLoaderTests
{
    private static string TestRclDllPath => Path.Combine(AppContext.BaseDirectory, "TestRCL.dll");

    private static string NewTempFolder() => Directory.CreateTempSubdirectory().FullName;

    [Fact]
    public void LoadComponentType_ReturnsComponentType_WhenDllContainsComponent()
    {
        var loader = new DynamicComponentLoader();

        var type = loader.LoadComponentType(NewTempFolder(), TestRclDllPath, "TestRCL.TestComponent");

        Assert.NotNull(type);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(type!));
        Assert.Equal("TestComponent", type!.Name);
    }

    [Fact]
    public void LoadComponentType_ReturnsNull_WhenTypeNameNotFound()
    {
        var loader = new DynamicComponentLoader();

        var type = loader.LoadComponentType(NewTempFolder(), TestRclDllPath, "TestRCL.DoesNotExist");

        Assert.Null(type);
    }

    [Fact]
    public void LoadComponentType_ReturnsNull_WhenTypeIsNotAComponent()
    {
        var loader = new DynamicComponentLoader();

        var type = loader.LoadComponentType(NewTempFolder(), TestRclDllPath, "TestRCL.NotAComponent");

        Assert.Null(type);
    }

    [Fact]
    public void LoadComponentType_Throws_WhenDllFileMissing()
    {
        // Characterization test: current behavior is to throw (File.ReadAllBytes).
        var loader = new DynamicComponentLoader();

        Assert.Throws<FileNotFoundException>(() =>
            loader.LoadComponentType(NewTempFolder(), Path.Combine(NewTempFolder(), "missing.dll"), "TestRCL.TestComponent"));
    }

    [Fact]
    public void UnloadPreviousAssembly_DoesNotThrow_WhenNothingLoaded()
    {
        var loader = new DynamicComponentLoader();

        loader.UnloadPreviousAssembly();
    }

    [Fact]
    public void UnloadPreviousAssembly_DoesNotThrow_AfterLoad()
    {
        var loader = new DynamicComponentLoader();
        loader.LoadComponentType(NewTempFolder(), TestRclDllPath, "TestRCL.TestComponent");

        loader.UnloadPreviousAssembly();
    }
}
