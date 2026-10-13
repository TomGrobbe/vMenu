using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleTargeting
{
    private const int DriverSeat = -1;

    // Roughly half a car's width. Legacy vMenu used 5, which over a 5m reach swept a blob centred on the
    // player and took the car behind them.
    private const float SearchRadius = 1.25f;

    private const int VehiclesOnly = 2;

    // Cargo culted from every script in the wild, this one included. Not a considered value.
    private const int ShapeTestOptions = 7;

    private const int ShapeTestNotReady = 1;

    private const int ShapeTestReady = 2;

    private const int MaxResultFrames = 10;

    // reach is how far ahead to look when the player is on foot, in metres.
    public static async Task<VehicleTarget> ResolveAsync(Ped ped, float reach)
    {
        var current = Current(ped);

        if (current.Found)
        {
            return current;
        }

        var entity = await FindInFrontAsync(ped, reach);

        return entity == 0 ? VehicleTarget.None : new VehicleTarget(entity, VehicleTargetKind.InFront);
    }

    // Only the vehicle the player is sitting in. Nothing is searched for, so this needs no await, which
    // is what options that act on your own vehicle want.
    public static VehicleTarget Current(Ped ped)
    {
        if (!ped.IsInVehicle())
        {
            return VehicleTarget.None;
        }

        var vehicle = ped.CurrentVehicle;

        if (vehicle is null || !vehicle.Exists())
        {
            return VehicleTarget.None;
        }

        var kind = NativeFixer.GetPedInVehicleSeat(vehicle.Handle, DriverSeat, false) == ped.Handle
            ? VehicleTargetKind.Driving
            : VehicleTargetKind.Passenger;

        return new VehicleTarget(vehicle.Handle, kind);
    }

    private static async Task<int> FindInFrontAsync(Ped ped, float reach)
    {
        var start = ped.Position;
        var end = API.GetOffsetFromEntityInWorldCoords(ped.Handle, 0f, reach, 0f);

        var test = API.StartShapeTestCapsule(
            start.X, start.Y, start.Z,
            end.X, end.Y, end.Z,
            SearchRadius,
            VehiclesOnly,
            ped.Handle,
            ShapeTestOptions);

        for (var frame = 0; frame < MaxResultFrames; frame++)
        {
            var status = NativeFixer.GetShapeTestResult(test, out var hit, out _, out _, out var entity);

            if (status == ShapeTestNotReady)
            {
                // A real frame: the test cannot resolve within the one that started it.
                await BaseScript.Delay(0);

                continue;
            }

            if (status != ShapeTestReady || hit == 0)
            {
                return 0;
            }

            return API.DoesEntityExist(entity) && API.IsEntityAVehicle(entity) ? entity : 0;
        }

        return 0;
    }
}
