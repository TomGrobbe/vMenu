using System.Globalization;
using System.Text;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

namespace vMenu.Enhanced.Ticks;

/// <summary>
/// <see cref="TickRegistry.Dump"/> as a panel on screen, redrawn whenever a tick starts or stops.
/// </summary>
/// <remarks>
/// Redraws are driven by a tick of its own rather than by <see cref="TickRegistry.Changed"/> directly,
/// because one <see cref="TickRegistry.Reevaluate"/> pass raises that event once per tick it flips —
/// a convar edit would otherwise post a message per affected loop. The flag collapses a burst into a
/// single snapshot, and the tick stops with the panel, so a hidden overlay costs nothing.
/// </remarks>
public static class TickOverlay
{
    private const string ToggleCommand = "vmenu_ticks_overlay";

    private const long RefreshIntervalMs = 100;

    private const string HideMessage = """{"type":"ticks","visible":false}""";

    private static readonly StringBuilder Builder = new(512);

    private static TickHandle? _tick;

    private static bool _visible;

    private static bool _dirty;

    private static bool _onRight = true;

    internal static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(ToggleCommand, false, new Action(Toggle));

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

        Builder.Clear();
        Builder.Append("""{"type":"ticks","visible":true,"side":""")
            .Append(_onRight ? "\"right\"" : "\"left\"")
            .Append(""","ticks":[""");

        for (var index = 0; index < handles.Count; index++)
        {
            var handle = handles[index];

            if (index > 0)
            {
                Builder.Append(',');
            }

            Builder.Append("""{"name":""");
            AppendString(handle.Name);
            Builder.Append(""","rate":""");
            AppendString(handle.Rate.ToString());
            Builder.Append(""","running":""").Append(handle.IsRunning ? "true" : "false").Append('}');
        }

        return Builder.Append("]}").ToString();
    }

    /// <summary>Written by hand because <c>System.Text.Json</c> does not load in the FiveM client sandbox.</summary>
    private static void AppendString(string text)
    {
        Builder.Append('"');

        foreach (var character in text)
        {
            switch (character)
            {
                case '"':
                    Builder.Append("\\\"");
                    break;
                case '\\':
                    Builder.Append("\\\\");
                    break;
                default:
                    if (character < ' ')
                    {
                        Builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        Builder.Append(character);
                    }

                    break;
            }
        }

        Builder.Append('"');
    }
}
