using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.PlayerList;

using FxEvents;
using FxEvents.Shared.Snowflakes;
using FxEvents.Shared.EventSubsystem;
using vMenu.Server.Functions;
using Newtonsoft.Json;
using vMenu.Shared.Objects;

namespace vMenu.Server.Events
{
    public class OnlinePlayerEvents : BaseScript
    {
        public OnlinePlayerEvents()
        {
            EventDispatcher.Mount("RequestPlayersList", new Func<Task<string>>(OnRequestPlayersList));
        }

        private async Task<string> OnRequestPlayersList()
        {
            List<OnlinePlayersCB> OnlinePlayersData = new();

            int PlayersCountAdd = Main.PlayerList.Count();

            foreach (CitizenFX.Core.Player player in Main.PlayerList)
            {
                OnlinePlayersData.Add(new OnlinePlayersCB
                {
                    Player = new Shared.Objects.Player()
                    {
                        Name = player.Name,
                        ServerId = int.Parse(player.Handle),
                        LocalId = 0,
                        CharacterHandle = player.Character.Handle
                    },
                    SteamIdentifier = player.Identifiers["steam"],
                    DiscordIdentifier = player.Identifiers["discord"]
                });

                PlayersCountAdd--;
            }

            while (PlayersCountAdd > 0)
            {
                await Delay(50);
            }

            return JsonConvert.SerializeObject(OnlinePlayersData);
        }
    }
}
