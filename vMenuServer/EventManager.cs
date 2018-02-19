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
        }

        private void KillPlayer([FromSource] Player source, [FromSource] Player target)
        {
            TriggerClientEvent(player: target, eventName: "vMenu:KillMe");
        }

        private void SummonPlayer([FromSource] Player source, [FromSource]Player target)
        {
            TriggerClientEvent(player: target, eventName: "vMenu:GoToPlayer", args: source.Handle);
        }
    }
}
