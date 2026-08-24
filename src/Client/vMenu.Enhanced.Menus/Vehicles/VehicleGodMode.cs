using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;
using VehicleOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

// Almost everything here is a flag the game remembers, so it is written when something moves rather
// than held down by a loop. The three that are not flags, restoring engine health, taking decals off
// and repairing, are actions, which is what the damage event is for.
public static class VehicleGodMode
{
    private const float FullEngineHealth = 1000f;

    private const PedProtections Protections =
        PedProtections.NotDraggedOut | PedProtections.NotShotInVehicle | PedProtections.NotKnockedOffBike;

    private static readonly PedProtection.Claim Protection = PedProtection.Register();

    private static bool _watching;

    // The vehicle the flags are written on, so leaving it can take them back off.
    private static int _written;

    private static int _tyresRemembered;

    private static bool _tyresCouldBurst;

    public static bool Enabled => UserDefaults.VehicleGodMode.Value && IsAllowed;

    // The stored values rather than the resolved ones: these are what the sub option checkboxes show,
    // and a player unticking the master toggle should not find its options unticked underneath it.
    public static bool Invincible => UserDefaults.VehicleGodInvincible.Value;

    public static bool ProtectEngine => UserDefaults.VehicleGodEngine.Value;

    public static bool PreventVisualDamage => UserDefaults.VehicleGodVisual.Value;

    public static bool StrongWheels => UserDefaults.VehicleGodStrongWheels.Value;

    public static bool BulletproofTyres => UserDefaults.VehicleGodBulletproofTyres.Value;

    public static bool PreventRampDamage => UserDefaults.VehicleGodRamp.Value;

