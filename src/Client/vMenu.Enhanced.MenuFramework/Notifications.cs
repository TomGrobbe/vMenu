using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>Short messages stacked above the minimap, drawn by the NUI page, not the game's feed.</summary>
// Nothing here takes focus or waits for an answer, so the page needs no handshake. Where the stack
// sits depends on the safe zone and the map state, neither of which the page can see, so the client
// works it out and sends it once per notification. A resize mid-notification leaves that one where
// it was, which beats a tick running all session to catch it.
public static class Notifications
{
    /// <summary>Long enough to read a sentence without hurrying, short enough not to sit in the way.</summary>
    public const int DefaultDurationMs = 8500;

    // Measured off the game's own HUD. No native reports either, the map being a scaleform rather
    // than a HUD component whose size could be asked for.
    private const float MinimapHeight = 1f / 5.674f;

    private const float BigmapHeight = 1f / 2.35f;

    private const int VisibilityCheckMs = 500;

    /// <summary>Thirty seconds of waiting for a spawn, after which the message is shown regardless.</summary>
    private const int MaxVisibilityChecks = 60;

    public static void Info(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Info, text, durationMs);

    public static void Success(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Success, text, durationMs);

    public static void Warning(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Warning, text, durationMs);

    public static void Error(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Error, text, durationMs);

    /// <summary>Shows once the player can actually see the screen.</summary>
    // For anything raised during startup. The page is not listening that early, and the stack draws
    // above the minimap, which is behind the loading screen anyway. Returns immediately when the
    // player is already in, so a caller that runs both at startup and at runtime can use this
    // unconditionally.
    public static async Task ShowWhenVisibleAsync(NotificationStyle style, MenuText text, int durationMs = DefaultDurationMs)
    {
        for (var attempt = 0; attempt < MaxVisibilityChecks; attempt++)
        {
            if (Native.NetworkIsSessionStarted() && !Native.IsScreenFadedOut())
            {
                break;
            }

            await API.Delay(VisibilityCheckMs);
        }

        // Shown even if the checks never passed. A player stuck on a black screen has bigger
        // problems, and a message they might miss beats one that is silently never sent.
        Show(style, text, durationMs);
    }

    public static void Show(NotificationStyle style, MenuText text, int durationMs = DefaultDurationMs)
    {
        var message = text.Resolve(Localizer.Current);

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        Native.SendNuiMessage(BuildMessage(Name(style), message, durationMs));
    }

    /// <summary>The box the stack grows out of, as fractions of the screen, lined up with the minimap.</summary>
    private static (float Left, float Bottom, float Width) Anchor()
    {
        // Halved because the safe zone is split between two opposite edges. The figure MenuAPI and
        // the legacy HUD both position by.
        var inset = (1f - Native.GetSafeZoneSize()) / 2f;

        var map = Native.IsRadarHidden()
            ? 0f
            : Native.IsBigmapActive() ? BigmapHeight : MinimapHeight;

        return (inset, inset + map, MinimapWidth());
    }

    // Always the unexpanded width, even under the big map, because a notification as wide as the
    // expanded map would be a banner across the screen.
    private static float MinimapWidth()
    {
        var aspect = Native.GetScreenAspectRatio(false);

        return aspect > 0f ? 1f / (4f * aspect) : 1f / 4f;
    }

    private static string BuildMessage(string style, string text, int durationMs)
    {
        var (left, bottom, width) = Anchor();

        return ClientJson.Serialize(new NotifyMessage
        {
            Style = style,
            Text = text,
            Duration = durationMs,
            Anchor = new AnchorBox
            {
                Left = Fraction(left),
                Bottom = Fraction(bottom),
                Width = Fraction(width),
            },
        });
    }

    /// <summary>Drops the float noise a screen measurement carries past what the page lays out.</summary>
    private static float Fraction(float value) => MathF.Round(value, 5);

    private sealed class NotifyMessage
    {
        public string Type { get; } = "notify";

        public required string Style { get; init; }

        public required string Text { get; init; }

        public required int Duration { get; init; }

        public required AnchorBox Anchor { get; init; }
    }

    private sealed class AnchorBox
    {
        public required float Left { get; init; }

        public required float Bottom { get; init; }

        public required float Width { get; init; }
    }

    private static string Name(NotificationStyle style) => style switch
    {
        NotificationStyle.Success => "success",
        NotificationStyle.Warning => "warning",
        NotificationStyle.Error => "error",
        _ => "info",
    };
}
