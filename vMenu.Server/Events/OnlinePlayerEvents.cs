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
using FxEvents.Shared.TypeExtensions;

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

            PlayerList Players = Main.PlayerList;

            int PlayersCountAdd = Players.Count();

            Players.ForEach((CitizenFX.Core.Player player) =>
            {
                if (player != null)
                {
                    Shared.Objects.Player playerDat = new Shared.Objects.Player()
                    {
                        Name = player.Name ?? "NULL",
                        ServerId = int.Parse(player.Handle ?? "0"),
                        LocalId = 0,
                        CharacterHandle = player.Character != null ? player.Character.Handle : 0,
                        PlayerLocation = new Shared.Objects.Vector3()
                        {
                            X = player.Character.Position.X,
                            Y = player.Character.Position.Y,
                            Z = player.Character.Position.Z,
                        }
                    };

                    OnlinePlayersData.Add(new OnlinePlayersCB()
                    {
                        Player = playerDat,
                        SteamIdentifier = player.Identifiers["steam"],
                        DiscordIdentifier = player.Identifiers["discord"]
                    });

                    PlayersCountAdd--;
                }
                else
                {
                    PlayersCountAdd--;
                }
            });

            while (PlayersCountAdd > 0)
            {
                await Delay(50);
            }

            return JsonConvert.SerializeObject(OnlinePlayersData);
        }
    }
}
