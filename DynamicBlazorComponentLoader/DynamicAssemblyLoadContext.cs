using System.Reflection;
using System.Runtime.Loader;

namespace DynamicBlazorComponentLoader;

public class DynamicAssemblyLoadContext : AssemblyLoadContext
{
    public DynamicAssemblyLoadContext() : base(isCollectible: true) { }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        // Optionally, you can load dependencies here if required
        return null;
    }
    public Assembly LoadAssemblyFromByteArray(byte[] assemblyData)
    {
        using (var ms = new MemoryStream(assemblyData))
        {
            return LoadFromStream(ms);
        }
    }
}
