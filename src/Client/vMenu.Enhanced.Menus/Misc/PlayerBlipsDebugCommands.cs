using System.Globalization;
using System.Numerics;
using System.Text;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Misc;

/// <summary>
/// Invents players who are not there, so player blips can be tested without a full server.
/// </summary>
/// <remarks>
/// The fakes are pushed in through the very same handler the server's snapshot arrives at, so they
/// travel the real wire format and land in the real store. Nothing downstream of that can tell them
/// apart from a genuine player, which is the point: a test that took a shortcut past the parsing and
/// the staleness rules would not be testing the thing that ships.
///
/// <para>
/// They are also all players the game has <em>not</em> streamed in, which is the harder of the two
/// cases and the one nobody can produce on their own. A player standing next to you tests the branch
/// that pins a blip to a character; only somebody the server merely mentioned tests the branch that
/// has to carry a blip around on coordinates.
/// </para>
/// </remarks>
public static class PlayerBlipsDebugCommands
{
    private const string Command = "vmenu_blips_test";

    /// <summary>Where the invented server ids start.</summary>
    // Far above anything FiveM will ever hand out, so a fake can never be mistaken for a real player
    // or quietly overwrite one in the store.
    private const int FirstFakeId = 900_001;

    private const int MaxFakes = 64;

    /// <summary>How often the fakes are pushed in again.</summary>
    // Not a choice so much as an obligation. A stored position is only trusted for fifteen seconds,
    // which is exactly the rule that stops a dropped message leaving a blip on the map forever, and a
    // fake nobody keeps repeating is subject to it like anybody else.
    private const long RefreshMs = 1000;

    /// <summary>How far out the nearest and furthest fakes are put.</summary>
    // Chosen to cross every threshold the blip code has: full size within 400m, shrinking to 1400m,
    // dimming past that, and off the ordinary minimap past 1000m. One run of the command should show
    // you all four happening at once.
    private const float NearestMetres = 120f;

    private const float FurthestMetres = 3000f;

    /// <summary>Roughly 137.5 degrees, the angle that spreads points around a centre most evenly.</summary>
    // Any whole fraction of a circle lines the fakes up into spokes, which makes it hard to tell one
    // ring of distances from another on the map.
    private const float GoldenAngle = 2.399963f;

    /// <summary>One of each kind of thing that gets its own blip artwork, plus a couple that do not.</summary>
    // Empty means on foot. The ordinary car and the pushbike are in here on purpose: both should stay
    // a plain dot, and a test that only contains special cases never catches the day it stops.
    private static readonly string[] Models =
    [
        string.Empty,
        "adder",
        "buzzard",
        "lazer",
        "rhino",
        "dinghy",
        "kosatka",
        "taxi",
        "oppressor2",
        "bmx",
    ];

    private static readonly List<Fake> Fakes = [];

    private static readonly StringBuilder Snapshot = new();

    private static TickHandle? _tick;

    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(Command, false, DebugCommands.Gate<string?>(Run));

