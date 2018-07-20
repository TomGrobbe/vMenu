using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

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
        public static bool VehicleSpecialGodMode
        {
            get { return GetSettingsBool("vehicleSpecialGodMode"); }
            set { SetSavedSettingsBool("vehicleSpecialGodMode", value); }
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
            set { SetSavedSettingsBool("vehicleNoBikeHelmet", value); }
        }

        public static bool VehicleHighbeamsOnHonk
        {
            get { return GetSettingsBool("vehicleHighbeamsOnHonk"); }
            set { SetSavedSettingsBool("vehicleHighbeamsOnHonk", value); }
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

        public static bool AutoEquipChute
        {
            get { return GetSettingsBool("autoEquipParachuteWhenInPlane"); }
            set { SetSavedSettingsBool("autoEquipParachuteWhenInPlane", value); }
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

        public static bool MiscLocationBlips
        {
            get { return GetSettingsBool("miscLocationBlips"); }
            set { SetSavedSettingsBool("miscLocationBlips", value); }
        }

        public static bool MiscShowPlayerBlips
        {
            get { return GetSettingsBool("miscShowPlayerBlips"); }
            set { SetSavedSettingsBool("miscShowPlayerBlips", value); }
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
                if (kvpString == "unlimitedStamina" || kvpString == "miscDeathNotifications" || kvpString == "miscJoinQuitNotifications" || kvpString == "vehicleSpawnerSpawnInside" || kvpString == "vehicleSpawnerReplacePrevious" || kvpString == "neverWanted" || kvpString == "voiceChatShowSpeaker" || kvpString == "voiceChatEnabled" || kvpString == "autoEquipParachuteWhenInPlane")
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
            float savedValue = GetResourceKvpFloat(SETTINGS_PREFIX + kvpString);
            if (savedValue.ToString() != null) // this can still become null for some reason, so that's why we check it.
            {
                if (savedValue.GetType() == typeof(float))
                {
                    return savedValue;
                }
                else
                {
                    return -1f;
                }
            }
            else
            {
                SetSavedSettingsFloat(SETTINGS_PREFIX + kvpString, -1f);
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
        public static void SaveSettings()
        {
            Dictionary<string, dynamic> prefs = new Dictionary<string, dynamic>();
            if (MainMenu.PlayerOptionsMenu != null)
            {
                EveryoneIgnorePlayer = MainMenu.PlayerOptionsMenu.PlayerIsIgnored;
                prefs.Add("everyoneIgnorePlayer", MainMenu.PlayerOptionsMenu.PlayerIsIgnored);

                FastRun = MainMenu.PlayerOptionsMenu.PlayerFastRun;
                prefs.Add("fastRun", MainMenu.PlayerOptionsMenu.PlayerFastRun);

                FastSwim = MainMenu.PlayerOptionsMenu.PlayerFastSwim;
                prefs.Add("fastSwim", MainMenu.PlayerOptionsMenu.PlayerFastSwim);

                NeverWanted = MainMenu.PlayerOptionsMenu.PlayerNeverWanted;
                prefs.Add("neverWanted", MainMenu.PlayerOptionsMenu.PlayerNeverWanted);

                NoRagdoll = MainMenu.PlayerOptionsMenu.PlayerNoRagdoll;
                prefs.Add("noRagdoll", MainMenu.PlayerOptionsMenu.PlayerNoRagdoll);

                PlayerGodMode = MainMenu.PlayerOptionsMenu.PlayerGodMode;
                prefs.Add("playerGodMode", MainMenu.PlayerOptionsMenu.PlayerGodMode);

                SuperJump = MainMenu.PlayerOptionsMenu.PlayerSuperJump;
                prefs.Add("superJump", MainMenu.PlayerOptionsMenu.PlayerSuperJump);

                UnlimitedStamina = MainMenu.PlayerOptionsMenu.PlayerStamina;
                prefs.Add("unlimitedStamina", MainMenu.PlayerOptionsMenu.PlayerStamina);
            }

            if (MainMenu.MiscSettingsMenu != null)
            {
                MiscDeathNotifications = MainMenu.MiscSettingsMenu.DeathNotifications;
                prefs.Add("miscDeathNotifications", MainMenu.MiscSettingsMenu.DeathNotifications);

                MiscJoinQuitNotifications = MainMenu.MiscSettingsMenu.JoinQuitNotifications;
                prefs.Add("miscJoinQuitNotifications", MainMenu.MiscSettingsMenu.JoinQuitNotifications);

                MiscSpeedKmh = MainMenu.MiscSettingsMenu.ShowSpeedoKmh;
                prefs.Add("miscSpeedKmh", MainMenu.MiscSettingsMenu.ShowSpeedoKmh);

                MiscSpeedMph = MainMenu.MiscSettingsMenu.ShowSpeedoMph;
                prefs.Add("miscSpeedMph", MainMenu.MiscSettingsMenu.ShowSpeedoMph);

                MiscShowLocation = MainMenu.MiscSettingsMenu.ShowLocation;
                prefs.Add("miscShowLocation", MainMenu.MiscSettingsMenu.ShowLocation);

                MiscLocationBlips = MainMenu.MiscSettingsMenu.ShowLocationBlips;
                prefs.Add("miscLocationBlips", MainMenu.MiscSettingsMenu.ShowLocationBlips);

                MiscShowPlayerBlips = MainMenu.MiscSettingsMenu.ShowPlayerBlips;
                prefs.Add("miscShowPlayerBlips", MainMenu.MiscSettingsMenu.ShowPlayerBlips);
            }

            if (MainMenu.VehicleOptionsMenu != null)
            {
                VehicleEngineAlwaysOn = MainMenu.VehicleOptionsMenu.VehicleEngineAlwaysOn;
                prefs.Add("vehicleEngineAlwaysOn", MainMenu.VehicleOptionsMenu.VehicleEngineAlwaysOn);

                VehicleGodMode = MainMenu.VehicleOptionsMenu.VehicleGodMode;
                prefs.Add("vehicleGodMode", MainMenu.VehicleOptionsMenu.VehicleGodMode);

                VehicleNoBikeHelmet = MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet;
                prefs.Add("vehicleNoBikeHelmet", MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet);

                VehicleNoSiren = MainMenu.VehicleOptionsMenu.VehicleNoSiren;
                prefs.Add("vehicleNoSiren", MainMenu.VehicleOptionsMenu.VehicleNoSiren);

                VehicleHighbeamsOnHonk = MainMenu.VehicleOptionsMenu.FlashHighbeamsOnHonk;
                prefs.Add("vehicleHighbeamsOnHonk", MainMenu.VehicleOptionsMenu.FlashHighbeamsOnHonk);
            }

            if (MainMenu.VehicleSpawnerMenu != null)
            {
                VehicleSpawnerReplacePrevious = MainMenu.VehicleSpawnerMenu.ReplaceVehicle;
                prefs.Add("vehicleSpawnerReplacePrevious", MainMenu.VehicleSpawnerMenu.ReplaceVehicle);

                VehicleSpawnerSpawnInside = MainMenu.VehicleSpawnerMenu.SpawnInVehicle;
                prefs.Add("vehicleSpawnerSpawnInside", MainMenu.VehicleSpawnerMenu.SpawnInVehicle);
            }

            if (MainMenu.VoiceChatSettingsMenu != null)
            {
                VoiceChatEnabled = MainMenu.VoiceChatSettingsMenu.EnableVoicechat;
                prefs.Add("voiceChatEnabled", MainMenu.VoiceChatSettingsMenu.EnableVoicechat);

                VoiceChatProximity = MainMenu.VoiceChatSettingsMenu.currentProximity;
                prefs.Add("voiceChatProximity", MainMenu.VoiceChatSettingsMenu.currentProximity);
            }

            if (MainMenu.WeaponOptionsMenu != null)
            {
                WeaponsNoReload = MainMenu.WeaponOptionsMenu.NoReload;
                prefs.Add("weaponsNoReload", MainMenu.WeaponOptionsMenu.NoReload);

                WeaponsUnlimitedAmmo = MainMenu.WeaponOptionsMenu.UnlimitedAmmo;
                prefs.Add("weaponsUnlimitedAmmo", MainMenu.WeaponOptionsMenu.UnlimitedAmmo);
            }

            Notify.Success("Your settings have been saved.");

            MainMenu.Cf.Log($"Saving preferences:\n{JsonConvert.SerializeObject(prefs)}");
        }

        #endregion
    }

}
