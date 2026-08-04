using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Short messages stacked above the minimap, drawn by the NUI page rather than the game's feed.
/// </summary>
/// <remarks>
/// Unlike <see cref="UserInput"/> nothing here takes focus or waits for an answer, so a notification
/// is a one-way message and the page needs no handshake to receive it.
/// <para>
/// Where the stack sits depends on the safe zone and on whether the map is expanded, neither of which
/// the page can see. The client works it out and sends it along, once per notification: a resize or
/// a map toggle mid-notification leaves that one where it was, which is worth a frame of drift
/// against a tick that would run for the entire session to catch it.
/// </para>
/// </remarks>
public static class Notifications
{
    /// <summary>Long enough to read a sentence without hurrying, short enough not to sit in the way.</summary>
    public const int DefaultDurationMs = 8500;

    /// <summary>
    /// The minimap's height as a fraction of the screen, and the same for the expanded map. Both are
    /// measured off the game's own HUD: no native reports either, and the map is a scaleform rather
    /// than a HUD component whose size could be asked for.
    /// </summary>
    private const float MinimapHeight = 1f / 5.674f;

    private const float BigmapHeight = 1f / 2.35f;

    public static void Info(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Info, text, durationMs);

    public static void Success(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Success, text, durationMs);

    public static void Warning(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Warning, text, durationMs);

    public static void Error(MenuText text, int durationMs = DefaultDurationMs) =>
        Show(NotificationStyle.Error, text, durationMs);

    public static void Show(NotificationStyle style, MenuText text, int durationMs = DefaultDurationMs)
    {
        var message = text.Resolve(Localizer.Current);

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        Native.SendNuiMessage(BuildMessage(Name(style), message, durationMs));
    }

    /// <summary>
    /// The box the stack grows out of, as fractions of the screen: how far in from the left edge, how
    /// far up from the bottom edge, and how wide. Sized and placed to line up with the minimap.
    /// </summary>
    private static (float Left, float Bottom, float Width) Anchor()
    {
        // The safe zone takes the same fraction off every edge, halved because it is split between
        // the two opposite sides. This is the figure MenuAPI and the legacy HUD both position by.
        var inset = (1f - Native.GetSafeZoneSize()) / 2f;

        var map = Native.IsRadarHidden()
            ? 0f
            : Native.IsBigmapActive() ? BigmapHeight : MinimapHeight;

        return (inset, inset + map, MinimapWidth());
    }

    /// <summary>
    /// The minimap is a quarter of the screen's height wide, which as a fraction of the width is that
    /// over the aspect ratio. Always the unexpanded width, even under the big map: a notification as
    /// wide as the expanded map would be a banner across the screen.
    /// </summary>
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
        public string Type => "notify";

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
