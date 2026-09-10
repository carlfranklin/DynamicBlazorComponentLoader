using System.Reflection;
using System.Runtime.Loader;

namespace DynamicBlazorComponentLoader;

/// <summary>
/// A collectible <see cref="AssemblyLoadContext"/> used to load and unload
/// component assemblies at runtime.
/// </summary>
public class DynamicAssemblyLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// Creates a new collectible <see cref="DynamicAssemblyLoadContext"/>.
    /// </summary>
    public DynamicAssemblyLoadContext() : base(isCollectible: true) { }

    /// <inheritdoc />
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Dependencies are resolved by the default context; return null to fall back.
        return null;
    }

    /// <summary>
    /// Loads an assembly from a byte array.
    /// </summary>
    /// <param name="assemblyData">The assembly bytes to load.</param>
    /// <returns>The loaded assembly.</returns>
    public Assembly LoadAssemblyFromByteArray(byte[] assemblyData)
    {
        using (var ms = new MemoryStream(assemblyData))
        {
            return LoadFromStream(ms);
        }
    }
}
