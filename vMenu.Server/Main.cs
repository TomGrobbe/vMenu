using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json;

using FxEvents;
using FxEvents.Shared.Snowflakes;

using Logger;
using vMenu.Server.Events;
using System.Data;
using vMenu.Shared.Objects;

namespace vMenu.Server
{
    public class Main : BaseScript
    {
        public static PlayerList PlayerList;
        public static Dictionary<string, string> UpdatedPerms = new();
        public Main()
        {
            PlayerList = Players;

            EventDispatcher.Initalize("vMenu:Inbound", "vMenu:Outbound", "vMenu:Signature");
            EventHandlers.Add("vMenu:UpdatePerms", new Action<int, string>(UpdatePerms));

            Debug.WriteLine("vMenu has started.");
        }
         private void UpdatePerms(int targetPlayer, string permissions)
        {

            string player = targetPlayer.ToString();

            string license = API.GetPlayerIdentifierByType(player, "license");
            if (Main.UpdatedPerms.TryGetValue(license, out var _))
            {
                UpdatedPerms[license] = permissions;
            }
            else
            {
                UpdatedPerms.Add(license, permissions);
            }

            foreach (var user in Players)
            {
                var val = user.Handle == targetPlayer.ToString();
                if (val)
                {
                    user.TriggerEvent("vMenu:RestartMenu");
                }
            }
        }       
    }
}
