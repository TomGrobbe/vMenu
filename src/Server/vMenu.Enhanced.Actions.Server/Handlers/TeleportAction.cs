using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;

using TeleportMenuPermissions = vMenu.Enhanced.Data.Permissions.Menus.TeleportMenu;

using System.Text.Json;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class TeleportActions
{
    private const string ConfigFile = "config/teleport-categories.json";
 
    public static void Register() =>
        ActionRegistry.Register(
            ActionIds.TeleportMenu.TeleportCategories,
            TeleportMenuPermissions.Category,
            TeleportCategories);
    
    private static ActionResponse TeleportCategories(Player source, string[] args)
    {
        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);
        return new ActionResponse(ActionStatus.Ok,[contents] );
    }
    
}
