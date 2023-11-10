using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScaleformUI;

using vMenu.Client.Menus;
using vMenu.Client.Menus.OnlinePlayersSubmenus;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Functions
{
    public class Notify : BaseScript
    {
        public static void Error(string message, bool blink = true, bool SaveToBrief = true)
        {
            Notifications.ShowNotification($"~h~Error:~h~ {message}", NotificationColor.Red, blink, SaveToBrief);
        } 
        public static void Success(string message, bool blink = true, bool SaveToBrief = true)
        {
            Notifications.ShowNotification($"~h~Success:~h~ {message}", NotificationColor.GreenLight, blink, SaveToBrief);
        }   
        public static void Info(string message, bool blink = true, bool SaveToBrief = true)
        {
            Notifications.ShowNotification($"~h~Info:~h~ {message}", NotificationColor.Blue, blink, SaveToBrief);
        }   
        public static void Alert(string message, bool blink = true, bool SaveToBrief = true)
        {
            Notifications.ShowNotification($"~h~Alert:~h~ {message}", NotificationColor.Yellow, blink, SaveToBrief);
        }   
        public static void Custom(string message, bool blink = true, bool SaveToBrief = true, NotificationColor color = NotificationColor.Cyan)
        {
            Notifications.ShowNotification(message, color, blink, SaveToBrief);
        }         
    }
    public class Subtitle : BaseScript
    {
        private static void subtitle(string message, int time, bool drawImmediately)
        {
            BeginTextCommandPrint("STRING");
            AddTextComponentSubstringPlayerName(message);
            EndTextCommandPrint(time, drawImmediately);
        }
        public static void Error(string message, int time = 5000, bool drawImmediately = true)
        {
            subtitle($"~h~~r~Error:~h~~s~ {message}", time, drawImmediately);
        }
        public static void Success(string message, int time = 5000, bool drawImmediately = true)
        {
            subtitle($"~h~~g~Success:~h~~s~ {message}", time, drawImmediately);
        }
        public static void Info(string message, int time = 5000, bool drawImmediately = true)
        {
            subtitle($"~h~~b~Info:~h~~s~ {message}", time, drawImmediately);
        }
        public static void Alert(string message, int time = 5000, bool drawImmediately = true)
        {
            subtitle($"~h~~y~Alert:~h~~s~ {message}", time, drawImmediately);
        }
        public static void Custom(string message, int time = 5000, bool drawImmediately = true)
        {
            subtitle(message, time, drawImmediately);
        }
    }
}
