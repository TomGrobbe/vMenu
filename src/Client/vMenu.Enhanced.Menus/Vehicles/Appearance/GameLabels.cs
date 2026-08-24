using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// GTA already ships every mod, colour and plate name in the player's game language, so vMenu
// translating them again would be work that produces a worse result. Everything here goes through
// MenuText.From, so it is looked up when the row is drawn and follows a language change for free.
public static class GameLabels
{
    private static readonly TextInfo TitleCase = new CultureInfo("en-US", false).TextInfo;

    public static bool Exists(string key) =>
        !string.IsNullOrEmpty(key) && Native.DoesTextLabelExist(key);

    // GetLabelText answers the string "NULL" rather than nothing for a key it does not know, which is
    // why an empty check on its own is not enough.
    public static string Text(string key, string fallback)
    {
        if (string.IsNullOrEmpty(key))
        {
            return fallback;
        }

        var text = Native.GetLabelText(key);

        return string.IsNullOrEmpty(text) || text == "NULL" ? fallback : text;
    }

    // A row's text, taken from the game, falling back to a vMenu translation.
    public static MenuText Game(string key, string fallbackLocKey) =>
        Exists(key) ? MenuText.From(() => Text(key, string.Empty)) : MenuText.Key(fallbackLocKey);

    // A row's text, taken from the game, falling back to text vMenu does not translate.
    public static MenuText GameOrLiteral(string key, string fallback) =>
        MenuText.From(() => Text(key, fallback));

    // Turns a GXT key such as BLUE_SILVER into Blue Silver. Only ever seen when the game has no text for
    // a key, which on a stock install means never.
    public static string Humanise(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        return TitleCase.ToTitleCase(key.Replace('_', ' ').ToLowerInvariant());
    }
}
