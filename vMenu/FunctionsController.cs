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
        
        CommonFunctions cf = new CommonFunctions();
        public FunctionsController()
        {
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            // Player options. Only run player options if the player options menu has actually been created.
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

            // Vehicle options. Only run vehicle options if the vehicle options menu has actually been created.
            if (MainMenu._vo != null)
            {
                if (DoesEntityExist(cf.GetVehicle()))
                {
                    // Vehicle.
                    Vehicle vehicle = new Vehicle(cf.GetVehicle());

                    // God mode
                    var god = MainMenu._vo.VehicleGodMode;
                    vehicle.CanBeVisiblyDamaged = !god;
                    vehicle.CanEngineDegrade = !god;
                    vehicle.CanTiresBurst = !god;
                    vehicle.CanWheelsBreak = !god;
                    vehicle.IsAxlesStrong = god;
                    vehicle.IsBulletProof = god;
                    vehicle.IsCollisionProof = god;
                    vehicle.IsExplosionProof = god;
                    vehicle.IsFireProof = god;
                    vehicle.IsInvincible = god;
                    vehicle.IsMeleeProof = god;

                    FreezeEntityPosition(vehicle.Handle, MainMenu._vo.VehicleFrozen);
                }

                if (MainMenu._vo.VehicleEngineAlwaysOn && DoesEntityExist(cf.GetVehicle(last: true)) && !DoesEntityExist(cf.GetVehicle(last: false)))
                {
                    SetVehicleEngineOn(cf.GetVehicle(last: true), true, true, true);
                }
            }
            if (cf == null)
            {
                await Delay(0);
            }
        }
    }
}
