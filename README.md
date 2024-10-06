# DynamicBlazorComponentLoader

A .NET 8 Library for use in a .NET 8 Blazor Web App with Server Interactivity that lets you replace Blazor Components (RCLs) in place at runtime.

## Getting Started

To load this library you can:

1. Download and add this class library project in your Blazor Server solution.
2. Ingest the NuGet package into your Blazor Server project

Before you can configure the library, you must create a folder in your Blazor Server project where you want to copy updated Razor Class Library DLLs.

Let's assume that we've created one called **TempDLLs**.

Add the code below to your *Program.cs* file to configure the temp directory and add the following services:

- `DynamicComponentLoader` Loads a new component given a path
- `DllWatcherService` Watches the temp folder and notifies you when files are copied to it

```c#
// configure the DllWatcherService to watch the TempDLLs folder 
builder.Services.Configure<DllWatcherOptions>(options =>
{
    var baseDir = AppContext.BaseDirectory;
    // Set the watch path to TempDLLs
    var tempDllPath = Path.Combine(baseDir, "TempDLLs");

    options.WatchPath = tempDllPath;
});
// Add the DynamicComponentLoader and DllWatcherService to the service collection
builder.Services.AddSingleton<DynamicComponentLoader>();
builder.Services.AddSingleton<DllWatcherService>();
```

Here is a demo component that exists in a RCL called *MyRCL*:

*VersionComponent.razor*:

```html
<h3>Version 1.0</h3>

<p>This is a regular Blazor Component with custom markup and code</p>

<button class="btn btn-primary" @onclick="Button_Click">Click me</button>
<br/>
<br/>

<p>@Message</p>
```

*VersionComponent.razor.cs*:

```c#
using Microsoft.AspNetCore.Components;

namespace MyRCL;

public partial class VersionComponent : ComponentBase
{
    protected string Message = string.Empty;

    [Parameter]
    public string Greeting { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> OnButtonClick { get; set; }

    protected async void Button_Click()
    {
        if (OnButtonClick.HasDelegate)
        {
            await OnButtonClick.InvokeAsync(Greeting);
        }
    }
}
```

If you want to test it, create this RCL project separate from your Blazor Server demo app.

In your demo app, add the following page:

*Dynamic.razor*:

```c#
@page "/dynamic"
@using Microsoft.AspNetCore.Components
@inject DynamicComponentLoader Loader
@inject DllWatcherService DllWatcher

<div>
    @if (dynamicComponentType != null)
    {
        <DynamicComponent Type="dynamicComponentType" Parameters="componentParameters" />
    }
    else
    {
        <p>Loading component...</p>
    }
</div>

<p>@Message</p>
@code {

    // You can create one of these for each type of component you want to load
    private Type dynamicComponentType;
    private Dictionary<string, object> componentParameters;
    private string Message { get; set; } = string.Empty;

    // This is the path where you can copy your Component DLLs.
    private string tempFolderPath = Path.Combine(AppContext.BaseDirectory, "TempDLLs");

    protected override void OnInitialized()
    {
        // Subscribe to the file change notification.
        // The folder monitored by the DllWatcher is defined in Program.cs
        DllWatcher.OnDllChangedAction = ReloadComponent;

        // Initial load of the component
        LoadComponents();
    }

    private void LoadComponents()
    {
        // Let's figure out WHERE to load the DLL from
        // Look in the temp folder
        var dllPath = Path.Combine(tempFolderPath, "MyRCL.dll");
        if (!File.Exists(dllPath))
        {
            // If it's not in the temp folder, let's use the one from the base directory
            dllPath = Path.Combine(AppContext.BaseDirectory, "MyRCL.dll");
        }

        // Load the specific component type by its fully qualified name
        dynamicComponentType = Loader.LoadComponentType(tempFolderPath, dllPath, "MyRCL.VersionComponent");

        // Set the parameters dynamically
        componentParameters = new Dictionary<string, object>
        {
            { "Greeting", "Hello from dynamically loaded component!" },
            { "OnButtonClick", EventCallback.Factory.Create<string>(this, HandleButtonClick) }
        };

        // Force a UI refresh
        StateHasChanged();
    }

    private async void ReloadComponent()
    {
        // This method is called when a file is copied to the temp folder
        await InvokeAsync(LoadComponents);
    }

    private void HandleButtonClick(string greeting)
    {
        Message = $"Button clicked with message: {greeting} at {DateTime.Now.ToLongTimeString()}";
    }
}
```

