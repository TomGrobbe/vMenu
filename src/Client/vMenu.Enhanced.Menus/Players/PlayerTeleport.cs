using System.Numerics;

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>
/// Moves the local player to a set of coordinates, waiting for the world there to exist first.
/// </summary>
internal static class PlayerTeleport
{
    private const int LoadSceneTimeoutMs = 3000;

    private const int CollisionTimeoutMs = 2000;

    /// <summary>How much world to pull in around the destination before moving anybody into it.</summary>
    private const float LoadSceneRadius = 50f;

    private const int FadeMs = 500;

    /// <summary>
    /// Moves to the exact height given. For coordinates that come off another player standing at
    /// them, where hunting for the ground would only find the roof of whatever they are inside.
    /// </summary>
    /// <param name="heading">Which way to face on arrival. Null keeps whichever way they already face.</param>
    public static Task ToCoordsAsync(Vector3 destination, float? heading = null) =>
        GoAsync(destination, findGround: false, heading);

    /// <summary>Moves to the spot on the map, working out how high the ground there is.</summary>
    /// <returns><see langword="false"/> if no ground was found, in which case nobody was moved.</returns>
    /// <inheritdoc cref="ToCoordsAsync(Vector3, float?)"/>
    public static Task<bool> ToGroundAsync(float x, float y, float? heading = null) =>
        GoAsync(new Vector3(x, y, 0f), findGround: true, heading);

    private static async Task<bool> GoAsync(Vector3 destination, bool findGround, float? heading)
    {
        if (API.Players.Local.Ped is not { } ped)
        {
            return false;
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

        // Without this the map outside the loaded scene never renders again, and the world turns into
        // flat brown mud.
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

        // A frozen vehicle does not settle onto its wheels, so it has to be let go for a moment.
        Native.FreezeEntityPosition(moving, false);
        Native.SetVehicleOnGroundProperly(moving, 5f);
        Native.FreezeEntityPosition(moving, true);

        return true;
    }
}
