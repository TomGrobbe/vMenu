using System.Globalization;
using System.Text.Json;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

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

    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
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

        API.OnNetEvent(AircraftWeaponEvents.ConfigRequest, new Action<Player>(OnRequested), false);
        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);
    }

    private static void OnRequested([FromSource] Player source) =>
        API.EmitClient(source.Handle, AircraftWeaponEvents.ConfigSet, _payload);

    // Only the network owner can change these, so a co-pilot's request is passed on to them.
    private static ActionResponse Forward(Player source, string[] args)
    {
        if (args.Length < 2
            || !ActionArgs.TryInt(args[0], out var networkId)
            || !AircraftWeaponEvents.IsKnown(args[1]))
        {
            return ActionResponse.InvalidRequest();
        }

        var player = source.Handle;

        if (!TakeAllowance(player))
        {
            return ActionResponse.Refused();
        }

        var vehicle = networkId == 0 ? 0 : Native.NetworkGetEntityFromNetworkId(networkId);

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
        var ped = Native.GetPlayerPed(source.Handle.ToString(CultureInfo.InvariantCulture));

        if (!IsInFront(vehicle, ped) && !Started(effect, networkId, player))
        {
            return ActionResponse.Refused();
        }

        var owner = Native.NetworkGetEntityOwner(vehicle);

        if (owner <= 0)
        {
            return ActionResponse.NotReady();
        }

        Remember(effect, networkId, player);

        API.EmitClient(owner, AircraftWeaponEvents.Apply, networkId.ToString(CultureInfo.InvariantCulture), effect);

        return ActionResponse.Ok();
    }

    private static bool IsAircraft(int vehicle) =>
        Native.GetVehicleType(vehicle) is "plane" or "heli"
        || AircraftWeaponProfiles.Knows(unchecked((uint)Native.GetEntityModel(vehicle)));

    private static bool IsInFront(int vehicle, int ped) =>
        ped != 0 && (Native.GetPedInVehicleSeat(vehicle, Pilot) == ped || Native.GetPedInVehicleSeat(vehicle, CoPilot) == ped);

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
            await API.Delay(LockOnBackstopMs);

            if (!BlockedBy.TryGetValue(networkId, out var current) || !ReferenceEquals(current, blocker))
            {
                return;
            }

            BlockedBy.Remove(networkId);

            var vehicle = Native.NetworkGetEntityFromNetworkId(networkId);

            if (vehicle == 0 || !Native.DoesEntityExist(vehicle))
            {
                return;
            }

            var owner = Native.NetworkGetEntityOwner(vehicle);

            if (owner > 0)
            {
                API.EmitClient(owner, AircraftWeaponEvents.Apply, networkId.ToString(CultureInfo.InvariantCulture), AircraftWeaponEvents.AllowLockOn);
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

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (source > 0)
        {
            Forget(source);
        }
    }

    private static void Load()
    {
        Aircraft.Clear();
        _payload = "{}";

        if (ResourceFile.Read(ConfigFile, "Config", "aircraft bombs and countermeasures use the built in positions")
            is not { } contents)
        {
            return;
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(contents, ParseOptions);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Config] {ConfigFile} could not be parsed, so aircraft bombs and countermeasures use the built in positions: {exception.Message}");
            return;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                Log.Error($"[Config] {ConfigFile} has to hold a single object of aircraft model names, so aircraft bombs and countermeasures use the built in positions.");
                return;
            }

            Read(document.RootElement);
        }

        _payload = ServerJson.Serialize(Aircraft);

        Log.Debug($"[Config] Aircraft weapon positions loaded for {Aircraft.Count} model(s) from {ConfigFile}.");
    }

    // Walked, not deserialized: a dictionary keeps the last of two duplicate keys silently.
    private static void Read(JsonElement root)
    {
        foreach (var aircraft in root.EnumerateObject())
        {
            if (aircraft.Name.Trim() is not { Length: > 0 } model)
            {
                continue;
            }

            var key = model.ToLowerInvariant();

            if (Aircraft.ContainsKey(key))
            {
                Log.Warning($"[Config] '{model}' is listed more than once in {ConfigFile}, so only the first one is used.");
                continue;
            }

            if (aircraft.Value.ValueKind != JsonValueKind.Object)
            {
                Log.Warning($"[Config] Skipping '{model}' in {ConfigFile}: its settings have to be written as an object.");
                continue;
            }

            foreach (var setting in aircraft.Value.EnumerateObject())
            {
                Unknown(setting.Name, model);
            }

            if (!ServerJson.TryDeserialize<AircraftWeaponOverride>(aircraft.Value.GetRawText(), out var entry, out var error)
                || entry is null)
            {
                Log.Warning($"[Config] Skipping '{model}' in {ConfigFile}: {error ?? "it could not be read."}");
                continue;
            }

            if (!IsFinite(entry))
            {
                Log.Warning($"[Config] Skipping '{model}' in {ConfigFile}: every number has to be a real number.");
                continue;
            }

            Aircraft[key] = Keep(entry, model);
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
