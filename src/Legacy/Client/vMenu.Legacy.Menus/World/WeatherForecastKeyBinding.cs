
using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.World;

public static class WeatherForecastKeyBinding
{
    private const string Command = "vmenu:forecast";

    private const string DefaultKey = "EQUALS";

    private static bool _registered;

    public static void Register(Action onPressed)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        Commands.Register(Command, false, onPressed);

        KeyMapping.Register(Command, null, "vMenu: Cycle the weather forecast off, compact or full", DefaultKey, null);
    }
}
