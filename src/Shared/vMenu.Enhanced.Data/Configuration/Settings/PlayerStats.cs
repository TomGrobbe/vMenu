namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class PlayerStats
{
    private const string CapNote =
        " A player can still pick a higher number and keep it saved, so their choice comes back in " +
        "full on a server that does not cap it. Only what vMenu writes into the game is limited.";

    public static readonly IntSetting MaxShooting = new("vMenu.Enhanced.PlayerOptions.MaxShooting")
    {
        Description = "The highest shooting ability the MP stats menu may hand out, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxStrength = new("vMenu.Enhanced.PlayerOptions.MaxStrength")
    {
        Description = "The highest strength the MP stats menu may hand out, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxStamina = new("vMenu.Enhanced.PlayerOptions.MaxStamina")
    {
        Description = "The highest stamina the MP stats menu may hand out, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxStealth = new("vMenu.Enhanced.PlayerOptions.MaxStealth")
    {
        Description = "The highest stealth ability the MP stats menu may hand out, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxFlying = new("vMenu.Enhanced.PlayerOptions.MaxFlying")
    {
        Description = "The highest flying ability the MP stats menu may hand out, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxDriving = new("vMenu.Enhanced.PlayerOptions.MaxDriving")
    {
        Description = "The highest driving ability the MP stats menu may hand out, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxLungCapacity = new("vMenu.Enhanced.PlayerOptions.MaxLungCapacity")
    {
        Description = "The highest lung capacity the MP stats menu may hand out, as a percentage." + CapNote,
        Default = 100,
    };
}
