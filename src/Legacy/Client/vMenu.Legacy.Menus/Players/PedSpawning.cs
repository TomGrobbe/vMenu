using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Weapons;
using vMenu.Enhanced.Permissions;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Players;

public static class PedSpawning
{
    private const int DriverSeat = -1;

    private const int NoSeat = -2;

    private const int WarpTimeout = 1000;

    private const int HeadBlendTimeout = 1000;

    private static readonly uint FreemodeMale = Hashing.Joaat("mp_m_freemode_01");

    private static readonly uint FreemodeFemale = Hashing.Joaat("mp_f_freemode_01");

    // The two freemode models carry a face, hair colour, overlays and tattoos that live nowhere near the
    // clothes, and vMenu has no way to read or write any of it yet. Everything that would only half work
    // on one of them asks here first.
    public static bool IsFreemode(uint hash) => hash == FreemodeMale || hash == FreemodeFemale;

    public static bool IsFreemodeMale(uint hash) => hash == FreemodeMale;

    public static uint FreemodeModel(bool male) => male ? FreemodeMale : FreemodeFemale;

    public static bool IsWearingFreemode() =>
        IsFreemode(NativeFixer.GetEntityModel(API.PlayerPedId()));

    public static bool IsSpawnable(string modelName) => IsSpawnable(Hashing.Joaat(modelName));

    public static bool IsSpawnable(uint hash) =>
        API.IsModelValid(hash) && API.IsModelInCdimage(hash) && API.IsModelAPed(hash);

    // The saved peds menu is not a way around a restricted ped list, so the ped list's own rules still
    // apply: a whitelisted model needs its own permission, and a listed one needs its category. A model
    // in neither is one the owner never restricted.
    public static bool IsPermitted(string modelName)
    {
        if (ClientPedPermissions.IsWhitelisted(modelName))
        {
            return ClientPedPermissions.CanSpawnPed(modelName, string.Empty);
        }

        return PedModelSync.Find(modelName) is not { } known
            || ClientPedPermissions.CanSpawnCategory(known.Category);
    }

    // False when the model is not a ped, or is not streamed in on this client.
    public static async Task<bool> SetPlayerModelAsync(string modelName) =>
        await SetPlayerModelAsync(Hashing.Joaat(modelName));

    // By hash as well as by name, because a saved ped only ever stored the hash. The game has no reverse
    // lookup for a ped model, so a save whose model the owner never listed has no name left to spawn from.
    public static async Task<bool> SetPlayerModelAsync(uint hash)
    {
        if (SceneLock.IsActive || !IsSpawnable(hash))
        {
            return false;
        }

        // Requested by hand rather than through a helper, the same way vehicle spawning does it.
        API.RequestModel(hash);

        while (!API.HasModelLoaded(hash))
        {
            await BaseScript.Delay(0);
        }

        // Only when it is not already the model being worn. Swapping to the same one throws the ped out of
        // whatever it was doing for no gain.
        if (NativeFixer.GetEntityModel(API.PlayerPedId()) != hash)
        {
            // Read before the swap and handed back after it. Nothing is raised until the swap is done, by which
            // point the ped holding the weapons is gone, so this is the last chance to read them.
            var carried = WeaponCarryOver.Capture();

            await SwapAsync(hash);

            await WeaponCarryOver.RestoreAsync(carried);
        }

        await ResetAppearanceAsync(hash);

        PedKeepProps.Apply(API.PlayerPedId());

        // A new ped walks the way its model walks, so the choice the player made has to be put back on by
        // hand. A saved ped applied on top of this brings its own and wins, which is right.
        await PedWalkingStyle.ReapplyAsync();

        API.SetModelAsNoLongerNeeded(hash);

        return true;
    }

    // Health, armour and the seat the player was in do not survive a model change on their own, so they
    // are read off the old ped and written back onto the new one.
    private static async Task SwapAsync(uint hash)
    {
        var ped = API.PlayerPedId();
        var player = API.PlayerId();

        var maxHealth = API.GetEntityMaxHealth(ped);
        var health = API.GetEntityHealth(ped);
        var maxArmour = API.GetPlayerMaxArmour(player);
        var armour = API.GetPedArmour(ped);

        ReadVehicle(ped, out var vehicle, out var seat);

        API.SetPlayerModel(player, hash);

        // A new ped, so everything below has to find it again rather than reuse the handle above.
        ped = API.PlayerPedId();

        API.SetPlayerMaxArmour(player, maxArmour);
        API.SetEntityMaxHealth(ped, maxHealth);
        // The last two are who did it and with what, which for putting health back is nobody.
        NativeFixer.SetEntityHealth(ped, health, 0, 0);
        API.SetPedArmour(ped, armour);

        if (vehicle != 0 && seat != NoSeat)
        {
            await WarpBackAsync(ped, vehicle, seat);
        }
    }

    // The game drops the player next to the vehicle rather than into it, so the seat is asked for again
    // until it takes. Frozen meanwhile, otherwise the ped walks off while being asked.
    private static async Task WarpBackAsync(int ped, int vehicle, int seat)
    {
        if (!API.DoesEntityExist(vehicle))
        {
            return;
        }

        API.FreezeEntityPosition(ped, true);

        var started = API.GetGameTimer();

        while (!API.IsPedInVehicle(ped, vehicle, false))
        {
            if (API.GetGameTimer() - started > WarpTimeout)
            {
                break;
            }

            API.ClearPedTasks(ped);

            await BaseScript.Delay(0);

            API.TaskWarpPedIntoVehicle(ped, vehicle, seat);
        }

        API.FreezeEntityPosition(ped, false);
    }

    private static async Task ResetAppearanceAsync(uint hash)
    {
        var ped = API.PlayerPedId();

        API.SetPedDefaultComponentVariation(ped);
        NativeFixer.ClearAllPedProps(ped, false);
        API.ClearPedDecorations(ped);
        API.ClearPedFacialDecorations(ped);

        if (!IsFreemode(hash))
        {
            return;
        }

        // The two freemode models have no face of their own and spawn grey and untextured without this.
        // Zeroes with an even blend between them is the game's own neutral starting point.
        API.SetPedHeadBlendData(ped, 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 0f, false);

        // Bounded, because this sits inside a menu row's handler and a blend that never reports back would
        // leave that row unusable for the rest of the session.
        var started = API.GetGameTimer();

        while (!API.HasPedHeadBlendFinished(ped) && API.GetGameTimer() - started < HeadBlendTimeout)
        {
            await BaseScript.Delay(0);
        }
    }

    private static void ReadVehicle(int ped, out int vehicle, out int seat)
    {
        vehicle = 0;
        seat = NoSeat;

        // The false is "not while climbing in", so a ped still walking towards a vehicle counts as on foot.
        if (!API.IsPedInAnyVehicle(ped, false))
        {
            return;
        }

        var handle = API.GetVehiclePedIsIn(ped, false);

        if (handle == 0 || !API.DoesEntityExist(handle))
        {
            return;
        }

        vehicle = handle;
        seat = FindSeat(handle, ped);
    }

    private static int FindSeat(int vehicle, int ped)
    {
        if (NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false) == ped)
        {
            return DriverSeat;
        }

        // Counts the driver's seat as one of them, so the last passenger index is two below the total.
        var seats = API.GetVehicleModelNumberOfSeats(NativeFixer.GetEntityModel(vehicle));

        for (var seat = 0; seat <= seats - 2; seat++)
        {
            if (NativeFixer.GetPedInVehicleSeat(vehicle, seat, false) == ped)
            {
                return seat;
            }
        }

        return NoSeat;
    }
}