    public static bool AutoRepair => UserDefaults.VehicleGodAutoRepair.Value;

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.God);

    // Call once at startup, after ClientConfig.Initialize.
    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        Set(UserDefaults.VehicleGodMode, enabled);
    }

    public static void SetInvincible(bool on) => Set(UserDefaults.VehicleGodInvincible, on);

    public static void SetProtectEngine(bool on) => Set(UserDefaults.VehicleGodEngine, on);

    public static void SetPreventVisualDamage(bool on) => Set(UserDefaults.VehicleGodVisual, on);

    public static void SetStrongWheels(bool on) => Set(UserDefaults.VehicleGodStrongWheels, on);

    public static void SetBulletproofTyres(bool on) => Set(UserDefaults.VehicleGodBulletproofTyres, on);

    public static void SetPreventRampDamage(bool on) => Set(UserDefaults.VehicleGodRamp, on);

    public static void SetAutoRepair(bool on) => Set(UserDefaults.VehicleGodAutoRepair, on);

    // The recorded handle is dropped rather than released, because whatever reset the flags has already
    // put that vehicle back the way it found it.
    public static void Reapply()
    {
        _written = 0;
        _tyresRemembered = 0;

        Apply();
    }

    private static void Set(BoolDefault preference, bool value)
    {
        preference.Value = value;

        Apply();
    }

    private static void Apply()
    {
        var on = Enabled;

        Protection.Set(on, Protections);

        Watch(on);

        if (_tyresRemembered != 0 && !Native.DoesEntityExist(_tyresRemembered))
        {
            _tyresRemembered = 0;
        }

        var vehicle = OwnVehicle.Driven();

        // The one the player was in and is no longer gets its own flags back, if the server wants that.
        if (_written != 0 && _written != vehicle && ClientConfig.Value(VehicleOptionsSettings.ClearGodModeOnExit))
        {
            Write(_written, on: false);
        }

        _written = 0;

        if (vehicle == 0)
        {
            return;
        }

        Write(vehicle, on);

        if (!on)
        {
            return;
        }

        _written = vehicle;

        // Not awaited: everything a caller of Apply needs has already happened, and the repair is a guarded
        // no-op on a vehicle that has gone away by the time its delay is up.
        _ = RepairIfWanted(vehicle);
    }

    // The one place that touches a vehicle. Off writes the game's defaults, which is the undo.
    private static void Write(int vehicle, bool on)
    {
        // Handles are recycled, so one recorded before a delete can name something else entirely.
        if (!Native.DoesEntityExist(vehicle) || !Native.IsEntityAVehicle(vehicle))
        {
            if (_tyresRemembered == vehicle)
            {
                _tyresRemembered = 0;
            }

            return;
        }

        var invincible = on && Invincible;
        var engine = on && ProtectEngine;
        var visual = on && PreventVisualDamage;
        var wheels = on && StrongWheels;
        var ramp = on && PreventRampDamage;

        Native.SetEntityInvincible(vehicle, invincible, false);
        Native.SetEntityProofs(
            vehicle,
            invincible,
            invincible,
            invincible,
            invincible,
            invincible,
            invincible,
            false,
            invincible);

        // Doors carry their own breakage flag, so an invincible car still loses one to a hard knock.
        var doors = Native.GetNumberOfVehicleDoors(vehicle);

        for (var door = 0; door < doors; door++)
        {
            Native.SetVehicleDoorCanBreak(vehicle, door, !invincible);
        }

        Native.SetVehicleEngineCanDegrade(vehicle, !engine);
        Native.SetVehicleCanBeVisiblyDamaged(vehicle, !visual);
        Native.SetVehicleWheelsCanBreak(vehicle, !wheels);
        Native.SetVehicleHasStrongAxles(vehicle, wheels);
        Native.SetRampVehicleReceivesRampDamage(vehicle, !ramp);

        WriteTyres(vehicle, on && BulletproofTyres);

        if (!on)
        {
            return;
        }

        // Damage that has already landed, which no flag can undo.
        if (engine && Native.GetVehicleEngineHealth(vehicle) < FullEngineHealth)
        {
            Native.SetVehicleEngineHealth(vehicle, FullEngineHealth);
        }

        if (visual && Native.IsVehicleDamaged(vehicle))
        {
            Native.RemoveDecalsFromVehicle(vehicle);
        }
    }

    private static void WriteTyres(int vehicle, bool bulletproof)
    {
        if (bulletproof)
        {
            if (_tyresRemembered != vehicle)
            {
                _tyresRemembered = vehicle;
                _tyresCouldBurst = Native.GetVehicleTyresCanBurst(vehicle);
            }

            Native.SetVehicleTyresCanBurst(vehicle, false);

            return;
        }

        if (_tyresRemembered != vehicle)
        {
            return;
        }

        _tyresRemembered = 0;

        Native.SetVehicleTyresCanBurst(vehicle, _tyresCouldBurst);
    }

    // Silent, unlike the repair option. A notification per collision would be intolerable.
    private static Task RepairIfWanted(int vehicle)
    {
        if (!AutoRepair || !Native.DoesEntityExist(vehicle) || !Native.IsVehicleDamaged(vehicle))
        {
            return Task.CompletedTask;
        }

        return VehicleRepair.ApplyAsync(vehicle);
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
            LocalVehicleTicks.VehicleDamagedAsync += OnDamagedAsync;

            return;
        }

        LocalVehicleTicks.VehicleChanged -= OnChanged;
        LocalVehicleTicks.VehicleDamagedAsync -= OnDamagedAsync;
    }

    // The combined event rather than three of them: it covers entering, leaving and swapping alike. A
    // seat change arrives here too, with the same vehicle either side, which Apply reads as nothing to
    // release and a rewrite of what is already there.
    private static void OnChanged(VehicleChanged _) => Apply();

    // Rewrites every flag, not just the repairs: ownership changing hands can drop them, and the first
    // hit that lands is the cheapest place to notice. Async so the repair's second pass is awaited.
    private static Task OnDamagedAsync(VehicleDamaged damage)
    {
        if (!Enabled || damage.Vehicle != _written)
        {
            return Task.CompletedTask;
        }

        Write(damage.Vehicle, on: true);

        return RepairIfWanted(damage.Vehicle);
    }
}
