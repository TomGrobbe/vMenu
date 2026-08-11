using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// The key that flips a helmet visor, held rather than tapped.
/// </summary>
/// <remarks>
/// Held because the same button does something else on a bike, and a hold is how the two are told
/// apart. Nothing runs until the key goes down: there is no tick watching for it and no cached idea
/// of whether the player is wearing a helmet, because another resource can change that at any moment
/// and a remembered answer would be wrong more often than it was right.
/// </remarks>
public static class VisorKeyBinding
{
    private const string Command = "vmenu:visor";

    /// <summary>How long the key has to be down before the visor moves.</summary>
    // Long enough that pressing the same button for its other job does not flip the visor as well,
    // short enough that holding it does not feel like waiting.
    private const int HoldMs = 400;

    /// <summary>Used when a server owner blanks the convar rather than leaving it alone.</summary>
    private const string FallbackKey = "F11";

    /// <summary>D-pad right, which is the button the game itself puts the visor on.</summary>
    // Tap it for the headlights, hold it for the visor, exactly as GTA Online does. That shared
    // button is the whole reason this is a hold and the reason the headlights are held off below.
    private const string FallbackButton = "LRIGHT_INDEX";

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

        var key = ClientConfig.Value(KeyBindings.VisorToggleKey);

        if (string.IsNullOrWhiteSpace(key))
        {
            key = FallbackKey;
        }

        var button = ClientConfig.Value(KeyBindings.VisorToggleButton);

        if (string.IsNullOrWhiteSpace(button))
        {
            button = FallbackButton;
        }

        SharedAPI.Commands.RegisterCommand($"+{Command}", false, new Action(OnPressed));
        SharedAPI.Commands.RegisterCommand($"-{Command}", false, new Action(() => _held = false));

        const string Description = "vMenu: Toggle helmet visor (hold)";

        // The same command under both mappers, which is how FiveM gives one action a key and a
        // button. Rebinding either one in the game's own settings leaves the other alone.
        Native.RegisterKeyMapping($"+{Command}", Description, "keyboard", key);
        Native.RegisterKeyMapping($"+{Command}", Description, "PAD_DIGITALBUTTON", button);
    }

    /// <summary>
    /// A key handler does not run on the game thread, and a native asked about the world from one
    /// answers as though nothing is there. Everything that touches the game is handed off.
    /// </summary>
    private static void OnPressed()
    {
        _held = true;

        if (!_measuring)
        {
            SharedAPI.RunOnMainThread(Dispatch);
        }
    }

    /// <summary>A command handler cannot await, so this is the fire and forget boundary.</summary>
    private static async void Dispatch() => await MeasureHoldAsync();

    /// <summary>
    /// Runs only while the key is down, then stops. There is no tick behind this and nothing to
    /// switch off when nobody is holding anything.
    /// </summary>
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

    /// <summary>
    /// Keeps the bike's headlights alone while the hold is being measured.
    /// </summary>
    // The controller default is D-pad right, which is also the headlight button, and that sharing is
    // deliberate because it is what the game does. Held off only for the fraction of a second it
    // takes to tell a tap from a hold, so tapping still works the headlights as normal.
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
