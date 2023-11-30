using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using FxEvents;
using FxEvents.Shared.EventSubsystem;
using FxEvents.Shared.Snowflakes;
using FxEvents.Shared.TypeExtensions;

using Newtonsoft.Json;

using vMenu.Server.Functions;
using vMenu.Shared.Objects;

using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.PlayerList;
using vMenu.Shared.Enums;
namespace vMenu.Server.Events
{
    public class PermissionManager : BaseScript
    {
        public PermissionManager()
        {
            EventDispatcher.Mount("RequestPermissions", new Func<int, Task<string>>(RequestPermissions));
        }
        private async Task<string> RequestPermissions(int source)
        {
            var perms = new Dictionary<Permission, bool>();

            string playerstring = source.ToString();
            string license = GetPlayerIdentifierByType(playerstring, "license");
            if (Main.UpdatedPerms.Count != 0)
            {
                if (Main.UpdatedPerms.TryGetValue(license, out var playerperms))
                {
                    return playerperms;
                }
            }
            if (!Convar.GetSettingsBool("vmenu_use_permissions"))
            {
                foreach (var p in Enum.GetValues(typeof(Permission)))
                {
                    var permission = (Permission)p;
                    switch (permission)
                    {
                        // don't allow any of the following permissions if perms are ignored.
                        case Permission.Everything:
                        case Permission.Staff:
                            perms.Add(permission, false);
                            break;
                        // do allow the rest
                        default:
                            perms.Add(permission, true);
                            break;
                    }
                }
            }
            else
            {
                foreach (var p in Enum.GetValues(typeof(Permission)))
                {
                    var permission = GetAceName((Permission)p);
                    if (!perms.ContainsKey((Permission)p))
                    {
                        perms.Add((Permission)p, IsPlayerAceAllowed(playerstring, permission)); // triggers IsAllowedServer
                    }
                }
            }
            await Delay(10);
            return JsonConvert.SerializeObject(perms);;
        }
        public static string GetAceName(Permission permission)
        {
            var name = permission.ToString();

            var prefix = "vMenu.";

            switch (name.Substring(0, 2))
            {
                case "WR":
                    prefix += "WorldRelatedOptions";
                    break;
                case "VO":
                    prefix += "VehicleOptions";
                    break;
                case "PO":
                    prefix += "PlayerOptions";
                    break;
                case "VC":
                    prefix += "VoiceChat";
                    break;
                case "VS":
                    prefix += "VehicleSpawner";
                    break;
                default:
                    return prefix + name;
            }

            return prefix + "." + name.Substring(2);
        }
    }
}
