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
    /// <summary>
    /// This class manages all things that need to be done every tick based on
    /// checkboxes/things changing in any of the (sub) menus.
    /// </summary>
    class FunctionsController : BaseScript
    {
        private CommonFunctions cf = MainMenu.cf;
        private int LastVehicle = 0;
        private bool SwitchedVehicle = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FunctionsController()
        {
            Tick += OnTick;
        }

        /// <summary>
        /// OnTick
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            // If CommonFunctions does not exist, wait.
            // CommonFunctions is required before anything can be accessed.
            if (cf == null)
            {
                await Delay(0);
            }
            // Else, do all checks.
            else
            {

                #region Check for vehicle changes.
                // Check if the player has switched to a new vehicle.
                var tmpVehicle = GetVehiclePedIsIn(PlayerPedId(), false);
                if (DoesEntityExist(tmpVehicle) && tmpVehicle != LastVehicle)
                {
                    // Set the last vehicle to the new vehicle entity.
                    LastVehicle = tmpVehicle;
                    SwitchedVehicle = true;
                    // Trigger an event to be handled elsewhere.
                    //TriggerEvent("vMenu:PlayerSwitchedVehicle");
                }
                #endregion

                #region PlayerOptions functions.
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
                #endregion

                #region VehicleOptions functions
                // Vehicle options. Only run vehicle options if the vehicle options menu has actually been created.
                if (MainMenu._vo != null)
                {
                    // If the player is inside a vehicle...
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

                        // Freeze Vehicle Position (if enabled).
                        FreezeEntityPosition(vehicle.Handle, MainMenu._vo.VehicleFrozen);

                        // If the torque multiplier is enabled.
                        if (MainMenu._vo.VehicleTorqueMultiplier)
                        {
                            // Set the torque multiplier to the selected value by the player.
                            // no need for an "else" to reset this value, because when it's not called every frame, nothing happens.
                            SetVehicleEngineTorqueMultiplier(vehicle.Handle, MainMenu._vo.VehicleTorqueMultiplierAmount);
                        }
                        // If the player has switched to a new vehicle, and the vehicle engine power multiplier is turned on. Set the new value.
                        if (SwitchedVehicle && MainMenu._vo.VehiclePowerMultiplier)
                        {
                            SwitchedVehicle = false;
                            // Only needs to be set once.
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, MainMenu._vo.VehiclePowerMultiplierAmount);
                        }

                        // Destroy the vehicle object.
                        vehicle = null;
                    }

                    // Manage vehicle engine always on.
                    if (MainMenu._vo.VehicleEngineAlwaysOn && DoesEntityExist(cf.GetVehicle(last: true)) && !DoesEntityExist(cf.GetVehicle(last: false)))
                    {
                        SetVehicleEngineOn(cf.GetVehicle(last: true), true, true, true);
                    }


                }
                #endregion
            }
        }
    }
}
