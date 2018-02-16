using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenu
{
    #region Notifications Class
    /// <summary>
    /// Notifications class to easilly show notifications using custom made templates,
    /// or completely custom style if none of the templates are fitting for the current task.
    /// </summary>
    public class Notification : BaseScript
    {
        public Notification()
        {
        }

        /// <summary>
        /// Show a custom notification above the minimap.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public void Custom(string message, bool blink = false, bool saveToBrief = true)
        {
            SetNotificationTextEntry("THREESTRINGS");
            AddTextComponentSubstringPlayerName(message);
            DrawNotification(blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Alert: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public void Alert(string message, bool blink = false, bool saveToBrief = true)
        {
            Custom("~y~~h~Alert~w~: " + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Error: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public void Error(string message, bool blink = false, bool saveToBrief = true)
        {
            Custom("~r~~h~Error~w~: " + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Info: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public void Info(string message, bool blink = false, bool saveToBrief = true)
        {
            Custom("~b~~h~Info~w~: " + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Success: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public void Success(string message, bool blink = false, bool saveToBrief = true)
        {
            Custom("~g~~h~Success~w~: " + message, blink, saveToBrief);
        }
    }
    #endregion
}
