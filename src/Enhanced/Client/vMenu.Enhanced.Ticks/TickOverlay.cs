using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Ticks;

// TickRegistry.Dump as a panel on screen. MenuAPI runs its own copy of this scheduler and exposes it
// read only, so its loops are listed here too rather than needing a panel of their own. Redraws run
// on a tick of their own rather than off TickRegistry.Changed, because one Reevaluate pass raises
// that event once per tick it flips, so a convar edit would post a message per affected loop.
public static class TickOverlay
{
    private const string ToggleCommand = "vmenu_ticks_overlay";

    private const long RefreshIntervalMs = 100;

    private const string HideMessage = """{"type":"ticks","visible":false}""";

    private static TickHandle? _tick;

    private static bool _visible;

    private static bool _dirty;

    private static bool _onRight = true;

    private static bool _paused;

    // The live state, not the stored preference.
    public static bool Visible => _visible;

    internal static void Initialize()
    {
        // Ungated on purpose, unlike every other command in here. This one is the way back out of an overlay
        // a player left switched on, so it has to work on a server that offers neither the developer features
        // menu nor the client debugging convar.
        SharedAPI.Commands.RegisterCommand(ToggleCommand, false, new Action(Toggle));

        TickRegistry.Changed += MarkDirty;
        MenuTicks.Changed += MarkDirty;

        _tick = TickRegistry.Register(
            "Ticks.Overlay",
            Flush,
            TickRate.Every(RefreshIntervalMs),
            () => _visible);
    }

    // Not read by the tick condition directly, because that runs while the resource is still starting
    // and the page would miss the first snapshot.
    public static void Restore() => Apply(UserDefaults.TicksOverlay.Value, persist: false);

    public static void Toggle() => API.RunOnMainThread(() => Set(!_visible));

    public static void Set(bool visible) => Apply(visible, persist: true);

    private static void Apply(bool visible, bool persist)
    {
        if (persist)
        {
            UserDefaults.TicksOverlay.Value = visible;
        }

        if (_visible == visible)
        {
            return;
        }

        _visible = visible;

        // Nothing else re-runs the condition, and hiding stops the tick before it could post the message
        // that empties the panel, so that one is sent from here.
        _tick?.Reevaluate();

        if (_visible)
        {
            _dirty = true;

            return;
        }

        Native.SendNuiMessage(HideMessage);
    }

    private static void MarkDirty() => _dirty = true;

    private static void Flush()
    {
        // Nothing announces an alignment change, so the side is polled rather than watched.
        var onRight = MenuController.MenuAlignment == MenuController.MenuAlignmentOption.Left;

        if (onRight != _onRight)
        {
            _onRight = onRight;
            _dirty = true;
        }

        // Polled for the same reason. The game blurs the world behind the pause menu and the panel is meant
        // to look like it belongs to that world, so it blurs with it.
        var paused = Native.IsPauseMenuActive();

        if (paused != _paused)
        {
            _paused = paused;
            _dirty = true;
        }

        if (!_dirty)
        {
            return;
        }

        _dirty = false;

        Native.SendNuiMessage(BuildMessage());
    }

    private static string BuildMessage()
    {
        var ours = TickRegistry.Handles;
        var theirs = MenuTicks.Handles;

        var rows = new TickRow[ours.Count + theirs.Count];
        var index = 0;

        foreach (var handle in ours)
        {
            rows[index++] = new TickRow
            {
                Source = "vMenu",
                Name = handle.Name,
                Rate = handle.Rate.ToString(),
                Running = handle.IsRunning,
            };
        }

        // Same shape, different assembly: MenuTickHandle and TickHandle share no base type, so this is a
        // second loop rather than one over a common interface.
        foreach (var handle in theirs)
        {
            rows[index++] = new TickRow
            {
                Source = "MenuAPI",
                Name = handle.Name,
                Rate = handle.Rate.ToString(),
                Running = handle.IsRunning,
            };
        }

        return ClientJson.Serialize(new TicksMessage
        {
            Side = _onRight ? "right" : "left",
            Paused = _paused,
            Ticks = rows,
        });
    }

    private sealed class TicksMessage
    {
        public string Type { get; } = "ticks";

        public bool Visible { get; } = true;

        public required string Side { get; init; }

        // So the panel can blur along with the world.
        public required bool Paused { get; init; }

        public required IReadOnlyList<TickRow> Ticks { get; init; }
    }

    private sealed class TickRow
    {
        // Which scheduler the tick belongs to, so the panel can group them.
        public required string Source { get; init; }

        public required string Name { get; init; }

        public required string Rate { get; init; }

        public required bool Running { get; init; }
    }
}
