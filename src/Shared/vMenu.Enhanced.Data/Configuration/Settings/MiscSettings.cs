namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class MiscSettings
{
    public const int MinClearAreaRadius = 1;

    public const int MaxClearAreaRadius = 1000;

    public static readonly IntSetting ClearAreaRadius =
        new("vMenu.Enhanced.MiscSettings.ClearAreaRadius")
        {
            Description =
                "How far around a player, in metres, the Clear Area button reaches.",
            Default = 100,
        };

    public static int ClampClearAreaRadius(int radius) =>
        radius < MinClearAreaRadius
            ? MinClearAreaRadius
            : radius > MaxClearAreaRadius
                ? MaxClearAreaRadius
                : radius;
}
