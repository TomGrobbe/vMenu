namespace vMenu.Enhanced.Events;

/// <summary>
/// Player and vehicle state changes as C# events, this eliminates the need for many (duplicate) tick functions constantly checking for changes.
/// </summary>
public static class GameEvents
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        LocalPlayerTicks.Initialize();
        LocalVehicleTicks.Initialize();
    }
}
