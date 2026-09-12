namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class MenuAppearance
{
    public static readonly StringSetting Skin = new("vMenu.Enhanced.MenuAppearance.Skin")
    {
        Description =
            "Choose a default theme: 'default', 'dark', 'cartoon' or 'gta'. You can also choose any plugin-added theme here.",
        Default = "default",
    };

    public static readonly StringSetting TitleAlignment = new("vMenu.Enhanced.MenuAppearance.TitleAlignment")
    {
        Description =
            "Where the title sits on the banner: 'left', 'center' or 'right'.",
        Default = "left",
    };

    public static readonly BoolSetting HeaderGlare = new("vMenu.Enhanced.MenuAppearance.HeaderGlare")
    {
        Description = "Enables or disables the Globe in the menu header and the glare effect, like GTA Online's interaction menu.",
        Default = true,
    };
}
