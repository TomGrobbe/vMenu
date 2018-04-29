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
        private CommonFunctions cf = MainMenu.Cf;
        public static string currentWeatherType = "CLEAR";
        public static bool blackoutMode = false;
        public static bool dynamicWeather = true;
        private string lastWeather = currentWeatherType;
        public static int currentHours = 9;
        public static int currentMinutes = 0;
        public static bool freezeTime = false;

        public static bool enableSync = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EventManager()
        {
            // Add event handlers.
            // Handle the SetPermissions event.
            EventHandlers.Add("vMenu:SetPermissions", new Action<dynamic>(UpdatePermissions));
            EventHandlers.Add("vMenu:GoToPlayer", new Action<string>(SummonPlayer));
            EventHandlers.Add("vMenu:KillMe", new Action(KillMe));
            EventHandlers.Add("vMenu:KickCallback", new Action<string>(KickCallback));
            EventHandlers.Add("vMenu:SetWeather", new Action<string, bool, bool>(SetWeather));
            EventHandlers.Add("vMenu:SetClouds", new Action<float, string>(SetClouds));
            EventHandlers.Add("vMenu:SetTime", new Action<int, int, bool>(SetTime));
            EventHandlers.Add("vMenu:SetOptions", new Action<dynamic>(UpdateSettings));
            EventHandlers.Add("vMenu:SetupAddonPeds", new Action<string, dynamic>(SetAddonModels));
            EventHandlers.Add("vMenu:SetupAddonCars", new Action<string, dynamic>(SetAddonModels));
            EventHandlers.Add("vMenu:SetupAddonWeapons", new Action<string, dynamic>(SetAddonModels));
            EventHandlers.Add("vMenu:GoodBye", new Action(GoodBye));
            EventHandlers.Add("vMenu:SetBanList", new Action<string>(UpdateBanList));

            Tick += WeatherSync;
            Tick += TimeSync;
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
            cf.Log("fuck you.");
            ForceSocialClubUpdate();
        }

        /// <summary>
        /// Triggers a settings update.
        /// </summary>
        /// <param name="options"></param>
        private void UpdateSettings(dynamic options)
        {
            cf.Log("Options are being updated.");
            MainMenu.SetOptions(options);
        }

        /// <summary>
        /// Triggers a permissions update.
        /// </summary>
        /// <param name="permissions"></param>
        private void UpdatePermissions(dynamic permissions)
        {
            cf.Log("Permissions are being updated.");
            MainMenu.SetPermissions(permissions);
        }

        /// <summary>
        /// Triggers a addons list update.
        /// </summary>
        /// <param name="addonType"></param>
        /// <param name="addons"></param>
        private void SetAddonModels(string addonType, dynamic addons)
        {
            cf.Log($"Addon models are being loaded. Addon type: {addonType}.");
            Dictionary<string, uint> models = new Dictionary<string, uint>();
            foreach (var addon in addons)
            {
                string modelName = addon.ToString();
                uint modelHash = (uint)GetHashKey(modelName);

                if (!models.ContainsKey(modelName))
                {
                    models.Add(modelName, modelHash);
                }
            }
            if (addonType == "vehicles")
            {
                VehicleSpawner.AddonVehicles = models;
                MainMenu.addonCarsLoaded = true;
            }
            else if (addonType == "peds")
            {
                PlayerAppearance.AddonPeds = models;
                MainMenu.addonPedsLoaded = true;
            }
            else if (addonType == "weapons")
            {
                WeaponOptions.AddonWeapons = models;
                MainMenu.addonWeaponsLoaded = true;
            }

        }

        /// <summary>
        /// OnTick loop to keep the weather synced.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherSync()
        {
            if (enableSync)
            {
                // Weather is set every 500ms, if it's changed, then it will transition to the new phase within 20 seconds.
                await Delay(500);

                var justChanged = false;
                if (currentWeatherType != lastWeather)
                {
                    cf.Log($"Start changing weather type.\nOld weather: {lastWeather}.\nNew weather type: {currentWeatherType}.\nBlackout? {blackoutMode}");
                    if (currentWeatherType == "XMAS")
                    {
                        RequestScriptAudioBank("ICE_FOOTSTEPS", false);
                        RequestScriptAudioBank("SNOW_FOOTSTEPS", false);
                        RequestNamedPtfxAsset("core_snow");
                        while (!HasNamedPtfxAssetLoaded("core_snow"))
                        {
                            await Delay(0);
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
                        ReleaseNamedScriptAudioBank("ICE_FOOTSTEPS");
                        ReleaseNamedScriptAudioBank("SNOW_FOOTSTEPS");
                    }
                    ClearWeatherTypePersist();
                    ClearOverrideWeather();
                    SetWeatherTypeNow(lastWeather);
                    lastWeather = currentWeatherType;
                    SetWeatherTypeOverTime(currentWeatherType, 15f);
                    int tmpTimer = GetGameTimer();
                    while (GetGameTimer() - tmpTimer < 15500) // wait 15.5 _real_ seconds
                    {
                        await Delay(0);
                    }
                    SetWeatherTypeNow(currentWeatherType);
                    justChanged = true;
                    cf.Log("done changing weather type.");
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
            if (enableSync)
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
                    // Time is synced every 2 seconds (which equals 1 in-game minute).
                    await Delay(2000);
                    currentMinutes++;
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
        private void SummonPlayer(string targetPlayer)
        {
            //MainMenu.Notify.Error(targetPlayer);
            cf.TeleportToPlayerAsync(GetPlayerFromServerId(int.Parse(targetPlayer)));
        }
    }
}
