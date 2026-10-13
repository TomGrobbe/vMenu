using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.Props;
using vMenu.Enhanced.Logging;

using vMenu.Legacy.Hosting;

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

        Script.EventHandlers[PropEvents.Spawned] += new Action<Player, int>(OnSpawned);
        Script.EventHandlers[PropEvents.Removed] += new Action<Player, int>(OnRemoved);

        Script.EventHandlers[DroppedEvent] += new Action<string, string?>(OnPlayerDropped);

        Commands.Register(DumpCommand, true, DebugCommands.Gate(Dump));
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

        SpawnedPropRegistry.RecordSpawn(source.ServerId(), networkId);

        Log.Debug($"[Props] {source} spawned a networked prop, network id {networkId}.");
    }

    private static void OnRemoved([FromSource] Player source, int networkId)
    {
        if (networkId == 0)
        {
            return;
        }

        SpawnedPropRegistry.RecordRemoval(source.ServerId(), networkId);

        Log.Debug($"[Props] {source} removed a networked prop, network id {networkId}.");
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
            SpawnedPropRegistry.Drop(source);
        }
    }
}
