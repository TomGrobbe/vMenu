using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Players.Character;

public enum TattooZone
{
    Hair,
    Head,
    Torso,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg,
    Badge,

    Addon,
}

public sealed class Tattoo
{
    public required string Collection { get; init; }

    public required string Name { get; init; }

    public string Label { get; init; } = string.Empty;

    public TattooZone Zone { get; init; }

    public bool ForMale { get; init; } = true;

    public bool ForFemale { get; init; } = true;

    public uint CollectionHash { get; init; }

    public uint NameHash { get; init; }

    public bool Fits(bool male) => male ? ForMale : ForFemale;
}

public static class TattooCatalogue
{
    private const string ConfigFile = "config/tattoos.json";

    private const int KnownVersion = 1;

    private static readonly List<Tattoo> Everything = [];

    // Packed pair key: the client sandbox has no comparer for a tuple key.
    private static readonly Dictionary<ulong, Tattoo> ByHash = [];

    private static readonly List<List<Tattoo>> ByZone = [];

    public static bool HasLoaded { get; private set; }

    public static bool HasAddons => Zone(TattooZone.Addon).Count > 0;

    public static void Load()
    {
        Everything.Clear();
        ByHash.Clear();
        ByZone.Clear();

        for (var zone = 0; zone <= (int)TattooZone.Addon; zone++)
        {
            ByZone.Add([]);
        }

        HasLoaded = true;

        var raw = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(raw))
        {
            Log.Warning($"[Tattoos] No {ConfigFile} found, so the character creator has no tattoos to offer.");

            return;
        }

        if (!ClientJson.TryDeserialize<TattooFile>(raw, out var file) || file is null)
        {
            Log.Error($"[Tattoos] {ConfigFile} could not be read as JSON, so there are no tattoos to offer.");

            return;
        }

        if (file.Version > KnownVersion)
        {
            Log.Warning(
                $"[Tattoos] {ConfigFile} says it is version {file.Version} and this build only knows "
                + $"version {KnownVersion}. Reading it anyway, but anything newer in it is ignored.");
        }

        Add(file.Tattoos);

        Log.Debug($"[Tattoos] {Everything.Count} tattoo(s) read from {ConfigFile}.");
    }

    public static IReadOnlyList<Tattoo> Zone(TattooZone zone)
    {
        var index = (int)zone;

        return index >= 0 && index < ByZone.Count ? ByZone[index] : [];
    }

    public static List<Tattoo> Zone(TattooZone zone, bool male)
    {
        var fitting = new List<Tattoo>();

        foreach (var tattoo in Zone(zone))
        {
            if (tattoo.Fits(male))
            {
                fitting.Add(tattoo);
            }
        }

        return fitting;
    }

    public static Tattoo? Resolve(uint collection, uint name) =>
        ByHash.TryGetValue(Key(collection, name), out var tattoo) ? tattoo : null;

    public static Tattoo? Find(string collection, string name) =>
        Resolve(Hash(collection), Hash(name));

    public static uint Hash(string value) => (uint)Native.GetHashKey(value);

    private static void Add(List<TattooEntry> entries)
    {
        foreach (var entry in entries)
        {
            var collection = entry.Collection.Trim();
            var name = entry.Name.Trim();

            if (collection.Length == 0 || name.Length == 0)
            {
                Log.Warning($"[Tattoos] Skipping an entry in {ConfigFile}: it has no collection or no name.");

                continue;
            }

            if (ZoneOf(entry.Zone) is not { } zone)
            {
                Log.Warning(
                    $"[Tattoos] Skipping '{name}' in {ConfigFile}: '{entry.Zone}' is not a zone. Use "
                    + "hair, head, torso, leftArm, rightArm, leftLeg, rightLeg, badge or addon.");

                continue;
            }

            var collectionHash = Hash(collection);
            var nameHash = Hash(name);
            var key = Key(collectionHash, nameHash);

            if (ByHash.ContainsKey(key))
            {
                Log.Warning($"[Tattoos] Skipping '{name}' in {ConfigFile}: it is listed more than once.");

                continue;
            }

            var male = !string.Equals(entry.Gender, "female", StringComparison.OrdinalIgnoreCase);
            var female = !string.Equals(entry.Gender, "male", StringComparison.OrdinalIgnoreCase);

            var tattoo = new Tattoo
            {
                Collection = collection,
                Name = name,
                Label = entry.Label.Trim(),
                Zone = zone,
                ForMale = male,
                ForFemale = female,
                CollectionHash = collectionHash,
                NameHash = nameHash,
            };

            Everything.Add(tattoo);
            ByHash[key] = tattoo;
            ByZone[(int)zone].Add(tattoo);
        }
    }

    private static TattooZone? ZoneOf(string zone) => zone.Trim().ToLowerInvariant() switch
    {
        "hair" => TattooZone.Hair,
        "head" => TattooZone.Head,
        "torso" => TattooZone.Torso,
        "leftarm" => TattooZone.LeftArm,
        "rightarm" => TattooZone.RightArm,
        "leftleg" => TattooZone.LeftLeg,
        "rightleg" => TattooZone.RightLeg,
        "badge" => TattooZone.Badge,
        "addon" => TattooZone.Addon,
        _ => null,
    };

    private static ulong Key(uint collection, uint name) => ((ulong)collection << 32) | name;

    private sealed class TattooFile
    {
        public int Version { get; set; } = KnownVersion;

        public List<TattooEntry> Tattoos { get; set; } = [];
    }

    private sealed class TattooEntry
    {
        public string Collection { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Zone { get; set; } = string.Empty;

        public string Gender { get; set; } = "both";

        public string Label { get; set; } = string.Empty;
    }
}
