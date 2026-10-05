namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Bully
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.Bully.Enabled")
    {
        Description =
            "Enables the Bully menu, a set of mean things staff can do to players, and the electrocuting door option for personal vehicles.",
        Default = false,
    };

    public static readonly BoolSetting DisableExplosions = new("vMenu.Enhanced.Bully.DisableExplosions")
    {
        Description =
            "Turns off the Explode and Up-n-Atomizer bully actions, for servers whose anti cheat flags explosions.",
        Default = false,
    };

    public static readonly BoolSetting DisableParticleEffects = new("vMenu.Enhanced.Bully.DisableParticleEffects")
    {
        Description =
            "Turns off the Fireworks and Beast Scare bully actions and the smoke of Transform Vehicle, for servers whose anti cheat flags particle effects.",
        Default = false,
    };
}
