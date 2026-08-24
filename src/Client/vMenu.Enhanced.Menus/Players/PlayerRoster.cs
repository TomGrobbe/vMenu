using System.Numerics;

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players;

// A class rather than a record: generated equality routes through
// EqualityComparer<string>.Default, which the sandbox refuses to load.
public sealed class RosteredPlayer(int slot, int serverId, int ped, Vector3 position)
{
    // Not stable: the same person gets a different slot on a different client, and a different one again
    // after they go out of range and come back. Never store this against anything.
    public int Slot { get; } = slot;

    // The server id, which is the same number on every machine and does not change.
    public int ServerId { get; } = serverId;

    public int Ped { get; } = ped;

    public Vector3 Position { get; } = position;
}

// Emphatically not the list of players on the server: under OneSync Infinity a client is only told
// about players inside its own scope, so on a busy server this is a small fraction of everyone
// online. Shared because several features want the same walk, and Refresh does the work at most
// once every MinimumAgeMs, handing back the cached answer in between.
public static class PlayerRoster
{
    private const int PlayerSlots = 256;

    // Short enough that the fastest consumer never sees an old list, long enough that several consumers
    // on different rates share one walk rather than each doing their own.
    private const int MinimumAgeMs = 200;

    private static readonly List<RosteredPlayer> Entries = [];

    private static readonly Dictionary<int, RosteredPlayer> ById = [];

    // Counted forwards to an expiry rather than backwards from the last refresh. The obvious way round
    // starts at a value far in the past, and subtracting one of those from the game clock overflows,
    // comes out negative, and reads as "refreshed a moment ago" forever.
    private static int _staleAt;

    // The local player is left in because some features do want to act on their own character, and
    // leaving them out would mean every caller that does had to walk the slots again themselves.
    public static IReadOnlyList<RosteredPlayer> All => Entries;

    public static void Refresh()
    {
        var now = Native.GetGameTimer();

        if (now < _staleAt)
        {
            return;
        }

        _staleAt = now + MinimumAgeMs;

        Entries.Clear();
        ById.Clear();

        for (var slot = 0; slot < PlayerSlots; slot++)
        {
            if (!Native.NetworkIsPlayerActive(slot))
            {
                continue;
            }

            var ped = Native.GetPlayerPed(slot);

            if (ped == 0 || !Native.DoesEntityExist(ped))
            {
                continue;
            }

            var serverId = Native.GetPlayerServerId(slot);

            // The runtime hands back 65535 for a slot it cannot resolve yet, and an entry nothing can be keyed
            // on is worse than no entry at all.
            if (serverId <= 0 || serverId == ushort.MaxValue)
            {
                continue;
            }

            var entry = new RosteredPlayer(slot, serverId, ped, Native.GetEntityCoords(ped, true));

            Entries.Add(entry);
            ById[serverId] = entry;
        }
    }

    public static bool TryGet(int serverId, out RosteredPlayer player) => ById.TryGetValue(serverId, out player!);

    public static bool IsStreamed(int serverId) => ById.ContainsKey(serverId);
}
