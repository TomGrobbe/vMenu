using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.MenuFramework;

public static class HudAnchor
{
    private const float MinimapHeight = 1f / 5.674f;

    private const float BigmapHeight = 1f / 2.35f;

    public static float Inset => (1f - Native.GetSafeZoneSize()) / 2f;

    public static (float Left, float Bottom, float Width) AboveMinimap()
    {
        var inset = Inset;

        var map = Native.IsRadarHidden()
            ? 0f
            : Native.IsBigmapActive() ? BigmapHeight : MinimapHeight;

        return (inset, inset + map, MinimapWidth());
    }

    public static float MinimapWidth()
    {
        var aspect = Native.GetScreenAspectRatio(false);

        return aspect > 0f ? 1f / (4f * aspect) : 1f / 4f;
    }

    public static float Fraction(float value) => MathF.Round(value, 5);
}
