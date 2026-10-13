using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives.Server;

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
        Script.EventHandlers[BullyEvents.PrivateWorld] += new Action<Player, string>(OnPrivateWorld);

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
            Enter(source.ServerId());
        }
        else
        {
            Leave(source.ServerId());
        }
    }

    private static void Enter(int target)
    {
        if (!Allowed.Remove(target, out var expiresAt) || Now > expiresAt || FreeBucket() is not { } bucket)
        {
            Script.TriggerClientEvent(target, BullyEvents.PrivateWorldMoved, BullyEvents.Off);

            return;
        }

        var id = Id(target);
        var stay = new Stay(API.GetPlayerRoutingBucket(id), bucket);

        Stays[target] = stay;

        Persist();

        NativeFixer.SetRoutingBucketPopulationEnabled(bucket, false);
        NativeFixer.SetPlayerRoutingBucket(id, bucket);

        _ = ReturnLaterAsync(target, stay);

        Script.TriggerClientEvent(target, BullyEvents.PrivateWorldMoved, BullyEvents.On);
    }

    private static void Leave(int target)
    {
        if (Stays.Remove(target, out var stay))
        {
            Persist();

            Return(target, stay);
        }

        Script.TriggerClientEvent(target, BullyEvents.PrivateWorldMoved, BullyEvents.Off);
    }

    private static int? FreeBucket()
    {
        var taken = new HashSet<int>(ManagedWorlds());

        foreach (var player in ConnectedPlayers.All())
        {
            taken.Add(API.GetPlayerRoutingBucket(Id(player.ServerId)));
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

        if (API.DoesPlayerExist(id) && API.GetPlayerRoutingBucket(id) == stay.Bucket)
        {
            NativeFixer.SetPlayerRoutingBucket(id, stay.Original);
        }
    }

    private static async Task ReturnLaterAsync(int target, Stay stay)
    {
        try
        {
            await BaseScript.Delay((int)MaxStayMs);

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
