using CitizenFX.FiveM.Server;

namespace vMenu.Enhanced.Updates.Server.Http;

internal static class HttpWait
{
    private const int PollMs = 100;

    // Waits on the thread the tick runs on, which is the only one that may call natives. No
    // TaskCompletionSource, deliberately: the request completes from a thread pool continuation, and
    // nothing here says where a TCS continuation would resume. Everything the caller does afterwards
    // calls a native, so awaiting API.Delay is what guarantees the tick thread, and it doubles as the
    // timeout, so a hung request is one mechanism not two.
    public static async Task<HttpReply> ForAsync(HttpSlot slot, int timeoutMs)
    {
        // GetGameTimer because this loop only ever runs on the tick thread, where it is free, and it is the
        // same clock API.Delay is measured against.
        var startedAt = Native.GetGameTimer();

        while (!slot.Done)
        {
            if (Native.GetGameTimer() - startedAt >= timeoutMs)
            {
                return HttpReply.TimedOut(timeoutMs);
            }

            await API.Delay(PollMs);
        }

        return slot.Reply!;
    }
}
