using System.Collections.Generic;

using CitizenFX.Core;

using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

namespace vMenuClient
{
    #region Error Templates
    /// <summary>
    /// List of error templates.
    /// </summary>
    public enum CommonErrors
    {
        NoVehicle,
        NeedToBeTheDriver,
        UnknownError,
        NotAllowed,
        InvalidModel,
        InvalidInput,
        InvalidSaveName,
        SaveNameAlreadyExists,
        CouldNotLoadSave,
        CouldNotLoad,
        PlayerNotFound,
        PedNotFound,
        WalkingStyleNotForMale,
        WalkingStyleNotForFemale,
        RightAlignedNotSupported,
    };

    /// <summary>
    /// Gets the formatted error message.
    /// </summary>
    public static class ErrorMessage
    {
        /// <summary>
        /// Returns the formatted error message for the specified error type.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="placeholderValue">An optional string that will be replaced inside the error message (if applicable).</param>
        /// <returns>The error message.</returns>
        public static string Get(CommonErrors errorType, string placeholderValue = null)
        {
            var outputMessage = "";
            var placeholder = placeholderValue != null ? " " + placeholderValue : "";
            outputMessage = errorType switch
            {
                CommonErrors.NeedToBeTheDriver => "你需要成为这辆车的司机.",
                CommonErrors.NoVehicle => $"你需要在车内{placeholder}.",
                CommonErrors.NotAllowed => $"你不被允许{placeholder}, 抱歉.",
                CommonErrors.InvalidModel => $"该模型~r~{placeholder} ~s~未找到, 你确定它是有效的吗？",
                CommonErrors.InvalidInput => $"输入~r~{placeholder} ~s~无效或你取消了操作, 请重试.",
                CommonErrors.InvalidSaveName => $"保存失败, 因为提供的保存名称~r~{placeholder} ~s~无效.",
                CommonErrors.SaveNameAlreadyExists => $"保存失败, 因为提供的保存名称~r~{placeholder} ~s~已经存在.",
                CommonErrors.CouldNotLoadSave => $"加载~r~{placeholder} ~s~失败!保存文件是否损坏？",
                CommonErrors.CouldNotLoad => $"无法加载~r~{placeholder}~s~, 抱歉!",
                CommonErrors.PedNotFound => $"指定的角色未找到.{placeholder}",
                CommonErrors.PlayerNotFound => $"指定的玩家未找到.{placeholder}",
                CommonErrors.WalkingStyleNotForMale => $"该行走风格不适用于男性角色.{placeholder}",
                CommonErrors.WalkingStyleNotForFemale => $"该行走风格不适用于女性角色.{placeholder}",
                CommonErrors.RightAlignedNotSupported => $"超宽屏幕不支持右对齐菜单.{placeholder}",
                _ => $"发生未知错误, 抱歉!{placeholder}",
            };
            return outputMessage;
        }
    }
    #endregion

