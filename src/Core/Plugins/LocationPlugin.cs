public sealed class LocationPlugin
{
    [KernelFunction]
    public string GetCurrentLocation()
    {
        return "Seattle";
    }
}