        _tick = TickRegistry.Register(
            "Player.Presence.Fakes",
            Push,
            TickRate.Every(RefreshMs),
            condition: () => Fakes.Count > 0);
    }

    private static void Run(string? argument)
    {
        var text = argument?.Trim() ?? string.Empty;

        if (text.Length == 0)
        {
            Report();

            return;
        }

        if (text.Equals("off", StringComparison.OrdinalIgnoreCase)
            || text.Equals("clear", StringComparison.OrdinalIgnoreCase)
            || text.Equals("0", StringComparison.Ordinal))
        {
            var had = Fakes.Count;

            Clear();

            Log.Info($"[Blips] {had} fake player(s) removed.");

            return;
        }

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count)
            || count < 1
            || count > MaxFakes)
        {
            Log.Warning($"[Blips] '{text}' is not a number of players between 1 and {MaxFakes}.");

            Report();

            return;
        }

        Spawn(count);
    }

    /// <summary>Puts a fresh set of fakes on the map, replacing whatever was there.</summary>
    private static void Spawn(int count)
    {
        Clear();

        var origin = Native.GetEntityCoords(Native.PlayerPedId(), true);

        for (var index = 0; index < count; index++)
        {
            // Spread evenly over the whole range rather than placed at random, so the same command
            // twice puts a blip at the same distance twice and you can compare the two.
            var travelled = count == 1 ? 0f : index / (float)(count - 1);
            var distance = NearestMetres + ((FurthestMetres - NearestMetres) * travelled);
            var angle = index * GoldenAngle;

            var model = Models[index % Models.Length];

            Fakes.Add(new Fake(
                FirstFakeId + index,
                new Vector3(
                    origin.X + (MathF.Cos(angle) * distance),
                    origin.Y + (MathF.Sin(angle) * distance),
                    origin.Z),
                model.Length == 0 ? 0 : VehicleBlipSprites.Hash(model),

                // Every third one, so the staff ring and the plain blip sit next to each other on the
                // map and the difference is actually visible.
                index % 3 == 0,
                $"Fake {index + 1} ({(int)distance}m {(model.Length == 0 ? "on foot" : model)})"));
        }

        Push();

        _tick?.Reevaluate();

        Log.Info($"[Blips] {count} fake player(s) placed between {(int)NearestMetres}m and {(int)FurthestMetres}m.");

        if (!PlayerPresence.BlipsWanted)
        {
            Log.Warning(
                "[Blips] Player blips are switched off, so none of them will be drawn. Turn them on "
                + "under Misc Settings, or check you have permission for them.");
        }

        Log.Info($"[Blips] {Command} off takes them away again.");
    }

    private static void Clear()
    {
        foreach (var fake in Fakes)
        {
            PlayerPresence.DropRemote(fake.ServerId);
        }

        Fakes.Clear();

        _tick?.Reevaluate();
    }

    /// <summary>Hands the whole set over as if the server had just sent it.</summary>
    private static void Push()
    {
        if (Fakes.Count == 0)
        {
            return;
        }

        Snapshot.Clear();

        foreach (var fake in Fakes)
        {
            // Turned a little on every push, which is what makes it obvious whether blip rotation is
            // working and whether the sprites that spin themselves are being left alone.
            fake.Heading = (fake.Heading + fake.Spin) % 360;

            PresenceRow.Append(
                Snapshot,
                fake.ServerId,
                fake.Position.X,
                fake.Position.Y,
                fake.Position.Z,
                fake.Heading,
                fake.Model,
                (fake.Model == 0 ? 0 : PresenceRow.FlagInVehicle) | (fake.Staff ? PresenceRow.FlagStaff : 0),
                fake.Name);
        }

        PlayerPresence.InjectSnapshot(Snapshot.ToString());
    }

    private static void Report()
    {
        Log.Info($"[Blips] Usage: {Command} <1-{MaxFakes} | off>, which invents players around you to draw blips for.");

        if (Fakes.Count == 0)
        {
            Log.Info("[Blips] There are none at the moment.");

            return;
        }

        var self = Native.GetEntityCoords(Native.PlayerPedId(), true);

        Log.Info($"[Blips] {Fakes.Count} in place:");

        foreach (var fake in Fakes)
        {
            var distance = (int)Vector3.Distance(self, fake.Position);

            Log.Info($"[Blips]   #{fake.ServerId} {fake.Name}, {distance}m away{(fake.Staff ? ", staff" : string.Empty)}");
        }
    }

    // A plain class rather than a record, matching the rest of this codebase: the generated equality
    // routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class Fake(int serverId, Vector3 position, uint model, bool staff, string name)
    {
        public int ServerId { get; } = serverId;

        public Vector3 Position { get; } = position;

        public uint Model { get; } = model;

        public bool Staff { get; } = staff;

        public string Name { get; } = name;

        public int Heading { get; set; }

        /// <summary>Degrees per push, varied per fake so they do not all turn in lockstep.</summary>
        public int Spin { get; } = 5 + (serverId % 7);
    }
}
