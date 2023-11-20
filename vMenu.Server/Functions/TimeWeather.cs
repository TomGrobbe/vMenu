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

        private static int MinuteClockSpeed => MathUtil.Clamp(Convar.GetSettingsInt("vmenu_ingame_minute_duration"), 100, 2000);

        private bool TimeChange
        {
            get { return Convar.GetSettingsBool("vmenu_smooth_change"); }
            set { SetConvarReplicated("vmenu_smooth_change", value.ToString().ToLower()); }
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

        private int SetCurrentHours = 0;
        private int SetCurrentMinutes = 0;


        [EventHandler("vMenu:UpdateServerTime")]
        internal void UpdateTime(int newHours, int newMinutes, bool freezeTimeNew)
        {
            SetCurrentHours = newHours;
            SetCurrentMinutes = newMinutes;
            FreezeTime = freezeTimeNew;
            TimeChange = true;
        }
        /// <summary>
        /// Loop used for syncing and keeping track of the time in-game.
        /// </summary>
        /// <returns></returns>
        private async Task TimeLoop()
        {
            var tempspeed = MinuteClockSpeed;
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
                if (TimeChange)
                {
                    tempspeed = 1;

                    var nval = ((SetCurrentHours - CurrentHours) * -1);
                    if (nval > 0)
                    {
                        nval = (nval + 12) * -1;
                    }
                    else
                    {
                        nval = nval + 12;
                    }

                    if (nval == 0)
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
                                CurrentHours ++;
                            }
                        }
                        else
                        {
                            CurrentMinutes = MathUtil.Clamp(CurrentMinutes + 5, 0, 59);
                        }    
                    }
                    else
                    {
                        if (nval < 0)
                        {
                            if (CurrentMinutes < 1)
                            {
                                CurrentMinutes = 59;
                                if (CurrentHours < 1)
                                {
                                    CurrentHours = 23;
                                }
                                else
                                {
                                    CurrentHours = CurrentHours - 1;
                                }
                            }
                            else
                            {
                                CurrentMinutes = MathUtil.Clamp(CurrentMinutes - 5, 0, 59);
                            }
                        }
                        else
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
                                    CurrentHours ++;
                                }
                            }
                            else
                            {
                                CurrentMinutes = MathUtil.Clamp(CurrentMinutes + 5, 0, 59);
                            }
                        }
                    }
                    if ((CurrentMinutes >= SetCurrentMinutes) && (CurrentHours == SetCurrentHours))
                    {
                        CurrentHours = SetCurrentHours;
                        CurrentMinutes = SetCurrentMinutes;
                        TimeChange = false;
                    }
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
                }
                await Delay(tempspeed);
            }
        }        
    }
}