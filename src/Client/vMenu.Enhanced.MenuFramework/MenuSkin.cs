using MenuAPI;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Logging;

using AppearanceSettings = vMenu.Enhanced.Data.Configuration.Settings.MenuAppearance;

namespace vMenu.Enhanced.MenuFramework;

public sealed class MenuSkinChoice
{
    internal MenuSkinChoice(string id, string name, string? resource)
    {
        Id = id;
        Name = name;
        Resource = resource;
    }

    public string Id { get; }

    public string Name { get; }

    public string? Resource { get; }
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

    private const int MaxIdLength = 40;

    private const int MaxPathLength = 400;

    private static readonly List<CustomSkin> Customs = [];

    private static string? _reported;

    private static string? _override;

    // Read by MenuHost, so a menu built after the skin was applied opens with the right banner.
    public static string? Banner { get; private set; }

    public static string CurrentId => Current().Id;

    public static string ConfiguredId => Configured().Id;

    public static bool IsOverridden => _override is not null;

    public static event Action? Changed;

    public static List<MenuSkinChoice> Choices()
    {
        var choices = new List<MenuSkinChoice>(Skins.Length + Customs.Count);

        foreach (var (aliases, name, _, _, _) in Skins)
        {
            choices.Add(new MenuSkinChoice(aliases[0], name, null));
        }

        foreach (var custom in Customs)
        {
            choices.Add(new MenuSkinChoice(custom.Id, custom.Name, custom.Resource));
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

    public static bool TryRegisterCustom(
        string resource,
        string id,
        string name,
        string css,
        string? banner,
        out string error,
        out string? warning)
    {
        id = id.Trim();
        warning = null;

        if (!IsUsableId(id))
        {
            error =
                $"'{id}' is not a usable theme id. Use letters, digits, dashes, underscores and dots, "
                + $"up to {MaxIdLength} characters.";

            return false;
        }

        if (IndexOfSkin(id) is not null)
        {
            error = $"'{id}' is one of vMenu's own theme names, pick another id.";
            return false;
        }

        if (FindCustom(id) is { } taken && !taken.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase))
        {
            error = $"'{id}' is already registered by resource '{taken.Resource}'.";
            return false;
        }

        if (ResourceUrl(resource, css.Trim()) is not { } url)
        {
            error =
                $"'{css}' is not a stylesheet vMenu can load for theme '{id}'. Use a path inside your own "
                + "resource, like 'themes/mytheme.css', or a full https://cfx-nui-<resource>/ url.";

            return false;
        }

        var display = name.Trim();

        NuiTuning.RegisterTheme(id, url);

        Replace(new CustomSkin(id, display.Length > 0 ? display : id, resource, BannerFor(resource, banner, out warning)));

        MenuAudit.ReportTheme(resource, id, display.Length > 0 ? display : id);

        error = string.Empty;

        return true;
    }

    public static int RemoveCustomFrom(string resource)
    {
        var removed = 0;

        for (var index = Customs.Count - 1; index >= 0; index--)
        {
            var custom = Customs[index];

            if (!custom.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Customs.RemoveAt(index);

            NuiTuning.UnregisterTheme(custom.Id);

            removed++;
        }

        if (removed > 0)
        {
            Apply();
        }

        return removed;
    }

    public static void Refresh() => Apply();

    public static bool TryApplyOverride(string id)
    {
        if (Find(id) is not { } skin)
        {
            return false;
        }

        _override = skin.Id;

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
        var skin = Current();

        NuiTuning.SetTheme(skin.Theme);

        Banner = skin.Banner;

        MenuRegistry.ApplyBanner(skin.Banner);

        try
        {
            Changed?.Invoke();
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] A skin Changed handler threw: {exception}");
        }
    }

    private static Resolved Current()
    {
        if (_override is { } wanted)
        {
            if (Find(wanted) is { } skin)
            {
                return skin;
            }

            _override = null;
        }

        return Configured();
    }

    private static Resolved Configured()
    {
        var raw = ClientConfig.Value(AppearanceSettings.Skin);

        if (Find(raw.Trim()) is { } skin)
        {
            return skin;
        }

        Report(
            raw,
            $"{AppearanceSettings.Skin.Name} is set to '{raw}', which is not a skin vMenu knows about. "
            + $"Using '{Skins[0].Aliases[0]}'. A theme another resource provides only counts once that "
            + "resource has registered it.");

        return SkinAt(0);
    }

    private static Resolved? Find(string value)
    {
        if (IndexOfSkin(value) is { } index)
        {
            return SkinAt(index);
        }

        return FindCustom(value) is { } custom ? new Resolved(custom.Id, custom.Id, custom.Banner) : null;
    }

    private static int? IndexOfSkin(string value)
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

    private static CustomSkin? FindCustom(string value)
    {
        foreach (var custom in Customs)
        {
            if (string.Equals(custom.Id, value, StringComparison.OrdinalIgnoreCase))
            {
                return custom;
            }
        }

        return null;
    }

    private static Resolved SkinAt(int index)
    {
        var (aliases, _, theme, _, banner) = Skins[index];

        return new Resolved(aliases[0], theme, banner);
    }

    private static void Replace(CustomSkin skin)
    {
        for (var index = 0; index < Customs.Count; index++)
        {
            if (string.Equals(Customs[index].Id, skin.Id, StringComparison.OrdinalIgnoreCase))
            {
                Customs[index] = skin;
                return;
            }
        }

        Customs.Add(skin);
    }

    private static string? ResourceUrl(string resource, string path)
    {
        if (path.Length is 0 or > MaxPathLength || path.Contains(".."))
        {
            return null;
        }

        if (path.StartsWith("https://cfx-nui-", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("nui://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        if (path.Contains("://") || path.StartsWith('/') || path.StartsWith('\\'))
        {
            return null;
        }

        return $"https://cfx-nui-{resource}/{path}";
    }

    private static string? BannerFor(string resource, string? banner, out string? warning)
    {
        warning = null;

        var value = banner?.Trim() ?? string.Empty;

        if (value.Length == 0)
        {
            return Skins[0].Banner;
        }

        if (IndexOfSkin(value) is { } index)
        {
            return Skins[index].Banner;
        }

        if (IsImagePath(value) && ResourceUrl(resource, value) is { } url)
        {
            return url;
        }

        warning =
            $"'{value}' is not one of vMenu's banners (default, dark, cartoon, none) and not an image "
            + $"inside your resource, so '{Skins[0].Aliases[0]}' is used instead. An image is a path "
            + "ending in .png, .jpg or .webp.";

        return Skins[0].Banner;
    }

    private static bool IsImagePath(string value) =>
        value.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
        || value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
        || value.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
        || value.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);

    private static bool IsUsableId(string id)
    {
        if (id.Length is 0 or > MaxIdLength)
        {
            return false;
        }

        foreach (var character in id)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.'))
            {
                return false;
            }
        }

        return true;
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

    private readonly struct Resolved
    {
        internal Resolved(string id, string? theme, string? banner)
        {
            Id = id;
            Theme = theme;
            Banner = banner;
        }

        internal string Id { get; }

        internal string? Theme { get; }

        internal string? Banner { get; }
    }

    private sealed class CustomSkin
    {
        internal CustomSkin(string id, string name, string resource, string? banner)
        {
            Id = id;
            Name = name;
            Resource = resource;
            Banner = banner;
        }

        internal string Id { get; }

        internal string Name { get; }

        internal string Resource { get; }

        internal string? Banner { get; }
    }
}
