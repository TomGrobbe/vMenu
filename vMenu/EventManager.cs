using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public class EventManager : BaseScript
    {
        // common functions.
        private static CommonFunctions cf = new CommonFunctions();

        /// <summary>
        /// Constructor.
        /// </summary>
        public EventManager()
        {
            // Add event handlers.
            EventHandlers.Add("vMenu:GoToPlayer", new Action<int>(SummonPlayer));
            EventHandlers.Add("vMenu:KillMe", new Action(KillMe));
            EventHandlers.Add("vMenu:KickCallback", new Action<string>(KickCallback));
        }

        /// <summary>
        /// Kick callback. Notifies the player why a kick was not successfull.
        /// </summary>
        /// <param name="reason"></param>
        private void KickCallback(string reason)
        {
            MainMenu.Notify.Custom(reason, true, false);
        }

        /// <summary>
        /// Kill this player, poor thing, someone wants you dead... R.I.P.
        /// </summary>
        private void KillMe()
        {
            MainMenu.Notify.Info("Someone wanted you dead.... Sorry.");
            SetEntityHealth(PlayerPedId(), 0);
        }

        /// <summary>
        /// Teleport to the specified player.
        /// </summary>
        /// <param name="targetPlayer"></param>
        private void SummonPlayer(int targetPlayer)
        {
            cf.TeleportToPlayerAsync(GetPlayerFromServerId(targetPlayer));
        }
    }
}
