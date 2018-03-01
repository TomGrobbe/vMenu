using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public class UserDefaults : BaseScript
    {

        // Constants.
        private const string SETTINGS_PREFIX = "settings_";

        // Constructor
        public UserDefaults() { }

        #region Public variables.
        #region PlayerOptions
        public static bool PlayerGodMode
        {
            get { return GetSettingsBool("playerGodMode"); }
            set { SetSavedSettingsBool("playerGodMode", value); }
        }

        public static bool UnlimitedStamina
        {
            get { return GetSettingsBool("unlimitedStamina"); }
            set { SetSavedSettingsBool("unlimitedStamina", value); }
        }

        public static bool FastRun
        {
            get { return GetSettingsBool("fastRun"); }
            set { SetSavedSettingsBool("fastRun", value); }
        }

        public static bool FastSwim
        {
            get { return GetSettingsBool("fastSwim"); }
            set { SetSavedSettingsBool("fastSwim", value); }
        }

        public static bool SuperJump
        {
            get { return GetSettingsBool("superJump"); }
            set { SetSavedSettingsBool("superJump", value); }
        }

        public static bool NoRagdoll
        {
            get { return GetSettingsBool("noRagdoll"); }
            set { SetSavedSettingsBool("noRagdoll", value); }
        }

        public static bool NeverWanted
        {
            get { return GetSettingsBool("neverWanted"); }
            set { SetSavedSettingsBool("neverWanted", value); }
        }

        public static bool EveryoneIgnorePlayer
        {
            get { return GetSettingsBool("everyoneIgnorePlayer"); }
            set { SetSavedSettingsBool("everyoneIgnorePlayer", value); }
        }
        #endregion

        #region Vehicle Options
        public static bool VehicleGodMode
        {
            get { return GetSettingsBool("vehicleGodMode"); }
            set { SetSavedSettingsBool("vehicleGodMode", value); }
        }

        public static bool VehicleEngineAlwaysOn
        {
            get { return GetSettingsBool("vehicleEngineAlwaysOn"); }
            set { SetSavedSettingsBool("vehicleEngineAlwaysOn", value); }
        }

        public static bool VehicleNoSiren
        {
            get { return GetSettingsBool("vehicleNoSiren"); }
            set { SetSavedSettingsBool("vehicleNoSiren", value); }
        }

        public static bool VehicleNoBikeHelmet
        {
            get { return GetSettingsBool("vehicleNoBikeHelmet"); }
            set { SetSavedSettingsBool("vehicleNoBikeHelemet", value); }
        }
        #endregion

        #region Vehicle Spawner Options
        public static bool VehicleSpawnerSpawnInside
        {
            get { return GetSettingsBool("vehicleSpawnerSpawnInside"); }
            set { SetSavedSettingsBool("vehicleSpawnerSpawnInside", value); }
        }

        public static bool VehicleSpawnerReplacePrevious
        {
            get { return GetSettingsBool("vehicleSpawnerReplacePrevious"); }
            set { SetSavedSettingsBool("vehicleSpawnerReplacePrevious", value); }
        }
        #endregion

        #region Weapon Options
        public static bool WeaponsNoReload
        {
            get { return GetSettingsBool("weaponsNoReload"); }
            set { SetSavedSettingsBool("weaponsNoReload", value); }
        }

        public static bool WeaponsUnlimitedAmmo
        {
            get { return GetSettingsBool("weaponsUnlimitedAmmo"); }
            set { SetSavedSettingsBool("weaponsUnlimitedAmmo", value); }
        }
        #endregion

        #region Misc Settings
        public static bool MiscJoinQuitNotifications
        {
            get { return GetSettingsBool("miscJoinQuitNotifications"); }
            set { SetSavedSettingsBool("miscJoinQuitNotifications", value); }
        }

        public static bool MiscDeathNotifications
        {
            get { return GetSettingsBool("miscDeathNotifications"); }
            set { SetSavedSettingsBool("miscDeathNotifications", value); }
        }
        #endregion

        #region Voice Chat Settings
        public static bool VoiceChatEnabled
        {
            get { return GetSettingsBool("voiceChatEnabled"); }
            set { SetSavedSettingsBool("voiceChatEnabled", value); }
        }
        #endregion


        #endregion


        #region Private functions
        /// <summary>
        /// Gets whether or not the specified setting is enabled or disabled in the saved user settings.
        /// Always returns false by default if the setting does not exist.
        /// </summary>
        /// <param name="kvpString">The setting to get.</param>
        /// <returns></returns>
        private static bool GetSettingsBool(string kvpString)
        {
            // Get the current value.
            var savedValue = GetResourceKvpString($"{SETTINGS_PREFIX}{kvpString}");
            // Check if it exists.
            bool exists = savedValue != "" && savedValue != null;
            // If not, create it and save the new default value of false.
            if (!exists)
            {
                // If the option is unlimitedStamina, then set that to true because we want this to be enabled by default.
                if (kvpString == "unlimitedStamina")
                {
                    SetSavedSettingsBool(kvpString, true);
                }
                // If it's not the unlimited stamina option, then just disable it by default.
                else
                {
                    SetSavedSettingsBool(kvpString, false);
                }
            }
            // Return the (new) value.
            return (GetResourceKvpString($"{SETTINGS_PREFIX}{kvpString}").ToLower() == "true");
        }

        /// <summary>
        /// Sets the new saved value for the specified setting.
        /// </summary>
        /// <param name="kvpString">The setting to save.</param>
        /// <param name="newValue">The new value for this setting.</param>
        private static void SetSavedSettingsBool(string kvpString, bool newValue)
        {
            SetResourceKvp(kvpString, newValue.ToString());
        }





        #endregion
    }

}
