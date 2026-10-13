using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.Props;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class PropSpawnActions
{
    private const string DroppedEvent = "playerDropped";

    private const string DumpCommand = "vmenu_props";

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(PropEvents.Spawned, new Action<Player, int>(OnSpawned), false);
        API.OnNetEvent(PropEvents.Removed, new Action<Player, int>(OnRemoved), false);

        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);

        SharedAPI.Commands.RegisterCommand(DumpCommand, true, DebugCommands.Gate(Dump));
    }

    private static void Dump()
    {
        if (SpawnedPropRegistry.PlayerCount == 0)
        {
            Log.Info("[Props] No networked props have been spawned through vMenu.");

            return;
        }

        foreach (var line in SpawnedPropRegistry.Describe())
        {
            Log.Info("[Props]   " + line);
        }
    }

    private static void OnSpawned([FromSource] Player source, int networkId)
    {
        if (networkId == 0)
        {
            return;
        }

        SpawnedPropRegistry.RecordSpawn(source.Handle, networkId);

        Log.Debug($"[Props] {source} spawned a networked prop, network id {networkId}.");
    }

    private static void OnRemoved([FromSource] Player source, int networkId)
    {
        if (networkId == 0)
        {
            return;
        }

        SpawnedPropRegistry.RecordRemoval(source.Handle, networkId);

        Log.Debug($"[Props] {source} removed a networked prop, network id {networkId}.");
    }

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (source > 0)
        {
            SpawnedPropRegistry.Drop(source);
        }
    }
}
