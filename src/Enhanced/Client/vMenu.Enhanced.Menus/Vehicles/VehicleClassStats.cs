using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework;

namespace vMenu.Enhanced.Menus.Vehicles;

internal static class VehicleClassStats
{
    internal static VehicleStats Normalise(uint modelHash, int vehicleClass)
    {
        if (!VehicleClassCeilings.TryGet(vehicleClass, out var ceiling))
        {
            return VehicleStats.None;
        }

        return new VehicleStats(
            Map(Native.GetVehicleModelEstimatedMaxSpeed(modelHash), 0f, ceiling.TopSpeed, 0f, 1f),
            Map(Native.GetVehicleModelAcceleration(modelHash), 0f, ceiling.Acceleration, 0f, 1f),
            Map(Native.GetVehicleModelMaxBraking(modelHash), 0f, ceiling.Braking, 0f, 1f),
            Map(Native.GetVehicleModelMaxTraction(modelHash), 0f, ceiling.Traction, 0f, 1f));
    }

    internal static float Map(float value, float minIn, float maxIn, float minOut, float maxOut)
    {
        var range = maxIn - minIn;

        if (range == 0f)
        {
            return minOut;
        }

        return ((value - minIn) * (maxOut - minOut) / range) + minOut;
    }
}
