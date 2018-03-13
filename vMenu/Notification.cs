﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

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
    };

    /// <summary>
    /// Gets the formatted error message.
    /// </summary>
    public struct ErrorMessage
    {
        /// <summary>
        /// Returns the formatted error message for the specified error type.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <param name="placeholderValue">An optional string that will be replaced inside the error message (if applicable).</param>
        /// <returns>The error message.</returns>
        public static string Get(CommonErrors errorType, string placeholderValue = null)
        {
            string outputMessage = "";
            string placeholder = placeholderValue != null ? " " + placeholderValue : "";
            switch (errorType)
            {
                case CommonErrors.NeedToBeTheDriver:
                    outputMessage = "You need to be the driver of this vehicle.";
                    break;
                case CommonErrors.NoVehicle:
                    outputMessage = $"You need to be inside a vehicle{placeholder}.";
                    break;
                case CommonErrors.NotAllowed:
                    outputMessage = $"You are not allowed to{placeholder}, sorry.";
                    break;
                case CommonErrors.InvalidModel:
                    outputMessage = $"This model~r~{placeholder} ~s~could not be found, are you sure it's valid?";
                    break;
                case CommonErrors.InvalidInput:
                    outputMessage = $"The input~r~{placeholder} ~s~is invalid or you cancelled the action, please try again.";
                    break;
                case CommonErrors.InvalidSaveName:
                    outputMessage = $"Saving failed because the provided save name~r~{placeholder} ~s~is invalid.";
                    break;
                case CommonErrors.SaveNameAlreadyExists:
                    outputMessage = $"Saving failed because the provided save name~r~{placeholder} ~s~already exists.";
                    break;
                case CommonErrors.CouldNotLoadSave:
                    outputMessage = $"Loading of~r~{placeholder} ~s~failed! Is the saves file corrupt?";
                    break;
                case CommonErrors.CouldNotLoad:
                    outputMessage = $"Could not load~r~{placeholder}~s~, sorry!";
                    break;
                case CommonErrors.PedNotFound:
                    outputMessage = $"The specified ped could not be found.{placeholder}";
                    break;
                case CommonErrors.PlayerNotFound:
                    outputMessage = $"The specified player could not be found.{placeholder}";
                    break;
                case CommonErrors.UnknownError:
                default:
                    outputMessage = $"An unknown error occurred, sorry!{placeholder}";
                    break;
            }
            return outputMessage;
        }
    }
    #endregion

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
            string[] messages = MainMenu.Cf.StringToArray(message);
            foreach (string msg in messages)
            {
                if (msg != null)
                {
                    AddTextComponentSubstringPlayerName(msg);
                }

            }
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
            Custom("~y~~h~Alert~h~~s~: " + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Alert: " prefixed to the message.
        /// </summary>
        /// <param name="errorMessage">The error message template.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        /// <param name="placeholderValue">An optional string that will be replaced inside the error message template.</param>
        public void Alert(CommonErrors errorMessage, bool blink = false, bool saveToBrief = true, string placeholderValue = null)
        {
            string message = ErrorMessage.Get(errorMessage, placeholderValue);
            Alert(message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Error: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public void Error(string message, bool blink = false, bool saveToBrief = true)
        {
            Custom("~r~~h~Error~h~~s~: " + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Error: " prefixed to the message.
        /// </summary>
        /// <param name="errorMessage">The error message template.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        /// <param name="placeholderValue">An optional string that will be replaced inside the error message template.</param>
        public void Error(CommonErrors errorMessage, bool blink = false, bool saveToBrief = true, string placeholderValue = null)
        {
            string message = ErrorMessage.Get(errorMessage, placeholderValue);
            Error(message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Info: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public void Info(string message, bool blink = false, bool saveToBrief = true)
        {
            Custom("~b~~h~Info~h~~s~: " + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Success: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public void Success(string message, bool blink = false, bool saveToBrief = true)
        {
            Custom("~g~~h~Success~h~~s~: " + message, blink, saveToBrief);
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
            var messages = MainMenu.Cf.StringToArray(message);
            foreach (var msg in messages)
            {
                AddTextComponentSubstringPlayerName(msg);
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
        public void Alert(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
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
        public void Error(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
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
        public void Info(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
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
        public void Success(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~g~" + prefix + " ~s~" : "~g~") + message, duration, drawImmediately);
        }
    }
    #endregion
}
