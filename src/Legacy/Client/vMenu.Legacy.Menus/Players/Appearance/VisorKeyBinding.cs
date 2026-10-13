using CitizenFX.Core;
using CitizenFX.Core.Native;

using MenuAPI;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players.Appearance;

public static class VisorKeyBinding
{
    private const string Command = "vmenu:visor";

    private const int HoldMs = 400;

    private const int SuppressAfterMs = 380;

    private const string Key = "F11";

    private const string Button = "LRIGHT_INDEX";

    private static bool _registered;

    private static bool _held;

    private static bool _measuring;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        var padCommand = KeyMapping.Pad(Command);

        API.RegisterCommand($"+{Command}", new Action<int, List<object>, string>((_, _, _) => OnPressed()), false);
        Commands.Register($"-{Command}", false, new Action(() => _held = false));
        API.RegisterCommand($"+{padCommand}", new Action<int, List<object>, string>((_, _, _) => OnPressed()), false);
        Commands.Register($"-{padCommand}", false, new Action(() => _held = false));

        KeyMapping.Register(
            $"+{Command}",
            $"+{padCommand}",
            "vMenu: Toggle helmet visor (hold)",
            Key,
            Button);
    }

    private static void OnPressed()
    {
        _held = true;

        if (!_measuring)
        {
            Script.RunOnMainThread(Dispatch);
        }
    }

    private static async void Dispatch() => await MeasureHoldAsync();

    private static async Task MeasureHoldAsync()
    {
        _measuring = true;

        try
        {
            var started = API.GetGameTimer();

            while (_held && API.GetGameTimer() - started < HoldMs)
            {
                if (API.GetGameTimer() - started >= SuppressAfterMs && VisorToggle.CanToggle(API.PlayerPedId()))
                {
                    SuppressHeadlight();
                }

                await BaseScript.Delay(0);
            }

            if (!_held)
            {
                return;
            }

            var suppress = VisorToggle.CanToggle(API.PlayerPedId());
            var toggle = VisorToggle.ToggleAsync();

            while (suppress && (_held || !toggle.IsCompleted))
            {
                SuppressHeadlight();

                await BaseScript.Delay(0);
            }

            await toggle;
        }
        finally
        {
            _measuring = false;
        }
    }

    private static void SuppressHeadlight()
    {
        var ped = API.PlayerPedId();

        if (!API.IsPedInAnyVehicle(ped, false))
        {
            return;
        }

        // Group 2 is "everything except the frontend", which is the one that covers driving.
        API.DisableControlAction(2, (int)Control.VehicleHeadlight, true);
    }
}
