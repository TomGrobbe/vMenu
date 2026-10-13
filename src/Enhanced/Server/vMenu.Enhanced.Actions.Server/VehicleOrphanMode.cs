using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Ticks.Server;

using VehicleSpawnerSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleSpawner;

namespace vMenu.Enhanced.Actions.Server;

public static class VehicleOrphanMode
{
    private const int VehicleEntityType = 2;

    private const long DrainIntervalMs = 250;

    private const long GiveUpAfterMs = 2000;

    private const int MaxWaitingPerPlayer = 64;

    private static readonly Dictionary<int, List<Pending>> Waiting = [];

    private static readonly List<int> Emptied = [];

    private static bool _registered;

    private static bool _reportedUnknown;

    private static bool _reportedFull;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        ServerTickRegistry.Register(
            "Vehicles.OrphanMode",
            Drain,
            TickRate.Every(DrainIntervalMs),
            () => Waiting.Count > 0);
    }

    public static void Queue(int serverId, int networkId)
    {
        if (networkId == 0 || Configured() == VehicleSpawnerSettings.DeleteWhenNotRelevant)
        {
            return;
        }

        if (!Waiting.TryGetValue(serverId, out var pending))
        {
            pending = [];

            Waiting[serverId] = pending;
        }

        pending.Add(new Pending(networkId, Native.GetGameTimer() + GiveUpAfterMs));

        while (pending.Count > MaxWaitingPerPlayer)
        {
            if (!_reportedFull)
            {
                _reportedFull = true;

                Log.Warning(
                    $"[Vehicles] A player has more than {MaxWaitingPerPlayer} vehicles waiting for an orphan mode "
                    + "at once. Dropping their oldest ones.");
            }

            pending.RemoveAt(0);
        }

        ServerTickRegistry.Reevaluate();
    }

    private static void Drain()
    {
        var mode = Configured();
        var now = Native.GetGameTimer();

        Emptied.Clear();

        foreach (var pair in Waiting)
        {
            var pending = pair.Value;

            for (var index = pending.Count - 1; index >= 0; index--)
            {
                var waiting = pending[index];
                var entity = Native.NetworkGetEntityFromNetworkId(waiting.NetworkId);

                if (entity != 0 && Native.DoesEntityExist(entity) && Native.GetEntityType(entity) == VehicleEntityType)
                {
                    Native.SetEntityOrphanMode(entity, mode);

                    pending.RemoveAt(index);

                    continue;
                }

                if (now >= waiting.Deadline)
                {
                    Log.Debug(
                        $"[Vehicles] Network id {waiting.NetworkId} never reached the server, "
                        + "so it keeps the default orphan mode.");

                    pending.RemoveAt(index);
                }
            }

            if (pending.Count == 0)
            {
                Emptied.Add(pair.Key);
            }
        }

        foreach (var serverId in Emptied)
        {
            Waiting.Remove(serverId);
        }

        Emptied.Clear();

        if (Waiting.Count == 0)
        {
            ServerTickRegistry.Reevaluate();
        }
    }

    private static int Configured()
    {
        var mode = ServerConfig.Value(VehicleSpawnerSettings.OrphanMode);

        if (!VehicleSpawnerSettings.IsKnownOrphanMode(mode) && !_reportedUnknown)
        {
            _reportedUnknown = true;

            Log.Warning(
                $"{VehicleSpawnerSettings.OrphanMode.Name} is set to {mode}, which is not 0, 1 or 2. "
                + $"Using {VehicleSpawnerSettings.OrphanMode.Default}.");
        }

        return VehicleSpawnerSettings.NormaliseOrphanMode(mode);
    }

    private readonly struct Pending(int networkId, long deadline)
    {
        public int NetworkId { get; } = networkId;

        public long Deadline { get; } = deadline;
    }
}
