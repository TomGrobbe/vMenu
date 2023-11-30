using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScaleformUI.Elements;
using ScaleformUI.Menu;

using static vMenu.Client.Functions.MenuFunctions;
using vMenu.Shared.Enums;

using vMenu.Client.Functions;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;
using vMenu.Client.Objects;

namespace vMenu.Client.Menus.WorldSubmenus
{
    public class TimeOptionsMenu
    {
        private static UIMenu timeOptionsMenu = null;
        public static UIMenuItem freezeTimeToggle;

        public TimeOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["TimeMenu"];

            timeOptionsMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Time Options").Create();

            freezeTimeToggle = new vMenuItem(MenuLanguage.Items["FreezeItem"], "Freeze/Unfreeze Time", "Enable or disable time freezing.").Create();
            UIMenuItem earlymorning = new vMenuItem(MenuLanguage.Items["EarlyMorningItem"], "Early Morning", "Set the time to 06:00.").Create();
            earlymorning.SetRightLabel("06:00");
            UIMenuItem morning = new vMenuItem(MenuLanguage.Items["MorningItem"], "Morning", "Set the time to 09:00.").Create();
            morning.SetRightLabel("09:00");
            UIMenuItem noon = new vMenuItem(MenuLanguage.Items["NoonItem"], "Noon", "Set the time to 12:00.").Create();
            noon.SetRightLabel("12:00");
            UIMenuItem earlyafternoon = new vMenuItem(MenuLanguage.Items["EarlyAfternoonItem"], "Early Afternoon", "Set the time to 15:00.").Create();
            earlyafternoon.SetRightLabel("15:00");
            UIMenuItem afternoon = new vMenuItem(MenuLanguage.Items["AfternoonItem"], "Afternoon", "Set the time to 18:00.").Create();
            afternoon.SetRightLabel("18:00");
            UIMenuItem evening = new vMenuItem(MenuLanguage.Items["EveningItem"], "Evening", "Set the time to 21:00.").Create();
            evening.SetRightLabel("21:00");
            UIMenuItem midnight = new vMenuItem(MenuLanguage.Items["MidnightItem"], "Midnight", "Set the time to 00:00.").Create();
            midnight.SetRightLabel("00:00");
            UIMenuItem night = new vMenuItem(MenuLanguage.Items["NightItem"], "Night", "Set the time to 03:00.").Create();
            night.SetRightLabel("03:00");
            
            var hours = new List<dynamic>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            var minutes = new List<dynamic>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };

            for (var i = 10; i < 60; i++)
            {
                if (i < 24)
                {
                    hours.Add(i.ToString());
                }
                minutes.Add(i.ToString());
            }

            UIMenuListItem hour = new UIMenuListItem(MenuLanguage.Items["Hour"].Name ?? "Set Custom Hour", hours, 0, MenuLanguage.Items["Hour"].Name ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            UIMenuListItem minute = new UIMenuListItem(MenuLanguage.Items["Minute"].Name ?? "Set Custom Minute", minutes, 0, MenuLanguage.Items["Minute"].Name ?? "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            

            timeOptionsMenu.OnListSelect += (sender, item, itemIndex) =>
            {
                var updatedminute = MathUtil.Clamp(Convar.GetSettingsInt("vmenu_current_minute"), 0, 59);
                var updatedhour = MathUtil.Clamp(Convar.GetSettingsInt("vmenu_current_hour"), 0, 23);
                var updatedfreeze = Convar.GetSettingsBool("vmenu_freeze_time");
                
                if (item == hour)
                {
                    updatedhour = item.Index;
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", updatedhour, updatedminute, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~{(updatedhour < 10 ? $"0{updatedhour}" : updatedhour.ToString())}~s~:~y~" + $"{(updatedminute < 10 ? $"0{updatedminute}" : updatedminute.ToString())}~s~.");
                }
                else if (item == minute)
                {
                    updatedminute = item.Index;
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", updatedhour, updatedminute, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~{(updatedhour < 10 ? $"0{updatedhour}" : updatedhour.ToString())}~s~:~y~" + $"{(updatedminute < 10 ? $"0{updatedminute}" : updatedminute.ToString())}~s~.");
                }
            };

            timeOptionsMenu.OnItemSelect += (sender, item, index) =>
            {
                var updatedminute = MathUtil.Clamp(Convar.GetSettingsInt("vmenu_current_minute"), 0, 59);
                var updatedhour = MathUtil.Clamp(Convar.GetSettingsInt("vmenu_current_hour"), 0, 23);
                var updatedfreeze = Convar.GetSettingsBool("vmenu_freeze_time");

                if (item == freezeTimeToggle)
                {
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", updatedhour, updatedminute, !updatedfreeze);
                    Subtitle.Info($"Time will now {(updatedfreeze ? "~y~continue" : "~o~freeze")}~s~.");
                }
                else if (item == earlymorning)
                {
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", 6, 0, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~06~s~:~y~" + $"00~s~.");
                }
                else if (item == morning)
                {
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", 9, 0, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~09~s~:~y~" + $"00~s~.");
                }
                else if (item == noon)
                {
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", 12, 0, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~12~s~:~y~" + $"00~s~.");
                }
                else if (item == earlyafternoon)
                {
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", 15, 0, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~15~s~:~y~" + $"00~s~.");
                }
                else if (item == afternoon)
                {
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", 18, 0, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~18~s~:~y~" + $"00~s~.");
                }
                else if (item == evening)
                {
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", 21, 0, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~21~s~:~y~" + $"00~s~.");
                }
                else if (item == midnight)
                {
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", 0, 0, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~00~s~:~y~" + $"00~s~.");
                }
                else if (item == night)
                {
                    BaseScript.TriggerServerEvent("vMenu:UpdateServerTime", 3, 0, updatedfreeze);
                    Subtitle.Info($"Time set to ~y~03~s~:~y~" + $"00~s~.");
                }

                Debug.WriteLine($"{item}");
            };

            bool timeallowed = IsAllowed(Permission.WRSetTime);
            bool freezeallowed = IsAllowed(Permission.WRFreezeTime);

            if (timeallowed)
            {
                timeOptionsMenu.AddItem(hour);
                timeOptionsMenu.AddItem(minute);
            }

            if (freezeallowed)
            {
                timeOptionsMenu.AddItem(freezeTimeToggle);
            }

            if (timeallowed)
            {
                timeOptionsMenu.AddItem(earlymorning);
                timeOptionsMenu.AddItem(morning);
                timeOptionsMenu.AddItem(noon);
                timeOptionsMenu.AddItem(earlyafternoon);
                timeOptionsMenu.AddItem(afternoon);
                timeOptionsMenu.AddItem(evening);
                timeOptionsMenu.AddItem(midnight);
                timeOptionsMenu.AddItem(night);
            }

            Main.Menus.Add(timeOptionsMenu);
        }

        public static UIMenu Menu()
        {
            return timeOptionsMenu;
        }
    }
}