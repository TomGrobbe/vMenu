using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

internal static class BombBay
{
    private const int HoldMs = 500;

    private const int CooldownMs = 750;

    private const int StickSpacingMs = 200;

    private const int BombsPerStick = 5;

    private const int DoorsShutMs = 2000;

    private const float BayLift = 0.4f;

    private const float AimDrop = 0.2f;

    private static readonly uint[] Weapons =
    [
        Hashing.Joaat("vehicle_weapon_bomb"),
        Hashing.Joaat("vehicle_weapon_bomb_incendiary"),
        Hashing.Joaat("vehicle_weapon_bomb_gas"),
        Hashing.Joaat("vehicle_weapon_bomb_cluster"),
    ];

    private static readonly uint[] Models =
    [
        Hashing.Joaat("w_smug_bomb_01"),
        Hashing.Joaat("w_smug_bomb_04"),
        Hashing.Joaat("w_smug_bomb_03"),
        Hashing.Joaat("w_smug_bomb_02"),
    ];

    private static bool _open;

    private static bool _rearmed = true;

    private static bool _dropArmed = true;

    private static int _toggleHeldSince;

    private static int _dropHeldSince;

    private static int _cooldownUntil;

    private static int _stickLeft;

    private static int _nextStickBomb;

    private static uint _loadedModel;

    private static uint _loadedWeapon;

    private static int _doorsShutSince;

    private static bool _doorsOpen;

    private static bool _seenOpen;

    private static bool _weaponChecked;

    private static bool _weaponDisabled;

    private static uint _disabledWeapon;

    private static bool _closedHelpShown;

    private static bool _openHelpShown;

    public static bool Busy => _open;

    public static bool DoorsOpen(AircraftSession session) => _open || API.GetAreBombBayDoorsOpen(session.Vehicle);

    public static uint WeaponFor(AircraftSession session) => Weapons[Math.Max(session.BombType, 0)];

    public static void Frame(AircraftSession session, int now)
    {
        if (session.BombType < 0)
        {
            if (_open)
            {
                Close(session);
            }

            BombCamera.Reset();

            return;
        }

        if (AircraftControls.Muted)
        {
            _rearmed = false;
            _dropArmed = false;
        }

        if (_open)
        {
            BlockControls(session);
        }

        if (_open && session.HasLanded())
        {
            Close(session);
        }

        if (_open)
        {
            WatchDoors(session, now);
        }

        if (!session.CanBomb())
        {
            _dropHeldSince = 0;
            _stickLeft = 0;

            if (HeldFor(now))
            {
                Countermeasures.Disarm();
            }
        }
        else if (_open)
        {
            WhileOpen(session, now);
        }
        else
        {
            WhileClosed(session, now);
        }

        BombCamera.Frame(session, DoorsOpen(session));

        if (API.IsControlReleased(AircraftControls.Group, AircraftControls.BombBay))
        {
            _rearmed = true;
        }

        if (API.IsDisabledControlReleased(AircraftControls.Group, AircraftControls.Drop))
        {
            _dropArmed = true;
        }
    }

    public static void Adopt(AircraftSession session)
    {
        var open = !_open
            && session.BombType >= 0
            && API.NetworkHasControlOfEntity(session.Vehicle)
            && API.GetAreBombBayDoorsOpen(session.Vehicle);

        // Seen open twice, so doors the other seat is closing right now are not taken over.
        if (open && _seenOpen && AircraftWeaponClaim.TryTake(session, quiet: true))
        {
            _open = true;
            _doorsShutSince = 0;
        }

        _seenOpen = open;
    }

    public static void Shut(AircraftSession session)
    {
        _open = false;
        _stickLeft = 0;
        _dropHeldSince = 0;
        _doorsShutSince = 0;

        BombCamera.Stop();
        RestoreControls(session);
        Release();
    }

    public static void Reset(AircraftSession? session)
    {
        if (session is not null && _open)
        {
            Close(session);
        }

        _open = false;
        _seenOpen = false;
        _doorsShutSince = 0;
        _rearmed = true;
        _dropArmed = true;
        _toggleHeldSince = 0;
        _dropHeldSince = 0;
        _stickLeft = 0;
        _closedHelpShown = false;
        _openHelpShown = false;

        if (session is not null)
        {
            RestoreControls(session);
        }

        Release();
    }

    private static void WhileClosed(AircraftSession session, int now)
    {
        if (!_closedHelpShown && AircraftHelp.CanShow && !API.GetAreBombBayDoorsOpen(session.Vehicle))
        {
            AircraftHelp.Show("BOMB_CAM_HELP");

            _closedHelpShown = true;
        }

        if (!HeldFor(now))
        {
            return;
        }

        Countermeasures.Disarm();

        if (!AircraftWeaponClaim.TryTake(session))
        {
            return;
        }

        AircraftWeaponOwner.Request(session.Vehicle, AircraftWeaponEvents.OpenBombBay);

        _open = true;
        _doorsShutSince = 0;
        _cooldownUntil = now + CooldownMs;
        _dropHeldSince = 0;
        _dropArmed = false;
    }

