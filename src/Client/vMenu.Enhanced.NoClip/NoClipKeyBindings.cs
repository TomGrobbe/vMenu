using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;

namespace vMenu.Enhanced.NoClip;

/// <summary>
/// Noclip's keyboard controls, registered as FiveM key mappings so players can rebind every one of
/// them under Settings, Key Bindings.
/// </summary>
// Command names are fixed rather than built from the resource name, because a player's binding is
// saved against the command: a fixed name means renaming the resource does not wipe their choice.
internal static class NoClipKeyBindings
{
    private const string Toggle = "vmenu:noclip:toggle";
    private const string Forward = "vmenu:noclip:forward";
    private const string Backward = "vmenu:noclip:backward";
    private const string TurnLeft = "vmenu:noclip:turnleft";
    private const string TurnRight = "vmenu:noclip:turnright";
    private const string Up = "vmenu:noclip:up";
    private const string Down = "vmenu:noclip:down";
    private const string SpeedUp = "vmenu:noclip:speedup";
    private const string SpeedDown = "vmenu:noclip:speeddown";
    private const string FollowCam = "vmenu:noclip:followcam";

    internal static bool ForwardHeld { get; private set; }

    internal static bool BackwardHeld { get; private set; }

    internal static bool TurnLeftHeld { get; private set; }

    internal static bool TurnRightHeld { get; private set; }

    internal static bool UpHeld { get; private set; }

    internal static bool DownHeld { get; private set; }

    internal static int ForwardControl { get; } = BindingControl($"+{Forward}");

    internal static int BackwardControl { get; } = BindingControl($"+{Backward}");

    internal static int TurnLeftControl { get; } = BindingControl($"+{TurnLeft}");

    internal static int TurnRightControl { get; } = BindingControl($"+{TurnRight}");

    internal static int UpControl { get; } = BindingControl($"+{Up}");

    internal static int DownControl { get; } = BindingControl($"+{Down}");

    internal static int SpeedUpControl { get; } = BindingControl(SpeedUp);

    internal static int SpeedDownControl { get; } = BindingControl(SpeedDown);

    internal static int FollowCamControl { get; } = BindingControl(FollowCam);

    private static bool _registered;

    /// <param name="onToggle">Runs when the toggle key is pressed.</param>
    /// <param name="onSpeedUp">Runs when the speed up key is pressed.</param>
    /// <param name="onSpeedDown">Runs when the speed down key is pressed.</param>
    /// <param name="onFollowCam">Runs when the follow cam key is pressed.</param>
    internal static void Register(Action onToggle, Action onSpeedUp, Action onSpeedDown, Action onFollowCam)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        var toggleKey = ClientConfig.Value(KeyBindings.NoClipToggleKey);
        if (string.IsNullOrWhiteSpace(toggleKey))
        {
            toggleKey = "F2";
        }

        RegisterPress(Toggle, "Toggle noclip", toggleKey, onToggle);
        RegisterPress(SpeedUp, "NoClip: increase speed", "LSHIFT", onSpeedUp);
        RegisterPress(SpeedDown, "NoClip: decrease speed", "LCONTROL", onSpeedDown);
        RegisterPress(FollowCam, "NoClip: toggle follow cam", "H", onFollowCam);

        RegisterHold(Forward, "NoClip: move forward", "W", held => ForwardHeld = held);
        RegisterHold(Backward, "NoClip: move backward", "S", held => BackwardHeld = held);
        RegisterHold(TurnLeft, "NoClip: turn left", "A", held => TurnLeftHeld = held);
        RegisterHold(TurnRight, "NoClip: turn right", "D", held => TurnRightHeld = held);
        RegisterHold(Up, "NoClip: move up", "Q", held => UpHeld = held);
        RegisterHold(Down, "NoClip: move down", "Z", held => DownHeld = held);
    }

    /// <summary>
    /// Clears the held directions, so a key still down when noclip switches off does not leave the
    /// entity drifting the moment it comes back on.
    /// </summary>
    internal static void ClearHeld()
    {
        ForwardHeld = false;
        BackwardHeld = false;
        TurnLeftHeld = false;
        TurnRightHeld = false;
        UpHeld = false;
        DownHeld = false;
    }

    /// <summary>The instructional button string for a binding, following whatever the player bound it to.</summary>
    internal static string Button(int control) => Native.GetControlInstructionalButton(0, control, true);

    private static void RegisterPress(string command, string description, string defaultKey, Action onPressed)
    {
        SharedAPI.Commands.RegisterCommand(command, false, onPressed);
        Native.RegisterKeyMapping(command, description, "keyboard", defaultKey);
    }

    /// <summary>
    /// Registers a press/release pair. FiveM runs "+command" on press and "-command" on release, and
    /// only the "+" form goes into the key mapping.
    /// </summary>
    private static void RegisterHold(string command, string description, string defaultKey, Action<bool> setHeld)
    {
        SharedAPI.Commands.RegisterCommand($"+{command}", false, new Action(() => setHeld(true)));
        SharedAPI.Commands.RegisterCommand($"-{command}", false, new Action(() => setHeld(false)));
        Native.RegisterKeyMapping($"+{command}", description, "keyboard", defaultKey);
    }

    // FiveM keys its bindings on joaat(command) with the top bit set, and its hooks on the game's
    // control functions all switch on that bit. int.MinValue is that top bit. The command has to be
    // the exact string it was registered with, '+' prefix and all.
    private static int BindingControl(string command) => API.HashSigned(command) | int.MinValue;
}
