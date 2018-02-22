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
        // Variables
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
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

            // Else if it does exist, do all checks.
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
                if (MainMenu.PlayerOptionsMenu != null)
                {
                    // Manage Player God Mode
                    SetEntityInvincible(PlayerPedId(), MainMenu.PlayerOptionsMenu.PlayerGodMode);

                    // Manage invisibility.
                    SetEntityVisible(PlayerPedId(), !MainMenu.PlayerOptionsMenu.PlayerInvisible, false);

                    // Manage Stamina
                    if (MainMenu.PlayerOptionsMenu.PlayerStamina)
                    {
                        ResetPlayerStamina(PlayerId());
                    }

                    // Manage Super jump.
                    if (MainMenu.PlayerOptionsMenu.PlayerSuperJump)
                    {
                        SetSuperJumpThisFrame(PlayerId());
                    }

                    // Manage PlayerNoRagdoll
                    SetPedCanRagdoll(PlayerPedId(), MainMenu.PlayerOptionsMenu.PlayerNoRagdoll);
                    SetPedCanRagdollFromPlayerImpact(PlayerPedId(), MainMenu.PlayerOptionsMenu.PlayerNoRagdoll);

                    // Manage never wanted.
                    if (MainMenu.PlayerOptionsMenu.PlayerNeverWanted && GetPlayerWantedLevel(PlayerId()) > 0)
                    {
                        ClearPlayerWantedLevel(PlayerId());
                    }

                    // Manage player is ignored by everyone.
                    SetEveryoneIgnorePlayer(PlayerId(), MainMenu.PlayerOptionsMenu.PlayerIsIgnored);

                    // Manage player frozen.
                    FreezeEntityPosition(PlayerPedId(), MainMenu.PlayerOptionsMenu.PlayerFrozen);
                }
                #endregion

                #region VehicleOptions functions
                // Vehicle options. Only run vehicle options if the vehicle options menu has actually been created.
                if (MainMenu.VehicleOptionsMenu != null)
                {
                    // If the player is inside a vehicle...
                    if (DoesEntityExist(cf.GetVehicle()))
                    {
                        // Vehicle.
                        Vehicle vehicle = new Vehicle(cf.GetVehicle());

                        // God mode
                        var god = MainMenu.VehicleOptionsMenu.VehicleGodMode;
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
                        FreezeEntityPosition(vehicle.Handle, MainMenu.VehicleOptionsMenu.VehicleFrozen);

                        // If the torque multiplier is enabled.
                        if (MainMenu.VehicleOptionsMenu.VehicleTorqueMultiplier)
                        {
                            // Set the torque multiplier to the selected value by the player.
                            // no need for an "else" to reset this value, because when it's not called every frame, nothing happens.
                            SetVehicleEngineTorqueMultiplier(vehicle.Handle, MainMenu.VehicleOptionsMenu.VehicleTorqueMultiplierAmount);
                        }
                        // If the player has switched to a new vehicle, and the vehicle engine power multiplier is turned on. Set the new value.
                        if (SwitchedVehicle)
                        {
                            // Only needs to be set once.
                            SwitchedVehicle = false;

                            // Vehicle engine power multiplier.
                            if (MainMenu.VehicleOptionsMenu.VehiclePowerMultiplier)
                            {
                                SetVehicleEnginePowerMultiplier(vehicle.Handle, MainMenu.VehicleOptionsMenu.VehiclePowerMultiplierAmount);
                            }
                            else
                            {
                                SetVehicleEnginePowerMultiplier(vehicle.Handle, 1f);
                            }
                            // No Siren Toggle
                            vehicle.IsSirenSilent = MainMenu.VehicleOptionsMenu.VehicleNoSiren;
                        }
                        // Destroy the vehicle object.
                        vehicle = null;
                    }

                    // Manage vehicle engine always on.
                    if (MainMenu.VehicleOptionsMenu.VehicleEngineAlwaysOn && DoesEntityExist(cf.GetVehicle(last: true)) && !DoesEntityExist(cf.GetVehicle(last: false)))
                    {
                        SetVehicleEngineOn(cf.GetVehicle(last: true), true, true, true);
                    }

                    // Manage "no helmet"
                    var ped = new Ped(PlayerPedId());
                    // If the no helmet feature is turned on, disalbe "ped can wear helmet"
                    if (MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet)
                    {
                        ped.CanWearHelmet = false;
                    }
                    // otherwise, allow helmets.
                    else if (!MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet)
                    {
                        ped.CanWearHelmet = true;
                    }
                    // If the player is still wearing a helmet, even if the option is set to: no helmet, then remove the helmet.
                    if (ped.IsWearingHelmet && MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet)
                    {
                        ped.RemoveHelmet(true);
                    }



                }
                #endregion
            }
        }
    }
}
