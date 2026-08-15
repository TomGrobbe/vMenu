using System.Numerics;

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>One player the game is currently simulating near us, as of the last refresh.</summary>
// A plain class rather than a record, matching the rest of this codebase: the generated equality
// routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
public sealed class RosteredPlayer(int slot, int serverId, int ped, Vector3 position)
{
    /// <summary>The local player index, which is what every client native wants.</summary>
    // Not stable: the same person gets a different slot on a different client, and a different one
    // again after they go out of range and come back. Never store this against anything.
    public int Slot { get; } = slot;

    /// <summary>The server id, which is the same number on every machine and does not change.</summary>
    public int ServerId { get; } = serverId;

    public int Ped { get; } = ped;

    public Vector3 Position { get; } = position;
}

/// <summary>
/// Everybody the game has streamed in near us right now.
/// </summary>
/// <remarks>
/// This is emphatically <em>not</em> the list of players on the server. Under OneSync Infinity a
/// client is only told about players inside its own scope, so on a busy server this is a small
/// fraction of everyone online. The full list only ever comes from the server.
///
/// <para>
/// Shared because several features want the same walk and none of them should pay for it twice.
/// <see cref="Refresh" /> is safe to call from every one of them: it does the work at most once
/// every <see cref="MinimumAgeMs" /> and hands back the cached answer in between.
/// </para>
/// </remarks>
public static class PlayerRoster
{
    /// <summary>The highest player slot the game will hand out.</summary>
    private const int PlayerSlots = 256;

    /// <summary>How stale the list is allowed to get before a caller triggers a rebuild.</summary>
    // Short enough that the fastest consumer never sees an old list, long enough that several
    // consumers on different rates share one walk rather than each doing their own.
    private const int MinimumAgeMs = 200;

    private static readonly List<RosteredPlayer> Entries = [];

    private static readonly Dictionary<int, RosteredPlayer> ById = [];

    /// <summary>When the cached list stops being good enough, on the game clock.</summary>
    // Counted forwards to an expiry rather than backwards from the last refresh. The obvious way
    // round starts the "last refresh" at some value far in the past, and subtracting one of those
    // from the game clock overflows, comes out negative, and reads as "refreshed a moment ago" for
    // ever. Which quietly means the list is never built at all, and everything downstream sees an
    // empty server.
    private static int _staleAt;

    /// <summary>Everyone streamed in, including this player.</summary>
    // The local player is left in because some features do want to act on their own character, and
    // leaving them out would mean every caller that does had to walk the slots again themselves.
    public static IReadOnlyList<RosteredPlayer> All => Entries;

    /// <summary>Rebuilds the list, unless it was rebuilt very recently.</summary>
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

            // The runtime hands back 65535 for a slot it cannot resolve yet, and an entry nothing can
            // be keyed on is worse than no entry at all.
            if (serverId <= 0 || serverId == ushort.MaxValue)
            {
                continue;
            }

            var entry = new RosteredPlayer(slot, serverId, ped, Native.GetEntityCoords(ped, true));

            Entries.Add(entry);
            ById[serverId] = entry;
        }
    }

    /// <summary>Whether this player is streamed in, and their details if so.</summary>
    public static bool TryGet(int serverId, out RosteredPlayer player) => ById.TryGetValue(serverId, out player!);

    /// <summary>Whether this player is streamed in.</summary>
    public static bool IsStreamed(int serverId) => ById.ContainsKey(serverId);
}
