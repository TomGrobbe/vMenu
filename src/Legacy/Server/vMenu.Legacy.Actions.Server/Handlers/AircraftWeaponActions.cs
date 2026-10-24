using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

using vMenu.Legacy.Hosting;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class AircraftWeaponActions
{
    private const string ConfigFile = "config/aircraft-weapons.json";

    private const int Pilot = -1;

    private const int CoPilot = 0;

    private const int Flares = 4;

    private const int SmokeAxes = 3;

    private const float MaxScale = 50f;

    private const int BurstAllowance = 10;

    private const long BurstWindowMs = 2000;

    private const string DroppedEvent = "playerDropped";

    private const long UndoWindowMs = 10000;

    private const int LockOnBackstopMs = 8000;

    private static readonly JsonLoadSettings LoadSettings = new()
    {
        CommentHandling = CommentHandling.Ignore,
    };

    private static readonly Dictionary<string, AircraftWeaponOverride> Aircraft = new(StringComparer.Ordinal);

    private static readonly HashSet<string> Settings = new(
        typeof(AircraftWeaponOverride).GetProperties().Select(property => property.Name), StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<int, Starter> OpenedBy = [];

    private static readonly Dictionary<int, Starter> BlockedBy = [];

    private static readonly Dictionary<int, Burst> Bursts = [];

    private static string _payload = "{}";

    public static void Register()
    {
        Load();

        ActionRegistry.Register(
            ActionIds.VehicleOptions.AircraftWeapon,
            VehicleOptionsPermissions.BombsAndFlares,
            Forward);

        Script.EventHandlers[AircraftWeaponEvents.ConfigRequest] += new Action<Player>(OnRequested);
        Script.EventHandlers[DroppedEvent] += new Action<string, string?>(OnPlayerDropped);
    }

    private static void OnRequested([FromSource] Player source) =>
        Script.TriggerClientEvent(source.ServerId(), AircraftWeaponEvents.ConfigSet, _payload);

    // Only the network owner can change these, so a co-pilot's request is passed on to them.
    private static ActionResponse Forward(Player source, string[] args)
    {
        if (args.Length < 2
            || !ActionArgs.TryInt(args[0], out var networkId)
            || !AircraftWeaponEvents.IsKnown(args[1]))
        {
            return ActionResponse.InvalidRequest();
        }

        var player = source.ServerId();

        if (!TakeAllowance(player))
        {
            return ActionResponse.Refused();
        }

        var vehicle = networkId == 0 ? 0 : API.NetworkGetEntityFromNetworkId(networkId);

        if (!Targets.IsVehicle(vehicle))
        {
            OpenedBy.Remove(networkId);
            BlockedBy.Remove(networkId);

            return ActionResponse.NotFound();
        }

        if (!IsAircraft(vehicle))
        {
            return ActionResponse.InvalidRequest();
        }

        var effect = args[1];
        var ped = API.GetPlayerPed(source.Handle);

        if (!IsInFront(vehicle, ped) && !Started(effect, networkId, player))
        {
            return ActionResponse.Refused();
        }

        var owner = API.NetworkGetEntityOwner(vehicle);

        if (owner <= 0)
        {
            return ActionResponse.NotReady();
        }

        Remember(effect, networkId, player);

        Script.TriggerClientEvent(owner, AircraftWeaponEvents.Apply, networkId.ToString(CultureInfo.InvariantCulture), effect);

        return ActionResponse.Ok();
    }

    private static bool IsAircraft(int vehicle) =>
        API.GetVehicleType(vehicle) is "plane" or "heli"
        || AircraftWeaponProfiles.Knows(unchecked((uint)API.GetEntityModel(vehicle)));

    private static bool IsInFront(int vehicle, int ped) =>
        ped != 0 && (API.GetPedInVehicleSeat(vehicle, Pilot) == ped || API.GetPedInVehicleSeat(vehicle, CoPilot) == ped);

    // Whoever opened the doors or blocked the lock-on can still undo it right after leaving the seat.
    private static bool Started(string effect, int networkId, int player) => effect switch
    {
        AircraftWeaponEvents.CloseBombBay => OpenedBy.TryGetValue(networkId, out var opener) && opener.IsRecent(player),
        AircraftWeaponEvents.AllowLockOn => BlockedBy.TryGetValue(networkId, out var blocker) && blocker.IsRecent(player),
        _ => false,
    };

    private static void Remember(string effect, int networkId, int player)
    {
        switch (effect)
        {
            case AircraftWeaponEvents.OpenBombBay:
                OpenedBy[networkId] = new Starter(player);
                break;
            case AircraftWeaponEvents.CloseBombBay:
                OpenedBy.Remove(networkId);
                break;
            case AircraftWeaponEvents.BlockLockOn:
                var blocker = new Starter(player);
                BlockedBy[networkId] = blocker;
                _ = AllowLaterAsync(networkId, blocker);
                break;
            case AircraftWeaponEvents.AllowLockOn:
                BlockedBy.Remove(networkId);
                break;
        }
    }

    // A client that never sends the allow would otherwise keep the aircraft safe from missiles for good.
    private static async Task AllowLaterAsync(int networkId, Starter blocker)
    {
        try
        {
            await BaseScript.Delay(LockOnBackstopMs);

            if (!BlockedBy.TryGetValue(networkId, out var current) || !ReferenceEquals(current, blocker))
            {
                return;
            }

            BlockedBy.Remove(networkId);

            var vehicle = API.NetworkGetEntityFromNetworkId(networkId);

            if (vehicle == 0 || !API.DoesEntityExist(vehicle))
            {
                return;
            }

            var owner = API.NetworkGetEntityOwner(vehicle);

            if (owner > 0)
            {
                Script.TriggerClientEvent(owner, AircraftWeaponEvents.Apply, networkId.ToString(CultureInfo.InvariantCulture), AircraftWeaponEvents.AllowLockOn);
            }
        }
        catch (Exception exception)
        {
            Log.Error($"[Vehicles] Allowing missile lock-on again for {networkId} failed: {exception}");
        }
    }

    private static bool TakeAllowance(int player)
    {
        var now = Environment.TickCount64;

        if (!Bursts.TryGetValue(player, out var burst))
        {
            Bursts[player] = burst = new Burst();
        }

        if (now - burst.Started >= BurstWindowMs)
        {
            burst.Started = now;
            burst.Count = 0;
        }

        return ++burst.Count <= BurstAllowance;
    }

    private static void Forget(int player)
    {
        Bursts.Remove(player);

        foreach (var networkId in OpenedBy.Where(entry => entry.Value.Source == player).Select(entry => entry.Key).ToList())
        {
            OpenedBy.Remove(networkId);
        }

        foreach (var networkId in BlockedBy.Where(entry => entry.Value.Source == player).Select(entry => entry.Key).ToList())
        {
            BlockedBy.Remove(networkId);
        }
    }

    private static bool IsFinite(AircraftWeaponOverride entry) =>
        new[] { entry.BombPosition, entry.CameraY, entry.CameraZ, entry.FlareStartY, entry.FlareStartZ, entry.FlareTargetY, entry.ChaffScale, entry.SmokeScale }
            .All(value => value is not { } number || Finite(number))
        && new[] { entry.FlareStartX, entry.ChaffSide, entry.ChaffLength, entry.ChaffHeight, entry.SmokeOffset }
            .All(values => values is null || values.All(Finite));

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    private static void Unknown(string setting, string model)
    {
        if (!Settings.Contains(setting))
        {
            Log.Warning($"[Config] Ignoring unknown setting '{setting}' on '{model}' in {ConfigFile}.");
        }
    }

    private static void OnPlayerDropped([FromSource] string sourceString, string? reason = null)
    {
        if (Script.IsFromPlayer(sourceString))
        {
            return;
        }

        var source = Script.SourceId(sourceString);

        if (source > 0)
        {
            Forget(source);
        }
    }

    private static void Load()
    {
        Aircraft.Clear();
        _payload = "{}";

        var contents = API.LoadResourceFile(API.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Info($"[Config] No {ConfigFile} found, so aircraft bombs and countermeasures use the built in positions.");
            return;
        }

        List<KeyValuePair<string, JToken>>? aircraft;

        try
        {
            aircraft = Parse(contents);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Config] {ConfigFile} could not be parsed, so aircraft bombs and countermeasures use the built in positions: {exception.Message}");
            return;
        }

        if (aircraft is null)
        {
            Log.Error($"[Config] {ConfigFile} has to hold a single object of aircraft model names, so aircraft bombs and countermeasures use the built in positions.");
            return;
        }

        Read(aircraft);

        _payload = ServerJson.Serialize(Aircraft);

        Log.Debug($"[Config] Aircraft weapon positions loaded for {Aircraft.Count} model(s) from {ConfigFile}.");
    }

    private static List<KeyValuePair<string, JToken>>? Parse(string contents)
    {
        using var reader = new JsonTextReader(new StringReader(contents))
        {
            DateParseHandling = DateParseHandling.None,
        };

        if (!Next(reader))
        {
            throw new JsonReaderException("The document holds no value.");
        }

        List<KeyValuePair<string, JToken>>? aircraft = null;

        if (reader.TokenType == JsonToken.StartObject)
        {
            aircraft = [];

            while (NextMember(reader, out var name))
            {
                aircraft.Add(new(name, JToken.ReadFrom(reader, LoadSettings)));
            }
        }
        else
        {
            reader.Skip();
        }

        if (Next(reader))
        {
            throw new JsonReaderException("Additional text was found after the root value.");
        }

        return aircraft;
    }

    private static bool NextMember(JsonReader reader, out string name)
    {
        name = string.Empty;

        if (!Next(reader))
        {
            throw new JsonReaderException("The document ended inside an object.");
        }

        if (reader.TokenType == JsonToken.EndObject)
        {
            return false;
        }

        name = (string)reader.Value!;

        if (!Next(reader))
        {
            throw new JsonReaderException("The document ended inside an object.");
        }

        return true;
    }

    private static bool Next(JsonReader reader)
    {
        do
        {
            if (!reader.Read())
            {
                return false;
            }
        }
        while (reader.TokenType == JsonToken.Comment);

        return true;
    }

    // Walked, not deserialized: a dictionary keeps the last of two duplicate keys silently.
    private static void Read(List<KeyValuePair<string, JToken>> aircraft)
    {
        foreach (var entry in aircraft)
        {
            if (entry.Key.Trim() is not { Length: > 0 } model)
            {
                continue;
            }

            var key = model.ToLowerInvariant();

            if (Aircraft.ContainsKey(key))
            {
                Log.Warning($"[Config] '{model}' is listed more than once in {ConfigFile}, so only the first one is used.");
                continue;
            }

            if (entry.Value.Type != JTokenType.Object)
            {
                Log.Warning($"[Config] Skipping '{model}' in {ConfigFile}: its settings have to be written as an object.");
                continue;
            }

            foreach (var setting in ((JObject)entry.Value).Properties())
            {
                Unknown(setting.Name, model);
            }

            if (!ServerJson.TryDeserialize<AircraftWeaponOverride>(entry.Value.ToString(Formatting.None), out var settings, out var error)
                || settings is null)
            {
                Log.Warning($"[Config] Skipping '{model}' in {ConfigFile}: {error ?? "it could not be read."}");
                continue;
            }

            if (!IsFinite(settings))
            {
                Log.Warning($"[Config] Skipping '{model}' in {ConfigFile}: every number has to be a real number.");
                continue;
            }

            Aircraft[key] = Keep(settings, model);
        }
    }

    private static AircraftWeaponOverride Keep(AircraftWeaponOverride entry, string model)
    {
        entry.FlareStartX = Sized(entry.FlareStartX, Flares, nameof(entry.FlareStartX), model);
        entry.ChaffSide = Sized(entry.ChaffSide, Flares, nameof(entry.ChaffSide), model);
        entry.ChaffLength = Sized(entry.ChaffLength, Flares, nameof(entry.ChaffLength), model);
        entry.ChaffHeight = Sized(entry.ChaffHeight, Flares, nameof(entry.ChaffHeight), model);
        entry.SmokeOffset = Sized(entry.SmokeOffset, SmokeAxes, nameof(entry.SmokeOffset), model);

        if (entry.BombPosition is { } position)
        {
            entry.BombPosition = Math.Clamp(position, 0f, 1f);
        }

        if (entry.ChaffScale is { } chaff)
        {
            entry.ChaffScale = Math.Clamp(chaff, 0f, MaxScale);
        }

        if (entry.SmokeScale is { } smoke)
        {
            entry.SmokeScale = Math.Clamp(smoke, 0f, MaxScale);
        }

        return entry;
    }

    private static float[]? Sized(float[]? values, int length, string name, string model)
    {
        if (values is null || values.Length == length)
        {
            return values;
        }

        Log.Warning($"[Config] Ignoring {char.ToLowerInvariant(name[0])}{name.Substring(1)} on '{model}' in {ConfigFile}: it needs exactly {length} numbers.");

        return null;
    }

    private sealed class Starter(int source)
    {
        public int Source { get; } = source;

        public long At { get; } = Environment.TickCount64;

        public bool IsRecent(int player) => Source == player && Environment.TickCount64 - At <= UndoWindowMs;
    }

    private sealed class Burst
    {
        public long Started { get; set; }

        public int Count { get; set; }
    }
}
