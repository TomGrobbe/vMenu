using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Weapons;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.Menus.Players;

public static class PedSpawning
{
    private const int DriverSeat = -1;

    private const int NoSeat = -2;

    /// <summary>How long to keep trying to put the new ped back in the seat the old one was in.</summary>
    private const int WarpTimeout = 1000;

    private const int HeadBlendTimeout = 1000;

    private static readonly uint FreemodeMale = API.Hash("mp_m_freemode_01");

    private static readonly uint FreemodeFemale = API.Hash("mp_f_freemode_01");

    /// <summary>
    /// One of the two models the online character creator builds on.
    /// </summary>
    /// <remarks>
    /// These carry a face, hair colour, overlays and tattoos that live nowhere near the clothes, and
    /// vMenu has no way to read or write any of it yet. Everything that would only half work on one
    /// of them asks here first.
    /// </remarks>
    public static bool IsFreemode(uint hash) => hash == FreemodeMale || hash == FreemodeFemale;

    public static bool IsFreemodeMale(uint hash) => hash == FreemodeMale;

    public static uint FreemodeModel(bool male) => male ? FreemodeMale : FreemodeFemale;

    /// <summary>Whether the player is wearing one right now.</summary>
    public static bool IsWearingFreemode() =>
        IsFreemode((uint)Native.GetEntityModel(Native.PlayerPedId()));

    /// <summary>Whether a model name is a ped this client could actually turn into.</summary>
    public static bool IsSpawnable(string modelName) => IsSpawnable(API.Hash(modelName));

    /// <inheritdoc cref="IsSpawnable(string)"/>
    public static bool IsSpawnable(uint hash) =>
        Native.IsModelValid(hash) && Native.IsModelInCdimage(hash) && Native.IsModelAPed(hash);

    /// <summary>Whether this client may turn into a model that did not come from a row of its own.</summary>
    // The saved peds menu is not a way around a restricted ped list, so the ped list's own rules
    // still apply: a whitelisted model needs its own permission, and a listed one needs its category.
    // A model in neither is one the owner never restricted, so nothing stands in the way of it.
    public static bool IsPermitted(string modelName)
    {
        if (ClientPedPermissions.IsWhitelisted(modelName))
        {
            return ClientPedPermissions.CanSpawnPed(modelName, string.Empty);
        }

        return PedModelSync.Find(modelName) is not { } known
            || ClientPedPermissions.CanSpawnCategory(known.Category);
    }

    /// <summary>Swaps the player onto a model and resets its appearance to the game's defaults.</summary>
    /// <returns>False when the model is not a ped, or is not streamed in on this client.</returns>
    public static async Task<bool> SetPlayerModelAsync(string modelName) =>
        await SetPlayerModelAsync(API.Hash(modelName));

    /// <inheritdoc cref="SetPlayerModelAsync(string)"/>
    // By hash as well as by name, because a saved ped only ever stored the hash. The game has no
    // reverse lookup for a ped model, so a save whose model the owner never listed has no name left
    // to spawn from and would otherwise be stuck in the collection unusable.
    public static async Task<bool> SetPlayerModelAsync(uint hash)
    {
        if (!IsSpawnable(hash))
        {
            return false;
        }

        // Requested by hand rather than through a helper, for the same reason vehicle spawning does
        // it: the convenience wrappers use DateTime, which is currently broken and crashes the game.
        // https://github.com/citizenfx/rfc/discussions/328
        Native.RequestModel(hash);

        while (!Native.HasModelLoaded(hash))
        {
            await API.Delay(0);
        }

        // Only when it is not already the model being worn. Swapping to the same one throws the ped
        // out of whatever it was doing for no gain.
        if ((uint)Native.GetEntityModel(Native.PlayerPedId()) != hash)
        {
            // Read before the swap and handed back after it, alongside the health and armour
            // SwapAsync already carries over. Nothing is raised until the swap is done, by which
            // point the ped holding the weapons is gone, so this is the last chance to read them.
            var carried = WeaponCarryOver.Capture();

            await SwapAsync(hash);

            await WeaponCarryOver.RestoreAsync(carried);
        }

        await ResetAppearanceAsync(hash);

        PedKeepProps.Apply(Native.PlayerPedId());

        // A new ped walks the way its model walks, so the choice the player made has to be put back
        // on by hand. A saved ped applied on top of this brings its own and wins, which is right.
        await PedWalkingStyle.ReapplyAsync();

        Native.SetModelAsNoLongerNeeded(hash);

        return true;
    }

