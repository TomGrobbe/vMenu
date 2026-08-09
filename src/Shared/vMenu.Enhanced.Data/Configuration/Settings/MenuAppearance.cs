namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class MenuAppearance
{
    public static readonly StringSetting TitleAlignment = new("vMenu.Enhanced.MenuAppearance.TitleAlignment")
    {
        Description =
            "Where the title sits on the banner at the top of every menu. Use 'left', 'center' or " +
            "'right'. Anything else is ignored and the title is put on the left.",
        Default = "left",
    };

    public static readonly StringSetting TitleFont = new("vMenu.Enhanced.MenuAppearance.TitleFont")
    {
        Description =
            "The font the title on the banner is drawn in. Use one of 'chaletlondon', 'housescript', " +
            "'monospace', 'chaletcomprimecologne' or 'pricedown', which is the Grand Theft Auto logo " +
            "font. A plain number also works, for a font some other resource added to the game itself. " +
            "Anything else is ignored and the title is drawn in Chalet Comprime Cologne.",
        Default = "chaletcomprimecologne",
    };

    public static readonly BoolSetting HeaderGlare = new("vMenu.Enhanced.MenuAppearance.HeaderGlare")
    {
        Description =
            "Draws the soft moving glow over the banner at the top of every menu, the same one GTA " +
            "Online has behind its own pause menu title. It drifts as the player turns the camera. " +
            "Turn it off for a flat banner that does not move.",
        Default = true,
    };
}
