namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Gameplay
{
    public static readonly IntSetting PvpMode = new("vMenu.Enhanced.Gameplay.PvpMode")
    {
        Description =
            "Whether players can hurt each other. 1 turns friendly fire on for everyone, 2 turns it " +
            "off for everyone, and 0 leaves it alone so another resource can manage it. On by " +
            "default, because the game's own default is that players cannot hurt each other at all, " +
            "which on a fresh server reads as vMenu having given everybody god mode.",
        Default = 1,
    };
}
