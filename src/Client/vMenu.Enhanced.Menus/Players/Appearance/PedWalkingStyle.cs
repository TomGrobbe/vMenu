using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// How the player walks, which survives a change of ped and a change of server.
/// </summary>
/// <remarks>
/// Legacy only offered this on the two freemode peds, because it swapped in animation dictionaries
/// picked by gender and there was no sensible answer for anything else. A movement clip set is one
/// name that the game applies to whatever ped you are wearing, so the restriction goes.
///
/// <para>
/// It does not follow that every clip set suits every ped. One authored for a human skeleton on an
/// animal will not load, which is why the wait below has an answer for failure rather than assuming
/// success.
/// </para>
/// </remarks>
public static class PedWalkingStyle
{
    /// <summary>How long to wait for the animations before giving up on a style.</summary>
    // Bounded because this sits behind a menu row. A clip set that never loads would otherwise leave
    // that row unusable for the rest of the session.
    private const int LoadTimeout = 1000;

    /// <summary>How quickly the new walk blends in. One second looks deliberate rather than abrupt.</summary>
    private const float BlendSeconds = 1f;

    /// <summary>The clip set in use, or empty for the walk the ped came with.</summary>
    public static string Current => UserDefaults.PlayerWalkingStyle.Value;

    /// <summary>Puts a walk on the player and remembers it.</summary>
    /// <returns>False when the game has no animations under that name for this ped.</returns>
    public static async Task<bool> ApplyAsync(string clipset)
    {
        var ped = Native.PlayerPedId();

        if (string.IsNullOrWhiteSpace(clipset))
        {
            Native.ResetPedMovementClipset(ped, 0f);

            UserDefaults.PlayerWalkingStyle.Value = string.Empty;

            return true;
        }

        if (!await LoadAsync(clipset))
        {
            return false;
        }

        Native.SetPedMovementClipset(ped, clipset, BlendSeconds);

        UserDefaults.PlayerWalkingStyle.Value = clipset;

        return true;
    }

    /// <summary>
    /// Puts the remembered walk back on. A new ped does not inherit it, so this runs after every
    /// model change rather than only when the player picks one.
    /// </summary>
    public static async Task ReapplyAsync()
    {
        if (Current is not { Length: > 0 } clipset)
        {
            return;
        }

        // Deliberately not reported and deliberately not forgotten. A walk that suits the ped the
        // player just left but not the one they just put on should still be there when they change
        // back, and a notification every time they change ped would be noise.
        if (await LoadAsync(clipset))
        {
            Native.SetPedMovementClipset(Native.PlayerPedId(), clipset, BlendSeconds);
        }
    }

    private static async Task<bool> LoadAsync(string clipset)
    {
        Native.RequestClipSet(clipset);

        var started = Native.GetGameTimer();

        while (!Native.HasClipSetLoaded(clipset))
        {
            if (Native.GetGameTimer() - started > LoadTimeout)
            {
                return false;
            }

            await API.Delay(0);
        }

        return true;
    }
}
