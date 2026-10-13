using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.MenuFramework;

public static class HudAnchor
{
    private const float MinimapHeight = 1f / 5.674f;

    private const float BigmapHeight = 1f / 2.35f;

    public static float Inset => (1f - API.GetSafeZoneSize()) / 2f;

    public static (float Left, float Bottom, float Width) AboveMinimap()
    {
        var inset = Inset;

        var map = API.IsRadarHidden()
            ? 0f
            : API.IsBigmapActive() ? BigmapHeight : MinimapHeight;

        return (inset, inset + map, MinimapWidth());
    }

    public static float MinimapWidth()
    {
        var aspect = API.GetScreenAspectRatio(false);

        return aspect > 0f ? 1f / (4f * aspect) : 1f / 4f;
    }

    public static float Fraction(float value) => MathF.Round(value, 5);
}
