using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// A helmet with a visor is really two hats, one with it up and one with it down, and flipping it is
// swapping between them halfway through an animation of the player reaching up. GetVariantProp is
// what says which two go together. Nothing here is cached, because another resource can put a
// different hat on the player at any moment.
public static class VisorToggle
{
    private const int StartTimeout = 1000;

    private const int RunTimeout = 3000;

    // How far into the animation the player's hand reaches the visor. Swapping at the start would have
    // the hat change before they touch it, and at the end after their hand came away.
    private const float SwapPoint = 0.39f;

    // The camera mode the game reports when the player is looking out of their own eyes.
    private const int FirstPersonView = 4;

    private const int PropApparel = 1;

    private const int NoVariant = 1849449579;

    private const float BlendIn = 8f;

    private const float BlendOut = 1f;

    // Upper body only, so the player keeps walking or riding while they do it.
    private const int UpperBodyLoop = 48;

    private static bool _running;

    public static bool CanRunNow() =>
        !MenuController.IsAnyMenuOpen()
        && !Native.IsPauseMenuActive()
        && !Native.IsPlayerSwitchInProgress()
        && Native.IsScreenFadedIn()
        && !Native.IsPedDeadOrDying(Native.PlayerPedId(), true);

    public static bool HasVisor(int ped)
    {
        var drawable = Native.GetPedPropIndex(ped, PedPropSlots.Hats, false);

        if (drawable < 0)
        {
            return false;
        }

        var texture = Native.GetPedPropTextureIndex(ped, PedPropSlots.Hats);
        var prop = (uint)Native.GetHashNameForProp(ped, PedPropSlots.Hats, drawable, texture);

        if (Native.GetShopPedApparelVariantPropCount(prop) <= 0)
        {
            return false;
        }

        return VisorAnimations.IsGoggles((uint)Native.GetEntityModel(ped), drawable)
            || Tagged(prop, "HELMET")
            || Tagged(prop, "FULL_FACE")
            || Tagged(prop, "DOME_HELMET");
    }

    private static bool Tagged(uint prop, string tag) =>
        Native.DoesShopPedApparelHaveRestrictionTag(prop, (uint)Native.GetHashKey(tag), PropApparel);

    public static async Task ToggleAsync()
    {
        // One at a time. The animation swaps the hat partway through, and two of them overlapping would each
        // be swapping to what the other just put on. Any permission at all for the menu is enough to use it,
        // so a player with none does not get the feature.
        if (_running || !ClientPermissions.HasAnyPermission || !CanRunNow())
        {
            return;
        }

        _running = true;

        try
        {
            await FlipAsync();
        }
        finally
        {
            _running = false;
        }
    }

    private static async Task FlipAsync()
    {
        var ped = Native.PlayerPedId();

        var drawable = Native.GetPedPropIndex(ped, PedPropSlots.Hats, false);

        if (drawable < 0)
        {
            return;
        }

        var texture = Native.GetPedPropTextureIndex(ped, PedPropSlots.Hats);
        var helmet = (uint)Native.GetHashNameForProp(ped, PedPropSlots.Hats, drawable, texture);

        if (!HasVisor(ped))
        {
            return;
        }

        Native.GetVariantProp(helmet, 0, out var altName, out _, out _);

        if (altName is 0 or NoVariant)
        {
            return;
        }

        var alt = new ShopPedPropBuffer();

        Native.GetShopPedProp((uint)altName, alt);

        await NextFrameAsync();

        var altDrawable = alt.Drawable;
        var altTexture = alt.Texture;

        if (altDrawable < 0)
        {
            return;
        }

        var model = (uint)Native.GetEntityModel(ped);
        var goggles = VisorAnimations.IsGoggles(model, drawable);

        var vehicle = Native.IsPedInAnyVehicle(ped, false) ? Native.GetVehiclePedIsIn(ped, false) : 0;

        if (goggles && vehicle != 0)
        {
            // The game has no goggles animation for somebody sitting on a bike, and playing the visor one instead
            // has the player reach for a visor that is not there.
            Notifications.Info(MenuText.Key(Loc.PlayerAppearance.VisorGogglesInVehicle));

            return;
        }

        var animation = AnimationName(model, drawable, altDrawable, goggles);
        var dictionary = vehicle != 0 ? VisorAnimations.ForVehicle(vehicle) : VisorAnimations.OnFoot;

        // Only the on foot animations have a first person version of their own.
        if (animation.StartsWith("pov_", StringComparison.Ordinal)
            && !string.Equals(dictionary, VisorAnimations.OnFoot, StringComparison.Ordinal))
        {
            animation = animation[4..];
        }

        if (!await LoadAsync(dictionary))
        {
            return;
        }

        await PlayAsync(ped, dictionary, animation, altDrawable, altTexture);

        Native.RemoveAnimDict(dictionary);
    }

