using System.Numerics;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Props;
using vMenu.Enhanced.Menus.Vehicles.Aircraft;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Ticks;

using FreecamMode = vMenu.Enhanced.Freecam.Freecam;
using NoClipMode = vMenu.Enhanced.NoClip.NoClip;
using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles.Mines;

public static class VehicleMines
{
    private const int ModReadMs = 1000;

    private const int HoldMs = 500;

    private const int CooldownMs = 750;

    private const int Driver = -1;

    private const int ControlGroup = 0;

    private const int HornControl = 86;

    private const int ContextControl = 51;

    private const int FrontendXControl = 203;

    private const float UprightLimit = 90f;

    private const float DropAt = 0.7f;

    private const float DropFurtherBackAt = 1f;

    private const float DropLift = 0.2f;

    private const float AimStep = 0.1f;

    private const float ShotReach = 0.25f;

    private const int ShotLoadMs = 1000;

    private const int ShotSettleMs = 250;

    private const int MaxMineDeletes = 4;

    private static TickHandle? _tick;

    private static int _vehicle;

    private static uint _model;

    private static VehicleMineGroup _group;

    private static bool _active;

    private static bool _watching;

    private static int _nextModRead;

    private static int _mod = -1;

    private static bool _sawBlade;

    private static bool _armed;

    private static int _heldSince;

    private static bool _held;

    private static int _cooldownUntil;

    private static uint _loadedModel;

    private static uint _loadedWeapon;

    private static bool _hornHelpShown;

