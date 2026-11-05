using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PlayerState;

namespace vMenu.Enhanced.Freecam;

// OneSync streams the world around the player's ped, so the ped travels with the camera, hidden.
// Otherwise you'd see no players around your freecam position if you go far way from your ped.

internal static class FreecamActor
{
    private const float LoadRadius = 80f;

    private const int ReturnTimeoutMs = 4000;

    private const int SyncDelayMs = 1000;

    private const int ControlIntervalMs = 500;

    private const float FadeDistance = 300f;

    private const float BesideDoor = -1.6f;

    private const int Unlocked = 1;

    private const int Locked = 2;

    private static Vector3 _origin;

    private static float _heading;

    private static int _vehicle;

    private static int _vehicleNetworkId;

    private static int _seat;

    private static int _lockStatus;

    private static int _returnStarted;

    private static int _nextControlRequest;

    private static bool _away;

    private static bool _leaving;

    private static int _outSince;

    internal static void Leave(int ped)
    {
        _origin = Native.GetEntityCoords(ped, false);
        _heading = Native.GetEntityHeading(ped);
        _vehicle = Native.IsPedInAnyVehicle(ped, false) ? Native.GetVehiclePedIsIn(ped, false) : 0;
        _vehicleNetworkId = NetworkIdOf(_vehicle);
        _seat = _vehicle != 0 ? SeatOf(_vehicle, ped) : 0;
        _away = true;
        _leaving = _vehicle != 0;

        if (!_leaving)
        {
            Conceal(ped);

            return;
        }

        Native.SetEntityVisible(ped, false, false);
        Native.ClearPedTasksImmediately(ped);

        _lockStatus = Native.GetVehicleDoorLockStatus(_vehicle);
        _outSince = 0;

        Native.SetVehicleDoorsLocked(_vehicle, Locked);

        if (_vehicleNetworkId != 0)
        {
            API.EmitServer(PlayerStateEvents.FreecamVehicle, _vehicleNetworkId, _lockStatus);
        }
    }

    internal static void Follow(int ped, Vector3 position)
    {
        // If a player is in a car, while enabling freecam, we need to boot them out the car first.
        // For some reason, OneSync keeps treating the car that the player was in as where the player is.
        // So even if we teleport the player out of the car, OneSync is very buggy and when we move away with
        // the camera, no NPC's will spawn anymore around the camera, only around where the car is.
        // So we make sure the player gets kicked from the car, and is in a properly out-of-car synced state
        // before we do anything to the player with the freecam.
        if (_leaving)
        {
            Native.SetEntityVisible(ped, false, false);

            var now = Native.GetGameTimer();

            if (Native.IsPedInAnyVehicle(ped, true))
            {
                Native.ClearPedTasksImmediately(ped);

                return;
            }

            if (_outSince == 0)
            {
                _outSince = now;
            }

            if (now - _outSince < SyncDelayMs)
            {
                return;
            }

            _leaving = false;
        }

        Conceal(ped);
        KeepControl();

        Native.SetEntityCoordsNoOffset(ped, position.X, position.Y, position.Z, false, false, false);
    }

    // Going too far away from the vehicle, would cause glitchyness if you returned to it.
    // So we do a screen fade to make it less janky.
    internal static bool NeedsFade(Vector3 camera) =>
        Vector3.Distance(camera, _origin) > FadeDistance
        || (_vehicleNetworkId != 0 && Vehicle() == 0);

    internal static void BeginReturn(int ped)
    {
        _returnStarted = Native.GetGameTimer();
        _nextControlRequest = 0;

        Native.SetEntityCoordsNoOffset(ped, _origin.X, _origin.Y, _origin.Z, false, false, false);
        Native.RequestCollisionAtCoord(_origin.X, _origin.Y, _origin.Z);
        Native.NewLoadSceneStartSphere(_origin.X, _origin.Y, _origin.Z, LoadRadius, 0);
    }