    private static void WhileOpen(AircraftSession session, int now)
    {
        if (!_openHelpShown && AircraftHelp.CanShow)
        {
            AircraftHelp.Show("BOMB_HELP");

            _openHelpShown = true;
        }

        if (HeldFor(now))
        {
            Close(session);

            Countermeasures.Disarm();

            return;
        }

        if (!Load(session) || !_doorsOpen)
        {
            _dropHeldSince = 0;

            return;
        }

        if (_stickLeft > 0)
        {
            if (now >= _nextStickBomb)
            {
                _stickLeft--;

                Drop(session, now, _stickLeft == 0);

                _nextStickBomb = now + StickSpacingMs;
            }

            return;
        }

        if (now < _cooldownUntil || !_dropArmed)
        {
            _dropHeldSince = 0;

            return;
        }

        if (API.IsDisabledControlJustReleased(AircraftControls.Group, AircraftControls.Drop))
        {
            Drop(session, now, true);
            _dropHeldSince = 0;

            return;
        }

        if (!API.IsDisabledControlPressed(AircraftControls.Group, AircraftControls.Drop))
        {
            _dropHeldSince = 0;
        }
        else if (_dropHeldSince == 0)
        {
            _dropHeldSince = now;
        }
        else if (now - _dropHeldSince >= HoldMs)
        {
            _dropHeldSince = 0;
            _dropArmed = false;
            _stickLeft = BombsPerStick;
            _nextStickBomb = now;
        }
    }

    private static bool HeldFor(int now)
    {
        if (!_rearmed || !API.IsControlPressed(AircraftControls.Group, AircraftControls.BombBay))
        {
            _toggleHeldSince = 0;

            return false;
        }

        if (_toggleHeldSince == 0)
        {
            _toggleHeldSince = now;

            return false;
        }

        if (now - _toggleHeldSince < HoldMs)
        {
            return false;
        }

        _toggleHeldSince = 0;
        _rearmed = false;

        return true;
    }

    private static void Close(AircraftSession session)
    {
        AircraftWeaponOwner.Request(session.Vehicle, AircraftWeaponEvents.CloseBombBay);

        Shut(session);
    }

    // A refused request, or doors shut from the other seat, would otherwise leave them open here for good.
    private static void WatchDoors(AircraftSession session, int now)
    {
        _doorsOpen = API.GetAreBombBayDoorsOpen(session.Vehicle);

        if (_doorsOpen)
        {
            _doorsShutSince = 0;
        }
        else if (_doorsShutSince == 0)
        {
            _doorsShutSince = now;
        }
        else if (now - _doorsShutSince >= DoorsShutMs)
        {
            Shut(session);
        }
    }

    private static void BlockControls(AircraftSession session)
    {
        foreach (var control in AircraftControls.BlockedWhileBombing)
        {
            API.DisableControlAction(AircraftControls.Group, control, true);
        }

        if (_weaponChecked || !session.IsPilot)
        {
            return;
        }

        _weaponChecked = true;

        if (!API.DoesVehicleHaveWeapons(session.Vehicle))
        {
            return;
        }

        var ped = API.PlayerPedId();

        if (NativeFixer.GetCurrentPedVehicleWeapon(ped, out var weapon))
        {
            _disabledWeapon = unchecked((uint)weapon);

            API.DisableVehicleWeapon(true, _disabledWeapon, session.Vehicle, ped);

            _weaponDisabled = true;
        }
    }

    private static void RestoreControls(AircraftSession session)
    {
        _weaponChecked = false;

        if (!_weaponDisabled)
        {
            return;
        }

        _weaponDisabled = false;

        if (API.DoesEntityExist(session.Vehicle))
        {
            API.DisableVehicleWeapon(false, _disabledWeapon, session.Vehicle, API.PlayerPedId());
        }
    }

    private static bool Load(AircraftSession session)
    {
        var model = Models[session.BombType];
        var weapon = Weapons[session.BombType];

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

    private static void Drop(AircraftSession session, int now, bool last)
    {
        var vehicle = session.Vehicle;

        NativeFixer.GetModelDimensions(session.Model, out var min, out var max);

        var frontLeft = API.GetOffsetFromEntityInWorldCoords(vehicle, min.X, max.Y, min.Z);
        var frontRight = API.GetOffsetFromEntityInWorldCoords(vehicle, max.X, max.Y, min.Z);
        var rearLeft = API.GetOffsetFromEntityInWorldCoords(vehicle, min.X, min.Y, min.Z);
        var rearRight = API.GetOffsetFromEntityInWorldCoords(vehicle, max.X, min.Y, min.Z);

        var lift = new Vector3(0f, 0f, BayLift);
        var front = Vector3.Lerp(frontLeft, frontRight, 0.5f) + lift;
        var rear = Vector3.Lerp(rearLeft, rearRight, 0.5f) + lift;
        var from = Vector3.Lerp(front, rear, session.Profile.BombPosition);

        var aim = new Vector3(0f, 0f, AimDrop);
        var to = Vector3.Lerp(front - aim, rear - aim, session.Profile.BombPosition - 0.0001f);

        var ped = API.PlayerPedId();

        NativeFixer.ShootSingleBulletBetweenCoordsIgnoreEntityNew(
            from.X, from.Y, from.Z, to.X, to.Y, to.Z,
            0, false, Weapons[session.BombType], ped, true, true, -1f, vehicle,
            false, false, 0, true, true, false, false);

        API.PlaySoundFrontend(-1, "bomb_deployed", "DLC_SM_Bomb_Bay_Bombs_Sounds", true);

        _cooldownUntil = now + CooldownMs;

        if (last && session.IsPilot)
        {
            BombCamera.FollowNextBomb();
        }
    }
}
