using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>Lets any other resource on this client put a message in vMenu's notification stack.</summary>
public static class NotificationEvents
{
    private const string NotifyEvent = "vmenu:notify";

    private const int MinDurationMs = 5000;

    private const int MaxDurationMs = 30000;

    public static void RegisterEventHandlers() =>
        Script.EventHandlers[NotifyEvent] += new Action<string, int, string>((message, durationMs, style) =>
            Notifications.Show(StyleFor(style ?? "info"), message, Duration(durationMs), Sender()));

    private static string? Sender()
    {
        var resource = API.GetInvokingResource();

        return string.IsNullOrEmpty(resource) || resource == API.GetCurrentResourceName()
            ? null
            : resource;
    }

    private static int Duration(int durationMs) =>
        Math.Clamp(durationMs <= 0 ? Notifications.DefaultDurationMs : durationMs, MinDurationMs, MaxDurationMs);

    private static NotificationStyle StyleFor(string style) => style.ToLowerInvariant() switch
    {
        "success" => NotificationStyle.Success,
        "warning" => NotificationStyle.Warning,
        "error" => NotificationStyle.Error,
        _ => NotificationStyle.Info,
    };
}
