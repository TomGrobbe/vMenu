using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Misc;

public static class BlipSprites
{
    public static int ForVehicleModel(uint model)
    {
        if (model == 0)
        {
            return VehicleBlipSprites.StandardSprite;
        }

        if (VehicleBlipSprites.ForModel(model) is { } special)
        {
            return special;
        }

        return VehicleBlipSprites.SpriteForKind(KindOf(model));
    }

    private static VehicleBlipKind KindOf(uint model)
    {
        if (API.IsThisModelAPlane(model))
        {
            return VehicleBlipKind.Plane;
        }

        if (API.IsThisModelAHeli(model))
        {
            return VehicleBlipKind.Heli;
        }

        return API.IsThisModelABoat(model) ? VehicleBlipKind.Boat : VehicleBlipKind.Land;
    }
}
