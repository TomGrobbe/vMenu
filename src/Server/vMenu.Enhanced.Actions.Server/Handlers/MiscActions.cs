using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Misc;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;

using MiscSettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.MiscSettings;
using MiscSettingsSettings = vMenu.Enhanced.Data.Configuration.Settings.MiscSettings;
using OnlinePlayerSettings = vMenu.Enhanced.Data.Configuration.Settings.OnlinePlayers;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class MiscActions
{
    private static readonly ActionRateLimit Limit = new(
        "clear area",
        OnlinePlayerSettings.ActionLimit,
        OnlinePlayerSettings.ActionLimitSeconds);

    private static bool _reportedRadius;

    public static void Register() =>
        ActionRegistry.Register(
            ActionIds.MiscSettings.ClearArea,
            MiscSettingsPermissions.ClearArea,
            ClearArea,
            Limit);

    private static ActionResponse ClearArea(Player source, string[] args)
    {
        var ped = source.PedIndex;

        if (ped <= 0 || !Native.DoesEntityExist(ped))
        {
            return ActionResponse.NotFound();
        }

        var position = Native.GetEntityCoords(ped);
        var bucket = Native.GetPlayerRoutingBucket(source.Handle.ToString(CultureInfo.InvariantCulture));
        var radius = Radius();
        var cleared = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            var handle = player.ServerId.ToString(CultureInfo.InvariantCulture);

            if (Native.GetPlayerRoutingBucket(handle) != bucket)
            {
                continue;
            }

            API.EmitClient(player.ServerId, MiscEvents.ClearArea, position.X, position.Y, position.Z, radius);

            cleared++;
        }

        Log.Debug($"[Misc] {source.Name} cleared {radius}m around themselves for {cleared} player(s).");

        return ActionResponse.Ok();
    }

    private static float Radius()
    {
        var configured = ServerConfig.Value(MiscSettingsSettings.ClearAreaRadius);
        var clamped = MiscSettingsSettings.ClampClearAreaRadius(configured);

        if (clamped != configured && !_reportedRadius)
        {
            _reportedRadius = true;

            Log.Warning(
                $"{MiscSettingsSettings.ClearAreaRadius.Name} is set to {configured}, which is outside "
                + $"{MiscSettingsSettings.MinClearAreaRadius} to {MiscSettingsSettings.MaxClearAreaRadius}. Using {clamped}.");
        }

        return clamped;
    }
}
