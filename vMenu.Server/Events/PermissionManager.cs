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

namespace vMenu.Server.Events
{
    public class PermissionManager : BaseScript
    {
        public PermissionManager()
        {
            EventDispatcher.Mount("RequestPermissions", new Func<int, Task<string>>(RequestPermissions));
        }
        private async Task<string> RequestPermissions([FromSource] int source)
        {
            var perms = new Dictionary<vMenu.Shared.Enums.PermissionList, bool>();
            foreach (var p in Enum.GetValues(typeof(vMenu.Shared.Enums.PermissionList)))
            {
                var permission = GetAceName((vMenu.Shared.Enums.PermissionList)p);
                if (!perms.ContainsKey((vMenu.Shared.Enums.PermissionList)p))
                {
                    perms.Add((vMenu.Shared.Enums.PermissionList)p, IsPlayerAceAllowed(source.ToString(), permission)); // triggers IsAllowedServer
                }
            }
            await Delay(10);
            return JsonConvert.SerializeObject(perms);;
        }
        private static string GetAceName(vMenu.Shared.Enums.PermissionList permission)
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
                default:
                    return prefix + name;
            }

            return prefix + "." + name.Substring(2);
        }
    }
}
