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

using vMenu.Client.Functions;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus.WorldSubmenus
{
    public class TimeOptionsMenu
    {
        private static UIMenu timeOptionsMenu = null;
        public static UIMenuItem freezeTimeToggle;

        public TimeOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["TimeOptionsMenu"];

            timeOptionsMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Time Options").Create();


            freezeTimeToggle = new UIMenuItem("Freeze/Unfreeze Time", "Enable or disable time freezing.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            UIMenuItem earlymorning = new UIMenuItem("Early Morning", "Set the time to 06:00.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            earlymorning.SetRightLabel("06:00");
            UIMenuItem morning = new UIMenuItem("Morning", "Set the time to 09:00.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            morning.SetRightLabel("09:00");
            UIMenuItem noon = new UIMenuItem("Noon", "Set the time to 12:00.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            noon.SetRightLabel("12:00");
            UIMenuItem earlyafternoon = new UIMenuItem("Early Afternoon", "Set the time to 15:00.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            earlyafternoon.SetRightLabel("15:00");
            UIMenuItem afternoon = new UIMenuItem("Afternoon", "Set the time to 18:00.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            afternoon.SetRightLabel("18:00");
            UIMenuItem evening = new UIMenuItem("Evening", "Set the time to 21:00.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            evening.SetRightLabel("21:00");
            UIMenuItem midnight = new UIMenuItem("Midnight", "Set the time to 00:00.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            midnight.SetRightLabel("00:00");
            UIMenuItem night = new UIMenuItem("Night", "Set the time to 03:00.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
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
            UIMenuListItem hour = new UIMenuListItem("Hour", hours, 0, "Hour", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            UIMenuListItem minute = new UIMenuListItem("Minute", minutes, 0, "Minute", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            

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

            timeOptionsMenu.AddItem(hour);
            timeOptionsMenu.AddItem(minute);
            timeOptionsMenu.AddItem(freezeTimeToggle);
            timeOptionsMenu.AddItem(earlymorning);
            timeOptionsMenu.AddItem(morning);
            timeOptionsMenu.AddItem(noon);
            timeOptionsMenu.AddItem(earlyafternoon);
            timeOptionsMenu.AddItem(afternoon);
            timeOptionsMenu.AddItem(evening);
            timeOptionsMenu.AddItem(midnight);
            timeOptionsMenu.AddItem(night);
            Main.Menus.Add(timeOptionsMenu);
        }

        public static UIMenu Menu()
        {
            return timeOptionsMenu;
        }
    }
}