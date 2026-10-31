using System.Globalization;

using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Integration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Ticks.Server;

namespace vMenu.Enhanced.Integration.Server;

public static class IntegrationHeatmap
{
    private const int CellSize = 50;

    private const int SampleMs = 10000;

    // Deliberately kept low, if the integration is not connected, it will keep the
    // last 10 pending calls with heatmap data, the rest is dropped.
    private const int MaxPending = 10;

    private static readonly HeatmapGrid Grid = new(CellSize);

    private static readonly Queue<string> Pending = new();

    private static readonly object PendingLock = new();

    private static long _minute = -1;

    public static void Initialize() =>
        ServerTickRegistry.Register(
            "integration:heatmap", Sample, TickRate.Every(SampleMs), condition: () => IntegrationAuth.IsConfigured);

    public static bool TryPeek(out string payload)
    {
        lock (PendingLock)
        {
            return Pending.TryPeek(out payload!);
        }
    }

    public static void Acknowledge(string payload)
    {
        lock (PendingLock)
        {
            if (Pending.TryPeek(out var head) && ReferenceEquals(head, payload))
            {
                Pending.Dequeue();
            }
        }
    }

    private static void Sample()
    {
        var minute = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60 * 60;
        if (minute != _minute)
        {
            Flush();
            _minute = minute;
        }

        Grid.BeginSample();

        foreach (var player in ConnectedPlayers.All())
        {
            var ped = API.GetPlayerPed(player.ServerId.ToString(CultureInfo.InvariantCulture));
            if (ped == 0 || !API.DoesEntityExist(ped))
            {
                continue;
            }

            var position = API.GetEntityCoords(ped);

            // A player still loading in sits at 0 0, which would build a fake hotspot there.
            if (Math.Abs(position.X) < 1f && Math.Abs(position.Y) < 1f)
            {
                continue;
            }

            Grid.Add(position.X, position.Y);
        }
    }

    private static void Flush()
    {
        if (_minute < 0 || Grid.IsEmpty)
        {
            Grid.Clear();

            return;
        }

        var payload = ServerJson.Serialize(Grid.Drain(_minute));

        lock (PendingLock)
        {
            Pending.Enqueue(payload);

            while (Pending.Count > MaxPending)
            {
                Pending.Dequeue();
            }
        }
    }
}
