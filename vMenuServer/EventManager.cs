using GHMatti.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuServer
{
    public class EventManager : BaseScript
    {
        public EventManager()
        {
            UpdateChecker uc = new UpdateChecker();
            EventHandlers.Add("vMenu:SummonPlayer", new Action<Player, Player>(SummonPlayer));
            EventHandlers.Add("vMenu:KillPlayer", new Action<Player, Player>(KillPlayer));
            EventHandlers.Add("vMenu:KickPlayer", new Action<Player, Player, string>(KickPlayer));
            EventHandlers.Add("vMenu:RequestPermissions", new Action<Player>(SendPermissions));
        }

        private void KickPlayer([FromSource] Player source, [FromSource] Player target, string kickReason = "You have been kicked from the server.")
        {
            if (!IsPlayerAceAllowed(target.Handle, "vMenu.dontkick"))
            {
                DropPlayer(target.Handle, kickReason);
            }
            else
            {
                TriggerClientEvent(player: source, eventName: "vMenu:KickCallback", args: "Sorry, this player can ~r~not ~w~be kicked.");
            }
        }

        private void KillPlayer([FromSource] Player source, [FromSource] Player target)
        {
            TriggerClientEvent(player: target, eventName: "vMenu:KillMe");
        }

        private void SummonPlayer([FromSource] Player source, [FromSource]Player target)
        {
            TriggerClientEvent(player: target, eventName: "vMenu:GoToPlayer", args: source.Handle);
        }

        private void SendPermissions([FromSource] Player player)
        {
            Dictionary<string, bool> permissions = new Dictionary<string, bool>
            {
                {"playerOptions", IsPlayerAceAllowed(player.Handle, "vMenu.playerOptions") },
                {"onlinePlayers", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers") },
                {"onlinePlayers_kick", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_kick") },
                {"onlinePlayers_teleport", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_teleport") },
                {"onlinePlayers_waypoint", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_waypoint") },
                {"onlinePlayers_spectate", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_spectate") },
                {"vehicleOptions", IsPlayerAceAllowed(player.Handle, "vMenu.vehicleOptions") },
                {"spawnVehicle", IsPlayerAceAllowed(player.Handle, "vMenu.spawnVehicle") },
                {"weatherOptions", IsPlayerAceAllowed(player.Handle, "vMenu.weatherOptions") },
                {"timeOptions", IsPlayerAceAllowed(player.Handle, "vMenu.timeOptions") },
                {"noclip", IsPlayerAceAllowed(player.Handle, "vMenu.noclip") },
                {"voiceChat", IsPlayerAceAllowed(player.Handle, "vMenu.voiceChat") },
            };

            //foreach (KeyValuePair<string, bool> dict in permissions)
            //{
            //Debug.WriteLine($"Key: {dict.Key}\r\nValue: {dict.Value}");
            //}
            TriggerClientEvent(player, "vMenu:SetPermissions", permissions);
        }
    }
}
