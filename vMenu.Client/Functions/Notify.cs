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

using vMenu.Client.KeyMappings;
using vMenu.Client.Menus;
using vMenu.Client.Menus.OnlinePlayersSubmenus;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Functions
{
    public class Notify
    {
        private static readonly object _padlock = new();
        private static Notify _instance;

        private Notify()
        {
            Debug.WriteLine("Notify Initialized");
        }

        internal static Notify Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new Notify();
                }
            }
        }

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

    public class Subtitle
    {
        private static readonly object _padlock = new();
        private static Subtitle _instance;

        private Subtitle()
        {
            Debug.WriteLine("Subtitle Initialized");
        }

        internal static Subtitle Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new Subtitle();
                }
            }
        }

        private static void DrawSubtitle(string message, int time, bool drawImmediately)
        {
            BeginTextCommandPrint("STRING");
            AddTextComponentSubstringPlayerName(message);
            EndTextCommandPrint(time, drawImmediately);
        }
        public static void Error(string message, int time = 5000, bool drawImmediately = true)
        {
            DrawSubtitle($"~h~~r~Error:~h~~s~ {message}", time, drawImmediately);
        }
        public static void Success(string message, int time = 5000, bool drawImmediately = true)
        {
            DrawSubtitle($"~h~~g~Success:~h~~s~ {message}", time, drawImmediately);
        }
        public static void Info(string message, int time = 5000, bool drawImmediately = true)
        {
            DrawSubtitle($"~h~~b~Info:~h~~s~ {message}", time, drawImmediately);
        }
        public static void Alert(string message, int time = 5000, bool drawImmediately = true)
        {
            DrawSubtitle($"~h~~y~Alert:~h~~s~ {message}", time, drawImmediately);
        }
        public static void Custom(string message, int time = 5000, bool drawImmediately = true)
        {
            DrawSubtitle(message, time, drawImmediately);
        }
    }
}
