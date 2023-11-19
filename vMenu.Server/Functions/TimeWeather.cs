using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;


namespace vMenu.Server.Functions
{
    internal class TimeWeather : BaseScript
    {
        public TimeWeather()
        {
            Tick += TimeLoop;
        }
        private int CurrentHours
        {
            get { return MathUtil.Clamp(Convar.GetSettingsInt("vmenu_current_hour"), 0, 23); }
            set { SetConvarReplicated("vmenu_current_hour", MathUtil.Clamp(value, 0, 23).ToString()); }
        }
        private int CurrentMinutes
        {
            get { return MathUtil.Clamp(Convar.GetSettingsInt("vmenu_current_minute"), 0, 59); }
            set { SetConvarReplicated("vmenu_current_minute", MathUtil.Clamp(value, 0, 59).ToString()); }
        }
        private int MinuteClockSpeed
        {
            get
            {
                var value = Convar.GetSettingsInt("vmenu_ingame_minute_duration");
                if (value < 100)
                {
                    value = 2000;
                }

                return value;
            }
        }
        private bool FreezeTime
        {
            get { return Convar.GetSettingsBool("vmenu_freeze_time"); }
            set { SetConvarReplicated("vmenu_freeze_time", value.ToString().ToLower()); }
        }
        private bool IsServerTimeSynced { get { return Convar.GetSettingsBool("vmenu_sync_to_machine_time"); } }
        /// <summary>
        /// Set and sync the time to all clients.
        /// </summary>
        /// <param name="newHours"></param>
        /// <param name="newMinutes"></param>
        /// <param name="freezeTimeNew"></param>
        [EventHandler("vMenu:UpdateServerTime")]
        internal void UpdateTime(int newHours, int newMinutes, bool freezeTimeNew)
        {
            CurrentHours = newHours;
            CurrentMinutes = newMinutes;
            FreezeTime = freezeTimeNew;
        }
        /// <summary>
        /// Loop used for syncing and keeping track of the time in-game.
        /// </summary>
        /// <returns></returns>
        private async Task TimeLoop()
        {
            if (IsServerTimeSynced)
            {
                var currentTime = DateTime.Now;
                CurrentMinutes = currentTime.Minute;
                CurrentHours = currentTime.Hour;

                // Update this once every 60 seconds.
                await Delay(60000);
            }
            else
            {
                if (!FreezeTime)
                {
                    if ((CurrentMinutes + 1) > 59)
                    {
                        CurrentMinutes = 0;
                        if ((CurrentHours + 1) > 23)
                        {
                            CurrentHours = 0;
                        }
                        else
                        {
                            CurrentHours++;
                        }
                    }
                    else
                    {
                        CurrentMinutes++;
                    }
                }
                await Delay(MinuteClockSpeed);
            }
        }        
    }
}