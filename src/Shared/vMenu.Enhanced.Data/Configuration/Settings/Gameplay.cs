namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Gameplay
{
    public static readonly IntSetting PvpMode = new("vMenu.Enhanced.Gameplay.PvpMode")
    {
        Description =
            "0 = vMenu does not touch PVP, 1 = PVP Enabled, 2 = PVP Disabled.",
        Default = 1,
    };

    public static readonly BoolSetting NoClipSmoke = new("vMenu.Enhanced.Gameplay.NoClipSmoke")
    {
        Description =
            "Shows a puff of smoke where a player goes into or comes out of noclip, for everyone nearby.",
        Default = true,
    };
}
