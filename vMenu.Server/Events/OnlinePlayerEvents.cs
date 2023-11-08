using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core.Native;
using static CitizenFX.Core.PlayerList;
using vMenu.Shared.Interfaces;

using FxEvents;
using FxEvents.Shared.Snowflakes;
using FxEvents.Shared.EventSubsystem;
using vMenu.Server.Functions;
using Newtonsoft.Json;

namespace vMenu.Server.Events
{
    public class OnlinePlayerEvents : BaseScript
    {
        public OnlinePlayerEvents()
        {
            try 
            {
                Debug.WriteLine("Test 1");
                EventDispatcher.Initalize("vMenu:Inbound", "vMenu:Outbound", "vMenu:Signature");
                Debug.WriteLine("Test 2");
                EventDispatcher.Mount("RequestPlayersList", new Func<ISource, Task<string>>(async ([FromSource] source) =>
                {
                    Debug.WriteLine("testtttt");

                    PlayerList OnlinePlayers = Players;

                    int PlayersCountAdd = OnlinePlayers.Count();

                    List<IOnlinePlayersCB> OnlinePlayersData = new();

                    foreach (Player player in OnlinePlayers)
                    {
                        OnlinePlayersData.Add(new IOnlinePlayersCB
                        {
                            PlayerName = player.Name,
                            ServerId = int.Parse(player.Handle),
                            Identifiers = player.Identifiers.ToArray()
                        });

                        PlayersCountAdd--;
                    }

                    while (PlayersCountAdd > 0)
                    {
                        await Delay(100);
                    }

                    return JsonConvert.SerializeObject(OnlinePlayersData);
                }));
                Debug.WriteLine("Test 3");
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.ToString());
            }
        }
    }
}
