using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

namespace vMenu.Enhanced.Menus.Players.Appearance;

public static class VisorKeyBinding
{
    private const string Command = "vmenu:visor";

    private const int HoldMs = 400;

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

        SharedAPI.Commands.RegisterCommand($"+{Command}", false, new Action(OnPressed));
        SharedAPI.Commands.RegisterCommand($"-{Command}", false, new Action(() => _held = false));
        SharedAPI.Commands.RegisterCommand($"+{padCommand}", false, new Action(OnPressed));
        SharedAPI.Commands.RegisterCommand($"-{padCommand}", false, new Action(() => _held = false));

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
            SharedAPI.RunOnMainThread(Dispatch);
        }
    }

    private static async void Dispatch() => await MeasureHoldAsync();

    private static async Task MeasureHoldAsync()
    {
        _measuring = true;

        try
        {
            var started = Native.GetGameTimer();

            while (_held && Native.GetGameTimer() - started < HoldMs)
            {
                SuppressHeadlight();

                await API.Delay(0);
            }

            if (_held)
            {
                await VisorToggle.ToggleAsync();
            }
        }
        finally
        {
            _measuring = false;
        }
    }

    private static void SuppressHeadlight()
    {
        var ped = Native.PlayerPedId();

        if (!Native.IsPedInAnyVehicle(ped, false))
        {
            return;
        }

        // Group 2 is "everything except the frontend", which is the one that covers driving.
        Native.DisableControlAction(2, (int)Control.VehicleHeadlight, true);
    }
}
