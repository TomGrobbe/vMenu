using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class BullyPrivateWorlds
{
    private const int HighestBucket = 65535;

    private const int LowestBucket = 1;

    private const long MaxStayMs = 5 * 60 * 1000;

    private static readonly Dictionary<int, long> Allowed = [];

    private static readonly Dictionary<int, Stay> Stays = [];

    private static long Now => Environment.TickCount64;

    public static Func<IReadOnlyCollection<int>> ManagedWorlds { get; set; } = () => [];

    internal static void Register()
    {
        API.OnNetEvent(BullyEvents.PrivateWorld, new Action<Player, string>(OnPrivateWorld), false);

        ReturnAfterRestart();
    }

    internal static void Allow(int target, long expiresAt) => Allowed[target] = expiresAt;

    internal static void Refused(int target) => Allowed.Remove(target);

    internal static bool IsBusy(int target) =>
        Stays.ContainsKey(target) || (Allowed.TryGetValue(target, out var expiresAt) && Now <= expiresAt);

    internal static void Forget(int target)
    {
        Allowed.Remove(target);

        if (Stays.Remove(target))
        {
            Persist();
        }
    }

    private static void OnPrivateWorld([FromSource] Player source, string state)
    {
        if (state == BullyEvents.On)
        {
            Enter(source.Handle);
        }
        else
        {
            Leave(source.Handle);
        }
    }

    private static void Enter(int target)
    {
        if (!Allowed.Remove(target, out var expiresAt) || Now > expiresAt || FreeBucket() is not { } bucket)
        {
            API.EmitClient(target, BullyEvents.PrivateWorldMoved, BullyEvents.Off);

            return;
        }

        var id = Id(target);
        var stay = new Stay(Native.GetPlayerRoutingBucket(id), bucket);

        Stays[target] = stay;

        Persist();

        Native.SetRoutingBucketPopulationEnabled(bucket, false);
        Native.SetPlayerRoutingBucket(id, bucket);

        _ = ReturnLaterAsync(target, stay);

        API.EmitClient(target, BullyEvents.PrivateWorldMoved, BullyEvents.On);
    }

    private static void Leave(int target)
    {
        if (Stays.Remove(target, out var stay))
        {
            Persist();

            Return(target, stay);
        }

        API.EmitClient(target, BullyEvents.PrivateWorldMoved, BullyEvents.Off);
    }

    private static int? FreeBucket()
    {
        var taken = new HashSet<int>(ManagedWorlds());

        foreach (var player in ConnectedPlayers.All())
        {
            taken.Add(Native.GetPlayerRoutingBucket(Id(player.ServerId)));
        }

        foreach (var stay in Stays.Values)
        {
            taken.Add(stay.Bucket);
        }

        for (var bucket = HighestBucket; bucket >= LowestBucket; bucket--)
        {
            if (!taken.Contains(bucket))
            {
                return bucket;
            }
        }

        return null;
    }

    private static void Return(int target, Stay stay)
    {
        var id = Id(target);

        if (Native.DoesPlayerExist(id) && Native.GetPlayerRoutingBucket(id) == stay.Bucket)
        {
            Native.SetPlayerRoutingBucket(id, stay.Original);
        }
    }

    private static async Task ReturnLaterAsync(int target, Stay stay)
    {
        try
        {
            await API.Delay((int)MaxStayMs);

            if (Stays.TryGetValue(target, out var current) && ReferenceEquals(current, stay))
            {
                Stays.Remove(target);

                Persist();

                Return(target, stay);

                Log.Warning($"[Bully] Returned {target} from their private world after they never left it.");
            }
        }
        catch (Exception exception)
        {
            Log.Error($"[Bully] Returning {target} from their private world failed: {exception}");
        }
    }

    private static void ReturnAfterRestart()
    {
        var saved = ServerStateBags.Get<int[]>(BullyEvents.GlobalBag, BullyEvents.PrivateWorldsKey) ?? [];

        for (var index = 0; index + 2 < saved.Length; index += 3)
        {
            Return(saved[index], new Stay(saved[index + 1], saved[index + 2]));
        }

        Persist();
    }

    private static void Persist() =>
        ServerStateBags.Set(
            BullyEvents.GlobalBag,
            BullyEvents.PrivateWorldsKey,
            Stays.SelectMany(pair => new[] { pair.Key, pair.Value.Original, pair.Value.Bucket }).ToArray(),
            replicated: false);

    private static string Id(int serverId) => serverId.ToString(CultureInfo.InvariantCulture);

    private sealed record Stay(int Original, int Bucket);
}
