using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Ticks;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

// The bombs, flares, chaff and smoke come from https://github.com/DevTestingPizza/Flares-and-Bombs and additional research
// from the R* decompiled scripts (to make sure we have all the recent planes/helicopters, since the DevTestingPizza repo is outdated).
// Optimized to run a lot more efficiently than before.
public static class AircraftWeapons
{
    private const int ModReadMs = 1000;

    private const int Pilot = -1;

    private const int CoPilot = 0;

    private static TickHandle? _tick;

    private static AircraftSession? _session;

    private static bool _watching;

    private static bool _suspended;

    private static int _nextModRead;

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.BombsAndFlares);

    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "Vehicles.AircraftWeapons",
            Frame,
            TickRate.PerFrame,
            () => _session is not null,
            autoStart: false);

        ClientPermissions.PermissionsChanged += Refresh;
        AircraftWeaponConfig.Changed += Restart;
        ResourceShutdown.Stopping += OnStopping;

        Refresh();
    }

    private static void Refresh()
    {
        Watch(IsAllowed);

        var found = IsAllowed ? Find() : null;

        if (found is not null && _session is not null && found.Vehicle == _session.Vehicle && found.Seat == _session.Seat)
        {
            return;
        }

        End();

        _session = found;
        _nextModRead = 0;

        _tick?.Reevaluate();
    }

    private static void Restart()
    {
        End();
        Refresh();
    }

    private static void OnStopping()
    {
        if (_session is { } session)
        {
            AircraftWeaponOwner.LetGo(session.Vehicle);
        }

        End();
    }

    private static void End()
    {
        var session = _session;

        _session = null;
        _suspended = false;

        StandDown(session);

        _tick?.Reevaluate();
    }

    private static void StandDown(AircraftSession? session)
    {
        BombBay.Reset(session);
        BombCamera.Reset();
        Countermeasures.Reset();
        AircraftWeaponClaim.Release();
    }

    private static void Frame()
    {
        if (_session is not { } session)
        {
            return;
        }

        if (!Native.DoesEntityExist(session.Vehicle))
        {
            End();

            return;
        }

        if (!Native.IsVehicleDriveable(session.Vehicle, false) || Native.IsPedInjured(Native.PlayerPedId()))
        {
            if (!_suspended)
            {
                StandDown(session);

                _suspended = true;
            }

            return;
        }

        if (_suspended)
        {
            _suspended = false;
            _nextModRead = 0;

            if (Native.GetPedInVehicleSeat(session.Vehicle, session.Seat, false) != Native.PlayerPedId())
            {
                End();

                return;
            }
        }

        var now = Native.GetGameTimer();

        if (now >= _nextModRead)
        {
            session.ReadMods();

            BombBay.Adopt(session);

            if (AircraftWeaponClaim.Contested(session))
            {
                BombBay.Shut(session);
                AircraftWeaponClaim.Release();
            }

            _nextModRead = now + ModReadMs;
        }

        if (session.BombType < 0 && session.Countermeasure == Countermeasure.None && !BombBay.Busy && !Countermeasures.Busy)
        {
            BombCamera.Reset();

            return;
        }

        session.Sample();

        AircraftControls.Muted = MenuController.IsAnyMenuOpen();

        BombBay.Frame(session, now);
        Countermeasures.Frame(session, now);

        if (AircraftWeaponClaim.Holds(session) && !BombBay.Busy && !Countermeasures.Busy)
        {
            AircraftWeaponClaim.Release();
        }
    }

    private static AircraftSession? Find()
    {
        var ped = Native.PlayerPedId();

        if (!Native.IsPedInAnyVehicle(ped, false) || Native.IsPedInjured(ped))
        {
            return null;
        }

        var vehicle = Native.GetVehiclePedIsIn(ped, false);

        int seat;

        if (Native.GetPedInVehicleSeat(vehicle, Pilot, false) == ped)
        {
            seat = Pilot;
        }
        else if (Native.GetPedInVehicleSeat(vehicle, CoPilot, false) == ped)
        {
            seat = CoPilot;
        }
        else
        {
            return null;
        }

        var model = Native.GetEntityModel(vehicle);

        return Resolve(vehicle, model) is { } profile ? new AircraftSession(vehicle, model, seat, profile) : null;
    }

    private static AircraftWeaponProfile? Resolve(int vehicle, uint model)
    {
        var profile = AircraftWeaponProfiles.ForModel(model);

        if (profile is null)
        {
            if (!Native.IsThisModelAPlane(model) && !Native.IsThisModelAHeli(model))
            {
                return null;
            }

            Native.GetModelDimensions(model, out var min, out _);

            var bombBay = Native.GetEntityBoneIndexByName(vehicle, "door_hatch_l") != -1
                && Native.GetEntityBoneIndexByName(vehicle, "door_hatch_r") != -1;

            profile = AircraftWeaponProfiles.Generic(bombBay, min.Z, min.Y);
        }

        AircraftWeaponConfig.For(model)?.ApplyTo(profile);

        return profile;
    }

    private static void Watch(bool watching)
    {
        if (watching == _watching)
        {
            return;
        }

        _watching = watching;

        if (watching)
        {
            LocalVehicleTicks.VehicleChanged += OnChanged;

            return;
        }

        LocalVehicleTicks.VehicleChanged -= OnChanged;
    }

    private static void OnChanged(VehicleChanged _) => Refresh();
}
