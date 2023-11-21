using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using FxEvents;

using Newtonsoft.Json;

using ScaleformUI;
using ScaleformUI.Menu;

using vMenu.Client.Menus;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;
using vMenu.Client.Events;
using vMenu.Client.Menus.WorldSubmenus;
using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Functions
{
    public class TimeWeather
    {
         private static readonly object _padlock = new();
        private static TimeWeather _instance;

        private TimeWeather()
        {
            if (Convar.GetSettingsBool("vmenu_use_time_sync"))
            {
                Main.Instance.AttachTick(TimeSync);
                Debug.WriteLine("TimeWeather Initialized");
            }
        }

        internal static TimeWeather Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new TimeWeather();
                }
            }
        }
        public static int GetServerMinutes => MathUtil.Clamp(Convar.GetSettingsInt("vmenu_current_minute"), 0, 59);
        public static int GetServerHours => MathUtil.Clamp(Convar.GetSettingsInt("vmenu_current_hour"), 0, 23);
        public static int GetServerMinuteDuration => Convar.GetSettingsInt("vmenu_ingame_minute_duration");
        public static bool IsServerTimeFrozen => Convar.GetSettingsBool("vmenu_freeze_time");
        //public static bool IsServerTimeSyncedWithMachineTime => Convar.GetSettingsBool("vmenu_sync_to_machine_time");
        /// <summary>
        /// This function will take care of time sync. It'll be called once, and never stop.
        /// </summary>
        /// <returns></returns>
        private async Task TimeSync()
        {
            NetworkOverrideClockTime(GetServerHours, GetServerMinutes, 0);
            if (!(TimeOptionsMenu.freezeTimeToggle == null))
            {
                TimeOptionsMenu.freezeTimeToggle.SetRightLabel($"{(GetServerHours < 10 ? $"0{GetServerHours}" : GetServerHours.ToString())}:{(GetServerMinutes < 10 ? $"0{GetServerMinutes}" : GetServerMinutes.ToString())}");
            }
            if (IsServerTimeFrozen)
            {
                await BaseScript.Delay(5);
            }
            else
            {
                if (Convar.GetSettingsBool("vmenu_smooth_change"))
                {
                    await BaseScript.Delay(1);
                }
                else
                {
                    await BaseScript.Delay(MathUtil.Clamp(GetServerMinuteDuration, 100, 2000));
                }
            }
        }
    }
}