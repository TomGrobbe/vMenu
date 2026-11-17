using CitizenFX.Core;
using CitizenFX.Core.Native;

using MenuAPI;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Props;
using vMenu.Enhanced.Menus.Vehicles.Aircraft;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Ticks;

using vMenu.Legacy.Hosting;

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
        Script.EventHandlers[VehicleMineProfiles.DetonatedEvent] += new Action<float, float, float>(OnDetonated);

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

        var ped = API.PlayerPedId();
        var vehicle = IsAllowed && API.IsPedInAnyVehicle(ped, false) ? API.GetVehiclePedIsIn(ped, false) : 0;

        if (vehicle == 0 || NativeFixer.GetPedInVehicleSeat(vehicle, Driver, false) != ped)
        {
            End();

            return;
        }

        if (_active && vehicle == _vehicle)
        {
            return;
        }

        var model = NativeFixer.GetEntityModel(vehicle);

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

        if (!API.DoesEntityExist(_vehicle))
        {
            End();

            return;
        }

        var now = API.GetGameTimer();

        if (now >= _nextModRead)
        {
            _mod = API.GetVehicleMod(_vehicle, VehicleMineProfiles.MineModSlot);
            _sawBlade = VehicleMineProfiles.HasSawBladeSlot(_model)
                && API.GetVehicleMod(_vehicle, VehicleMineProfiles.SawBladeModSlot) > -1;
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
                API.DisableControlAction(ControlGroup, HornControl, true);
            }

            ShowHelp();

            var loaded = Load();

            if (_armed)
            {
                Handle(control, now, loaded);
            }
        }

        if (API.IsDisabledControlReleased(ControlGroup, control))
        {
            _armed = true;
        }
    }

    private static void Handle(int control, int now, bool loaded)
    {
        if (API.IsDisabledControlPressed(ControlGroup, control))
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

        if (!API.IsDisabledControlJustReleased(ControlGroup, control))
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

        return NativeFixer.IsUsingKeyboardAndMouse(2) ? FrontendXControl : ContextControl;
    }

    private static bool CanUse()
    {
        var ped = API.PlayerPedId();

        return _mod >= 0
            && NativeFixer.GetPedInVehicleSeat(_vehicle, Driver, false) == ped
            && !API.IsPedInjured(ped)
            && API.IsVehicleDriveable(_vehicle, false)
            && API.IsEntityUpright(_vehicle, UprightLimit)
            && API.IsVehicleOnAllWheels(_vehicle)
            && !API.IsEntityUpsidedown(_vehicle);
    }

    private static bool IsMuted() =>
        MenuController.IsAnyMenuOpen()
        || API.IsPauseMenuActive()
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

        AircraftHelp.Show(NativeFixer.IsUsingKeyboardAndMouse(2) ? "AVMH_MINE_PC" : "AVMH_MINE");
    }

    private static bool Load()
    {
        var model = VehicleMineProfiles.Model(_group);
        var weapon = VehicleMineProfiles.Weapon(_group, _mod);

        if (!API.IsModelInCdimage(model))
        {
            return false;
        }

        if (model != _loadedModel || weapon != _loadedWeapon)
        {
            Release();

            _loadedModel = model;
            _loadedWeapon = weapon;
        }

        if (API.HasModelLoaded(model) && API.HasWeaponAssetLoaded(weapon))
        {
            return true;
        }

        API.RequestModel(model);
        API.RequestWeaponAsset(weapon, 31, 0);

        return false;
    }

    private static void Release()
    {
        if (_loadedModel != 0)
        {
            API.SetModelAsNoLongerNeeded(_loadedModel);
        }

        if (_loadedWeapon != 0)
        {
            API.RemoveWeaponAsset(_loadedWeapon);
        }

        _loadedModel = 0;
        _loadedWeapon = 0;
    }

    private static void Drop()
    {
        NativeFixer.GetModelDimensions(_model, out var min, out var max);

        var frontLeft = API.GetOffsetFromEntityInWorldCoords(_vehicle, min.X, max.Y, min.Z);
        var frontRight = API.GetOffsetFromEntityInWorldCoords(_vehicle, max.X, max.Y, min.Z);
        var rearLeft = API.GetOffsetFromEntityInWorldCoords(_vehicle, min.X, min.Y, min.Z);
        var rearRight = API.GetOffsetFromEntityInWorldCoords(_vehicle, max.X, min.Y, min.Z);

        var lift = new Vector3(0f, 0f, DropLift);
        var front = Vector3.Lerp(frontLeft, frontRight, 0.5f) + lift;
        var rear = Vector3.Lerp(rearLeft, rearRight, 0.5f) + lift;

        var at = _sawBlade || VehicleMineProfiles.DropsFurtherBack(_model) ? DropFurtherBackAt : DropAt;

        var from = Vector3.Lerp(front, rear, at);
        var to = Vector3.Lerp(front - lift, rear - lift, at + AimStep);

        NativeFixer.ShootSingleBulletBetweenCoordsIgnoreEntityNew(
            from.X, from.Y, from.Z, to.X, to.Y, to.Z,
            0, true, VehicleMineProfiles.Weapon(_group, _mod), API.PlayerPedId(), true, true, -1f, 0,
            false, false, 0, true, true, false, false);
    }

    private static void OnDetonated(float x, float y, float z) => _ = DetonateAsync(x, y, z);

    // Our copy of a mine set off elsewhere stays armed, shooting it makes it go off here too.
    private static async Task DetonateAsync(float x, float y, float z)
    {
        if (HasMineNear(x, y, z))
        {
            var weapon = VehicleMineProfiles.DetonateShotWeapon;
            var giveUpAt = API.GetGameTimer() + ShotLoadMs;

            API.RequestWeaponAsset(weapon, 31, 0);

            while (!API.HasWeaponAssetLoaded(weapon) && API.GetGameTimer() < giveUpAt)
            {
                await BaseScript.Delay(0);
            }

            if (API.HasWeaponAssetLoaded(weapon))
            {
                API.ShootSingleBulletBetweenCoords(
                    x, y, z + ShotReach, x, y, z - ShotReach,
                    1, true, weapon, API.PlayerPedId(), false, true, -1f);

                await BaseScript.Delay(ShotSettleMs);
            }

            API.RemoveWeaponAsset(weapon);
        }

        API.ClearAreaOfProjectiles(x, y, z, VehicleMineProfiles.DetonatedClearRadius, 0);

        RemoveMineObjects(x, y, z);
    }

    // Clearing skips copies of mines other players dropped, so those are deleted as objects.
    private static void RemoveMineObjects(float x, float y, float z)
    {
        foreach (var model in VehicleMineProfiles.AllMineModels)
        {
            for (var tries = 0; tries < MaxMineDeletes; tries++)
            {
                var mine = API.GetClosestObjectOfType(x, y, z, VehicleMineProfiles.DetonatedClearRadius, model, false, false, false);

                if (mine == 0 || !API.DoesEntityExist(mine))
                {
                    break;
                }

                API.SetEntityAsMissionEntity(mine, true, true);
                API.DeleteObject(ref mine);
            }
        }
    }

    private static bool HasMineNear(float x, float y, float z)
    {
        foreach (var weapon in VehicleMineProfiles.AllMineWeapons)
        {
            if (API.IsProjectileTypeWithinDistance(x, y, z, weapon, VehicleMineProfiles.DetonatedClearRadius, false))
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
