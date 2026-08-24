using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Permissions;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

namespace vMenu.Enhanced.Menus.World;

public static class SnowballPickup
{
    private const string Command = "vmenu:snowball";

    private const string Key = "G";

    private const string Button = "LRIGHT_INDEX";

    private const string Dictionary = "anim@mp_snowball";

    private const string Animation = "pickup_snowball";

    private const string SnowballWeapon = "weapon_snowball";

    private const string Unarmed = "weapon_unarmed";

    private const int PerPickup = 2;

    private const int FallbackMaxAmmo = 10;

    private const float BlendIn = 8f;

    private const float BlendOut = 1f;

    private const int StartTimeout = 1000;

    private const string CreatedEvent = "CreateObject";

    private const string InterruptEvent = "Interrupt";

    private static bool _registered;

    private static bool _running;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        var padCommand = KeyMapping.Pad(Command);

        SharedAPI.Commands.RegisterCommand(Command, false, new Action(OnPressed));
        SharedAPI.Commands.RegisterCommand(padCommand, false, new Action(OnPressed));

        KeyMapping.Register(Command, padCommand, "vMenu: Pick up a snowball", Key, Button);
    }

    private static void OnPressed()
    {
        if (_running || !CanPickUp())
        {
            return;
        }

        SharedAPI.RunOnMainThread(Dispatch);
    }

    private static async void Dispatch() => await PickUpAsync();

    private static bool CanPickUp()
    {
        if (!WorldSnow.Wanted || !ClientPermissions.IsAllowed(WeaponOptionsPermissions.Snowball))
        {
            return false;
        }

        if (MenuController.IsAnyMenuOpen()
            || Native.IsPauseMenuActive()
            || Native.IsPlayerSwitchInProgress()
            || !Native.IsScreenFadedIn())
        {
            return false;
        }

        var ped = Native.PlayerPedId();

        if (Native.IsPedDeadOrDying(ped, true)
            || Native.IsPedInAnyVehicle(ped, false)
            || !Native.IsPedOnFoot(ped))
        {
            return false;
        }

        if (Native.GetInteriorFromEntity(ped) != 0)
        {
            return false;
        }

        if (Native.IsPedFalling(ped)
            || Native.IsPedInParachuteFreeFall(ped)
            || Native.IsPedBeingStunned(ped, 0)
            || Native.IsPedWalking(ped)
            || Native.IsPedRunning(ped)
            || Native.IsPedSprinting(ped)
            || Native.IsPedSwimming(ped)
            || Native.IsPedSwimmingUnderWater(ped)
            || Native.IsPedDiving(ped))
        {
            return false;
        }

        // Legacy had this test the wrong way round and so only worked with a weapon drawn.
        var selected = Native.GetSelectedPedWeapon(ped);

        return selected == API.Hash(Unarmed) || selected == API.Hash(SnowballWeapon);
    }

    private static async Task PickUpAsync()
    {
        _running = true;

        try
        {
            await ScoopAsync();
        }
        finally
        {
            _running = false;
        }
    }

    private static async Task ScoopAsync()
    {
        var ped = Native.PlayerPedId();
        var weapon = API.Hash(SnowballWeapon);
        var max = Native.GetMaxAmmo(ped, weapon, out var reported) ? reported : FallbackMaxAmmo;

        if (Native.GetAmmoInPedWeapon(ped, weapon) >= max)
        {
            return;
        }

        if (!await LoadAsync())
        {
            return;
        }

        Native.ClearPedTasks(ped);
        Native.SetPedCurrentWeaponVisible(ped, false, true, false, false);

        Native.TaskPlayAnim(ped, Dictionary, Animation, BlendIn, BlendOut, -1, 0, 0f, false, 0, false);

        await RunAsync(ped, weapon, max);

        Native.RemoveAnimDict(Dictionary);
    }

    private static async Task RunAsync(int ped, uint weapon, int max)
    {
        var granted = false;
        var started = Native.GetGameTimer();
        var limit = (int)(Native.GetAnimDuration(Dictionary, Animation) * 1000f) + StartTimeout;

        while (Native.GetGameTimer() - started < limit)
        {
            await API.Delay(0);

            if (Native.HasAnimEventFired(ped, API.Hash(InterruptEvent)))
            {
                return;
            }

            if (!granted && Native.HasAnimEventFired(ped, API.Hash(CreatedEvent)))
            {
                granted = true;

                Grant(ped, weapon, max);
            }

            if (granted && Native.GetEntityAnimCurrentTime(ped, Dictionary, Animation) >= 0.97f)
            {
                return;
            }
        }
    }

    private static void Grant(int ped, uint weapon, int max)
    {
        Native.AddAmmoToPed(ped, weapon, PerPickup);
        Native.GiveWeaponToPed(ped, weapon, 0, true, true);

        if (Native.GetAmmoInPedWeapon(ped, weapon) > max)
        {
            Native.SetPedAmmo(ped, weapon, max, false);
        }
    }

    private static async Task<bool> LoadAsync()
    {
        if (Native.HasAnimDictLoaded(Dictionary))
        {
            return true;
        }

        Native.RequestAnimDict(Dictionary);

        var started = Native.GetGameTimer();

        while (!Native.HasAnimDictLoaded(Dictionary))
        {
            if (Native.GetGameTimer() - started > StartTimeout)
            {
                return false;
            }

            await API.Delay(0);
        }

        return true;
    }
}
