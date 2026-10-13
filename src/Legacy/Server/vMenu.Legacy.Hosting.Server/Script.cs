using CitizenFX.Core;

using vMenu.Enhanced.Logging;

namespace vMenu.Legacy.Hosting;

public static class Script
{
    private static readonly object QueueLock = new();

    private static readonly Queue<Action> MainThreadQueue = new();

    private static Action<Func<Task>>? _addTick;

    private static Action<Func<Task>>? _removeTick;

    private static int _mainThreadId;

    public static EventHandlerDictionary EventHandlers { get; private set; } = null!;

    public static ExportDictionary Exports { get; private set; } = null!;

    public static StateBag GlobalState { get; private set; } = null!;

    public static PlayerList Players { get; private set; } = null!;

    public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

    public static void Attach(
        EventHandlerDictionary eventHandlers,
        ExportDictionary exports,
        StateBag globalState,
        PlayerList players,
        Action<Func<Task>> addTick,
        Action<Func<Task>> removeTick)
    {
        EventHandlers = eventHandlers;
        Exports = exports;
        GlobalState = globalState;
        Players = players;
        _addTick = addTick;
        _removeTick = removeTick;
        _mainThreadId = Thread.CurrentThread.ManagedThreadId;

        addTick(DrainMainThreadQueue);
    }

    public static int ServerId(this Player player) => int.Parse(player.Handle);

    public static bool IsFromPlayer(string? source) => source is not null && source.StartsWith("net:", StringComparison.Ordinal);

    public static int SourceId(string? source) =>
        source is not null && int.TryParse(source.Substring(source.LastIndexOf(':') + 1), out var id) ? id : 0;

    public static void TriggerClientEvent(int target, string eventName, params object?[] args)
    {
        if (target == -1)
        {
            BaseScript.TriggerClientEvent(eventName, args);

            return;
        }

        if (Players[target] is { } player)
        {
            BaseScript.TriggerClientEvent(player, eventName, args);
        }
    }

    public static void TriggerClientEvent(Player target, string eventName, params object?[] args) =>
        BaseScript.TriggerClientEvent(target, eventName, args);

    public static void TriggerLatentClientEvent(int target, int bytesPerSecond, string eventName, params object?[] args)
    {
        if (target == -1)
        {
            BaseScript.TriggerLatentClientEvent(eventName, bytesPerSecond, args);

            return;
        }

        if (Players[target] is { } player)
        {
            BaseScript.TriggerLatentClientEvent(player, eventName, bytesPerSecond, args);
        }
    }

    public static void AddTick(Func<Task> tick) => _addTick?.Invoke(tick);

    public static void RemoveTick(Func<Task> tick) => _removeTick?.Invoke(tick);

    public static void RunOnMainThread(Action action)
    {
        if (IsMainThread)
        {
            action();

            return;
        }

        NextTick(action);
    }

    public static void NextTick(Action action)
    {
        lock (QueueLock)
        {
            MainThreadQueue.Enqueue(action);
        }
    }

    private static Task DrainMainThreadQueue()
    {
        while (true)
        {
            Action next;

            lock (QueueLock)
            {
                if (MainThreadQueue.Count == 0)
                {
                    break;
                }

                next = MainThreadQueue.Dequeue();
            }

            try
            {
                next();
            }
            catch (Exception exception)
            {
                Log.Error($"[Script] A queued main thread call threw: {exception}");
            }
        }

        return Task.CompletedTask;
    }
}
