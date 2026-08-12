using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>How somebody died, when it was not a weapon that did it.</summary>
// ByKiller is null where naming somebody would read as nonsense. Walking into barbed wire is not
// something another player does to you, even if they were the reason you ran.
internal sealed class DeathCause(string solo, string? byKiller)
{
    public string Solo { get; } = solo;

    public string? ByKiller { get; } = byKiller;
}

/// <summary>
/// The game's pseudo weapons, which are how it reports a death nothing was holding.
/// </summary>
internal static class DeathCauses
{
    private static readonly (string SpawnName, DeathCause Cause)[] Table =
    [
        ("WEAPON_RUN_OVER_BY_CAR", new(Loc.DeathNotifications.RunOver, Loc.DeathNotifications.RunOverBy)),
        ("WEAPON_RAMMED_BY_CAR", new(Loc.DeathNotifications.RunOver, Loc.DeathNotifications.RunOverBy)),
        ("WEAPON_FALL", new(Loc.DeathNotifications.Fell, Loc.DeathNotifications.FellBy)),
        ("WEAPON_DROWNING", new(Loc.DeathNotifications.Drowned, Loc.DeathNotifications.DrownedBy)),
        ("WEAPON_DROWNING_IN_VEHICLE", new(Loc.DeathNotifications.Drowned, Loc.DeathNotifications.DrownedBy)),
        ("WEAPON_EXPLOSION", new(Loc.DeathNotifications.BlownUp, Loc.DeathNotifications.BlownUpBy)),
        ("WEAPON_FIRE", new(Loc.DeathNotifications.Burned, Loc.DeathNotifications.BurnedBy)),
        ("WEAPON_ELECTRIC_FENCE", new(Loc.DeathNotifications.Electrocuted, null)),
        ("WEAPON_BARBED_WIRE", new(Loc.DeathNotifications.BarbedWire, null)),
        ("WEAPON_ANIMAL", new(Loc.DeathNotifications.Mauled, null)),
        ("WEAPON_COUGAR", new(Loc.DeathNotifications.Mauled, null)),
        ("WEAPON_HELI_CRASH", new(Loc.DeathNotifications.Rotors, Loc.DeathNotifications.RotorsBy)),
        ("WEAPON_BLEEDING", new(Loc.DeathNotifications.BledOut, Loc.DeathNotifications.BledOutBy)),
        ("WEAPON_EXHAUSTION", new(Loc.DeathNotifications.Exhausted, null)),
        ("WEAPON_UNARMED", new(Loc.DeathNotifications.Beaten, Loc.DeathNotifications.BeatenBy)),
    ];

    private static Dictionary<uint, DeathCause>? _byHash;

    /// <summary>The cause this hash describes, or null when it is an ordinary weapon.</summary>
    internal static DeathCause? Find(uint hash) => Index().GetValueOrDefault(hash);

    private static Dictionary<uint, DeathCause> Index()
    {
        if (_byHash is not null)
        {
            return _byHash;
        }

        var index = new Dictionary<uint, DeathCause>();

        foreach (var (spawnName, cause) in Table)
        {
            index[API.Hash(spawnName)] = cause;
        }

        _byHash = index;

        return index;
    }
}
