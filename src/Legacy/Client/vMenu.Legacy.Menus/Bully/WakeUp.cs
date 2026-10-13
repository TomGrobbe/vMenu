using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Bully;

internal static class WakeUp
{
    private const string Dictionary = "get_up@standard";

    private const string Clip = "front";

    private const int Flags = 8 | 131072 | 1048576;

    private const int HoldMs = 2000;

    private const int FadeMs = 3000;

    private const int AfterFade = 7;

    private static bool _covered;

    public static Task<bool> LoadAsync() => Streaming.AnimDictAsync(Dictionary);

    public static void Release() => API.RemoveAnimDict(Dictionary);

    public static async Task CoverAsync()
    {
        await FadeAsync(fadeIn: true);

        _covered = true;

        BullyTask.Run(HoldCoverAsync, "WakeUp.HoldCover");
    }

    public static void Uncover() => _covered = false;

    public static async Task CancelAsync()
    {
        _covered = false;

        await FadeAsync(fadeIn: false);
    }

    public static async Task RevealAsync(bool animated)
    {
        var ped = API.PlayerPedId();

        // Left unfrozen so the ped settles onto the ground while the screen is still white, not after it gets up.
        API.FreezeEntityPosition(ped, false);

        if (animated)
        {
            NativeFixer.TaskPlayAnim(ped, Dictionary, Clip, 8f, -8f, -1, Flags, 0f, false, 0, false);
        }

        var holdUntil = API.GetGameTimer() + HoldMs;

        while (API.GetGameTimer() < holdUntil)
        {
            if (animated)
            {
                API.SetEntityAnimCurrentTime(ped, Dictionary, Clip, 0f);
            }

            await BaseScript.Delay(0);
        }

        _covered = false;

        await FadeAsync(fadeIn: false);
    }

    private static async Task FadeAsync(bool fadeIn)
    {
        var endsAt = API.GetGameTimer() + FadeMs;

        while (API.GetGameTimer() < endsAt)
        {
            var progress = 1f - (endsAt - API.GetGameTimer()) / (float)FadeMs;
            var alpha = (int)(255 * (fadeIn ? progress : 1f - progress));

            Draw(Math.Clamp(alpha, 0, 255));

            await BaseScript.Delay(0);
        }
    }

    private static async Task HoldCoverAsync()
    {
        while (_covered)
        {
            Draw(255);

            await BaseScript.Delay(0);
        }
    }

    private static void Draw(int alpha)
    {
        API.SetScriptGfxDrawOrder(AfterFade);
        NativeFixer.DrawRect(0.5f, 0.5f, 3f, 3f, 255, 255, 255, alpha, false);
    }
}
