using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
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
        private Dictionary<int, string> playerList = new Dictionary<int, string>();
        private List<int> deadPlayers = new List<int>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public FunctionsController()
        {
            foreach (Player p in new PlayerList())
            {
                playerList.Add(p.Handle, p.Name);
            }
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
                var tmpVehicle = cf.GetVehicle();
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
                    SetPedCanRagdoll(PlayerPedId(), !MainMenu.PlayerOptionsMenu.PlayerNoRagdoll);


                    // Fall off bike / dragged out of car.
                    if (MainMenu.VehicleOptionsMenu != null)
                    {
                        SetPedCanBeKnockedOffVehicle(PlayerPedId(), ((MainMenu.PlayerOptionsMenu.PlayerNoRagdoll || MainMenu.VehicleOptionsMenu.VehicleGodMode) ? 1 : 0));
                        SetPedCanBeDraggedOut(PlayerPedId(), (MainMenu.PlayerOptionsMenu.PlayerIsIgnored || MainMenu.VehicleOptionsMenu.VehicleGodMode || MainMenu.PlayerOptionsMenu.PlayerGodMode));
                        SetPedCanBeShotInVehicle(PlayerPedId(), !(MainMenu.PlayerOptionsMenu.PlayerGodMode || MainMenu.VehicleOptionsMenu.VehicleGodMode));
                    }
                    else
                    {
                        SetPedCanBeKnockedOffVehicle(PlayerPedId(), ((MainMenu.PlayerOptionsMenu.PlayerNoRagdoll) ? 1 : 0));
                        SetPedCanBeDraggedOut(PlayerPedId(), (MainMenu.PlayerOptionsMenu.PlayerIsIgnored));
                        SetPedCanBeShotInVehicle(PlayerPedId(), !(MainMenu.PlayerOptionsMenu.PlayerGodMode));
                    }

                    // Manage never wanted.
                    if (MainMenu.PlayerOptionsMenu.PlayerNeverWanted && GetPlayerWantedLevel(PlayerId()) > 0)
                    {
                        ClearPlayerWantedLevel(PlayerId());
                    }

                    // Manage player is ignored by everyone.
                    SetEveryoneIgnorePlayer(PlayerId(), MainMenu.PlayerOptionsMenu.PlayerIsIgnored);
                    SetPoliceIgnorePlayer(PlayerId(), MainMenu.PlayerOptionsMenu.PlayerIsIgnored);
                    SetPlayerCanBeHassledByGangs(PlayerId(), !(MainMenu.PlayerOptionsMenu.PlayerIsIgnored || MainMenu.PlayerOptionsMenu.PlayerGodMode));

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
                    if (MainMenu.VehicleOptionsMenu.VehicleEngineAlwaysOn && DoesEntityExist(cf.GetVehicle(lastVehicle: true)) && !DoesEntityExist(cf.GetVehicle(lastVehicle: false)))
                    {
                        SetVehicleEngineOn(cf.GetVehicle(lastVehicle: true), true, true, true);
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

                #region Misc Settings
                if (MainMenu.MiscSettingsMenu != null)
                {
                    // Show speedometer km/h
                    if (MainMenu.MiscSettingsMenu.ShowSpeedoKmh)
                    {
                        ShowSpeedKmh();
                    }

                    // Show speedometer mph
                    if (MainMenu.MiscSettingsMenu.ShowSpeedoMph)
                    {
                        ShowSpeedMph();
                    }

                    // Show coordinates.
                    if (MainMenu.MiscSettingsMenu.ShowCoordinates)
                    {
                        var pos = GetEntityCoords(PlayerPedId(), true);
                        cf.DrawTextOnScreen($"~r~X~t~: {Math.Round(pos.X, 2)}" +
                            $"~n~~r~Y~t~: {Math.Round(pos.Y, 2)}" +
                            $"~n~~r~Z~t~: {Math.Round(pos.Z, 2)}" +
                            $"~n~~r~Heading~t~: {Math.Round(GetEntityHeading(PlayerPedId()), 1)}", 0.45f, 0f, 0.38f, Alignment.Left, (int)Font.ChaletLondon);
                    }

                    // Hide hud.
                    if (MainMenu.MiscSettingsMenu.HideHud)
                    {
                        HideHudAndRadarThisFrame();
                    }

                    // Hide radar.
                    if (MainMenu.MiscSettingsMenu.HideRadar)
                    {
                        DisplayRadar(false);
                    }
                    // Show radar.
                    else
                    {
                        DisplayRadar(true);
                    }

                    // Show location & time.
                    if (MainMenu.MiscSettingsMenu.ShowLocation)
                    {
                        // Get the current location.
                        var currentPos = GetEntityCoords(PlayerPedId(), true);

                        // Get the nearest vehicle node.
                        var nodePos = currentPos;
                        var node = GetNthClosestVehicleNode(currentPos.X, currentPos.Y, currentPos.Z, 0, ref nodePos, 0, 0, 0);

                        // Create the default prefix.
                        var prefix = "~w~";

                        // If the vehicle node is further away than 1400f, then the player is not near a valid road.
                        // So we set the prefix to "Near " (<streetname>).
                        if (Vdist2(currentPos.X, currentPos.Y, currentPos.Z, nodePos.X, nodePos.Y, nodePos.Z) > 1400f)
                        {
                            prefix = "~m~Near ~w~";
                        }

                        // Get the heading.
                        float heading = Game.PlayerPed.Heading;
                        string headingCharacter = "";

                        // Heading Facing North
                        if (heading > 320 || heading < 45)
                        {
                            headingCharacter = "N";
                        }
                        // Heading Facing West
                        else if (heading >= 45 && heading <= 135)
                        {
                            headingCharacter = "W";
                        }
                        // Heading Facing South
                        else if (heading > 135 && heading < 225)
                        {
                            headingCharacter = "S";
                        }
                        // Heading Facing East
                        else
                        {
                            headingCharacter = "E";
                        }

                        // Get the safezone size for x and y to be able to move with the minimap.
                        float safeZoneSizeX = (1 / GetSafeZoneSize() / 3.0f) - 0.358f;
                        float safeZoneSizeY = (1 / GetSafeZoneSize() / 3.6f) - 0.27f;

                        // Get the cross road.
                        var p1 = (uint)1; // unused
                        var crossing = (uint)1;
                        GetStreetNameAtCoord(currentPos.X, currentPos.Y, currentPos.Z, ref p1, ref crossing);
                        var crossingName = GetStreetNameFromHashKey(crossing);

                        // Set the suffix for the road name to the corssing name, or to an empty string if there's no crossing.
                        string suffix = (crossingName != "" && crossingName != "NULL" && crossingName != null) ? "~t~ / " + crossingName : "";

                        // Draw the street name + crossing.
                        cf.DrawTextOnScreen(prefix + World.GetStreetName(currentPos) + suffix, 0.234f + safeZoneSizeX, 0.925f - safeZoneSizeY, 0.48f);
                        // Draw the zone name.
                        cf.DrawTextOnScreen(World.GetZoneLocalizedName(currentPos), 0.234f + safeZoneSizeX, 0.9485f - safeZoneSizeY, 0.45f);

                        // Draw the left border for the heading character.
                        cf.DrawTextOnScreen("~t~|", 0.188f + safeZoneSizeX, 0.915f - safeZoneSizeY, 1.2f, Alignment.Left);
                        // Draw the heading character.
                        cf.DrawTextOnScreen(headingCharacter, 0.208f + safeZoneSizeX, 0.915f - safeZoneSizeY, 1.2f, Alignment.Center);
                        // Draw the right border for the heading character.
                        cf.DrawTextOnScreen("~t~|", 0.228f + safeZoneSizeX, 0.915f - safeZoneSizeY, 1.2f, Alignment.Right);

                        // Get and draw the time.
                        var tth = GetClockHours();
                        var ttm = GetClockMinutes();
                        var th = (tth < 10) ? $"0{tth.ToString()}" : tth.ToString();
                        var tm = (ttm < 10) ? $"0{ttm.ToString()}" : ttm.ToString();
                        cf.DrawTextOnScreen($"~c~{th}:{tm}", 0.208f + safeZoneSizeX, 0.9748f - safeZoneSizeY, 0.40f, Alignment.Center);
                    }

                    #region Join / Quit notifications
                    // Join/Quit notifications
                    if (MainMenu.MiscSettingsMenu.JoinQuitNotifications)
                    {
                        PlayerList plist = new PlayerList();
                        Dictionary<int, string> pl = new Dictionary<int, string>();
                        foreach (Player p in plist)
                        {
                            pl.Add(p.Handle, p.Name);
                        }
                        // new player joined.
                        if (pl.Count > playerList.Count)
                        {
                            foreach (KeyValuePair<int, string> player in pl)
                            {
                                if (!playerList.Contains(player))
                                {
                                    Notify.Custom($"~g~{player.Value}~s~ joined the server.");
                                }
                            }
                        }
                        // player left.
                        else if (pl.Count < playerList.Count)
                        {
                            foreach (KeyValuePair<int, string> player in playerList)
                            {
                                if (!pl.Contains(player))
                                {
                                    Notify.Custom($"~r~{player.Value}~s~ left the server.");
                                }
                            }
                        }
                        playerList = pl;
                    }
                    #endregion

                    #region Death Notifications
                    // Death notifications
                    if (MainMenu.MiscSettingsMenu.DeathNotifications)
                    {
                        PlayerList pl = new PlayerList();
                        foreach (Player p in pl)
                        {
                            if (p.IsDead)
                            {
                                if (deadPlayers.Contains(p.Handle)) { return; }
                                var killer = p.Character.GetKiller();
                                if (killer != null)
                                {
                                    if (killer.Handle != p.Character.Handle)
                                    {
                                        if (killer.Exists())
                                        {
                                            foreach (Player playerKiller in pl)
                                            {
                                                if (playerKiller.Character.Handle == killer.Handle)
                                                {
                                                    Notify.Custom($"~o~{p.Name} ~s~has been murdered by ~y~{playerKiller.Name}~s~.");
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Notify.Custom($"~o~{p.Name} ~s~has been murdered.");
                                        }
                                    }
                                    else
                                    {
                                        Notify.Custom($"~o~{p.Name} ~s~committed suicide.");
                                    }
                                }
                                else
                                {
                                    Notify.Custom($"~o~{p.Name} ~s~died.");
                                }
                                deadPlayers.Add(p.Handle);
                            }
                            else
                            {
                                if (deadPlayers.Contains(p.Handle))
                                {
                                    deadPlayers.Remove(p.Handle);
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                #region Update Time Options Menu
                if (MainMenu.TimeOptionsMenu != null)
                {
                    if (MainMenu.TimeOptionsMenu.freezeTimeToggle != null && MainMenu.TimeOptionsMenu.GetMenu().Visible && !EventManager.freezeTime)
                    {
                        var hours = GetClockHours();
                        var minutes = GetClockMinutes();
                        var hoursString = hours < 10 ? "0" + hours.ToString() : hours.ToString();
                        var minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();
                        MainMenu.TimeOptionsMenu.freezeTimeToggle.SetRightLabel($"(Current Time {hoursString}:{minutesString})");
                    }
                }
                #endregion

                #region Weapon Options
                if (MainMenu.WeaponOptionsMenu != null)
                {
                    if (MainMenu.WeaponOptionsMenu.NoReload && Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Minigun)
                    {
                        PedSkipNextReloading(Game.PlayerPed.Handle);
                    }
                    SetPedInfiniteAmmoClip(Game.PlayerPed.Handle, MainMenu.WeaponOptionsMenu.UnlimitedAmmo);
                }
                #endregion
            }
        }

        #region Show Speedometers Functions
        /// <summary>
        /// Shows the current speed in km/h.
        /// Must be in a vehicle.
        /// </summary>
        private void ShowSpeedKmh()
        {
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                SetTextFont(4);
                SetTextScale(1.0f, 0.7f);
                SetTextJustification(2);
                SetTextWrap(0f, 0.995f);
                SetTextOutline();
                BeginTextCommandDisplayText("STRING");

                int speed = int.Parse(Math.Round(GetEntitySpeed(cf.GetVehicle()) * 3.6f).ToString());

                AddTextComponentSubstringPlayerName($"{speed} KM/h");
                EndTextCommandDisplayText(0f, 0.955f);

                HideHudComponentThisFrame((int)HudComponent.StreetName);
            }
        }

        /// <summary>
        /// Shows the current speed in mph.
        /// Must be in a vehicle.
        /// </summary>
        private void ShowSpeedMph()
        {
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                SetTextFont(4);
                SetTextScale(1.0f, 0.7f);
                SetTextJustification(2);
                SetTextWrap(0f, 0.995f);
                SetTextOutline();
                BeginTextCommandDisplayText("STRING");

                int speed = int.Parse(Math.Round(GetEntitySpeed(cf.GetVehicle()) * 2.23694f).ToString());

                AddTextComponentSubstringPlayerName($"{speed} MPH");
                if (MainMenu.MiscSettingsMenu.ShowSpeedoKmh)
                {
                    EndTextCommandDisplayText(0f, 0.925f);
                }
                else
                {
                    EndTextCommandDisplayText(0f, 0.955f);
                    HideHudComponentThisFrame((int)HudComponent.StreetName);
                }
            }
        }
        #endregion
    }
}
