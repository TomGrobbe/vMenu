using MenuAPI;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Logging;

using AppearanceSettings = vMenu.Enhanced.Data.Configuration.Settings.MenuAppearance;

namespace vMenu.Enhanced.MenuFramework;

public static class HeaderStyle
{
    private static string? _reportedAlignment;

    // Call after ClientConfig.Initialize, before the menus are built.
    public static void Initialize()
    {
        ClientConfig.AddEventListenerFor(
            [
                AppearanceSettings.TitleAlignment,
                AppearanceSettings.HeaderGlare,
            ],
            Apply);

        Apply();
    }

    private static void Apply()
    {
        MenuController.DefaultTitleAlignment = Alignment();
        MenuController.DefaultTitleFont = MenuFont.ChaletComprimeCologne;
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

    private static void Report(ref string? reported, string raw, string message)
    {
        if (string.Equals(reported, raw, StringComparison.Ordinal))
        {
            return;
        }

        reported = raw;

        Log.Warning($"[Menu] {message}");
    }
}
