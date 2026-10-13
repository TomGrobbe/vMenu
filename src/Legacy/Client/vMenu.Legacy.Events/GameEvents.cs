namespace vMenu.Enhanced.Events;

// Player and vehicle state changes as C# events, which saves every feature running its own tick to
// watch for the same thing.
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

        EventDebugCommands.Initialize();
    }
}
