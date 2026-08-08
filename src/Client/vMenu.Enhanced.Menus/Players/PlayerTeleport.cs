using System.Numerics;

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>
/// Moves the local player to a set of coordinates, waiting for the world there to exist first.
/// </summary>
/// <remarks>
/// The height is used exactly as given. These coordinates come off another player who is standing at
/// them, so there is nothing to look up: hunting for the ground would only find the roof of whatever
/// they happen to be standing inside.
/// </remarks>
internal static class PlayerTeleport
{
    private const int LoadSceneTimeoutMs = 3000;

    private const int CollisionTimeoutMs = 2000;

    /// <summary>How much world to pull in around the destination before moving anybody into it.</summary>
    private const float LoadSceneRadius = 50f;

    private const int FadeMs = 500;

    public static async Task ToCoordsAsync(Vector3 destination)
    {
        if (API.Players.Local.Ped is not { } ped)
        {
            return;
        }

        var pedHandle = ped.Handle;

        // Whoever is driving takes the car with them. A passenger is left to walk.
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
            await PlaceAsync(pedHandle, moving, driving, destination);
        }
        finally
        {
            Native.FreezeEntityPosition(moving, false);
            Native.NetworkFadeInEntity(moving, true, false);

            Native.DoScreenFadeIn(FadeMs);
        }
    }

    private static async Task PlaceAsync(int pedHandle, int moving, bool driving, Vector3 destination)
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

        // Without this the map outside the loaded scene never renders again, and the world turns into
        // flat brown mud.
        Native.NewLoadSceneStop();

        Native.SetEntityCoords(moving, destination.X, destination.Y, destination.Z, false, false, false, true);

        started = Native.GetGameTimer();

        while (!Native.HasCollisionLoadedAroundEntity(pedHandle) && Native.GetGameTimer() - started < CollisionTimeoutMs)
        {
            await API.Delay(0);
        }

        if (!driving)
        {
            return;
        }

        // A frozen vehicle does not settle onto its wheels, so it has to be let go for a moment.
        Native.FreezeEntityPosition(moving, false);
        Native.SetVehicleOnGroundProperly(moving, 5f);
        Native.FreezeEntityPosition(moving, true);
    }
}
