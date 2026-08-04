using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;

namespace vMenu.Enhanced.NoClip;

internal static class NoClipKeyBindings
{
    private const string Toggle = "vmenu:noclip:togglenc";
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

        RegisterPress(Toggle, "vMenu NoClip: Toggle on/off", toggleKey, onToggle);
        RegisterPress(SpeedUp, "vMenu NoClip: Increase speed", "LSHIFT", onSpeedUp);
        RegisterPress(SpeedDown, "vMenu NoClip: Decrease speed", "LCONTROL", onSpeedDown);
        RegisterPress(FollowCam, "vMenu NoClip: Toggle follow cam", "H", onFollowCam);

        RegisterHold(Forward, "vMenu NoClip: Move forward", "W", held => ForwardHeld = held);
        RegisterHold(Backward, "vMenu NoClip: Move backward", "S", held => BackwardHeld = held);
        RegisterHold(TurnLeft, "vMenu NoClip: Turn left", "A", held => TurnLeftHeld = held);
        RegisterHold(TurnRight, "vMenu NoClip: Turn right", "D", held => TurnRightHeld = held);
        RegisterHold(Up, "vMenu NoClip: Move up", "Q", held => UpHeld = held);
        RegisterHold(Down, "vMenu NoClip: Move down", "Z", held => DownHeld = held);
    }

    internal static void ClearHeld()
    {
        ForwardHeld = false;
        BackwardHeld = false;
        TurnLeftHeld = false;
        TurnRightHeld = false;
        UpHeld = false;
        DownHeld = false;
    }

    private static void RegisterPress(string command, string description, string defaultKey, Action onPressed)
    {
        SharedAPI.Commands.RegisterCommand(command, false, onPressed);
        Native.RegisterKeyMapping(command, description, "keyboard", defaultKey);
    }

    private static void RegisterHold(string command, string description, string defaultKey, Action<bool> setHeld)
    {
        SharedAPI.Commands.RegisterCommand($"+{command}", false, new Action(() => setHeld(true)));
        SharedAPI.Commands.RegisterCommand($"-{command}", false, new Action(() => setHeld(false)));
        Native.RegisterKeyMapping($"+{command}", description, "keyboard", defaultKey);
    }

    private static int BindingControl(string command) => API.HashSigned(command) | int.MinValue;
}
