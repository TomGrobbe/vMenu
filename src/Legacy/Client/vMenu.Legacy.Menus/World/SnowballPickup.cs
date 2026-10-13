using CitizenFX.Core;
using CitizenFX.Core.Native;

using MenuAPI;

using vMenu.Enhanced.Permissions;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

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

        API.RegisterCommand(Command, new Action<int, List<object>, string>((_, _, _) => OnPressed()), false);
        API.RegisterCommand(padCommand, new Action<int, List<object>, string>((_, _, _) => OnPressed()), false);

        KeyMapping.Register(Command, padCommand, "vMenu: Pick up a snowball", Key, Button);
    }

    private static void OnPressed()
    {
        // Before the guard: CanPickUp is all natives, and off the game thread they answer as though
        // the world is empty.
        Script.RunOnMainThread(() =>
        {
            if (_running || !CanPickUp())
            {
                return;
            }

            Dispatch();
        });
    }

    private static async void Dispatch() => await PickUpAsync();

    private static bool CanPickUp()
    {
        if (!WorldSnow.Wanted || !ClientPermissions.IsAllowed(WeaponOptionsPermissions.Snowball))
        {
            return false;
        }

        if (MenuController.IsAnyMenuOpen()
            || API.IsPauseMenuActive()
            || API.IsPlayerSwitchInProgress()
            || !API.IsScreenFadedIn())
        {
            return false;
        }

        var ped = API.PlayerPedId();

        if (API.IsPedDeadOrDying(ped, true)
            || API.IsPedInAnyVehicle(ped, false)
            || !API.IsPedOnFoot(ped))
        {
            return false;
        }

        if (API.GetInteriorFromEntity(ped) != 0)
        {
            return false;
        }

        if (API.IsPedFalling(ped)
            || API.IsPedInParachuteFreeFall(ped)
            || NativeFixer.IsPedBeingStunned(ped, 0)
            || API.IsPedWalking(ped)
            || API.IsPedRunning(ped)
            || API.IsPedSprinting(ped)
            || API.IsPedSwimming(ped)
            || API.IsPedSwimmingUnderWater(ped)
            || API.IsPedDiving(ped))
        {
            return false;
        }

        // Legacy had this test the wrong way round and so only worked with a weapon drawn.
        var selected = NativeFixer.GetSelectedPedWeapon(ped);

        return selected == Hashing.Joaat(Unarmed) || selected == Hashing.Joaat(SnowballWeapon);
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
        var ped = API.PlayerPedId();
        var weapon = Hashing.Joaat(SnowballWeapon);
        var max = NativeFixer.GetMaxAmmo(ped, weapon, out var reported) ? reported : FallbackMaxAmmo;

        if (API.GetAmmoInPedWeapon(ped, weapon) >= max)
        {
            return;
        }

        if (!await LoadAsync())
        {
            return;
        }

        API.ClearPedTasks(ped);
        API.SetPedCurrentWeaponVisible(ped, false, true, false, false);

        NativeFixer.TaskPlayAnim(ped, Dictionary, Animation, BlendIn, BlendOut, -1, 0, 0f, false, 0, false);

        await RunAsync(ped, weapon, max);

        API.RemoveAnimDict(Dictionary);
    }

    private static async Task RunAsync(int ped, uint weapon, int max)
    {
        var granted = false;
        var started = API.GetGameTimer();
        var limit = (int)(API.GetAnimDuration(Dictionary, Animation) * 1000f) + StartTimeout;

        while (API.GetGameTimer() - started < limit)
        {
            await BaseScript.Delay(0);

            if (API.HasAnimEventFired(ped, Hashing.Joaat(InterruptEvent)))
            {
                return;
            }

            if (!granted && API.HasAnimEventFired(ped, Hashing.Joaat(CreatedEvent)))
            {
                granted = true;

                Grant(ped, weapon, max);
            }

            if (granted && API.GetEntityAnimCurrentTime(ped, Dictionary, Animation) >= 0.97f)
            {
                return;
            }
        }
    }

    private static void Grant(int ped, uint weapon, int max)
    {
        API.AddAmmoToPed(ped, weapon, PerPickup);
        API.GiveWeaponToPed(ped, weapon, 0, true, true);

        if (API.GetAmmoInPedWeapon(ped, weapon) > max)
        {
            NativeFixer.SetPedAmmo(ped, weapon, max, false);
        }
    }

    private static async Task<bool> LoadAsync()
    {
        if (API.HasAnimDictLoaded(Dictionary))
        {
            return true;
        }

        API.RequestAnimDict(Dictionary);

        var started = API.GetGameTimer();

        while (!API.HasAnimDictLoaded(Dictionary))
        {
            if (API.GetGameTimer() - started > StartTimeout)
            {
                return false;
            }

            await BaseScript.Delay(0);
        }

        return true;
    }
}
