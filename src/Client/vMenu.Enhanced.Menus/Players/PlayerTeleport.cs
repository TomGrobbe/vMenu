using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Vehicles;

namespace vMenu.Enhanced.Menus.Players;

internal static class PlayerTeleport
{
    private const int LoadSceneTimeoutMs = 3000;

    private const int CollisionTimeoutMs = 2000;

    private const float LoadSceneRadius = 50f;

    private const int FadeMs = 500;

    private const int DriverSeat = -1;

    public static Task ToCoordsAsync(Vector3 destination, float? heading = null) =>
        GoAsync(destination, findGround: false, heading);

    public static Task<bool> ToGroundAsync(float x, float y, float? heading = null) =>
        GoAsync(new Vector3(x, y, 0f), findGround: true, heading);

    public static async Task<bool> IntoVehicleAsync(int networkId, Vector3 destination)
    {
        await ToCoordsAsync(destination);

        if (API.Players.Local.Ped is not { } ped)
        {
            return false;
        }

        var vehicle = await NetworkEntity.ResolveAsync(networkId);

        if (vehicle == 0)
        {
            return false;
        }

        if (!Native.AreAnyVehicleSeatsFree(vehicle))
        {
            return false;
        }

        Native.SetPedIntoVehicle(ped.Handle, vehicle, -2);

        return true;
    }


    private static async Task<bool> GoAsync(Vector3 destination, bool findGround, float? heading)
    {
        if (API.Players.Local.Ped is not { } ped)
        {
            return false;
        }

        var pedHandle = ped.Handle;

        var vehicle = Native.GetVehiclePedIsIn(pedHandle, false);
        var driving = vehicle != 0
            && Native.DoesEntityExist(vehicle)
            && Native.GetPedInVehicleSeat(vehicle, -1, false) == pedHandle;

        var moving = driving ? vehicle : pedHandle;

        if (!driving)
        {
            Native.ClearPedTasksImmediately(pedHandle);
        }

        Native.FreezeEntityPosition(moving, true);
        Native.NetworkFadeOutEntity(moving, true, false);

        Native.DoScreenFadeOut(FadeMs);

        while (!Native.IsScreenFadedOut())
        {
            await API.Delay(0);
        }

        try
        {
            return await PlaceAsync(pedHandle, moving, driving, destination, findGround, heading);
        }
        finally
        {
            Native.FreezeEntityPosition(moving, false);
            Native.NetworkFadeInEntity(moving, true, false);

            Native.DoScreenFadeIn(FadeMs);
        }
    }

    private static async Task<bool> PlaceAsync(int pedHandle, int moving, bool driving, Vector3 destination, bool findGround, float? heading)
    {
        Native.RequestCollisionAtCoord(destination.X, destination.Y, destination.Z);
        Native.SetFocusPosAndVel(destination.X, destination.Y, destination.Z, 0f, 0f, 0f);

        Native.NewLoadSceneStart(
            destination.X, destination.Y, destination.Z,
            destination.X, destination.Y, destination.Z,
            LoadSceneRadius,
            0);

        var started = Native.GetGameTimer();

        while (!Native.IsNewLoadSceneLoaded() && Native.GetGameTimer() - started < LoadSceneTimeoutMs)
        {
            await API.Delay(0);
        }

        Native.ClearFocus();

        Native.NewLoadSceneStop();

        if (findGround)
        {
            if (await GroundHeight.FindAsync(moving, destination.X, destination.Y) is not { } ground)
            {
                return false;
            }

            destination.Z = ground;
        }

        Native.SetEntityCoords(moving, destination.X, destination.Y, destination.Z, false, false, false, true);

        if (heading is { } facing)
        {
            Native.SetEntityHeading(moving, facing);
        }

        started = Native.GetGameTimer();

        while (!Native.HasCollisionLoadedAroundEntity(pedHandle) && Native.GetGameTimer() - started < CollisionTimeoutMs)
        {
            await API.Delay(0);
        }

        if (!driving)
        {
            return true;
        }

        Native.FreezeEntityPosition(moving, false);
        Native.SetVehicleOnGroundProperly(moving, 5f);
        Native.FreezeEntityPosition(moving, true);

        return true;
    }
}
