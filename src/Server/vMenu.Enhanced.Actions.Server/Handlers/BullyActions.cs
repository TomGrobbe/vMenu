using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;

using BullyPermissions = vMenu.Enhanced.Data.Permissions.Menus.Bully;
using BullySettings = vMenu.Enhanced.Data.Configuration.Settings.Bully;
using OnlinePlayerSettings = vMenu.Enhanced.Data.Configuration.Settings.OnlinePlayers;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class BullyActions
{
    private const string DroppedEvent = "playerDropped";

    private const int DriverSeat = -1;

    private const int MaxSeat = 16;

    private const int PedType = 1;

    private const int VehicleType = 2;

    private const long EffectWindowMs = 60 * 1000;

    private const long SwapSettleMs = 1000;

    private const int ConfirmMs = 7000;

    private const float MaxBeamHeight = 200f;

    private const float GroupRadius = 75f;

    private const float MaxShove = 5f;

    private const int MaxSpawnsPerEffect = 32;

    private static readonly ActionRateLimit Limit = new(
        "bully",
        OnlinePlayerSettings.ActionLimit,
        OnlinePlayerSettings.ActionLimitSeconds);

    private static readonly Dictionary<int, HashSet<string>> Toggles = [];

    private static readonly HashSet<string> ServerToggles = new(StringComparer.Ordinal);

    private static readonly Dictionary<int, Transform> Transforms = [];

    private static readonly Dictionary<int, long> Abductions = [];

    private static readonly Dictionary<int, MugRound> Mugs = [];

    private static readonly Dictionary<int, PendingEffect> Pending = [];

    private static readonly Dictionary<int, SpawnRecord> Spawned = [];

    private static readonly Dictionary<int, SpawnWindow> SpawnWindows = [];

    private static HashSet<uint>? _spawnedModels;

    private static int _lastRequestId;

    private static bool _registered;

    private static bool Enabled => ServerConfig.Value(BullySettings.Enabled);

    private static long Now => Environment.TickCount64;

    private static HashSet<uint> SpawnedModels =>
        _spawnedModels ??= BullyModels.Spawned.Select(model => (uint)Native.GetHashKey(model)).ToHashSet();

    public static void Register()
    {
        foreach (var effect in BullyEffects.All)
        {
            Gate(ActionIds.Bully.Effect(effect.Id), (source, args) => ApplyEffect(effect, source, args));
            Gate(ActionIds.Bully.Everyone(effect.Id), (source, args) => ApplyEveryone(effect, source, args));
        }

        foreach (var on in new[] { true, false })
        {
            foreach (var toggle in BullyToggles.All)
            {
                Gate(ActionIds.Bully.Toggle(toggle, on), (source, args) => Task.FromResult(SetToggle(toggle, on, source, args)));
            }

            foreach (var toggle in BullyToggles.ServerWide)
            {
                Gate(ActionIds.Bully.ServerToggle(toggle, on), (source, _) => Task.FromResult(SetServerToggle(toggle, on, source)));
            }
        }

        ActionRegistry.Register(ActionIds.Bully.GetToggles, BullyPermissions.Menu, GetToggles);
        ActionRegistry.Register(ActionIds.Bully.GetActive, BullyPermissions.Menu, GetActive);
        ActionRegistry.Register(ActionIds.Bully.StopAll, BullyPermissions.Menu, StopAll, Limit);

        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(BullyEvents.Ready, new Action<Player>(OnReady), false);
        API.OnNetEvent(BullyEvents.Result, new Action<Player, string, string>(OnResult), false);
        API.OnNetEvent(BullyEvents.TransformSwap, new Action<Player, string, string>(OnTransformSwap), false);
        API.OnNetEvent(
            BullyEvents.AbductionBeam,
            new Action<Player, string, string, string, string, string>(OnAbductionBeam),
            false);
        API.OnNetEvent(BullyEvents.Shove, new Action<Player, string, string, string>(OnShove), false);
        API.OnNetEvent(BullyEvents.Spawned, new Action<Player, string>(OnSpawned), false);
        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);

        ServerConfig.AddEventListenerFor([BullySettings.Enabled], OnEnabledChanged);

        BullyPrivateWorlds.Register();

        CleanUpAfterRestart();
    }

    private static void Gate(string actionId, Func<Player, string[], Task<ActionResponse>> handler) =>
        ActionRegistry.Register(
            actionId,
            BullyPermissions.Menu,
            (source, args) => Enabled
                ? handler(source, args)
                : Task.FromResult(ActionResponse.Refused(BullyEvents.RefusedDisabled)),
            Limit);

    private static bool TurnedOff(BullyEffect effect) =>
        effect.TurnedOffBy is { } setting && ServerConfig.Value(setting);

    private static async Task<ActionResponse> ApplyEffect(BullyEffect effect, Player source, string[] args)
    {
        if (TurnedOff(effect))
        {
            return ActionResponse.Refused(BullyEvents.RefusedTurnedOff);
        }

        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        if (PedOf(target) is not { } ped)
        {
            return ActionResponse.NotReady();
        }

        var name = NameOf(target);

        if (BullyPrivateWorlds.IsBusy(target))
        {
            return ActionResponse.Refused(BullyEvents.RefusedBusy, name);
        }

        if (Unmet(effect.Requirement, ped) is { } reason)
        {
            return ActionResponse.Refused(reason, name);
        }

        var outcome = (await ConfirmAsync([Dispatch(target, ped, effect, Option(args, 1), [target])]))[0];

        Settle(target, effect, outcome);

        Log.Info($"[Bully] {source.Name} {effect.Verb} {name} ({outcome}).");

        return outcome switch
        {
            BullyEvents.Started => ActionResponse.Ok(name),
            BullyEvents.Unconfirmed => ActionResponse.Ok(name, BullyEvents.Unconfirmed),
            _ => ActionResponse.Refused(outcome, name),
        };
    }

    private static async Task<ActionResponse> ApplyEveryone(BullyEffect effect, Player source, string[] args)
    {
        if (TurnedOff(effect))
        {
            return ActionResponse.Refused(BullyEvents.RefusedTurnedOff);
        }

        var includeSelf = Option(args, 0) == BullyEvents.On;

        var targets = ConnectedPlayers.All()
            .Where(player => includeSelf || player.ServerId != source.Handle)
            .Where(player => !BullyPrivateWorlds.IsBusy(player.ServerId))
            .Select(player => (Id: player.ServerId, Ped: PedOf(player.ServerId) ?? 0))
            .Where(target => target.Ped != 0 && Unmet(effect.Requirement, target.Ped) is null)
            .Select(target => new Target(
                target.Id,
                target.Ped,
                Native.GetEntityCoords(target.Ped),
                Native.GetPlayerRoutingBucket(Id(target.Id))))
            .ToList();

        var groups = effect.Grouped ? Group(targets) : targets.Select(target => new List<Target> { target }).ToList();

        var sent = groups
            .Select(group => (
                Host: group[0].Id,
                Size: group.Count,
                Outcome: Dispatch(group[0].Id, group[0].Ped, effect, Option(args, 1), group.Select(member => member.Id).ToList())))
            .ToList();

        var outcomes = await ConfirmAsync(sent.Select(send => send.Outcome).ToList());
        var reached = sent.Where((_, index) => outcomes[index] == BullyEvents.Started).Sum(send => send.Size);

        for (var index = 0; index < sent.Count; index++)
        {
            Settle(sent[index].Host, effect, outcomes[index]);
        }

        Log.Info($"[Bully] {source.Name} {effect.Verb} everyone ({reached} of {targets.Count} player(s)).");

        return ActionResponse.Ok(reached.ToString(CultureInfo.InvariantCulture));
    }

    private static List<List<Target>> Group(List<Target> targets)
    {
        var groups = new List<List<Target>>();
        var left = new List<Target>(targets);

        while (left.Count > 0)
        {
            var seed = left[0];
            var group = new List<Target> { seed };

            left.RemoveAt(0);

            foreach (var other in left.OrderBy(other => Vector3.Distance(other.Position, seed.Position)).ToList())
            {
                if (group.All(member => member.Bucket == other.Bucket
                    && Vector3.Distance(member.Position, other.Position) <= GroupRadius))
                {
                    group.Add(other);
                    left.Remove(other);
                }
            }

            groups.Add(group);
        }

        return groups;
    }

    private static void Settle(int target, BullyEffect effect, string outcome)
    {
        if (effect.Id == BullyEffects.Haircut && outcome is not (BullyEvents.Started or BullyEvents.Unconfirmed))
        {
            BullyPrivateWorlds.Refused(target);
        }

        if (effect.Id == BullyEffects.Transform
            && outcome is not (BullyEvents.Started or BullyEvents.Unconfirmed or BullyEvents.RefusedBusy)
            && Transforms.TryGetValue(target, out var transform)
            && transform.Replacement == 0
            && !transform.Busy)
        {
            Transforms.Remove(target);
        }
    }

    private static Task<string> Dispatch(int target, int ped, BullyEffect effect, string option, IReadOnlyCollection<int> victims)
    {
        var expires = Now + EffectWindowMs;

        switch (effect.Id)
        {
            case BullyEffects.Transform when !Transforms.TryGetValue(target, out var running) || Now > running.ExpiresAt:
                Transforms[target] = new Transform(expires, Native.NetworkGetNetworkIdFromEntity(Native.GetVehiclePedIsIn(ped, false)));
                break;
            case BullyEffects.Abduct:
                Abductions[target] = expires;
                break;
            case BullyEffects.Haircut:
                BullyPrivateWorlds.Allow(target, expires);
                break;
            case BullyEffects.Mug:
                Mugs[target] = new MugRound(expires, victims.Where(victim => victim != target).ToHashSet());
                break;
        }

        if (effect.SpawnsEntities && effect.Id != BullyEffects.Transform)
        {
            SpawnWindows[target] = new SpawnWindow(expires);
        }

        var requestId = ++_lastRequestId;
        var pending = new PendingEffect(target, new TaskCompletionSource<string>());

        Pending[requestId] = pending;

        API.EmitClient(
            target,
            BullyEvents.Apply,
            Id(requestId),
            effect.Id,
            option,
            string.Join(BullyEvents.ListSeparator, victims.Select(Id)));

        return pending.Outcome.Task;
    }

    private static async Task<string[]> ConfirmAsync(IReadOnlyList<Task<string>> outcomes)
    {
        try
        {
            await Task.WhenAny(Task.WhenAll(outcomes), API.Delay(ConfirmMs));
        }
        finally
        {
            await API.Delay(0);
        }

        foreach (var unanswered in Pending.Where(pair => outcomes.Contains(pair.Value.Outcome.Task)).Select(pair => pair.Key).ToList())
        {
            Pending.Remove(unanswered);
        }

        return outcomes.Select(outcome => outcome.IsCompletedSuccessfully ? outcome.Result : BullyEvents.Unconfirmed).ToArray();
    }

    private static void OnResult([FromSource] Player source, string requestId, string outcome)
    {
        if (TryInt(requestId, out var id)
            && Pending.TryGetValue(id, out var pending)
            && pending.Target == source.Handle)
        {
            Pending.Remove(id);

            pending.Outcome.TrySetResult(outcome);
        }
    }

    private static string? Unmet(BullyRequirement requirement, int ped)
    {
        var vehicle = Native.GetVehiclePedIsIn(ped, false);

        return requirement switch
        {
            BullyRequirement.OnFoot when vehicle != 0 => BullyEvents.RefusedOnFoot,
            BullyRequirement.Driving when vehicle == 0 || Native.GetPedInVehicleSeat(vehicle, DriverSeat) != ped =>
                BullyEvents.RefusedDriving,
            _ => null,
        };
    }

    private static ActionResponse SetToggle(string toggle, bool on, Player source, string[] args)
    {
        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        if (on && BullyPrivateWorlds.IsBusy(target))
        {
            return ActionResponse.Refused(BullyEvents.RefusedBusy, NameOf(target));
        }

        SetPlayerToggle(target, toggle, on);

        Log.Info($"[Bully] {source.Name} turned {toggle} {(on ? "on" : "off")} for {NameOf(target)}.");

        return ActionResponse.Ok(on ? BullyEvents.On : BullyEvents.Off, NameOf(target));
    }

    private static void SetPlayerToggle(int target, string toggle, bool on)
    {
        if (!Toggles.TryGetValue(target, out var active))
        {
            Toggles[target] = active = new HashSet<string>(StringComparer.Ordinal);
        }

        _ = on ? active.Add(toggle) : active.Remove(toggle);

        if (active.Count == 0)
        {
            Toggles.Remove(target);
        }

        if (toggle == BullyToggles.Lod)
        {
            PublishLod();
        }

        API.EmitClient(target, BullyEvents.Toggle, toggle, on ? BullyEvents.On : BullyEvents.Off, BullyEvents.PlayerScope);
    }

    private static ActionResponse SetServerToggle(string toggle, bool on, Player source)
    {
        _ = on ? ServerToggles.Add(toggle) : ServerToggles.Remove(toggle);

        EmitAll(BullyEvents.Toggle, toggle, on ? BullyEvents.On : BullyEvents.Off, BullyEvents.ServerScope);

        Log.Info($"[Bully] {source.Name} turned the server wide {toggle} {(on ? "on" : "off")}.");

        return ActionResponse.Ok(on ? BullyEvents.On : BullyEvents.Off);
    }

    private static ActionResponse GetToggles(Player source, string[] args) =>
        TryResolveTarget(args, out var target) && Toggles.TryGetValue(target, out var active)
            ? ActionResponse.Ok(active.ToArray())
            : ActionResponse.Ok();

    private static ActionResponse GetActive(Player source, string[] args) =>
        ActionResponse.Ok(ServerToggles
            .Select(toggle => $"{BullyEvents.ServerScope}{BullyEvents.Separator}{toggle}")
            .Concat(Toggles.SelectMany(pair => pair.Value.Select(toggle =>
                $"{BullyEvents.PlayerScope}{BullyEvents.Separator}{toggle}{BullyEvents.Separator}{Id(pair.Key)}{BullyEvents.Separator}{NameOf(pair.Key)}")))
            .ToArray());

    private static ActionResponse StopAll(Player source, string[] args)
    {
        StopEverything();

        Log.Info($"[Bully] {source.Name} stopped every bully effect.");

        return ActionResponse.Ok();
    }

    private static void StopEverything()
    {
        Toggles.Clear();
        ServerToggles.Clear();
        Abductions.Clear();
        Mugs.Clear();

        PublishLod();

        EmitAll(BullyEvents.StopAll);
    }

    private static void OnEnabledChanged()
    {
        if (!Enabled)
        {
            StopEverything();
        }
    }

    private static void OnReady([FromSource] Player source)
    {
        if (!Enabled)
        {
            return;
        }

        foreach (var toggle in ServerToggles)
        {
            API.EmitClient(source.Handle, BullyEvents.Toggle, toggle, BullyEvents.On, BullyEvents.ServerScope);
        }
    }

    private static void OnTransformSwap([FromSource] Player source, string fromNetId, string toNetId) =>
        _ = TransformSwapAsync(source.Handle, fromNetId, toNetId);

    private static async Task TransformSwapAsync(int source, string fromNetId, string toNetId)
    {
        if (!Transforms.TryGetValue(source, out var transform)
            || transform.Busy
            || !TryInt(fromNetId, out var from)
            || !TryInt(toNetId, out var to))
        {
            return;
        }

        transform.Busy = true;

        try
        {
            if (transform.Replacement == 0)
            {
                await SwapInAsync(source, transform, from, to);
            }
            else if (from == transform.Replacement && to == transform.Original)
            {
                Transforms.Remove(source);

                SwapBack(source, transform);
            }
        }
        catch (Exception exception)
        {
            Log.Error($"[Bully] Transform swap for {source} failed: {exception}");
        }
        finally
        {
            transform.Busy = false;
        }
    }

    // The replacement is created by the target's own client a moment before it reports the swap, so it
    // may not have reached the server yet. Waited for briefly rather than refused outright.
    private static async Task SwapInAsync(int source, Transform transform, int from, int to)
    {
        if (from != transform.Original || Now > transform.ExpiresAt)
        {
            return;
        }

        var settleBy = Now + SwapSettleMs;

        while (!OwnedVehicle(to, source) && Now < settleBy)
        {
            await API.Delay(100);
        }

        if (!OwnedVehicle(to, source) || Entity(from) is not { } original)
        {
            return;
        }

        transform.Replacement = to;

        if (Entity(to) is { } replacement)
        {
            Record(to, source, ModelOf(replacement));
        }

        foreach (var (occupant, seat) in Occupants(original, source))
        {
            transform.Passengers[occupant] = seat;

            API.EmitClient(occupant, BullyEvents.TransformSeat, Id(to), Id(seat));
        }
    }

    private static void SwapBack(int source, Transform transform)
    {
        var passengers = Entity(transform.Replacement) is { } replacement
            ? Occupants(replacement, source)
            : transform.Passengers.Where(pair => Native.DoesPlayerExist(Id(pair.Key))).Select(pair => (pair.Key, pair.Value)).ToList();

        foreach (var (passenger, seat) in passengers)
        {
            API.EmitClient(passenger, BullyEvents.TransformSeat, Id(transform.Original), Id(seat));
        }
    }

    private static List<(int Player, int Seat)> Occupants(int vehicle, int except)
    {
        var players = ConnectedPlayers.All();
        var occupants = new List<(int Player, int Seat)>();

        for (var seat = DriverSeat; seat < MaxSeat; seat++)
        {
            if (ConnectedPlayers.Owning(players, Native.GetPedInVehicleSeat(vehicle, seat)) is { } occupant
                && occupant.ServerId != except)
            {
                occupants.Add((occupant.ServerId, seat));
            }
        }

        return occupants;
    }

    private static int? Entity(int networkId)
    {
        var entity = networkId == 0 ? 0 : Native.NetworkGetEntityFromNetworkId(networkId);

        return entity != 0 && Native.DoesEntityExist(entity) ? entity : null;
    }

    private static bool OwnedVehicle(int networkId, int owner) =>
        Entity(networkId) is { } entity
        && Native.GetEntityType(entity) == VehicleType
        && Native.NetworkGetEntityOwner(entity) == owner;

    private static void OnAbductionBeam([FromSource] Player source, string state, string x, string y, string z, string height)
    {
        if (!Abductions.TryGetValue(source.Handle, out var expires)
            || Now > expires
            || !float.TryParse(height, NumberStyles.Float, CultureInfo.InvariantCulture, out var beamHeight)
            || beamHeight is < 0f or > MaxBeamHeight)
        {
            return;
        }

        if (state != BullyEvents.On)
        {
            Abductions.Remove(source.Handle);
        }

        var id = Id(source.Handle);
        var bucket = Native.GetPlayerRoutingBucket(id);

        foreach (var player in ConnectedPlayers.All())
        {
            if (player.ServerId != source.Handle && Native.GetPlayerRoutingBucket(Id(player.ServerId)) == bucket)
            {
                API.EmitClient(player.ServerId, BullyEvents.ShowAbductionBeam, id, state, x, y, z, height);
            }
        }
    }

    private static void OnShove([FromSource] Player source, string victimId, string x, string y)
    {
        if (!Mugs.TryGetValue(source.Handle, out var round)
            || Now > round.ExpiresAt
            || !TryInt(victimId, out var victim)
            || !round.Victims.Remove(victim)
            || !float.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var pushX)
            || !float.TryParse(y, NumberStyles.Float, CultureInfo.InvariantCulture, out var pushY)
            || !Native.DoesPlayerExist(Id(victim)))
        {
            return;
        }

        var push = new Vector2(pushX, pushY);

        if (push.Length() > MaxShove)
        {
            push = Vector2.Normalize(push) * MaxShove;
        }

        API.EmitClient(
            victim,
            BullyEvents.Shoved,
            push.X.ToString(CultureInfo.InvariantCulture),
            push.Y.ToString(CultureInfo.InvariantCulture));
    }

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (source <= 0)
        {
            return;
        }

        if (Toggles.Remove(source))
        {
            PublishLod();
        }

        Transforms.Remove(source);
        Abductions.Remove(source);
        Mugs.Remove(source);

        BullyPrivateWorlds.Forget(source);

        foreach (var pending in Pending.Values.Where(pending => pending.Target == source).ToList())
        {
            pending.Outcome.TrySetResult(BullyEvents.RefusedFailed);
        }

        SpawnWindows.Remove(source);

        DeleteSpawned(source);
    }

    private static void OnSpawned([FromSource] Player source, string networkId) =>
        _ = SpawnedAsync(source.Handle, networkId);

    private static async Task SpawnedAsync(int source, string networkId)
    {
        try
        {
            if (!SpawnWindows.TryGetValue(source, out var window)
                || Now > window.ExpiresAt
                || window.Count >= MaxSpawnsPerEffect
                || !TryInt(networkId, out var id)
                || (Spawned.TryGetValue(id, out var existing) && Still(id, existing) is not null))
            {
                return;
            }

            window.Count++;

            var settleBy = Now + SwapSettleMs;

            while (!Owned(id, source) && Now < settleBy)
            {
                await API.Delay(100);
            }

            if (Entity(id) is { } entity && Owned(id, source) && SpawnedModels.Contains(ModelOf(entity)))
            {
                Record(id, source, ModelOf(entity));
            }
        }
        catch (Exception exception)
        {
            Log.Error($"[Bully] Recording a spawn for {source} failed: {exception}");
        }
    }

    private static bool Owned(int networkId, int owner) =>
        Entity(networkId) is { } entity && Native.NetworkGetEntityOwner(entity) == owner;

    private static void Record(int networkId, int owner, uint model)
    {
        Spawned[networkId] = new SpawnRecord(owner, model);

        PersistSpawned();
    }

    private static void PersistSpawned()
    {
        foreach (var gone in Spawned.Where(pair => Still(pair.Key, pair.Value) is null).Select(pair => pair.Key).ToList())
        {
            Spawned.Remove(gone);
        }

        ServerStateBags.Set(
            BullyEvents.GlobalBag,
            BullyEvents.SpawnedKey,
            Spawned.SelectMany(pair => new[] { pair.Key, pair.Value.Owner, (int)pair.Value.Model }).ToArray(),
            replicated: false);
    }

    private static int? Still(int networkId, SpawnRecord record) =>
        Entity(networkId) is { } entity && ModelOf(entity) == record.Model ? entity : null;

    private static void CleanUpAfterRestart()
    {
        var saved = ServerStateBags.Get<int[]>(BullyEvents.GlobalBag, BullyEvents.SpawnedKey) ?? [];

        for (var index = 0; index + 2 < saved.Length; index += 3)
        {
            Spawned[saved[index]] = new SpawnRecord(saved[index + 1], (uint)saved[index + 2]);
        }

        DeleteSpawned(null);

        PersonalVehicleRegistry.PublishElectrified();
        PublishLod();
    }

    private static void DeleteSpawned(int? owner)
    {
        if (Spawned.Count == 0)
        {
            return;
        }

        var players = ConnectedPlayers.All();

        foreach (var (networkId, record) in Spawned.ToList())
        {
            if (owner is { } id && record.Owner != id)
            {
                continue;
            }

            if (Still(networkId, record) is not { } entity)
            {
                Spawned.Remove(networkId);
            }
            else if (!HoldsPlayer(players, entity))
            {
                Native.DeleteEntity(entity);

                Spawned.Remove(networkId);
            }
        }

        PersistSpawned();
    }

    private static bool HoldsPlayer(IReadOnlyList<ConnectedPlayer> players, int entity) =>
        Native.GetEntityType(entity) switch
        {
            PedType => ConnectedPlayers.Owning(players, entity) is not null,
            VehicleType => Enumerable.Range(DriverSeat, MaxSeat + 1)
                .Any(seat => ConnectedPlayers.Owning(players, Native.GetPedInVehicleSeat(entity, seat)) is not null),
            _ => false,
        };

    private static void PublishLod() =>
        Native.SetConvarReplicated(
            BullyEvents.LodConvar,
            string.Join(BullyEvents.ListSeparator, Toggles.Where(pair => pair.Value.Contains(BullyToggles.Lod)).Select(pair => Id(pair.Key))));

    private static uint ModelOf(int entity) => (uint)Native.GetEntityModel(entity);

    private static void EmitAll(string eventName, params object[] args)
    {
        foreach (var player in ConnectedPlayers.All())
        {
            API.EmitClient(player.ServerId, eventName, args);
        }
    }

    private static string Option(string[] args, int index) => args.Length > index ? args[index] : string.Empty;

    private static string Id(int serverId) => serverId.ToString(CultureInfo.InvariantCulture);

    private static bool TryInt(string value, out int result) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

    private static int? PedOf(int serverId)
    {
        var ped = Native.GetPlayerPed(Id(serverId));

        return ped != 0 && Native.DoesEntityExist(ped) ? ped : null;
    }

    private static string NameOf(int serverId) => Native.GetPlayerName(Id(serverId));

    private static bool TryResolveTarget(string[] args, out int serverId) =>
        TryInt(Option(args, 0), out serverId) && Native.DoesPlayerExist(Id(serverId));

    private sealed record Target(int Id, int Ped, Vector3 Position, int Bucket);

    private sealed record PendingEffect(int Target, TaskCompletionSource<string> Outcome);

    private sealed record MugRound(long ExpiresAt, HashSet<int> Victims);

    private sealed record SpawnRecord(int Owner, uint Model);

    private sealed class SpawnWindow(long expiresAt)
    {
        public long ExpiresAt { get; } = expiresAt;

        public int Count { get; set; }
    }

    private sealed class Transform(long expiresAt, int original)
    {
        public long ExpiresAt { get; } = expiresAt;

        public int Original { get; } = original;

        public int Replacement { get; set; }

        public bool Busy { get; set; }

        public Dictionary<int, int> Passengers { get; } = [];
    }
}
