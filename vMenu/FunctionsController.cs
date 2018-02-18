using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    class FunctionsController : BaseScript
    {

        public FunctionsController()
        {
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            // Check if the menu actually exists... we don't want null pointer exceptions or illegal access errors!
            if (MainMenu._po != null)
            {
                // Manage Player God Mode
                SetEntityInvincible(PlayerPedId(), MainMenu._po.PlayerGodMode);

                // Manage invisibility.
                SetEntityVisible(PlayerPedId(), !MainMenu._po.PlayerInvisible, false);

                // Manage Stamina
                if (MainMenu._po.PlayerStamina)
                {
                    ResetPlayerStamina(PlayerId());
                }

                // Manage Super jump.
                if (MainMenu._po.PlayerSuperJump)
                {
                    SetSuperJumpThisFrame(PlayerId());
                }

                // Manage PlayerNoRagdoll
                SetPedCanRagdoll(PlayerPedId(), MainMenu._po.PlayerNoRagdoll);
                SetPedCanRagdollFromPlayerImpact(PlayerPedId(), MainMenu._po.PlayerNoRagdoll);

                // Manage never wanted.
                if (MainMenu._po.PlayerNeverWanted && GetPlayerWantedLevel(PlayerId()) > 0)
                {
                    ClearPlayerWantedLevel(PlayerId());
                }

                // Manage player is ignored by everyone.
                SetEveryoneIgnorePlayer(PlayerId(), MainMenu._po.PlayerIsIgnored);

                // Manage player frozen.
                FreezeEntityPosition(PlayerPedId(), MainMenu._po.PlayerFrozen);
            }
        }
    }
}
