using Microsoft.AspNetCore.Components;

namespace TestRCL;

// Test fixture: a Blazor component the loader should be able to find.
public class TestComponent : ComponentBase
{
    [Parameter]
    public string Greeting { get; set; } = string.Empty;
}

// Test fixture: a plain class the loader should reject.
public class NotAComponent
{
}