    /// <summary>
    /// The swap itself. Health, armour and the seat the player was in do not survive a model change
    /// on their own, so they are read off the old ped and written back onto the new one.
    /// </summary>
    private static async Task SwapAsync(uint hash)
    {
        var ped = Native.PlayerPedId();
        var player = Native.PlayerId();

        var maxHealth = Native.GetEntityMaxHealth(ped);
        var health = Native.GetEntityHealth(ped);
        var maxArmour = Native.GetPlayerMaxArmour(player);
        var armour = Native.GetPedArmour(ped);

        ReadVehicle(ped, out var vehicle, out var seat);

        Native.SetPlayerModel(player, hash);

        // A new ped, so everything below has to find it again rather than reuse the handle above.
        ped = Native.PlayerPedId();

        Native.SetPlayerMaxArmour(player, maxArmour);
        Native.SetEntityMaxHealth(ped, maxHealth);
        // The last two are who did it and with what, which for putting health back is nobody.
        Native.SetEntityHealth(ped, health, 0, 0);
        Native.SetPedArmour(ped, armour);

        if (vehicle != 0 && seat != NoSeat)
        {
            await WarpBackAsync(ped, vehicle, seat);
        }
    }

    /// <summary>
    /// The game drops the player next to the vehicle rather than into it, so the seat is asked for
    /// again until it takes. Frozen meanwhile, otherwise the ped walks off while being asked.
    /// </summary>
    private static async Task WarpBackAsync(int ped, int vehicle, int seat)
    {
        if (!Native.DoesEntityExist(vehicle))
        {
            return;
        }

        Native.FreezeEntityPosition(ped, true);

        var started = Native.GetGameTimer();

        while (!Native.IsPedInVehicle(ped, vehicle, false))
        {
            if (Native.GetGameTimer() - started > WarpTimeout)
            {
                break;
            }

            Native.ClearPedTasks(ped);

            await API.Delay(0);

            Native.TaskWarpPedIntoVehicle(ped, vehicle, seat);
        }

        Native.FreezeEntityPosition(ped, false);
    }

    /// <summary>
    /// The clean default look: whatever the model ships with, and nothing left over from the ped
    /// that was worn before.
    /// </summary>
    private static async Task ResetAppearanceAsync(uint hash)
    {
        var ped = Native.PlayerPedId();

        Native.SetPedDefaultComponentVariation(ped);
        Native.ClearAllPedProps(ped, false);
        Native.ClearPedDecorations(ped);
        Native.ClearPedFacialDecorations(ped);

        if (!IsFreemode(hash))
        {
            return;
        }

        // The two freemode models have no face of their own and spawn grey and untextured without
        // this. Zeroes with an even blend between them is the game's own neutral starting point.
        Native.SetPedHeadBlendData(ped, 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 0f, false);

        // Bounded, because this sits inside a menu row's handler and a blend that never reports back
        // would leave that row unusable for the rest of the session.
        var started = Native.GetGameTimer();

        while (!Native.HasPedHeadBlendFinished(ped) && Native.GetGameTimer() - started < HeadBlendTimeout)
        {
            await API.Delay(0);
        }
    }

    private static void ReadVehicle(int ped, out int vehicle, out int seat)
    {
        vehicle = 0;
        seat = NoSeat;

        // The false is "not while climbing in", so a ped still walking towards a vehicle counts as
        // being on foot.
        if (!Native.IsPedInAnyVehicle(ped, false))
        {
            return;
        }

        var handle = Native.GetVehiclePedIsIn(ped, false);

        if (handle == 0 || !Native.DoesEntityExist(handle))
        {
            return;
        }

        vehicle = handle;
        seat = FindSeat(handle, ped);
    }

    private static int FindSeat(int vehicle, int ped)
    {
        if (Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) == ped)
        {
            return DriverSeat;
        }

        // Counts the driver's seat as one of them, so the last passenger index is two below the total.
        var seats = Native.GetVehicleModelNumberOfSeats((uint)Native.GetEntityModel(vehicle));

        for (var seat = 0; seat <= seats - 2; seat++)
        {
            if (Native.GetPedInVehicleSeat(vehicle, seat, false) == ped)
            {
                return seat;
            }
        }

        return NoSeat;
    }
}
