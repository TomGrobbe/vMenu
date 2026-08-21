using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;

using PersonalVehicleSettings = vMenu.Enhanced.Data.Configuration.Settings.PersonalVehicle;

namespace vMenu.Enhanced.Actions.Server;

public static class RemoteVehicleControl
{
    private const int MaxCandidates = 3;

    private static readonly Dictionary<int, PendingPerform> Unanswered = [];

    private static int _lastRequestId;

    private static bool _registered;

    public static void RegisterEventHandlers()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(PersonalVehicleEvents.Performed, new Action<Player, string, bool>(OnPerformed), false);
    }

    public static async Task<ActionResponse> PerformAsync(
        Player owner,
        int networkId,
        int entity,
        string action,
        params string[] args)
    {
        var candidates = Candidates(owner, entity);

        if (candidates.Count == 0)
        {
            return ActionResponse.NotReady();
        }

        var timeoutMs = ServerConfig.Value(PersonalVehicleSettings.ControlTimeout);

        foreach (var candidate in candidates)
        {
            if (await AskAsync(candidate, networkId, action, args, timeoutMs))
            {
                Log.Debug($"[PersonalVehicle] '{action}' on {networkId} was carried out by {candidate}.");

                return ActionResponse.Ok();
            }
        }

        Log.Info(
            $"[PersonalVehicle] {owner.Name} asked for '{action}' on {networkId}, "
            + $"which none of the {candidates.Count} player(s) near it could carry out.");

        return ActionResponse.NotReady();
    }

    private static async Task<bool> AskAsync(int target, int networkId, string action, string[] args, int timeoutMs)
    {
        var requestId = ++_lastRequestId;
        var answered = new TaskCompletionSource<bool>();

        Unanswered[requestId] = new PendingPerform(target, answered);

        API.EmitClient(
            target,
            PersonalVehicleEvents.Perform,
            requestId.ToString(CultureInfo.InvariantCulture),
            networkId.ToString(CultureInfo.InvariantCulture),
            action,
            args);

        try
        {
            var timeout = API.Delay(timeoutMs);

            return await Task.WhenAny(answered.Task, timeout) != timeout && answered.Task.Result;
        }
        finally
        {
            Unanswered.Remove(requestId);

            await API.Delay(0);
        }
    }

    private static List<int> Candidates(Player owner, int entity)
    {
        var position = Native.GetEntityCoords(entity);
        var bucket = Native.GetEntityRoutingBucket(entity);
        var range = ServerConfig.Value(PersonalVehicleSettings.ControlRange);

        var reachable = new List<(int ServerId, float DistanceSquared)>();

        foreach (var player in ConnectedPlayers.All())
        {
            var handle = player.ServerId.ToString(CultureInfo.InvariantCulture);
            var ped = Native.GetPlayerPed(handle);

            if (ped == 0 || !Native.DoesEntityExist(ped) || Native.GetPlayerRoutingBucket(handle) != bucket)
            {
                continue;
            }

            var distanceSquared = Vector3.DistanceSquared(Native.GetEntityCoords(ped), position);

            if (distanceSquared > range * range)
            {
                continue;
            }

            reachable.Add((player.ServerId, distanceSquared));
        }

        reachable.Sort(ByDistance);

        var picked = new List<int>();

        foreach (var entry in reachable)
        {
            if (entry.ServerId == owner.Handle)
            {
                picked.Insert(0, entry.ServerId);

                continue;
            }

            picked.Add(entry.ServerId);
        }

        if (picked.Count > MaxCandidates)
        {
            picked.RemoveRange(MaxCandidates, picked.Count - MaxCandidates);
        }

        return picked;
    }

    private static int ByDistance((int ServerId, float DistanceSquared) left, (int ServerId, float DistanceSquared) right) =>
        left.DistanceSquared.CompareTo(right.DistanceSquared);

    private static void OnPerformed([FromSource] Player source, string requestId, bool carriedOut)
    {
        if (!int.TryParse(requestId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            || !Unanswered.TryGetValue(id, out var pending))
        {
            return;
        }

        if (pending.Target != source.Handle)
        {
            Log.Warning($"[PersonalVehicle] {source.Name} answered request {id}, which was not theirs. Ignored.");

            return;
        }

        pending.Answered.TrySetResult(carriedOut);
    }

    private sealed class PendingPerform(int target, TaskCompletionSource<bool> answered)
    {
        public int Target { get; } = target;

        public TaskCompletionSource<bool> Answered { get; } = answered;
    }
}
