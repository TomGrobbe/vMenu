using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Server.Native;
using CitizenFX.Server;

namespace vMenu.Server.Events
{
    public class OnlinePlayerEvents : ServerScript
    {
        public OnlinePlayerEvents() { }

        [EventHandler("vMenu:Server:RequestPlayersList", Binding.Remote)]
        private void vMenuOnPlayerListRequest(Callback networkCB)
        {
            PlayerList players = new PlayerList();

            networkCB(players);
        }
    }
}
