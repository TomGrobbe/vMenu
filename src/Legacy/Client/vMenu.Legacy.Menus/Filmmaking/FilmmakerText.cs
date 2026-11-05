using System.Globalization;

using vMenu.Enhanced.Freecam;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;

using FreecamCamera = vMenu.Enhanced.Freecam.Freecam;

namespace vMenu.Enhanced.Menus.Filmmaking;

internal static class FilmmakerText
{
    private const int NameLength = 40;

    internal static MenuText Number(float value, string format = "0.##") =>
        MenuText.Literal(value.ToString(format, CultureInfo.InvariantCulture));

    internal static MenuText Number(int value) =>
        MenuText.Literal(value.ToString(CultureInfo.InvariantCulture));

    internal static string Format(float value, string format = "0.##") =>
        value.ToString(format, CultureInfo.InvariantCulture);

    internal static IReadOnlyList<MenuText> Keys(params string[] keys) =>
        keys.Select(MenuText.Key).ToList();

    internal static ButtonEntry Placeholder(string textKey) => new(textKey)
    {
        ReadEnabled = static () => false,
    };

    internal static bool NeedsFreecam()
    {
        if (FreecamCamera.IsActive)
        {
            return false;
        }

        Notifications.Error(MenuText.Key(Loc.Freecam.NotActive));

        return true;
    }

    internal static async Task<string?> PromptNameAsync(string current = "")
    {
        var name = (await UserInput.GetTextAsync(MenuText.Key(Loc.Filmmaker.NamePrompt), NameLength, current))?.Trim();

        return string.IsNullOrEmpty(name) ? null : name;
    }

    internal static bool Report(FilmSaveOutcome outcome, string successKey, string name)
    {
        switch (outcome)
        {
            case FilmSaveOutcome.Saved:
                Notifications.Success(MenuText.Key(successKey, ("name", MenuText.Literal(name))));

                return true;

            case FilmSaveOutcome.NameTaken:
                Notifications.Error(MenuText.Key(Loc.Filmmaker.NameTaken, ("name", MenuText.Literal(name))));

                return false;

            default:
                Notifications.Error(MenuText.Key(Loc.Filmmaker.SaveRefused));

                return false;
        }
    }
}
