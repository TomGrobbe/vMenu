using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Serialization;

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

    internal static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(ToggleCommand, false, DebugCommands.Gate(Toggle));

        TickRegistry.Changed += MarkDirty;

        _tick = TickRegistry.Register(
            "Ticks.Overlay",
            Flush,
            TickRate.Every(RefreshIntervalMs),
            () => _visible);
    }

    public static void Toggle()
    {
        _visible = !_visible;

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
