using MenuAPI;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Logging;

using AppearanceSettings = vMenu.Enhanced.Data.Configuration.Settings.MenuAppearance;

namespace vMenu.Enhanced.MenuFramework;

public static class MenuSkin
{
    // Paths are resolved against the NUI page, which is ui/index.html, so these are ui/themes and
    // ui/menuapi-banners on disk.
    private static readonly (string[] Aliases, string? Theme, string? Path, string? Banner)[] Skins =
    [
        (["default", "vmenu"], "Default", "themes/default.css", "default"),
        (["gta", "none", "vanilla"], null, null, null),
        (["dark", "vmenudark", "vmenu-dark", "vmenu dark"], "Dark", "themes/dark.css", "dark"),
        (["cartoon"], "Cartoon", "themes/cartoon.css", "cartoon"),
    ];

    private static string? _reported;

    // Read by MenuHost, so a menu built after the skin was applied opens with the right banner.
    public static string? Banner { get; private set; }

    // Call after ClientConfig.Initialize, before the menus are built.
    public static void Initialize()
    {
        foreach (var (_, theme, path, _) in Skins)
        {
            if (theme is not null && path is not null)
            {
                NuiTuning.RegisterTheme(theme, path);
            }
        }

        ClientConfig.AddEventListenerFor([AppearanceSettings.Skin], Apply);

        Apply();
    }

    private static void Apply()
    {
        var (theme, banner) = Resolve();

        NuiTuning.SetTheme(theme);

        Banner = banner;

        MenuRegistry.ApplyBanner(banner);
    }

    private static (string? Theme, string? Banner) Resolve()
    {
        var raw = ClientConfig.Value(AppearanceSettings.Skin);
        var value = raw.Trim();

        foreach (var (aliases, theme, _, banner) in Skins)
        {
            foreach (var alias in aliases)
            {
                if (string.Equals(alias, value, StringComparison.OrdinalIgnoreCase))
                {
                    return (theme, banner);
                }
            }
        }

        Report(
            raw,
            $"{AppearanceSettings.Skin.Name} is set to '{raw}', which is not a skin vMenu knows about. Using '{Skins[0].Aliases[0]}'.");

        return (Skins[0].Theme, Skins[0].Banner);
    }

    private static void Report(string raw, string message)
    {
        if (string.Equals(_reported, raw, StringComparison.Ordinal))
        {
            return;
        }

        _reported = raw;

        Log.Warning($"[Menu] {message}");
    }
}
