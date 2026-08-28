using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Serialization;

using AdminSettings = vMenu.Enhanced.Data.Configuration.Settings.Admin;

namespace vMenu.Enhanced.Menus.Admin;

public static class Announcements
{
    private const int TextLimit = 200;

    private static int _nextId = -1;

    private static bool _busy;

    public static async Task SendAsync()
    {
        if (_busy)
        {
            return;
        }

        var typed = await UserInput.GetTextAsync(MenuText.Key(Loc.Admin.AnnouncePrompt), TextLimit);

        if (typed is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(typed))
        {
            Notifications.Warning(MenuText.Key(Loc.Admin.AnnounceEmpty));

            return;
        }

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(ActionIds.Admin.Announce, typed.Trim());

            if (result.Status != ActionStatus.Ok || result.Data.Length < 1)
            {
                AdminReport.Show(result);

                return;
            }

            Notifications.Success(MenuText.Key(
                Loc.Admin.AnnounceDone,
                ("count", MenuText.Literal(result.Data[0]))));
        }
        finally
        {
            _busy = false;
        }
    }

    public static void Show(string text)
    {
        var localizer = Localizer.Current;

        var body = MenuText
            .Key(Loc.Admin.AnnounceBanner, ("text", MenuText.Literal(text)))
            .Resolve(localizer);

        Native.SendNuiMessage(ClientJson.Serialize(new AnnouncementMessage
        {
            Id = _nextId--,
            Title = localizer.Get(Loc.Admin.AnnounceHeading),
            Text = body,
            Duration = DisplayMs(),
        }));
    }

    private static int DisplayMs() =>
        Math.Max(1, ClientConfig.Value(AdminSettings.AnnouncementSeconds)) * 1000;

    private sealed class AnnouncementMessage
    {
        public string Type { get; } = "staff_alert";

        public string Variant { get; } = "announcement";

        public required int Id { get; init; }

        public required string Title { get; init; }

        public required string Text { get; init; }

        public required int Duration { get; init; }
    }
}
