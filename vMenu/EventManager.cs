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

            Tick += WeatherSync;
            Tick += TimeSync;
        }

        private void UpdatePermissions(dynamic permissions)
        {
            MainMenu.SetPermissions(permissions);
        }

        /// <summary>
        /// OnTick loop to keep the weather synced.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherSync()
        {
            // Weather is set every second, if it's changed, then it will transition to the new phase within 20 seconds.
            await Delay(1000);
            var justChanged = false;
            if (currentWeatherType != lastWeather)
            {
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
                await Delay(16000);
                //SetWeatherTypePersist(currentWeatherType);
                SetWeatherTypeNow(currentWeatherType);
                justChanged = true;
            }
            if (!justChanged)
            {
                SetWeatherTypeNowPersist(currentWeatherType);
            }
            SetBlackout(blackoutMode);

        }

        /// <summary>
        /// OnTick loop to keep the time synced.
        /// </summary>
        /// <returns></returns>
        private async Task TimeSync()
        {
            // If time is frozen...
            if (freezeTime)
            {
                // Time is set every second to make sure it never changes (even with some lag).
                await Delay(1000);
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
            EventManager.currentWeatherType = newWeather;
            EventManager.blackoutMode = blackoutEnabled;
            EventManager.dynamicWeather = dynamicChanges;
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
