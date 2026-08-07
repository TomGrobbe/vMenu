using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Ticks;

/// <summary>
/// <see cref="TickRegistry.Dump"/> as a panel on screen, redrawn whenever a tick starts or stops.
/// </summary>
// Redraws run on a tick of its own rather than off TickRegistry.Changed, because one Reevaluate pass
// raises that event once per tick it flips, so a convar edit would post a message per affected loop.
// The flag collapses a burst into one snapshot, and the tick stops with the panel.
public static class TickOverlay
{
    private const string ToggleCommand = "vmenu_ticks_overlay";

    private const long RefreshIntervalMs = 100;

    private const string HideMessage = """{"type":"ticks","visible":false}""";

    private static TickHandle? _tick;

    private static bool _visible;

    private static bool _dirty;

    private static bool _onRight = true;

    /// <summary>Whether the panel is on screen. The live state, not the stored preference.</summary>
    public static bool Visible => _visible;

    internal static void Initialize()
    {
        // Ungated on purpose, unlike every other command in here. This one is the way back out of an
        // overlay a player left switched on, so it has to work on a server that offers neither the
        // developer features menu nor the client debugging convar.
        SharedAPI.Commands.RegisterCommand(ToggleCommand, false, new Action(Toggle));

        TickRegistry.Changed += MarkDirty;

        _tick = TickRegistry.Register(
            "Ticks.Overlay",
            Flush,
            TickRate.Every(RefreshIntervalMs),
            () => _visible);
    }

    /// <summary>Puts the panel back the way the player left it. Call once at startup.</summary>
    // Not read by the tick condition directly, because that runs while the resource is still starting
    // and the page would miss the first snapshot.
    public static void Restore() => Apply(UserDefaults.TicksOverlay.Value, persist: false);

    public static void Toggle() => Set(!_visible);

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

        // Nothing else re-runs the condition, and hiding stops the tick before it could post the
        // message that empties the panel, so that one is sent from here.
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

        if (!_dirty)
        {
            return;
        }

        _dirty = false;

        Native.SendNuiMessage(BuildMessage());
    }

    private static string BuildMessage()
    {
        var handles = TickRegistry.Handles;
        var rows = new TickRow[handles.Count];

        for (var index = 0; index < handles.Count; index++)
        {
            var handle = handles[index];

            rows[index] = new TickRow
            {
                Name = handle.Name,
                Rate = handle.Rate.ToString(),
                Running = handle.IsRunning,
            };
        }

        return ClientJson.Serialize(new TicksMessage
        {
            Side = _onRight ? "right" : "left",
            Ticks = rows,
        });
    }

    private sealed class TicksMessage
    {
        public string Type { get; } = "ticks";

        public bool Visible { get; } = true;

        public required string Side { get; init; }

        public required IReadOnlyList<TickRow> Ticks { get; init; }
    }

    private sealed class TickRow
    {
        public required string Name { get; init; }

        public required string Rate { get; init; }

        public required bool Running { get; init; }
    }
}
