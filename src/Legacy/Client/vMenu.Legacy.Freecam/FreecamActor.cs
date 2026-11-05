using System.Numerics;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.PlayerState;

using vMenu.Enhanced.BrokenNatives;

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
        _origin = API.GetEntityCoords(ped, false);
        _heading = API.GetEntityHeading(ped);
        _vehicle = API.IsPedInAnyVehicle(ped, false) ? API.GetVehiclePedIsIn(ped, false) : 0;
        _vehicleNetworkId = NetworkIdOf(_vehicle);
        _seat = _vehicle != 0 ? SeatOf(_vehicle, ped) : 0;
        _away = true;
        _leaving = _vehicle != 0;

        if (!_leaving)
        {
            Conceal(ped);

            return;
        }

        API.SetEntityVisible(ped, false, false);
        API.ClearPedTasksImmediately(ped);

        _lockStatus = API.GetVehicleDoorLockStatus(_vehicle);
        _outSince = 0;

        API.SetVehicleDoorsLocked(_vehicle, Locked);

        if (_vehicleNetworkId != 0)
        {
            BaseScript.TriggerServerEvent(PlayerStateEvents.FreecamVehicle, _vehicleNetworkId, _lockStatus);
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
            API.SetEntityVisible(ped, false, false);

            var now = API.GetGameTimer();

            if (API.IsPedInAnyVehicle(ped, true))
            {
                API.ClearPedTasksImmediately(ped);

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

        API.SetEntityCoordsNoOffset(ped, position.X, position.Y, position.Z, false, false, false);
    }

    // Going too far away from the vehicle, would cause glitchyness if you returned to it.
    // So we do a screen fade to make it less janky.
    internal static bool NeedsFade(Vector3 camera) =>
        Vector3.Distance(camera, _origin) > FadeDistance
        || (_vehicleNetworkId != 0 && Vehicle() == 0);

    internal static void BeginReturn(int ped)
    {
        _returnStarted = API.GetGameTimer();
        _nextControlRequest = 0;

        API.SetEntityCoordsNoOffset(ped, _origin.X, _origin.Y, _origin.Z, false, false, false);
        NativeFixer.RequestCollisionAtCoord(_origin.X, _origin.Y, _origin.Z);
        API.NewLoadSceneStartSphere(_origin.X, _origin.Y, _origin.Z, LoadRadius, 0);
    }

    internal static bool TickReturn(int ped)
    {
        Conceal(ped);

        API.SetEntityCoordsNoOffset(ped, _origin.X, _origin.Y, _origin.Z, false, false, false);
        NativeFixer.RequestCollisionAtCoord(_origin.X, _origin.Y, _origin.Z);

        KeepControl();

        if (API.GetGameTimer() - _returnStarted > ReturnTimeoutMs)
        {
            return true;
        }

        return API.IsNewLoadSceneLoaded() && (_vehicleNetworkId == 0 || Vehicle() != 0);
    }

    internal static void Finish(int ped)
    {
        if (!_away)
        {
            return;
        }

        _away = false;
        _leaving = false;

        API.NewLoadSceneStop();

        if (_vehicleNetworkId != 0)
        {
            BaseScript.TriggerServerEvent(PlayerStateEvents.FreecamDone);
        }

        API.SetEntityCoordsNoOffset(ped, _origin.X, _origin.Y, _origin.Z, false, false, false);
        API.SetEntityHeading(ped, _heading);

        API.FreezeEntityPosition(ped, false);
        NativeFixer.SetEntityInvincible(ped, false, false);
        API.SetEntityCollision(ped, true, true);
        API.SetEntityVisible(ped, true, false);
        API.SetEveryoneIgnorePlayer(API.PlayerId(), false);
        API.SetPoliceIgnorePlayer(API.PlayerId(), false);
        API.ActivatePhysics(ped);

        var vehicle = Vehicle();

        if (vehicle != 0)
        {
            API.SetVehicleDoorsLocked(vehicle, _lockStatus > 0 ? _lockStatus : Unlocked);

            if (!NativeFixer.IsEntityDead(vehicle, false) && NativeFixer.IsVehicleSeatFree(vehicle, _seat, false))
            {
                API.SetPedIntoVehicle(ped, vehicle, _seat);
                API.NetworkRequestControlOfEntity(vehicle);
            }
            else
            {
                var beside = API.GetOffsetFromEntityInWorldCoords(vehicle, BesideDoor, 0f, 0f);

                API.SetEntityCoords(ped, beside.X, beside.Y, beside.Z, false, false, false, false);
            }
        }

        _vehicle = 0;
        _vehicleNetworkId = 0;

        API.SetGameplayCamRelativeHeading(0f);
        API.SetGameplayCamRelativePitch(0f, 1f);
    }

    // Whoever picked the car up while this client was out of range has to hand it back once it streams
    // in again, so it can be unlocked on the spot.
    private static void KeepControl()
    {
        var now = API.GetGameTimer();

        if (now < _nextControlRequest)
        {
            return;
        }

        _nextControlRequest = now + ControlIntervalMs;

        var vehicle = Vehicle();

        if (vehicle != 0 && !API.NetworkHasControlOfEntity(vehicle))
        {
            API.NetworkRequestControlOfEntity(vehicle);
        }
    }

    private static int Vehicle()
    {
        if (_vehicleNetworkId != 0)
        {
            _vehicle = API.NetworkDoesNetworkIdExist(_vehicleNetworkId) ? API.NetworkGetEntityFromNetworkId(_vehicleNetworkId) : 0;
        }

        return _vehicle != 0 && API.DoesEntityExist(_vehicle) ? _vehicle : 0;
    }

    private static int NetworkIdOf(int entity) =>
        entity != 0 && API.DoesEntityExist(entity) && API.NetworkGetEntityIsNetworked(entity)
            ? API.NetworkGetNetworkIdFromEntity(entity)
            : 0;

    private static void Conceal(int ped)
    {
        API.FreezeEntityPosition(ped, true);
        NativeFixer.SetEntityInvincible(ped, true, false);
        API.SetEntityCollision(ped, false, false);
        API.SetEntityVisible(ped, false, false);
        API.SetEveryoneIgnorePlayer(API.PlayerId(), true);
        API.SetPoliceIgnorePlayer(API.PlayerId(), true);
    }

    private static int SeatOf(int vehicle, int ped)
    {
        var passengers = API.GetVehicleMaxNumberOfPassengers(vehicle);

        for (var seat = -1; seat < passengers; seat++)
        {
            if (NativeFixer.GetPedInVehicleSeat(vehicle, seat, false) == ped)
            {
                return seat;
            }
        }

        return -1;
    }
}
