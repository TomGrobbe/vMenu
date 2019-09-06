using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class EventManager : BaseScript
    {
        // common functions.
        public static bool CurrentlySwitchingWeather { get; private set; } = false;
        public static string currentWeatherType = GetSettingsString(Setting.vmenu_default_weather);
        public static bool blackoutMode = false;
        public static bool dynamicWeather = GetSettingsBool(Setting.vmenu_enable_dynamic_weather);
        private string lastWeather = currentWeatherType;
        public static int currentHours = GetSettingsInt(Setting.vmenu_default_time_hour);
        public static int currentMinutes = GetSettingsInt(Setting.vmenu_default_time_min);
        public static bool freezeTime = GetSettingsBool(Setting.vmenu_freeze_time);
        private int minuteTimer = GetGameTimer();
        private int minuteClockSpeed = 2000;
        private static bool DontDoTimeSyncRightNow = false;
        private static bool SmoothTimeTransitionsEnabled => GetSettingsBool(Setting.vmenu_smooth_time_transitions);
        private int currentServerHours = currentHours;
        private int currentServerMinutes = currentMinutes;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EventManager()
        {
            // Add event handlers.
            // Handle the SetPermissions event.
            EventHandlers.Add("vMenu:SetAddons", new Action(SetAddons));
            EventHandlers.Add("vMenu:SetPermissions", new Action<string>(MainMenu.SetPermissions));
            EventHandlers.Add("vMenu:GoToPlayer", new Action<string>(SummonPlayer));
            EventHandlers.Add("vMenu:KillMe", new Action<string>(KillMe));
            EventHandlers.Add("vMenu:Notify", new Action<string>(NotifyPlayer));
            EventHandlers.Add("vMenu:SetWeather", new Action<string, bool, bool>(SetWeather));
            EventHandlers.Add("vMenu:SetClouds", new Action<float, string>(SetClouds));
            EventHandlers.Add("vMenu:SetTime", new Action<int, int, bool>(SetTime));
            EventHandlers.Add("vMenu:GoodBye", new Action(GoodBye));
            EventHandlers.Add("vMenu:SetBanList", new Action<string>(UpdateBanList));
            EventHandlers.Add("vMenu:OutdatedResource", new Action<string>(NotifyOutdatedVersion));
            EventHandlers.Add("vMenu:ClearArea", new Action<float, float, float>(ClearAreaNearPos));
            EventHandlers.Add("vMenu:updatePedDecors", new Action(UpdatePedDecors));
            EventHandlers.Add("playerSpawned", new Action(SetAppearanceOnFirstSpawn));
            EventHandlers.Add("vMenu:GetOutOfCar", new Action<int, int>(GetOutOfCar));
            EventHandlers.Add("vMenu:PrivateMessage", new Action<string, string>(PrivateMessage));
            EventHandlers.Add("vMenu:UpdateTeleportLocations", new Action<string>(UpdateTeleportLocations));
            Tick += WeatherSync;
            Tick += TimeSync;
        }

        private bool firstSpawn = true;
        /// <summary>
        /// Sets the saved character whenever the player first spawns.
        /// </summary>
        private async void SetAppearanceOnFirstSpawn()
        {
            if (firstSpawn)
            {
                firstSpawn = false;
                if (MainMenu.MiscSettingsMenu != null && MainMenu.MpPedCustomizationMenu != null && MainMenu.MiscSettingsMenu.MiscRespawnDefaultCharacter && !string.IsNullOrEmpty(GetResourceKvpString("vmenu_default_character")) && !GetSettingsBool(Setting.vmenu_disable_spawning_as_default_character))
                {
                    await MainMenu.MpPedCustomizationMenu.SpawnThisCharacter(GetResourceKvpString("vmenu_default_character"), false);
                }
                while (!IsScreenFadedIn() || IsPlayerSwitchInProgress() || IsPauseMenuActive() || GetIsLoadingScreenActive())
                {
                    await Delay(0);
                }
                if (MainMenu.WeaponLoadoutsMenu != null && MainMenu.WeaponLoadoutsMenu.WeaponLoadoutsSetLoadoutOnRespawn && IsAllowed(Permission.WLEquipOnRespawn))
                {
                    var saveName = GetResourceKvpString("vmenu_string_default_loadout");
                    if (!string.IsNullOrEmpty(saveName))
                    {
                        await SpawnWeaponLoadoutAsync(saveName, true, false, true);
                    }

                }
            }
        }

        /// <summary>
        /// Sets the addon models from the addons.json file.
        /// </summary>
        private void SetAddons()
        {
            // reset addons
            VehicleSpawner.AddonVehicles = new Dictionary<string, uint>();
            WeaponOptions.AddonWeapons = new Dictionary<string, uint>();
            PlayerAppearance.AddonPeds = new Dictionary<string, uint>();

            string jsonData = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
            try
            {
                // load new addons.
                var addons = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonData);

                // load vehicles
                if (addons.ContainsKey("vehicles"))
                {
                    foreach (string addon in addons["vehicles"])
                    {
                        if (!VehicleSpawner.AddonVehicles.ContainsKey(addon))
                            VehicleSpawner.AddonVehicles.Add(addon, (uint)GetHashKey(addon));
                        else
                            Debug.WriteLine($"[vMenu] [Error] Your addons.json file contains 2 or more entries with the same vehicle name! ({addon}) Please remove duplicate lines!");
                    }
                }

                // load weapons
                if (addons.ContainsKey("weapons"))
                {
                    foreach (string addon in addons["weapons"])
                    {
                        if (!WeaponOptions.AddonWeapons.ContainsKey(addon))
                            WeaponOptions.AddonWeapons.Add(addon, (uint)GetHashKey(addon));
                        else
                            Debug.WriteLine($"[vMenu] [Error] Your addons.json file contains 2 or more entries with the same weapon name! ({addon}) Please remove duplicate lines!");
                    }
                }

                // load peds.
                if (addons.ContainsKey("peds"))
                {
                    foreach (string addon in addons["peds"])
                    {
                        if (!PlayerAppearance.AddonPeds.ContainsKey(addon))
                            PlayerAppearance.AddonPeds.Add(addon, (uint)GetHashKey(addon));
                        else
                            Debug.WriteLine($"[vMenu] [Error] Your addons.json file contains 2 or more entries with the same ped name! ({addon}) Please remove duplicate lines!");
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine($"\n\n^1[vMenu] [ERROR] ^7Your addons.json file contains a problem! Error details: {ex.Message}\n\n");
            }

            //FunctionsController.flaresAllowed = false;
            //FunctionsController.bombsAllowed = false;

            currentHours = GetSettingsInt(Setting.vmenu_default_time_hour);
            currentHours = (currentHours >= 0 && currentHours < 24) ? currentHours : 9;
            currentMinutes = GetSettingsInt(Setting.vmenu_default_time_min);
            currentMinutes = (currentMinutes >= 0 && currentMinutes < 60) ? currentMinutes : 0;

            minuteClockSpeed = GetSettingsInt(Setting.vmenu_ingame_minute_duration);
            minuteClockSpeed = (minuteClockSpeed > 0) ? minuteClockSpeed : 2000;

            UpdateWeatherParticlesOnce();

            MainMenu.ConfigOptionsSetupComplete = true;
        }

        /// <summary>
        /// Notifies the player that the current version of vMenu is outdated.
        /// </summary>
        private async void NotifyOutdatedVersion(string message)
        {
            Debug.WriteLine("\n\n\n\n[vMenu] [WARNING] vMenu is outdated, please update as soon as possible. Update info:\n" + message + "\n\n\n\n");
            while (IsHudHidden() || !IsHudPreferenceSwitchedOn() || !IsScreenFadedIn() || IsPlayerSwitchInProgress())
            {
                await Delay(0);
            }

            Log("Sending alert now after the hud is confirmed to be enabled.");

            Notify.Error("vMenu is outdated. Please update as soon as possible!", true, true);
            Notify.Custom(message, true, true);
        }

        /// <summary>
        /// Update ban list.
        /// </summary>
        /// <param name="list"></param>
        private void UpdateBanList(string list)
        {
            if (MainMenu.BannedPlayersMenu != null)
                MainMenu.BannedPlayersMenu.UpdateBanList(list);
        }

        /// <summary>
        /// Used for cheating idiots.
        /// </summary>
        private void GoodBye()
        {
            Log("fuck you.");
            ForceSocialClubUpdate();
        }

        /// <summary>
        /// Loads/unloads the snow fx particles if needed.
        /// </summary>
        private async void UpdateWeatherParticlesOnce()
        {
            if (currentWeatherType.ToUpper() == "XMAS")
            {
                if (!HasNamedPtfxAssetLoaded("core_snow"))
                {
                    RequestNamedPtfxAsset("core_snow");
                    while (!HasNamedPtfxAssetLoaded("core_snow"))
                    {
                        await Delay(0);
                    }
                }
                UseParticleFxAssetNextCall("core_snow");
                SetForceVehicleTrails(true);
                SetForcePedFootstepsTracks(true);
            }
            else
            {
                SetForceVehicleTrails(false);
                SetForcePedFootstepsTracks(false);
                RemoveNamedPtfxAsset("core_snow");
            }
        }


        /// <summary>
        /// OnTick loop to keep the weather synced.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherSync()
        {
            if (GetSettingsBool(Setting.vmenu_enable_weather_sync))
            {
                // Weather is set every 500ms, if it's changed, then it will transition to the new phase within 20 seconds.
                await Delay(500);

                var justChanged = false;
                UpdateWeatherParticlesOnce();
                if (currentWeatherType != lastWeather)
                {
                    Log($"Start changing weather type.\nOld weather: {lastWeather}.\nNew weather type: {currentWeatherType}.\nBlackout? {blackoutMode}.\nThis change will take 45.5 seconds!");
                    CurrentlySwitchingWeather = true;
                    ClearWeatherTypePersist();
                    ClearOverrideWeather();
                    SetWeatherTypeNow(lastWeather);
                    var previousWeather = lastWeather;
                    lastWeather = currentWeatherType;
                    SetWeatherTypeOverTime(currentWeatherType, 30f);
                    int tmpTimer = GetGameTimer();

                    // Wait until the transition is completed.
                    while (GetGameTimer() - tmpTimer < 30000) // wait 30 _real_ seconds
                    {
                        // To update the current state in the menu subtitle counter pre-text.
                        float weatherChangeState = ((float)GetGameTimer() - (float)tmpTimer) / 30000f;

                        if (MainMenu.WeatherOptionsMenu != null)
                        {
                            MainMenu.WeatherOptionsMenu.GetMenu().CounterPreText = $"(Cooldown {Math.Ceiling(30f - (30f * weatherChangeState))}) ";
                        }
                        await Delay(100);
                    }

                    // Reset the intensity to make the game handle the intensity correctly.
                    SetRainFxIntensity(-1f);

                    // Set the new weather type to be persistent.
                    SetWeatherTypeNow(currentWeatherType);
                    justChanged = true;

                    // Dbg logging
                    Log("done changing weather type (duration: 45.5 seconds)");

                    // Stop currently switching weather type checks.
                    CurrentlySwitchingWeather = false;

                    // Reset the menu subtitle counter pre-text.
                    if (MainMenu.WeatherOptionsMenu != null)
                        MainMenu.WeatherOptionsMenu.GetMenu().CounterPreText = null;

                    TriggerEvent("vMenu:WeatherChangeComplete", previousWeather, currentWeatherType);
                }
                if (!justChanged)
                {
                    SetWeatherTypeNowPersist(currentWeatherType);
                }
                SetBlackout(blackoutMode);
            }
        }

        /// <summary>
        /// This function will take care of time sync. It'll be called once, and never stop.
        /// </summary>
        /// <returns></returns>
        private async Task TimeSync()
        {
            // Check if the time sync should be disabled.
            if (GetSettingsBool(Setting.vmenu_enable_time_sync))
            {

                // If time is frozen...
                if (freezeTime)
                {
                    // Time is set every tick to make sure it never changes (even with some lag).
                    await Delay(0);
                    NetworkOverrideClockTime(currentHours, currentMinutes, 0);
                }
                // Otherwise...
                else
                {
                    if (minuteClockSpeed > 2000)
                    {
                        await Delay(2000);
                    }
                    else
                    {
                        await Delay(minuteClockSpeed);
                    }
                    // only add a minute if the timer has reached the configured duration (2000ms (2s) by default).
                    if (GetGameTimer() - minuteTimer > minuteClockSpeed)
                    {
                        currentMinutes++;
                        minuteTimer = GetGameTimer();
                    }

                    if (currentMinutes > 59)
                    {
                        currentMinutes = 0;
                        currentHours++;
                    }
                    if (currentHours > 23)
                    {
                        currentHours = 0;
                    }
                    NetworkOverrideClockTime(currentHours, currentMinutes, 0);
                }
            }
        }

        /// <summary>
        /// Set the cloud hat type.
        /// </summary>
        /// <param name="opacity"></param>
        /// <param name="cloudsType"></param>
        private void SetClouds(float opacity, string cloudsType)
        {
            if (opacity == 0f && cloudsType == "removed")
            {
                ClearCloudHat();
            }
            else
            {
                SetCloudHatOpacity(opacity);
                SetCloudHatTransition(cloudsType, 4f);
            }
        }

        /// <summary>
        /// Update the current weather.
        /// </summary>
        /// <param name="newWeather"></param>
        /// <param name="blackoutEnabled"></param>
        private void SetWeather(string newWeather, bool blackoutEnabled, bool dynamicChanges)
        {
            currentWeatherType = newWeather;
            blackoutMode = blackoutEnabled;
            dynamicWeather = dynamicChanges;
            if (MainMenu.WeatherOptionsMenu != null)
            {
                MainMenu.WeatherOptionsMenu.dynamicWeatherEnabled.Checked = dynamicChanges;
                MainMenu.WeatherOptionsMenu.blackout.Checked = blackoutEnabled;
            }
        }

        /// <summary>
        /// Update the current time.
        /// </summary>
        /// <param name="newHours"></param>
        /// <param name="newMinutes"></param>
        /// <param name="freezeTime"></param>
        private async void SetTime(int newHours, int newMinutes, bool freezeTime)
        {
            currentServerHours = newHours;
            currentServerMinutes = newMinutes;

            bool IsTimeDifferenceTooSmall()
            {
                var totalDifference = 0;
                totalDifference += (newHours - currentHours) * 60;
                totalDifference += (newMinutes - currentMinutes);

                if (totalDifference < 15 && totalDifference > -120)
                    return true;

                return false;
            }

            EventManager.freezeTime = freezeTime;

            if (SmoothTimeTransitionsEnabled && !IsTimeDifferenceTooSmall())
            {
                if (!DontDoTimeSyncRightNow)
                {
                    DontDoTimeSyncRightNow = true;
                    EventManager.freezeTime = false;

                    var oldSpeed = minuteClockSpeed;

                    while (currentHours != currentServerHours || currentMinutes != currentServerMinutes)
                    {
                        EventManager.freezeTime = false;
                        await Delay(0);
                        minuteClockSpeed = 1;
                    }
                    EventManager.freezeTime = freezeTime;

                    minuteClockSpeed = oldSpeed;

                    DontDoTimeSyncRightNow = false;
                }
            }
            else
            {
                currentHours = currentServerHours;
                currentMinutes = currentServerMinutes;
            }
        }

        /// <summary>
        /// Used by events triggered from the server to notify a user.
        /// </summary>
        /// <param name="message"></param>
        private void NotifyPlayer(string message)
        {
            Notify.Custom(message, true, true);
        }

        /// <summary>
        /// Kill this player, poor thing, someone wants you dead... R.I.P.
        /// </summary>
        private void KillMe(string sourceName)
        {
            Notify.Alert($"You have been killed by <C>{GetSafePlayerName(sourceName)}</C>~s~ using the ~r~Kill Player~s~ option in vMenu.");
            SetEntityHealth(Game.PlayerPed.Handle, 0);
        }

        /// <summary>
        /// Teleport to the specified player.
        /// </summary>
        /// <param name="targetPlayer"></param>
        private void SummonPlayer(string targetPlayer)
        {
            TeleportToPlayer(GetPlayerFromServerId(int.Parse(targetPlayer)));
        }

        /// <summary>
        /// Clear the area around the provided x, y, z coordinates. Clears everything like (destroyed) objects, peds, (ai) vehicles, etc.
        /// Also restores broken streetlights, etc.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void ClearAreaNearPos(float x, float y, float z)
        {
            ClearAreaOfEverything(x, y, z, 100f, false, false, false, false);
        }

        /// <summary>
        /// Kicks the current player from the specified vehicle if they're inside and don't own the vehicle themselves.
        /// </summary>
        /// <param name="vehNetId"></param>
        /// <param name="vehicleOwnedBy"></param>
        private async void GetOutOfCar(int vehNetId, int vehicleOwnedBy)
        {
            if (NetworkDoesNetworkIdExist(vehNetId))
            {
                int veh = NetToVeh(vehNetId);
                if (DoesEntityExist(veh))
                {
                    Vehicle vehicle = new Vehicle(veh);

                    if (vehicle == null || !vehicle.Exists())
                        return;

                    if (Game.PlayerPed.IsInVehicle(vehicle) && vehicleOwnedBy != Game.Player.ServerId)
                    {
                        if (!vehicle.IsStopped)
                        {
                            Notify.Alert("The owner of this vehicle is reclaiming their personal vehicle. You will be kicked from this vehicle in about 10 seconds. Stop the vehicle now to avoid taking damage.", false, true);
                        }

                        // Wait for the vehicle to come to a stop, or 10 seconds, whichever is faster.
                        var timer = GetGameTimer();
                        while (vehicle != null && vehicle.Exists() && !vehicle.IsStopped)
                        {
                            await Delay(0);
                            if (GetGameTimer() - timer > (10 * 1000)) // 10 second timeout
                            {
                                break;
                            }
                        }

                        // just to make sure they're actually still inside the vehicle and the vehicle still exists.
                        if (vehicle != null && vehicle.Exists() && Game.PlayerPed.IsInVehicle(vehicle))
                        {
                            // Make the ped jump out because the car isn't stopped yet.
                            if (!vehicle.IsStopped)
                            {
                                Notify.Info("You were warned, now you'll have to suffer the consequences!");
                                TaskLeaveVehicle(Game.PlayerPed.Handle, vehicle.Handle, 4160);
                            }
                            // Make the ped exit gently.
                            else
                            {
                                TaskLeaveVehicle(Game.PlayerPed.Handle, vehicle.Handle, 0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates ped decorators for the clothing animation when players have joined.
        /// </summary>
        private async void UpdatePedDecors()
        {
            await Delay(1000);
            int backup = PlayerAppearance.ClothingAnimationType;
            PlayerAppearance.ClothingAnimationType = -1;
            await Delay(100);
            PlayerAppearance.ClothingAnimationType = backup;
        }

        /// <summary>
        /// Updates the teleports locations data from the server side locations.json, because that doesn't update client side on change.
        /// </summary>
        /// <param name="jsonData"></param>
        private void UpdateTeleportLocations(string jsonData)
        {
            MiscSettings.TpLocations = JsonConvert.DeserializeObject<List<vMenuShared.ConfigManager.TeleportLocation>>(jsonData);
        }
    }
}
