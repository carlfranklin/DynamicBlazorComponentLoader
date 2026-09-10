using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace DynamicBlazorComponentLoader;

/// <summary>
/// Loads Blazor components (RCLs) from DLL files at runtime into a collectible
/// <see cref="DynamicAssemblyLoadContext"/>, allowing them to be replaced in place.
/// </summary>
public class DynamicComponentLoader
{
    private readonly ILogger<DynamicComponentLoader> _logger;
    private DynamicAssemblyLoadContext? _loadContext;

    /// <summary>
    /// Creates a new <see cref="DynamicComponentLoader"/>.
    /// </summary>
    /// <param name="logger">Logger used to report load and cleanup activity.</param>
    public DynamicComponentLoader(ILogger<DynamicComponentLoader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Loads a component type from a DLL file by its fully qualified type name.
    /// </summary>
    /// <param name="tempFolderPath">Folder used for temporary assembly files.</param>
    /// <param name="dllPath">Path to the DLL to load.</param>
    /// <param name="componentName">Fully qualified name of the component type to load.</param>
    /// <returns>The component type, or <c>null</c> if it could not be found or is not a component.</returns>
    public Type? LoadComponentType(string tempFolderPath, string dllPath, string componentName)
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
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Loaded component type {ComponentName} from {DllPath}", componentName, dllPath);
                }

                return componentType;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Type {ComponentName} not found or is not a component in {DllPath}", componentName, dllPath);
            }
        }

        return null;
    }

    /// <summary>
    /// Unloads the previously loaded assembly, if any.
    /// </summary>
    public void UnloadPreviousAssembly()
    {
        if (_loadContext != null)
        {
            // Unload the previous assembly
            _loadContext.Unload();
            _loadContext = null;
        }
    }

    /// <summary>
    /// Deletes any leftover DLL files from the temp folder.
    /// </summary>
    /// <param name="tempFolderPath">Folder containing temporary assembly files.</param>
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
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug(ex, "Failed to delete old assembly {File}", file);
                    }
                }
            }
        }
    }
}