    #region Notifications class
    /// <summary>
    /// Notifications class to easilly show notifications using custom made templates,
    /// or completely custom style if none of the templates are fitting for the current task.
    /// </summary>
    public static class Notify
    {
        /// <summary>
        /// Show a custom notification above the minimap.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Custom(string message, bool blink = true, bool saveToBrief = true)
        {
            SetNotificationTextEntry("CELL_EMAIL_BCON"); // 10x ~a~
            foreach (var s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                AddTextComponentSubstringPlayerName(s);
            }
            DrawNotification(blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Alert: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Alert(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~y~~h~警告~h~~s~: " + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Alert: " prefixed to the message.
        /// </summary>
        /// <param name="errorMessage">The error message template.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        /// <param name="placeholderValue">An optional string that will be replaced inside the error message template.</param>
        public static void Alert(CommonErrors errorMessage, bool blink = true, bool saveToBrief = true, string placeholderValue = null)
        {
            var message = ErrorMessage.Get(errorMessage, placeholderValue);
            Alert(message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Error: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Error(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~r~~h~错误~h~~s~: " + message, blink, saveToBrief);
            Debug.Write("[vMenu] [ERROR] " + message + "\n");
        }

        /// <summary>
        /// Show a notification with "Error: " prefixed to the message.
        /// </summary>
        /// <param name="errorMessage">The error message template.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        /// <param name="placeholderValue">An optional string that will be replaced inside the error message template.</param>
        public static void Error(CommonErrors errorMessage, bool blink = true, bool saveToBrief = true, string placeholderValue = null)
        {
            var message = ErrorMessage.Get(errorMessage, placeholderValue);
            Error(message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Info: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Info(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~b~~h~通知~h~~s~: " + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Success: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Success(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~g~~h~成功~h~~s~: " + message, blink, saveToBrief);
        }

        /// <summary>
        /// Shows a custom notification with an image attached.
        /// </summary>
        /// <param name="textureDict"></param>
        /// <param name="textureName"></param>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="subtitle"></param>
        /// <param name="safeToBrief"></param>
        public static void CustomImage(string textureDict, string textureName, string message, string title, string subtitle, bool saveToBrief, int iconType = 0)
        {
            SetNotificationTextEntry("CELL_EMAIL_BCON"); // 10x ~a~
            foreach (var s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                AddTextComponentSubstringPlayerName(s);
            }
            SetNotificationMessage(textureName, textureDict, false, iconType, title, subtitle);
            DrawNotification(false, saveToBrief);
        }
    }
    #endregion

    #region Custom Subtitle class
    /// <summary>
    /// Custom Subtitle class used to display subtitles using preformatted templates.
    /// Optionally you can also use a blank/custom style if you don't want to use an existing template.
    /// </summary>
    public static class Subtitle
    {
        /// <summary>
        /// Custom (white/custom text style subtitle)
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        public static void Custom(string message, int duration = 2500, bool drawImmediately = true)
        {
            BeginTextCommandPrint("CELL_EMAIL_BCON"); // 10x ~a~
            foreach (var s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                AddTextComponentSubstringPlayerName(s);
            }
            EndTextCommandPrint(duration, drawImmediately);
        }

        /// <summary>
        /// Alert (yellow text subtitle).
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        /// <param name="prefix">(Optional) add a prefix to your message, if you use this, only the prefix will be colored. The rest of the message will be left white.</param>
        public static void Alert(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~y~" + prefix + " ~s~" : "~y~") + message, duration, drawImmediately);
        }

        /// <summary>
        /// Error (red text subtitle).
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        /// <param name="prefix">(Optional) add a prefix to your message, if you use this, only the prefix will be colored. The rest of the message will be left white.</param>
        public static void Error(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~r~" + prefix + " ~s~" : "~r~") + message, duration, drawImmediately);
        }

        /// <summary>
        /// Info (blue text subtitle).
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        /// <param name="prefix">(Optional) add a prefix to your message, if you use this, only the prefix will be colored. The rest of the message will be left white.</param>
        public static void Info(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~b~" + prefix + " ~s~" : "~b~") + message, duration, drawImmediately);
        }

        /// <summary>
        /// Success (green text subtitle).
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="duration">(Optional) duration in ms.</param>
        /// <param name="drawImmediately">(Optional) draw the notification immediately or wait for the previous subtitle text to disappear.</param>
        /// <param name="prefix">(Optional) add a prefix to your message, if you use this, only the prefix will be colored. The rest of the message will be left white.</param>
        public static void Success(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~g~" + prefix + " ~s~" : "~g~") + message, duration, drawImmediately);
        }
    }
    #endregion

    public static class HelpMessage
    {


        public enum Label
        {
            EXIT_INTERIOR_HELP_MESSAGE
        }

        private static readonly Dictionary<Label, KeyValuePair<string, string>> labels = new()
        {
            [Label.EXIT_INTERIOR_HELP_MESSAGE] = new KeyValuePair<string, string>("EXIT_INTERIOR_HELP_MESSAGE", "Press ~INPUT_CONTEXT~ to exit the building.")
        };



        public static void Custom(string message) => Custom(message, 6000, true);
        public static void Custom(string message, int duration) => Custom(message, duration, true);
        public static void Custom(string message, int duration, bool sound)
        {
            var array = CommonFunctions.StringToArray(message);
            if (IsHelpMessageBeingDisplayed())
            {
                ClearAllHelpMessages();
            }
            BeginTextCommandDisplayHelp("CELL_EMAIL_BCON");
            foreach (var s in array)
            {
                AddTextComponentSubstringPlayerName(s);
            }
            EndTextCommandDisplayHelp(0, false, sound, duration);
        }

        public static void CustomLooped(Label label)
        {
            if (GetLabelText(labels[label].Key) == "NULL")
            {
                AddTextEntry(labels[label].Key, labels[label].Value);
            }
            //string[] array = CommonFunctions.StringToArray(message);
            //if (IsHelpMessageBeingDisplayed())
            //{
            //    ClearAllHelpMessages();
            //}
            //BeginTextCommandDisplayHelp("CELL_EMAIL_BCON");
            //foreach (string s in array)
            //{
            //    AddTextComponentSubstringPlayerName(s);
            //}
            DisplayHelpTextThisFrame(labels[label].Key, true);
            //EndTextCommandDisplayHelp(0, true, false, -1);
        }
    }
}
