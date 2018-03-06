using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public static class UserDefaults
    {

        // Constants.
        private const string SETTINGS_PREFIX = "settings_";

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

        public static bool MiscSpeedKmh
        {
            get { return GetSettingsBool("miscSpeedoKmh"); }
            set { SetSavedSettingsBool("miscSpeedoKmh", value); }
        }

        public static bool MiscSpeedMph
        {
            get { return GetSettingsBool("miscSpeedoMph"); }
            set { SetSavedSettingsBool("miscSpeedoMph", value); }
        }

        public static bool MiscShowLocation
        {
            get { return GetSettingsBool("miscShowLocation"); }
            set { SetSavedSettingsBool("miscShowLocation", value); }
        }
        #endregion

        #region Voice Chat Settings
        public static bool VoiceChatEnabled
        {
            get { return GetSettingsBool("voiceChatEnabled"); }
            set { SetSavedSettingsBool("voiceChatEnabled", value); }
        }

        public static float VoiceChatProximity
        {
            get { return GetSettingsFloat("voiceChatProximity"); }
            set { SetSavedSettingsFloat("voiceChatProximity", value); }
        }

        public static bool ShowCurrentSpeaker
        {
            get { return GetSettingsBool("voiceChatShowSpeaker"); }
            set { SetSavedSettingsBool("voiceChatShowSpeaker", value); }
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
            string savedValue = GetResourceKvpString($"{SETTINGS_PREFIX}{kvpString}");
            // Check if it exists.
            bool exists = savedValue != "" && savedValue != null;
            // If not, create it and save the new default value of false.
            if (!exists)
            {
                // Some options should be enabled by default:
                if (kvpString == "unlimitedStamina" || kvpString == "miscDeathNotifications" || kvpString == "miscJoinQuitNotifications"
                    || kvpString == "vehicleSpawnerSpawnInside" || kvpString == "vehicleSpawnerReplacePrevious" || kvpString == "neverWanted"
                    || kvpString == "voiceChatShowSpeaker" || kvpString == "voiceChatEnabled")
                {
                    SetSavedSettingsBool(kvpString, true);
                    return true;
                }
                // All other options should be disabled by default:
                else
                {
                    SetSavedSettingsBool(kvpString, false);
                    return false;
                }
            }
            else
            {
                // Return the (new) value.
                return (GetResourceKvpString($"{SETTINGS_PREFIX}{kvpString}").ToLower() == "true");
            }
        }

        /// <summary>
        /// Sets the new saved value for the specified setting.
        /// </summary>
        /// <param name="kvpString">The setting to save.</param>
        /// <param name="newValue">The new value for this setting.</param>
        private static void SetSavedSettingsBool(string kvpString, bool newValue)
        {
            SetResourceKvp(SETTINGS_PREFIX + kvpString, newValue.ToString());
        }

        private static float GetSettingsFloat(string kvpString)
        {
            if ((FindKvp(StartFindKvp(SETTINGS_PREFIX + kvpString)) ?? "") != "")
            {
                float savedValue = GetResourceKvpFloat(SETTINGS_PREFIX + kvpString);
                return savedValue;
            }
            else
            {
                return -1f;
            }
        }

        private static void SetSavedSettingsFloat(string kvpString, float newValue)
        {
            SetResourceKvpFloat(SETTINGS_PREFIX + kvpString, newValue);
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Saves all personal settings to the client storage.
        /// </summary>
        public static void SaveSettingsAsync()
        {
            if (MainMenu.PlayerOptionsMenu != null)
            {
                EveryoneIgnorePlayer = MainMenu.PlayerOptionsMenu.PlayerIsIgnored;
                FastRun = MainMenu.PlayerOptionsMenu.PlayerFastRun;
                FastSwim = MainMenu.PlayerOptionsMenu.PlayerFastSwim;
                NeverWanted = MainMenu.PlayerOptionsMenu.PlayerNeverWanted;
                NoRagdoll = MainMenu.PlayerOptionsMenu.PlayerNoRagdoll;
                PlayerGodMode = MainMenu.PlayerOptionsMenu.PlayerGodMode;
                SuperJump = MainMenu.PlayerOptionsMenu.PlayerSuperJump;
                UnlimitedStamina = MainMenu.PlayerOptionsMenu.PlayerStamina;
            }

            if (MainMenu.MiscSettingsMenu != null)
            {
                MiscDeathNotifications = MainMenu.MiscSettingsMenu.DeathNotifications;
                MiscJoinQuitNotifications = MainMenu.MiscSettingsMenu.JoinQuitNotifications;
                MiscSpeedKmh = MainMenu.MiscSettingsMenu.ShowSpeedoKmh;
                MiscSpeedMph = MainMenu.MiscSettingsMenu.ShowSpeedoMph;
                MiscShowLocation = MainMenu.MiscSettingsMenu.ShowLocation;
            }

            if (MainMenu.VehicleOptionsMenu != null)
            {
                VehicleEngineAlwaysOn = MainMenu.VehicleOptionsMenu.VehicleEngineAlwaysOn;
                VehicleGodMode = MainMenu.VehicleOptionsMenu.VehicleGodMode;
                VehicleNoBikeHelmet = MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet;
                VehicleNoSiren = MainMenu.VehicleOptionsMenu.VehicleNoSiren;
            }

            if (MainMenu.VehicleSpawnerMenu != null)
            {
                VehicleSpawnerReplacePrevious = MainMenu.VehicleSpawnerMenu.ReplaceVehicle;
                VehicleSpawnerSpawnInside = MainMenu.VehicleSpawnerMenu.SpawnInVehicle;
            }

            // Todo:
            //VoiceChatEnabled
            //WeaponsNoReload
            //WeaponsUnlimitedAmmo


            MainMenu.Notify.Success("Your settings have been saved.");

        }

        #endregion
    }

}