    private static string AnimationName(uint model, int drawable, int altDrawable, bool goggles)
    {
        var name = goggles
            ? Direction(drawable < altDrawable, "goggles")

            // A handful of helmets have their two versions the other way round, so the same comparison would have
            // the player pull a visor down to raise it.
            : Direction(VisorAnimations.IsInverted(model, drawable) ? drawable > altDrawable : drawable < altDrawable, "visor");

        if (Native.GetFollowPedCamViewMode() != FirstPersonView)
        {
            return name;
        }

        // There is no first person goggles animation, so it borrows the visor one, which from behind the
        // player's own eyes looks near enough the same.
        return "pov_" + (goggles ? name.Replace("goggles", "visor") : name);
    }

    private static string Direction(bool up, string what) => $"{what}_{(up ? "up" : "down")}";

    private static async Task NextFrameAsync()
    {
        var asked = Native.GetFrameCount();

        while (Native.GetFrameCount() == asked)
        {
            await API.Delay(0);
        }
    }

    private static async Task<bool> LoadAsync(string dictionary)
    {
        if (Native.HasAnimDictLoaded(dictionary))
        {
            return true;
        }

        Native.RequestAnimDict(dictionary);

        var started = Native.GetGameTimer();

        while (!Native.HasAnimDictLoaded(dictionary))
        {
            if (Native.GetGameTimer() - started > StartTimeout)
            {
                return false;
            }

            await API.Delay(0);
        }

        return true;
    }

    // Both waits are bounded, because this is reached from a key the player is holding and one that never
    // came back would leave that key doing nothing for the rest of the session.
    private static async Task PlayAsync(int ped, string dictionary, string animation, int drawable, int texture)
    {
        Native.ClearPedTasks(ped);

        Native.TaskPlayAnim(ped, dictionary, animation, BlendIn, BlendOut, -1, UpperBodyLoop, 0f, false, 0, false);

        var started = Native.GetGameTimer();

        while (Native.GetEntityAnimCurrentTime(ped, dictionary, animation) <= 0f)
        {
            if (Native.GetGameTimer() - started > StartTimeout)
            {
                Native.ClearPedTasks(ped);

                return;
            }

            await API.Delay(0);
        }

        var swapped = false;

        started = Native.GetGameTimer();

        while (Native.GetEntityAnimCurrentTime(ped, dictionary, animation) > 0f)
        {
            if (Native.GetGameTimer() - started > RunTimeout)
            {
                break;
            }

            if (!swapped && Native.GetEntityAnimCurrentTime(ped, dictionary, animation) > SwapPoint)
            {
                swapped = true;

                Native.SetPedPropIndex(ped, PedPropSlots.Hats, drawable, texture, true, false);
            }

            await API.Delay(0);
        }

        // A run that timed out never reached the swap point, so the helmet has to be put on anyway, otherwise
        // the player watched an animation that did nothing.
        if (!swapped)
        {
            Native.SetPedPropIndex(ped, PedPropSlots.Hats, drawable, texture, true, false);
        }

        Native.ClearPedTasks(ped);
    }
}
