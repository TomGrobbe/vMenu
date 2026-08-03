using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Entities;
using CitizenFX.FiveM.Client.Extensions;

namespace vMenu.Enhanced.Menus.Vehicles;

/// <summary>
/// Works out which vehicle a player means: the one they are in, or the one they are facing.
/// </summary>
public static class VehicleTargeting
{
    private const int DriverSeat = -1;

    /// <summary>
    /// Roughly half a car's width. Legacy vMenu used 5, which over a 5m reach swept a blob centred on
    /// the player and took the car behind them.
    /// </summary>
    private const float SearchRadius = 1.25f;

    private const int VehiclesOnly = 2;

    /// <summary>Cargo culted from every script in the wild, this one included. Not a considered value.</summary>
    private const int ShapeTestOptions = 7;

    private const int ShapeTestNotReady = 1;

    private const int ShapeTestReady = 2;

    private const int MaxResultFrames = 10;

    /// <param name="reach">How far ahead to look when the player is on foot, in metres.</param>
    public static async Task<VehicleTarget> ResolveAsync(Ped ped, float reach)
    {
        if (ped.IsPedInAnyVehicle())
        {
            var vehicle = ped.Vehicle;

            if (vehicle is null || !vehicle.Exists)
            {
                return VehicleTarget.None;
            }

            var kind = Native.GetPedInVehicleSeat(vehicle.Handle, DriverSeat, false) == ped.Handle
                ? VehicleTargetKind.Driving
                : VehicleTargetKind.Passenger;

            return new VehicleTarget(vehicle.Handle, kind);
        }

        var entity = await FindInFrontAsync(ped, reach);

        return entity == 0 ? VehicleTarget.None : new VehicleTarget(entity, VehicleTargetKind.InFront);
    }

    private static async Task<int> FindInFrontAsync(Ped ped, float reach)
    {
        var start = ped.Position;
        var end = Native.GetOffsetFromEntityInWorldCoords(ped.Handle, 0f, reach, 0f);

        var test = Native.StartShapeTestCapsule(
            start.X, start.Y, start.Z,
            end.X, end.Y, end.Z,
            SearchRadius,
            VehiclesOnly,
            ped.Handle,
            ShapeTestOptions);

        for (var frame = 0; frame < MaxResultFrames; frame++)
        {
            // Through the fixer: both generated overloads push a Vector3, which the native API throws on.
            var status = BrokenNatives.NativeFixer.GetShapeTestResult(test, out var hit, out _, out _, out var entity);

            if (status == ShapeTestNotReady)
            {
                // A real frame: the test cannot resolve within the one that started it.
                await API.Delay(0);

                continue;
            }

            if (status != ShapeTestReady || hit == 0)
            {
                return 0;
            }

            return Native.DoesEntityExist(entity) && Native.IsEntityAVehicle(entity) ? entity : 0;
        }

        return 0;
    }
}
