using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Storage;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// Legacy only offered this on the two freemode peds, because it swapped in animation dictionaries
// picked by gender. A movement clip set is one name the game applies to whatever ped you are
// wearing, so the restriction goes. It does not follow that every clip set suits every ped: one
// authored for a human skeleton will not load on an animal, which is why the wait below can fail.
public static class PedWalkingStyle
{
    // Bounded because this sits behind a menu row. A clip set that never loads would otherwise leave
    // that row unusable for the rest of the session.
    private const int LoadTimeout = 1000;

    // One second looks deliberate rather than abrupt.
    private const float BlendSeconds = 1f;

    // The clip set in use, or empty for the walk the ped came with.
    public static string Current => UserDefaults.PlayerWalkingStyle.Value;

    // False when the game has no animations under that name for this ped.
    public static async Task<bool> ApplyAsync(string clipset)
    {
        var ped = API.PlayerPedId();

        if (string.IsNullOrWhiteSpace(clipset))
        {
            if (!PlayerIntoxication.HoldsWalk)
            {
                API.ResetPedMovementClipset(ped, 0f);
            }

            UserDefaults.PlayerWalkingStyle.Value = string.Empty;

            return true;
        }

        if (!await LoadAsync(clipset))
        {
            return false;
        }

        if (!PlayerIntoxication.HoldsWalk)
        {
            API.SetPedMovementClipset(ped, clipset, BlendSeconds);
        }

        UserDefaults.PlayerWalkingStyle.Value = clipset;

        return true;
    }

    // A new ped does not inherit the remembered walk, so this runs after every model change rather than
    // only when the player picks one.
    public static async Task ReapplyAsync()
    {
        if (PlayerIntoxication.HoldsWalk || Current is not { Length: > 0 } clipset)
        {
            return;
        }

        // Deliberately not reported and deliberately not forgotten. A walk that suits the ped the player
        // just left but not the one they just put on should still be there when they change back.
        if (await LoadAsync(clipset))
        {
            API.SetPedMovementClipset(API.PlayerPedId(), clipset, BlendSeconds);
        }
    }

    private static async Task<bool> LoadAsync(string clipset)
    {
        API.RequestClipSet(clipset);

        var started = API.GetGameTimer();

        while (!API.HasClipSetLoaded(clipset))
        {
            if (API.GetGameTimer() - started > LoadTimeout)
            {
                return false;
            }

            await BaseScript.Delay(0);
        }

        return true;
    }
}
