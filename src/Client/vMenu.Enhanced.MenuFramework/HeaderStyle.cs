using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;

using AppearanceSettings = vMenu.Enhanced.Data.Configuration.Settings.MenuAppearance;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>Keeps MenuAPI's banner defaults in step with the appearance settings.</summary>
// Nothing is rebuilt when one changes: MenuAPI resolves these defaults every frame it draws a
// banner, so a menu that already exists picks the new look up on its own.
public static class HeaderStyle
{
    private static readonly (string Name, int Id)[] Fonts =
    [
        ("ChaletLondon", MenuFont.ChaletLondon),
        ("HouseScript", MenuFont.HouseScript),
        ("Monospace", MenuFont.Monospace),
        ("ChaletComprimeCologne", MenuFont.ChaletComprimeCologne),
        ("Pricedown", MenuFont.Pricedown),
    ];

    private static string? _reportedAlignment;

    private static string? _reportedFont;

    /// <summary>Call after <see cref="ClientConfig.Initialize"/>, before the menus are built.</summary>
    public static void Initialize()
    {
        ClientConfig.Changed += Apply;

        Apply();
    }

    private static void Apply()
    {
        MenuController.DefaultTitleAlignment = Alignment();
        MenuController.DefaultTitleFont = Font();
        MenuController.DefaultShowHeaderGlare = ClientConfig.Value(AppearanceSettings.HeaderGlare);
    }

    private static Menu.TitleAlignmentOption Alignment()
    {
        var raw = ClientConfig.Value(AppearanceSettings.TitleAlignment);
        var value = raw.Trim();

        if (string.Equals(value, "left", StringComparison.OrdinalIgnoreCase))
        {
            return Menu.TitleAlignmentOption.Left;
        }

        if (string.Equals(value, "center", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "centre", StringComparison.OrdinalIgnoreCase))
        {
            return Menu.TitleAlignmentOption.Center;
        }

        if (string.Equals(value, "right", StringComparison.OrdinalIgnoreCase))
        {
            return Menu.TitleAlignmentOption.Right;
        }

        Report(
            ref _reportedAlignment,
            raw,
            $"{AppearanceSettings.TitleAlignment.Name} is set to '{raw}', which is not left, center or right. Using left.");

        return Menu.TitleAlignmentOption.Left;
    }

    private static int Font()
    {
        var raw = ClientConfig.Value(AppearanceSettings.TitleFont);
        var value = raw.Trim();

        foreach (var font in Fonts)
        {
            if (string.Equals(font.Name, value, StringComparison.OrdinalIgnoreCase))
            {
                return font.Id;
            }
        }

        // A bare id as well as a name, so a font another resource registered at runtime can be used.
        if (ConvarValue.ParseInt(value) is { } id && id >= 0)
        {
            return id;
        }

        Report(
            ref _reportedFont,
            raw,
            $"{AppearanceSettings.TitleFont.Name} is set to '{raw}', which is not a font vMenu knows about. Using Chalet Comprime Cologne.");

        return MenuFont.ChaletComprimeCologne;
    }

    // Every setting change runs Apply, so without this one bad value complains again on every
    // unrelated change.
    private static void Report(ref string? reported, string raw, string message)
    {
        if (string.Equals(reported, raw, StringComparison.Ordinal))
        {
            return;
        }

        reported = raw;

        API.Log.Warn($"[Menu] {message}");
    }
}