    private static bool _keyHelpShown;

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.VehicleMines);

    public static void RegisterEventHandlers() =>
        API.OnNetEvent(VehicleMineProfiles.DetonatedEvent, new Action<float, float, float>(OnDetonated), false);

    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "Vehicles.VehicleMines",
            Frame,
            TickRate.PerFrame,
            () => _active,
            autoStart: false);

        ClientPermissions.PermissionsChanged += Refresh;
        ResourceShutdown.Stopping += End;

        Refresh();
    }

    private static void Refresh()
    {
        Watch(IsAllowed);

        var ped = Native.PlayerPedId();
        var vehicle = IsAllowed && Native.IsPedInAnyVehicle(ped, false) ? Native.GetVehiclePedIsIn(ped, false) : 0;

        if (vehicle == 0 || Native.GetPedInVehicleSeat(vehicle, Driver, false) != ped)
        {
            End();

            return;
        }

        if (_active && vehicle == _vehicle)
        {
            return;
        }

        var model = Native.GetEntityModel(vehicle);

        if (VehicleMineProfiles.ForModel(model) is not VehicleMineGroup group)
        {
            End();

            return;
        }

        End();

        _vehicle = vehicle;
        _model = model;
        _group = group;
        _active = true;

        _tick?.Reevaluate();
    }

    private static void End()
    {
        _active = false;
        _vehicle = 0;
        _model = 0;
        _mod = -1;
        _sawBlade = false;
        _nextModRead = 0;
        _armed = false;
        _heldSince = 0;
        _held = false;

        Release();

        _tick?.Reevaluate();
    }

    private static void Frame()
    {
        if (!_active)
        {
            return;
        }

        if (!Native.DoesEntityExist(_vehicle))
        {
            End();

            return;
        }

        var now = Native.GetGameTimer();

        if (now >= _nextModRead)
        {
            _mod = Native.GetVehicleMod(_vehicle, VehicleMineProfiles.MineModSlot);
            _sawBlade = VehicleMineProfiles.HasSawBladeSlot(_model)
                && Native.GetVehicleMod(_vehicle, VehicleMineProfiles.SawBladeModSlot) > -1;
            _nextModRead = now + ModReadMs;
        }

        if (_mod < 0)
        {
            Disarm();

            return;
        }

        var control = Control();

        if (!CanUse() || IsMuted())
        {
            Disarm();
        }
        else
        {
            if (VehicleMineProfiles.UsesHorn(_group))
            {
                Native.DisableControlAction(ControlGroup, HornControl, true);
            }

            ShowHelp();

            var loaded = Load();

            if (_armed)
            {
                Handle(control, now, loaded);
            }
        }

        if (Native.IsDisabledControlReleased(ControlGroup, control))
        {
            _armed = true;
        }
    }

    private static void Handle(int control, int now, bool loaded)
    {
        if (Native.IsDisabledControlPressed(ControlGroup, control))
        {
            if (_heldSince == 0)
            {
                _heldSince = now;
            }
            else if (now - _heldSince >= HoldMs)
            {
                _held = true;
            }
        }

        if (!Native.IsDisabledControlJustReleased(ControlGroup, control))
        {
            return;
        }

        var held = _held;

        _heldSince = 0;
        _held = false;

        if (held || now < _cooldownUntil || !loaded)
        {
            return;
        }

        Drop();

        _cooldownUntil = now + CooldownMs;
    }

    private static void Disarm()
    {
        _armed = false;
        _heldSince = 0;
        _held = false;
    }

    private static int Control()
    {
        if (VehicleMineProfiles.UsesHorn(_group))
        {
            return HornControl;
        }

        return Native.IsUsingKeyboardAndMouse(2) ? FrontendXControl : ContextControl;
    }

    private static bool CanUse()
    {
        var ped = Native.PlayerPedId();

        return _mod >= 0
            && Native.GetPedInVehicleSeat(_vehicle, Driver, false) == ped
            && !Native.IsPedInjured(ped)
            && Native.IsVehicleDriveable(_vehicle, false)
            && Native.IsEntityUpright(_vehicle, UprightLimit)
            && Native.IsVehicleOnAllWheels(_vehicle)
            && !Native.IsEntityUpsidedown(_vehicle);
    }

    private static bool IsMuted() =>
        MenuController.IsAnyMenuOpen()
        || Native.IsPauseMenuActive()
        || PropPlacement.IsActive
        || FreecamMode.IsActive
        || NoClipMode.IsActive
        || SceneLock.IsActive;

    private static void ShowHelp()
    {
        var horn = VehicleMineProfiles.UsesHorn(_group);

        if ((horn ? _hornHelpShown : _keyHelpShown) || !AircraftHelp.CanShow)
        {
            return;
        }

        if (horn)
        {
            _hornHelpShown = true;

            AircraftHelp.Show("HELP_PROXMINVEH");

            return;
        }

        _keyHelpShown = true;

        AircraftHelp.Show(Native.IsUsingKeyboardAndMouse(2) ? "AVMH_MINE_PC" : "AVMH_MINE");
    }

    private static bool Load()
    {
        var model = VehicleMineProfiles.Model(_group);
        var weapon = VehicleMineProfiles.Weapon(_group, _mod);

        if (!Native.IsModelInCdimage(model))
        {
            return false;
        }

        if (model != _loadedModel || weapon != _loadedWeapon)
        {
            Release();

            _loadedModel = model;
            _loadedWeapon = weapon;
        }

        if (Native.HasModelLoaded(model) && Native.HasWeaponAssetLoaded(weapon))
        {
            return true;
        }

        Native.RequestModel(model);
        Native.RequestWeaponAsset(weapon, 31, 0);

        return false;
    }

    private static void Release()
    {
        if (_loadedModel != 0)
        {
            Native.SetModelAsNoLongerNeeded(_loadedModel);
        }

        if (_loadedWeapon != 0)
        {
            Native.RemoveWeaponAsset(_loadedWeapon);
        }

        _loadedModel = 0;
        _loadedWeapon = 0;
    }

    private static void Drop()
    {
        Native.GetModelDimensions(_model, out var min, out var max);

        var frontLeft = Native.GetOffsetFromEntityInWorldCoords(_vehicle, min.X, max.Y, min.Z);
        var frontRight = Native.GetOffsetFromEntityInWorldCoords(_vehicle, max.X, max.Y, min.Z);
        var rearLeft = Native.GetOffsetFromEntityInWorldCoords(_vehicle, min.X, min.Y, min.Z);
        var rearRight = Native.GetOffsetFromEntityInWorldCoords(_vehicle, max.X, min.Y, min.Z);

        var lift = new Vector3(0f, 0f, DropLift);
        var front = Vector3.Lerp(frontLeft, frontRight, 0.5f) + lift;
        var rear = Vector3.Lerp(rearLeft, rearRight, 0.5f) + lift;

        var at = _sawBlade || VehicleMineProfiles.DropsFurtherBack(_model) ? DropFurtherBackAt : DropAt;

        var from = Vector3.Lerp(front, rear, at);
        var to = Vector3.Lerp(front - lift, rear - lift, at + AimStep);

        Native.ShootSingleBulletBetweenCoordsIgnoreEntityNew(
            from.X, from.Y, from.Z, to.X, to.Y, to.Z,
            0, true, VehicleMineProfiles.Weapon(_group, _mod), Native.PlayerPedId(), true, true, -1f, 0,
            false, false, 0, true, true, false, false);
    }

    private static void OnDetonated(float x, float y, float z) => _ = DetonateAsync(x, y, z);

    // Our copy of a mine set off elsewhere stays armed, shooting it makes it go off here too.
    private static async Task DetonateAsync(float x, float y, float z)
    {
        if (HasMineNear(x, y, z))
        {
            var weapon = VehicleMineProfiles.DetonateShotWeapon;
            var giveUpAt = Native.GetGameTimer() + ShotLoadMs;

            Native.RequestWeaponAsset(weapon, 31, 0);

            while (!Native.HasWeaponAssetLoaded(weapon) && Native.GetGameTimer() < giveUpAt)
            {
                await API.Delay(0);
            }

            if (Native.HasWeaponAssetLoaded(weapon))
            {
                Native.ShootSingleBulletBetweenCoords(
                    x, y, z + ShotReach, x, y, z - ShotReach,
                    1, true, weapon, Native.PlayerPedId(), false, true, -1f);

                await API.Delay(ShotSettleMs);
            }

            Native.RemoveWeaponAsset(weapon);
        }

        Native.ClearAreaOfProjectiles(x, y, z, VehicleMineProfiles.DetonatedClearRadius, false);

        RemoveMineObjects(x, y, z);
    }

    // Clearing skips copies of mines other players dropped, so those are deleted as objects.
    private static void RemoveMineObjects(float x, float y, float z)
    {
        foreach (var model in VehicleMineProfiles.AllMineModels)
        {
            for (var tries = 0; tries < MaxMineDeletes; tries++)
            {
                var mine = Native.GetClosestObjectOfType(x, y, z, VehicleMineProfiles.DetonatedClearRadius, model, false, false, false);

                if (mine == 0 || !Native.DoesEntityExist(mine))
                {
                    break;
                }

                Native.SetEntityAsMissionEntity(mine, true, true);
                Native.DeleteObject(ref mine);
            }
        }
    }

    private static bool HasMineNear(float x, float y, float z)
    {
        foreach (var weapon in VehicleMineProfiles.AllMineWeapons)
        {
            if (Native.IsProjectileTypeWithinDistance(x, y, z, weapon, VehicleMineProfiles.DetonatedClearRadius, false))
            {
                return true;
            }
        }

        return false;
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
