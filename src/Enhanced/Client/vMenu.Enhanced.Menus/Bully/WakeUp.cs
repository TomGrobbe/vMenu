using CitizenFX.FiveM.Client;

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

    public static void Release() => Native.RemoveAnimDict(Dictionary);

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
        var ped = Native.PlayerPedId();

        // Left unfrozen so the ped settles onto the ground while the screen is still white, not after it gets up.
        Native.FreezeEntityPosition(ped, false);

        if (animated)
        {
            Native.TaskPlayAnim(ped, Dictionary, Clip, 8f, -8f, -1, Flags, 0f, false, 0, false);
        }

        var holdUntil = Native.GetGameTimer() + HoldMs;

        while (Native.GetGameTimer() < holdUntil)
        {
            if (animated)
            {
                Native.SetEntityAnimCurrentTime(ped, Dictionary, Clip, 0f);
            }

            await API.Delay(0);
        }

        _covered = false;

        await FadeAsync(fadeIn: false);
    }

    private static async Task FadeAsync(bool fadeIn)
    {
        var endsAt = Native.GetGameTimer() + FadeMs;

        while (Native.GetGameTimer() < endsAt)
        {
            var progress = 1f - (endsAt - Native.GetGameTimer()) / (float)FadeMs;
            var alpha = (int)(255 * (fadeIn ? progress : 1f - progress));

            Draw(Math.Clamp(alpha, 0, 255));

            await API.Delay(0);
        }
    }

    private static async Task HoldCoverAsync()
    {
        while (_covered)
        {
            Draw(255);

            await API.Delay(0);
        }
    }

    private static void Draw(int alpha)
    {
        Native.SetScriptGfxDrawOrder(AfterFade);
        Native.DrawRect(0.5f, 0.5f, 3f, 3f, 255, 255, 255, alpha, false);
    }
}
