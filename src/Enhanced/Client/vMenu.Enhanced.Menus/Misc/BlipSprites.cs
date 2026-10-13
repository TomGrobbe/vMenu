using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;

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
        if (Native.IsThisModelAPlane(model))
        {
            return VehicleBlipKind.Plane;
        }

        if (Native.IsThisModelAHeli(model))
        {
            return VehicleBlipKind.Heli;
        }

        return Native.IsThisModelABoat(model) ? VehicleBlipKind.Boat : VehicleBlipKind.Land;
    }
}