    internal static bool TickReturn(int ped)
    {
        Conceal(ped);

        Native.SetEntityCoordsNoOffset(ped, _origin.X, _origin.Y, _origin.Z, false, false, false);
        Native.RequestCollisionAtCoord(_origin.X, _origin.Y, _origin.Z);

        KeepControl();

        if (Native.GetGameTimer() - _returnStarted > ReturnTimeoutMs)
        {
            return true;
        }

        return Native.IsNewLoadSceneLoaded() && (_vehicleNetworkId == 0 || Vehicle() != 0);
    }

    internal static void Finish(int ped)
    {
        if (!_away)
        {
            return;
        }

        _away = false;
        _leaving = false;

        Native.NewLoadSceneStop();

        if (_vehicleNetworkId != 0)
        {
            API.EmitServer(PlayerStateEvents.FreecamDone);
        }

        Native.SetEntityCoordsNoOffset(ped, _origin.X, _origin.Y, _origin.Z, false, false, false);
        Native.SetEntityHeading(ped, _heading);

        Native.FreezeEntityPosition(ped, false);
        Native.SetEntityInvincible(ped, false, false);
        Native.SetEntityCollision(ped, true, true);
        Native.SetEntityVisible(ped, true, false);
        Native.SetEveryoneIgnorePlayer(Native.PlayerId(), false);
        Native.SetPoliceIgnorePlayer(Native.PlayerId(), false);
        Native.ActivatePhysics(ped);

        var vehicle = Vehicle();

        if (vehicle != 0)
        {
            Native.SetVehicleDoorsLocked(vehicle, _lockStatus > 0 ? _lockStatus : Unlocked);

            if (!Native.IsEntityDead(vehicle, false) && Native.IsVehicleSeatFree(vehicle, _seat, false))
            {
                Native.SetPedIntoVehicle(ped, vehicle, _seat);
                Native.NetworkRequestControlOfEntity(vehicle);
            }
            else
            {
                var beside = Native.GetOffsetFromEntityInWorldCoords(vehicle, BesideDoor, 0f, 0f);

                Native.SetEntityCoords(ped, beside.X, beside.Y, beside.Z, false, false, false, false);
            }
        }

        _vehicle = 0;
        _vehicleNetworkId = 0;

        Native.SetGameplayCamRelativeHeading(0f);
        Native.SetGameplayCamRelativePitch(0f, 1f);
    }

    // Whoever picked the car up while this client was out of range has to hand it back once it streams
    // in again, so it can be unlocked on the spot.
    private static void KeepControl()
    {
        var now = Native.GetGameTimer();

        if (now < _nextControlRequest)
        {
            return;
        }

        _nextControlRequest = now + ControlIntervalMs;

        var vehicle = Vehicle();

        if (vehicle != 0 && !Native.NetworkHasControlOfEntity(vehicle))
        {
            Native.NetworkRequestControlOfEntity(vehicle);
        }
    }

    private static int Vehicle()
    {
        if (_vehicleNetworkId != 0)
        {
            _vehicle = Native.NetworkDoesNetworkIdExist(_vehicleNetworkId) ? Native.NetworkGetEntityFromNetworkId(_vehicleNetworkId) : 0;
        }

        return _vehicle != 0 && Native.DoesEntityExist(_vehicle) ? _vehicle : 0;
    }

    private static int NetworkIdOf(int entity) =>
        entity != 0 && Native.DoesEntityExist(entity) && Native.NetworkGetEntityIsNetworked(entity)
            ? Native.NetworkGetNetworkIdFromEntity(entity)
            : 0;

    private static void Conceal(int ped)
    {
        Native.FreezeEntityPosition(ped, true);
        Native.SetEntityInvincible(ped, true, false);
        Native.SetEntityCollision(ped, false, false);
        Native.SetEntityVisible(ped, false, false);
        Native.SetEveryoneIgnorePlayer(Native.PlayerId(), true);
        Native.SetPoliceIgnorePlayer(Native.PlayerId(), true);
    }

    private static int SeatOf(int vehicle, int ped)
    {
        var passengers = Native.GetVehicleMaxNumberOfPassengers(vehicle);

        for (var seat = -1; seat < passengers; seat++)
        {
            if (Native.GetPedInVehicleSeat(vehicle, seat, false) == ped)
            {
                return seat;
            }
        }

        return -1;
    }
}
