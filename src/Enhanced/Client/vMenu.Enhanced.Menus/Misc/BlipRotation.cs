using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;

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

        var vehicleClass = Native.GetVehicleClassFromName(model);

        wanted = vehicleClass != MotorcycleClass
            && vehicleClass != BicycleClass
            && !Native.IsThisModelAQuadbike(model)
            && !Native.IsThisModelAnAmphibiousQuadbike(model);

        ByModel[model] = wanted;

        return wanted;
    }
}
