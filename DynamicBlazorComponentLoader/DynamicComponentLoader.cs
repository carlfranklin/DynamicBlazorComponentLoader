using Microsoft.AspNetCore.Components;
namespace DynamicBlazorComponentLoader;
public class DynamicComponentLoader
{
    private DynamicAssemblyLoadContext _loadContext;

    public Type LoadComponentType(string tempFolderPath, string dllPath, string componentName)
    {
        // Ensure the temp folder exists
        Directory.CreateDirectory(tempFolderPath);

        // Unload the previous assembly, if any
        UnloadPreviousAssembly();

        // Load the new assembly from the given path as a byte array, not directly.
        // This is because the assembly is locked by the runtime when loaded directly.
        _loadContext = new DynamicAssemblyLoadContext();
        var bytes = File.ReadAllBytes(dllPath);
        var newAssembly = _loadContext.LoadAssemblyFromByteArray(bytes);

        if (newAssembly != null)
        {
            // Try to find the type by its full name
            var componentType = newAssembly.GetType(componentName);

            if (componentType != null && typeof(ComponentBase).IsAssignableFrom(componentType))
            {
                // Return the component type
                return componentType;
            }
        }

        return null;
    }

    public void UnloadPreviousAssembly()
    {
        if (_loadContext != null)
        {
            // Unload the previous assembly
            _loadContext.Unload();
            _loadContext = null;

            // Force garbage collection to fully unload the assembly and free file handles
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public void CleanUpOldAssemblies(string tempFolderPath)
    {
        if (Directory.Exists(tempFolderPath))
        {
            foreach (var file in Directory.GetFiles(tempFolderPath, "*.dll"))
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException ex)
                {
                    // Handle exceptions, maybe log the issue if needed
                }
            }
        }
    }
}
