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

    // Never null once Register has run, which is the only time keys exist.
    private static Func<bool> _isActive = static () => false;

    // isActive is asked before every key that changes something. A FiveM key mapping is a normal binding
    // that fires whenever the key is pressed, noclip or not, and these sit on keys the player uses
    // constantly: without this, sprinting would quietly wind the noclip speed up while you walk around.
    // The toggle is exempt for the obvious reason, and the movement keys because they only record which
    // way you are holding, which nothing reads until noclip is running.
    internal static void Register(Func<bool> isActive, Action onToggle, Action onSpeedUp, Action onSpeedDown, Action onFollowCam)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        _isActive = isActive;

        var toggleKey = ClientConfig.Value(KeyBindings.NoClipToggleKey);
        if (string.IsNullOrWhiteSpace(toggleKey))
        {
            toggleKey = "F2";
        }

        RegisterPress(Toggle, "vMenu NoClip: Toggle on/off", toggleKey, onToggle);
        RegisterPress(SpeedUp, "vMenu NoClip: Increase speed", "LSHIFT", WhileActive(onSpeedUp));
        RegisterPress(SpeedDown, "vMenu NoClip: Decrease speed", "LCONTROL", WhileActive(onSpeedDown));
        RegisterPress(FollowCam, "vMenu NoClip: Toggle follow cam", "H", WhileActive(onFollowCam));

        RegisterHold(Forward, "vMenu NoClip: Move forward", "W", held => ForwardHeld = held);
        RegisterHold(Backward, "vMenu NoClip: Move backward", "S", held => BackwardHeld = held);
        RegisterHold(TurnLeft, "vMenu NoClip: Turn left", "A", held => TurnLeftHeld = held);
        RegisterHold(TurnRight, "vMenu NoClip: Turn right", "D", held => TurnRightHeld = held);
        RegisterHold(Up, "vMenu NoClip: Move up", "Q", held => UpHeld = held);
        RegisterHold(Down, "vMenu NoClip: Move down", "Z", held => DownHeld = held);
    }

    private static Action WhileActive(Action handler) => () =>
    {
        if (_isActive())
        {
            handler();
        }
    };

    private static void RegisterPress(string command, string description, string defaultKey, Action onPressed)
    {
        SharedAPI.Commands.RegisterCommand(command, false, onPressed);
        Native.RegisterKeyMapping(command, description, "keyboard", defaultKey);
    }

    // Deliberately not gated on noclip being active. These track the key itself, so a player already
    // holding W when they switch noclip on carries straight on moving.
    private static void RegisterHold(string command, string description, string defaultKey, Action<bool> setHeld)
    {
        SharedAPI.Commands.RegisterCommand($"+{command}", false, new Action(() => setHeld(true)));
        SharedAPI.Commands.RegisterCommand($"-{command}", false, new Action(() => setHeld(false)));
        Native.RegisterKeyMapping($"+{command}", description, "keyboard", defaultKey);
    }

    private static int BindingControl(string command) => API.HashSigned(command) | int.MinValue;
}
