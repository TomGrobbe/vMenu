
using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Vehicles;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Players;

internal static class PlayerTeleport
{
    private const int LoadSceneTimeoutMs = 3000;

    private const int CollisionTimeoutMs = 2000;

    private const float LoadSceneRadius = 50f;

    private const int FadeMs = 500;


    private const int AnyFreeSeat = -2;

    public static Task<bool> ToCoordsAsync(Vector3 destination, float? heading = null) =>
        GoAsync(destination, findGround: false, heading);

    public static Task<bool> ToGroundAsync(float x, float y, float? heading = null) =>
        GoAsync(new Vector3(x, y, 0f), findGround: true, heading);

    public static async Task<bool> IntoVehicleAsync(int networkId, Vector3 destination, int seat = AnyFreeSeat)
    {
        await ToCoordsAsync(destination);

        if (Game.PlayerPed is not { } ped)
        {
            return false;
        }

        var vehicle = await NetworkEntity.ResolveAsync(networkId);

        if (vehicle == 0)
        {
            return false;
        }

        var free = seat == AnyFreeSeat
            ? API.AreAnyVehicleSeatsFree(vehicle)
            : NativeFixer.IsVehicleSeatFree(vehicle, seat, false);

        if (!free)
        {
            return false;
        }

        API.SetPedIntoVehicle(ped.Handle, vehicle, seat);

        return true;
    }


    private static async Task<bool> GoAsync(Vector3 destination, bool findGround, float? heading)
    {
        if (SceneLock.IsActive || Game.PlayerPed is not { } ped)
        {
            return false;
        }

        var pedHandle = ped.Handle;

        var vehicle = API.GetVehiclePedIsIn(pedHandle, false);
        var driving = vehicle != 0
            && API.DoesEntityExist(vehicle)
            && NativeFixer.GetPedInVehicleSeat(vehicle, -1, false) == pedHandle;

        var moving = driving ? vehicle : pedHandle;

        if (!driving)
        {
            API.ClearPedTasksImmediately(pedHandle);
        }

        API.FreezeEntityPosition(moving, true);
        API.NetworkFadeOutEntity(moving, true, false);

        API.DoScreenFadeOut(FadeMs);

        while (!API.IsScreenFadedOut())
        {
            await BaseScript.Delay(0);
        }

        try
        {
            return await PlaceAsync(pedHandle, moving, driving, destination, findGround, heading);
        }
        finally
        {
            API.FreezeEntityPosition(moving, false);
            NativeFixer.NetworkFadeInEntity(moving, true, false);

            API.DoScreenFadeIn(FadeMs);
        }
    }

    private static async Task<bool> PlaceAsync(int pedHandle, int moving, bool driving, Vector3 destination, bool findGround, float? heading)
    {
        NativeFixer.RequestCollisionAtCoord(destination.X, destination.Y, destination.Z);
        API.SetFocusPosAndVel(destination.X, destination.Y, destination.Z, 0f, 0f, 0f);

        API.NewLoadSceneStart(
            destination.X, destination.Y, destination.Z,
            destination.X, destination.Y, destination.Z,
            LoadSceneRadius,
            0);

        var started = API.GetGameTimer();

        while (!API.IsNewLoadSceneLoaded() && API.GetGameTimer() - started < LoadSceneTimeoutMs)
        {
            await BaseScript.Delay(0);
        }

        API.ClearFocus();

        API.NewLoadSceneStop();

        if (findGround)
        {
            if (await GroundHeight.FindAsync(moving, destination.X, destination.Y) is not { } ground)
            {
                return false;
            }

            destination.Z = ground;
        }

        API.SetEntityCoords(moving, destination.X, destination.Y, destination.Z, false, false, false, true);

        if (heading is { } facing)
        {
            API.SetEntityHeading(moving, facing);
        }

        started = API.GetGameTimer();

        while (!API.HasCollisionLoadedAroundEntity(pedHandle) && API.GetGameTimer() - started < CollisionTimeoutMs)
        {
            await BaseScript.Delay(0);
        }

        if (!driving)
        {
            return true;
        }

        API.FreezeEntityPosition(moving, false);
        NativeFixer.SetVehicleOnGroundProperly(moving, 5f);
        API.FreezeEntityPosition(moving, true);

        return true;
    }
}
