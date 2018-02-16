using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenu
{
    #region Notifications Struct
    /// <summary>
    /// Notifications struct to easilly show notifications using custom made templates,
    /// or completely custom style if none of the templates are fitting for the current task.
    /// </summary>
    public struct Notification
    {
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

    #region Custom Subtitles Struct
    /// <summary>
    /// Custom Subtitles struct used to display subtitles using preformatted templates.
    /// Optionally you can also use a blank/custom style if you don't want to use an existing template.
    /// </summary>
    public struct Subtitles
    {

        /// <summary>
        /// Custom (white/custom text style subtitle)
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        public void Custom(string message, int duration = 2500, bool drawImmediately = true)
        {
            BeginTextCommandPrint("THREESTRINGS");
            AddTextComponentSubstringPlayerName(message);
            EndTextCommandPrint(duration, drawImmediately);
        }

        /// <summary>
        /// Alert (yellow text subtitle).
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        /// <param name="prefix">(Optional) add a prefix to your message, if you use this, only the prefix will be colored. The rest of the message will be left white.</param>
        public void Alert(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~y~" + prefix + " ~w~" : "~y~") + message, duration, drawImmediately);
        }

        /// <summary>
        /// Error (red text subtitle).
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        /// <param name="prefix">(Optional) add a prefix to your message, if you use this, only the prefix will be colored. The rest of the message will be left white.</param>
        public void Error(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~r~" + prefix + " ~w~" : "~r~") + message, duration, drawImmediately);
        }

        /// <summary>
        /// Info (blue text subtitle).
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        /// <param name="prefix">(Optional) add a prefix to your message, if you use this, only the prefix will be colored. The rest of the message will be left white.</param>
        public void Info(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~b~" + prefix + " ~w~" : "~b~") + message, duration, drawImmediately);
        }

        /// <summary>
        /// Success (green text subtitle).
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        /// <param name="prefix">(Optional) add a prefix to your message, if you use this, only the prefix will be colored. The rest of the message will be left white.</param>
        public void Success(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~g~" + prefix + " ~w~" : "~g~") + message, duration, drawImmediately);
        }

    }
    #endregion
}
