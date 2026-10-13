using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives.Server;

namespace vMenu.Enhanced.Http.Server;

internal static class HttpWait
{
    private const int PollMs = 100;

    // Waits on the thread the tick runs on, which is the only one that may call natives. No
    // TaskCompletionSource, deliberately: the request completes from a thread pool continuation, and
    // nothing here says where a TCS continuation would resume. Everything the caller does afterwards
    // calls a native, so awaiting BaseScript.Delay is what guarantees the tick thread, and it doubles as the
    // timeout, so a hung request is one mechanism not two.
    public static async Task<HttpReply> ForAsync(HttpSlot slot, int timeoutMs)
    {
        // GetGameTimer because this loop only ever runs on the tick thread, where it is free, and it is the
        // same clock BaseScript.Delay is measured against.
        var startedAt = NativeFixer.GetGameTimer();

        while (!slot.Done)
        {
            if (NativeFixer.GetGameTimer() - startedAt >= timeoutMs)
            {
                return HttpReply.TimedOut(timeoutMs);
            }

            await BaseScript.Delay(PollMs);
        }

        return slot.Reply!;
    }
}
