using MenuAPI;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Logging;

using AppearanceSettings = vMenu.Enhanced.Data.Configuration.Settings.MenuAppearance;

namespace vMenu.Enhanced.MenuFramework;

public sealed class MenuSkinChoice
{
    internal MenuSkinChoice(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; }

    public string Name { get; }
}

public static class MenuSkin
{
    // Paths are resolved against the NUI page, which is ui/index.html, so these are ui/themes and
    // ui/menuapi-banners on disk.
    private static readonly (string[] Aliases, string Name, string? Theme, string? Path, string? Banner)[] Skins =
    [
        (["default", "vmenu"], "Default", "Default", "themes/default.css", "default"),
        (["gta", "none", "vanilla"], "GTA V", null, null, null),
        (["dark", "vmenudark", "vmenu-dark", "vmenu dark"], "Dark", "Dark", "themes/dark.css", "dark"),
        (["cartoon"], "Cartoon", "Cartoon", "themes/cartoon.css", "cartoon"),
    ];

    private static string? _reported;

    private static int? _override;

    // Read by MenuHost, so a menu built after the skin was applied opens with the right banner.
    public static string? Banner { get; private set; }

    public static string CurrentId => Skins[_override ?? ConfiguredIndex()].Aliases[0];

    public static string ConfiguredId => Skins[ConfiguredIndex()].Aliases[0];

    public static bool IsOverridden => _override is not null;

    public static event Action? Changed;

    public static List<MenuSkinChoice> Choices()
    {
        var choices = new List<MenuSkinChoice>(Skins.Length);

        foreach (var (aliases, name, _, _, _) in Skins)
        {
            choices.Add(new MenuSkinChoice(aliases[0], name));
        }

        return choices;
    }

    // Call after ClientConfig.Initialize, before the menus are built.
    public static void Initialize()
    {
        foreach (var (_, _, theme, path, _) in Skins)
        {
            if (theme is not null && path is not null)
            {
                NuiTuning.RegisterTheme(theme, path);
            }
        }

        ClientConfig.AddEventListenerFor([AppearanceSettings.Skin], Apply);

        Apply();
    }

    public static bool TryApplyOverride(string id)
    {
        if (IndexOf(id) is not { } index)
        {
            return false;
        }

        _override = index;

        Apply();

        return true;
    }

    public static void ClearOverride()
    {
        if (_override is null)
        {
            return;
        }

        _override = null;

        Apply();
    }

    private static void Apply()
    {
        var (_, _, theme, _, banner) = Skins[_override ?? ConfiguredIndex()];

        NuiTuning.SetTheme(theme);

        Banner = banner;

        MenuRegistry.ApplyBanner(banner);

        try
        {
            Changed?.Invoke();
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] A skin Changed handler threw: {exception}");
        }
    }

    private static int ConfiguredIndex()
    {
        var raw = ClientConfig.Value(AppearanceSettings.Skin);

        if (IndexOf(raw.Trim()) is { } index)
        {
            return index;
        }

        Report(
            raw,
            $"{AppearanceSettings.Skin.Name} is set to '{raw}', which is not a skin vMenu knows about. Using '{Skins[0].Aliases[0]}'.");

        return 0;
    }

    private static int? IndexOf(string value)
    {
        for (var index = 0; index < Skins.Length; index++)
        {
            foreach (var alias in Skins[index].Aliases)
            {
                if (string.Equals(alias, value, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }
        }

        return null;
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
