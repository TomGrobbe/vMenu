using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// Flips the visor on a motorcycle helmet, with the animation the game has for it.
/// </summary>
/// <remarks>
/// A helmet with a visor is really two hats, one with it up and one with it down, and flipping it is
/// swapping between them halfway through an animation of the player reaching up. The game knows which
/// two go together, which is what <c>GetVariantProp</c> answers.
///
/// <para>
/// Nothing here is cached. Another resource can put a different hat on the player at any moment, so
/// every question is asked again at the moment the key is released rather than remembered from the
/// last time somebody looked.
/// </para>
/// </remarks>
public static class VisorToggle
{
    /// <summary>How long to wait for the animation to start before giving up on it.</summary>
    private const int StartTimeout = 1000;

    /// <summary>How long to let it run before deciding it is stuck.</summary>
    private const int RunTimeout = 3000;

    /// <summary>How far into the animation the player's hand reaches the visor.</summary>
    // Swapping the hat at the start would have it change before they touch it, and at the end would
    // have their hand come away before anything moved.
    private const float SwapPoint = 0.39f;

    /// <summary>The camera mode the game reports when the player is looking out of their own eyes.</summary>
    private const int FirstPersonView = 4;

    private const float BlendIn = 8f;

    private const float BlendOut = 1f;

    /// <summary>Upper body only, so the player keeps walking or riding while they do it.</summary>
    private const int UpperBodyLoop = 48;

    private static bool _running;

    /// <summary>Whether now is a sensible moment to play an animation on the player.</summary>
    public static bool CanRunNow() =>
        !MenuController.IsAnyMenuOpen()
        && !Native.IsPauseMenuActive()
        && !Native.IsPlayerSwitchInProgress()
        && Native.IsScreenFadedIn()
        && !Native.IsPedDeadOrDying(Native.PlayerPedId(), true);

    /// <summary>Flips the visor, if the player is wearing a helmet that has one.</summary>
    public static async Task ToggleAsync()
    {
        // One at a time. The animation swaps the hat partway through, and two of them overlapping
        // would each be swapping to what the other just put on.
        // Permission check is there so that if the player has no permissions at all,
        // it won't enable this feature. If they have any permission for the menu, then
        // they're allowed to use this feature.
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

        // No alternate version means a hat rather than a helmet with a visor. Nothing to say about it:
        // the player pressed a key that does not apply, which is not an error.
        if (Native.GetShopPedApparelVariantPropCount(helmet) <= 0)
        {
            return;
        }

        Native.GetVariantProp(helmet, 0, out var altDrawable, out var altTexture, out _);

        var model = (uint)Native.GetEntityModel(ped);
        var goggles = VisorAnimations.IsGoggles(model, drawable);

        var vehicle = Native.IsPedInAnyVehicle(ped, false) ? Native.GetVehiclePedIsIn(ped, false) : 0;

        if (goggles && vehicle != 0)
        {
            // The game has no goggles animation for somebody sitting on a bike, and playing the visor
            // one instead has the player reach for a visor that is not there.
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

            // A handful of helmets have their two versions the other way round, so the same comparison
            // would have the player pull a visor down to raise it.
            : Direction(VisorAnimations.IsInverted(model, drawable) ? drawable > altDrawable : drawable < altDrawable, "visor");

        if (Native.GetFollowPedCamViewMode() != FirstPersonView)
        {
            return name;
        }

        // There is no first person goggles animation, so it borrows the visor one, which from behind
        // the player's own eyes looks near enough the same.
        return "pov_" + (goggles ? name.Replace("goggles", "visor") : name);
    }

    private static string Direction(bool up, string what) => $"{what}_{(up ? "up" : "down")}";

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

    /// <summary>
    /// Plays the reach, and swaps the helmet for its other half at the moment the player's hand gets
    /// there.
    /// </summary>
    // Both waits are bounded, because this is reached from a key the player is holding and one that
    // never came back would leave that key doing nothing for the rest of the session.
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

        // A run that timed out never reached the swap point, so the helmet has to be put on anyway,
        // otherwise the player watched an animation that did nothing.
        if (!swapped)
        {
            Native.SetPedPropIndex(ped, PedPropSlots.Hats, drawable, texture, true, false);
        }

        Native.ClearPedTasks(ped);
    }
}
