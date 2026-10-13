using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Misc;

public static class BlipRotation
{
    private const int MotorcycleClass = 8;

    private const int BicycleClass = 13;

    private static readonly Dictionary<uint, bool> ByModel = [];

    public static bool Wanted(uint model, int sprite)
    {
        if (!VehicleBlipSprites.Rotates(sprite))
        {
            return false;
        }

        if (model == 0)
        {
            return true;
        }

        if (ByModel.TryGetValue(model, out var wanted))
        {
            return wanted;
        }

        var vehicleClass = API.GetVehicleClassFromName(model);

        wanted = vehicleClass != MotorcycleClass
            && vehicleClass != BicycleClass
            && !API.IsThisModelAQuadbike(model)
            && !NativeFixer.IsThisModelAnAmphibiousQuadbike(model);

        ByModel[model] = wanted;

        return wanted;
    }
}
