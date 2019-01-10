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
        public static string currentWeatherType = GetSettingsString(Setting.vmenu_default_weather);
        public static bool blackoutMode = false;
        public static bool dynamicWeather = GetSettingsBool(Setting.vmenu_enable_dynamic_weather);
        private string lastWeather = currentWeatherType;
        public static int currentHours = GetSettingsInt(Setting.vmenu_default_time_hour);
        public static int currentMinutes = GetSettingsInt(Setting.vmenu_default_time_min);
        public static bool freezeTime = GetSettingsBool(Setting.vmenu_freeze_time);
        private int minuteTimer = GetGameTimer();
        private int minuteClockSpeed = 2000;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EventManager()
        {
            // Add event handlers.
            // Handle the SetPermissions event.
            //EventHandlers.Add("vMenu:ConfigureClient", new Action<dynamic, dynamic, dynamic, dynamic>(ConfigureClient));
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
            EventHandlers.Add("vMenu:OutdatedResource", new Action(NotifyOutdatedVersion));
            EventHandlers.Add("vMenu:ClearArea", new Action<float, float, float>(ClearAreaNearPos));
            EventHandlers.Add("vMenu:updatePedDecors", new Action(UpdatePedDecors));
            EventHandlers.Add("playerSpawned", new Action(SetAppearanceOnFirstSpawn));
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
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", true);
                }
            }
        }

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
                        VehicleSpawner.AddonVehicles.Add(addon, (uint)GetHashKey(addon));
                    }
                }

                // load weapons
                if (addons.ContainsKey("weapons"))
                {
                    foreach (string addon in addons["weapons"])
                    {
                        WeaponOptions.AddonWeapons.Add(addon, (uint)GetHashKey(addon));
                    }
                }

                // load peds.
                if (addons.ContainsKey("peds"))
                {
                    foreach (string addon in addons["peds"])
                    {
                        PlayerAppearance.AddonPeds.Add(addon, (uint)GetHashKey(addon));
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine($"\n\n^1[vMenu] [ERROR] ^7Your addons.json file contains a problem! Error details: {ex.Message}\n\n");
            }

            FunctionsController.flaresAllowed = false;
            FunctionsController.bombsAllowed = false;

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
        private async void NotifyOutdatedVersion()
        {
            Debug.Write("\n\n\n\n[vMenu] vMenu is outdated, please update asap.\n\n\n\n");
            await Delay(5000);
            Log("Sending alert now.");
            if (GetSettingsBool(Setting.vmenu_outdated_version_notify_players))
            {
                Notify.Alert("vMenu is outdated, if you are the server administrator, please update vMenu as soon as possible.", true, true);
            }
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

                    ClearWeatherTypePersist();
                    ClearOverrideWeather();
                    SetWeatherTypeNow(lastWeather);
                    lastWeather = currentWeatherType;
                    SetWeatherTypeOverTime(currentWeatherType, 45f);
                    int tmpTimer = GetGameTimer();
                    while (GetGameTimer() - tmpTimer < 45500) // wait 45.5 _real_ seconds
                    {
                        await Delay(0);
                    }
                    SetWeatherTypeNow(currentWeatherType);
                    justChanged = true;
                    Log("done changing weather type (duration: 45.5 seconds)");
                }
                if (!justChanged)
                {
                    SetWeatherTypeNowPersist(currentWeatherType);
                }
                SetBlackout(blackoutMode);
                SetWind(0f);
                SetWindDirection(0f);
                SetWindSpeed(0f);
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
        private void SetTime(int newHours, int newMinutes, bool freezeTime)
        {
            EventManager.freezeTime = freezeTime;
            currentMinutes = newMinutes;
            currentHours = newHours;
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

        private async void UpdatePedDecors()
        {
            await Delay(1000);
            int backup = PlayerAppearance.ClothingAnimationType;
            PlayerAppearance.ClothingAnimationType = -1;
            await Delay(100);
            PlayerAppearance.ClothingAnimationType = backup;
        }
    }
}
