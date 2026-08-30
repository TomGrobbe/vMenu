using System.Runtime.CompilerServices;

namespace vMenu.Enhanced.Data.Ticks;

// This exists because running certain natives off the main thread will cause the game to crash.
// Reported to CFX, this is a workaround until they can properly fix it.

public readonly struct MainThreadHop(Func<bool> isMainThread, Action<Action> scheduleOnMainThread)
{
    public Awaiter GetAwaiter() => new(isMainThread, scheduleOnMainThread);

    public readonly struct Awaiter(Func<bool> isMainThread, Action<Action> scheduleOnMainThread)
        : ICriticalNotifyCompletion
    {
        public bool IsCompleted => isMainThread();

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation) => scheduleOnMainThread(continuation);

        public void UnsafeOnCompleted(Action continuation) => scheduleOnMainThread(continuation);
    }
}
