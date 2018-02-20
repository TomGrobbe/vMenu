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
        private List<string> aceNames = new List<string>() {
            // Menu access
            "vMenu.menus.*",
            "vMenu.menus.onlinePlayers",
            "vMenu.menus.playerOptions",
            "vMenu.menus.vehicleOptions",
            "vMenu.menus.vehicleSpawner",
            "vMenu.menus.playerSkin",
            "vMenu.menus.savedVehicles",
            "vMenu.menus.time",
            "vMenu.menus.weather",
            "vMenu.menus.weapons",
            "vMenu.menus.misc",
            "vMenu.menus.voicechat",

            // Online Players
            "vMenu.onlinePlayers.*",
            "vMenu.onlinePlayers.teleport",
            "vMenu.onlinePlayers.waypoint",
            "vMenu.onlinePlayers.spectate",
            "vMenu.onlinePlayers.summon",
            "vMenu.onlinePlayers.kill",
            "vMenu.onlinePlayers.kick",

            // Player Options
            "vMenu.playerOptions.*",
            "vMenu.playerOptions.god",
            "vMenu.playerOptions.invisible",
            "vMenu.playerOptions.stamina",
            "vMenu.playerOptions.fastrun",
            "vMenu.playerOptions.fastswim",
            "vMenu.playerOptions.superjump",
            "vMenu.playerOptions.noragdoll",
            "vMenu.playerOptions.neverwanted",
            "vMenu.playerOptions.setwanted",
            "vMenu.playerOptions.ignore",
            "vMenu.playerOptions.options",
            "vMenu.playerOptions.actions",
            "vMenu.playerOptions.scenarios",
            "vMenu.playerOptions.freeze",

            // Vehicle Options
            "vMenu.vehicleOptions.*",
            "vMenu.vehicleOptions.god",
            "vMenu.vehicleOptions.fix",
            "vMenu.vehicleOptions.clean",
            "vMenu.vehicleOptions.dirt",
            "vMenu.vehicleOptions.plate",
            "vMenu.vehicleOptions.mods",
            "vMenu.vehicleOptions.colors",
            "vMenu.vehicleOptions.liveries",
            "vMenu.vehicleOptions.extras",
            "vMenu.vehicleOptions.doors",
            "vMenu.vehicleOptions.windows",
            "vMenu.vehicleOptions.delete",
            "vMenu.vehicleOptions.freeze",
            "vMenu.vehicleOptions.torque",
            "vMenu.vehicleOptions.power",
            "vMenu.vehicleOptions.flip",
            "vMenu.vehicleOptions.alarm",
            "vMenu.vehicleOptions.cycleseats",
            "vMenu.vehicleOptions.engine",
            "vMenu.vehicleOptions.nosiren",
            "vMenu.vehicleOptions.nohelmet",

            // Vehicle Spawner
            "vMenu.vehicleSpawner.*",
            "vMenu.vehicleSpawner.spawninside",
            "vMenu.vehicleSpawner.replace",
            "vMenu.vehicleSpawner.spawnbyname",
            "vMenu.vehicleSpawner.category.*",
            "vMenu.vehicleSpawner.category.compacts",
            "vMenu.vehicleSpawner.category.sedans",
            "vMenu.vehicleSpawner.category.suv",
            "vMenu.vehicleSpawner.category.coupes",
            "vMenu.vehicleSpawner.category.muscle",
            "vMenu.vehicleSpawner.category.sportsclassic",
            "vMenu.vehicleSpawner.category.sports",
            "vMenu.vehicleSpawner.category.super",
            "vMenu.vehicleSpawner.category.motorcycles",
            "vMenu.vehicleSpawner.category.offroad",
            "vMenu.vehicleSpawner.category.industrial",
            "vMenu.vehicleSpawner.category.utility",
            "vMenu.vehicleSpawner.category.vans",
            "vMenu.vehicleSpawner.category.cycles",
            "vMenu.vehicleSpawner.category.boats",
            "vMenu.vehicleSpawner.category.helicopters",
            "vMenu.vehicleSpawner.category.planes",
            "vMenu.vehicleSpawner.category.service",
            "vMenu.vehicleSpawner.category.emergency",
            "vMenu.vehicleSpawner.category.military",
            "vMenu.vehicleSpawner.category.commercial",
            "vMenu.vehicleSpawner.category.trains",
        };

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
            Dictionary<string, bool> permissions = new Dictionary<string, bool>();
            //{

            //    {"playerOptions", IsPlayerAceAllowed(player.Handle, "vMenu.playerOptions") },
            //    {"onlinePlayers", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers") },
            //    {"onlinePlayers_kick", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_kick") },
            //    {"onlinePlayers_teleport", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_teleport") },
            //    {"onlinePlayers_waypoint", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_waypoint") },
            //    {"onlinePlayers_spectate", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_spectate") },
            //    {"vehicleOptions", IsPlayerAceAllowed(player.Handle, "vMenu.vehicleOptions") },
            //    {"spawnVehicle", IsPlayerAceAllowed(player.Handle, "vMenu.spawnVehicle") },
            //    {"weatherOptions", IsPlayerAceAllowed(player.Handle, "vMenu.weatherOptions") },
            //    {"timeOptions", IsPlayerAceAllowed(player.Handle, "vMenu.timeOptions") },
            //    {"noclip", IsPlayerAceAllowed(player.Handle, "vMenu.noclip") },
            //    {"voiceChat", IsPlayerAceAllowed(player.Handle, "vMenu.voiceChat") },
            //};
            foreach (string ace in aceNames)
            {
                var safeName = ace.Replace(".", "_");
                permissions.Add(safeName, IsPlayerAceAllowed(player.Handle, ace));
            }

            //foreach (KeyValuePair<string, bool> dict in permissions)
            //{
            //Debug.WriteLine($"Key: {dict.Key}\r\nValue: {dict.Value}");
            //}
            TriggerClientEvent(player, "vMenu:SetPermissions", permissions);
        }
    }
}
