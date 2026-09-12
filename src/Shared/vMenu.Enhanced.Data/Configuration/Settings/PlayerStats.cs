namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class PlayerStats
{
    private const string CapNote =
        " A player can still pick a higher number but vMenu will only apply it up to the limits configured here.";

    public static readonly IntSetting MaxShooting = new("vMenu.Enhanced.PlayerOptions.MaxShooting")
    {
        Description = "Max shooting ability stat value, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxStrength = new("vMenu.Enhanced.PlayerOptions.MaxStrength")
    {
        Description = "Max strength stat value, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxStamina = new("vMenu.Enhanced.PlayerOptions.MaxStamina")
    {
        Description = "Max stamina stat value, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxStealth = new("vMenu.Enhanced.PlayerOptions.MaxStealth")
    {
        Description = "Max stealth ability stat value, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxFlying = new("vMenu.Enhanced.PlayerOptions.MaxFlying")
    {
        Description = "Max flying ability stat value, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxDriving = new("vMenu.Enhanced.PlayerOptions.MaxDriving")
    {
        Description = "Max driving ability stat value, as a percentage." + CapNote,
        Default = 100,
    };

    public static readonly IntSetting MaxLungCapacity = new("vMenu.Enhanced.PlayerOptions.MaxLungCapacity")
    {
        Description = "Max lung capacity stat value, as a percentage." + CapNote,
        Default = 100,
    };
}
