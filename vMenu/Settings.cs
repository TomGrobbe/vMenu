using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public static class Settings
    {

        // Constants.
        private const string SETTINGS_PREFIX = "settings_";

        #region Public variables.
        #region Voxty Options

        public static bool VoxtyHandOnRadio
        {
            get => GetSettingsBool("voxtyHandOnRadio");
            set => SetSavedSettingsBool("voxtyHandOnRadio", value);
        }

        public static bool VoxtyMinimapLock
        {
            get => GetSettingsBool("voxtyMinimapLock");
            set => SetSavedSettingsBool("voxtyMinimapLock", value);
        }

        public static bool VoxtyCurrentlyTalking
        {
            get => GetSettingsBool("voxtyCurrentlyTalking");
            set => SetSavedSettingsBool("voxtyCurrentlyTalking", value);
        }

        #region Framework Options    

        public static bool FrameworkCashHud
        {
            get => GetSettingsBool("frameworkCashHud");
            set => SetSavedSettingsBool("frameworkCashHud", value);
        }

        public static bool VoxtyForecasts
        {
            get => GetSettingsBool("voxtyForecasts");
            set => SetSavedSettingsBool("voxtyForecasts", value);
        }

        #endregion

        #region HUD Options    

        public static bool VoxtyPostalDisplay
        {
            get => GetSettingsBool("voxtyPostalDisplay");
            set => SetSavedSettingsBool("voxtyPostalDisplay", value);
        }

        public static bool VoxtyPriorityDisplay
        {
            get => GetSettingsBool("voxtyPriorityDisplay");
            set => SetSavedSettingsBool("voxtyPriorityDisplay", value);
        }

        public static bool VoxtyRepairShopDisplay
        {
            get => GetSettingsBool("voxtyRepairShopDisplay");
            set => SetSavedSettingsBool("voxtyRepairShopDisplay", value);
        }

        #endregion

        #region Map Blip Options    

        public static bool VoxtyAtmBlips
        {
            get => GetSettingsBool("voxtyAtmBlipDisplay");
            set => SetSavedSettingsBool("voxtyAtmBlipDisplay", value);
        }

        public static bool VoxtyGasBlips
        {
            get => GetSettingsBool("voxtyGasBlipDisplay");
            set => SetSavedSettingsBool("voxtyGasBlipDisplay", value);
        }

        public static bool VoxtyRepairBlips
        {
            get => GetSettingsBool("voxtyRepairShopDisplay");
            set => SetSavedSettingsBool("voxtyRepairShopDisplay", value);
        }

        public static bool VoxtyInteriorBlips
        {
            get => GetSettingsBool("voxtyInteriorBlipDisplay");
            set => SetSavedSettingsBool("voxtyInteriorBlipDisplay", value);
        }
        public static bool VoxtyPropertyBlips
        {
            get => GetSettingsBool("voxtyPorpertyBlipDisplay");
            set => SetSavedSettingsBool("voxtyPorpertyBlipDisplay", value);
        }


        #endregion

        #region ELS Options    

        public static float VoxtyAirhornClickVolume
        {
            get => GetSettingsFloat("voxtyAirhornClickVolume");
            set => SetSavedSettingsFloat("voxtyAirhornClickVolume", value);
        }

        public static float VoxtyLightingClickVolume
        {
            get => GetSettingsFloat("voxtyLightingClickVolume");
            set => SetSavedSettingsFloat("voxtyLightingClickVolume", value);
        }

        public static float VoxtySirenClickVolume
        {
            get => GetSettingsFloat("voxtySirenClickVolume");
            set => SetSavedSettingsFloat("voxtySirenClickVolume", value);
        }

        public static bool VoxtyFiveSirenSystem
        {
            get => GetSettingsBool("voxtyFiveSirenSystem");
            set => SetSavedSettingsBool("voxtyFiveSirenSystem", value);
        }

        #endregion

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
            bool exists = !string.IsNullOrEmpty(savedValue);
            // If not, create it and save the new default value of false.
            if (!exists)
            {
                // Some options should be enabled by default:
                if (
                    kvpString == "frameworkCashHud" ||
                    kvpString == "voxtyHandOnRadio" ||
                    kvpString == "voxtyPostalDisplay" ||
                    kvpString == "voxtyPriorityDisplay" ||
                    kvpString == "voxtyAtmBlipDisplay" ||
                    kvpString == "voxtyGasBlipDisplay" ||
                    kvpString == "voxtyRepairShopDisplay" ||
                    kvpString == "voxtyInteriorBlipDisplay" ||
                    kvpString == "voxtyPorpertyBlipDisplay"
                    )
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
                return GetResourceKvpString($"{SETTINGS_PREFIX}{kvpString}").ToLower() == "true";
            }
        }

        /// <summary>
        /// Sets the new saved value for the specified setting.
        /// </summary>
        /// <param name="kvpString">The setting to save.</param>
        /// <param name="newValue">The new value for this setting.</param>
        private static void SetSavedSettingsBool(string kvpString, bool newValue) => SetResourceKvp(SETTINGS_PREFIX + kvpString, newValue.ToString());

        private static float GetSettingsFloat(string kvpString)
        {
            float savedValue = GetResourceKvpFloat(SETTINGS_PREFIX + kvpString);
            if (savedValue.ToString() != null) // this can still become null for some reason, so that's why we check it.
            {
                if (savedValue is float)
                {
                    if (savedValue == 0 &&
                        (
                            kvpString == "voxtyAirhornClickVolume" ||
                            kvpString == "voxtyLightingClickVolume" ||
                            kvpString == "voxtySirenClickVolume"
                        )
                    )
                    {
                        SetSavedSettingsFloat(SETTINGS_PREFIX + kvpString, 0.3f);
                        return 0.3f;
                    }

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
        private static void SetSavedSettingsFloat(string kvpString, float newValue) => SetResourceKvpFloat(SETTINGS_PREFIX + kvpString, newValue);

        private static int GetSettingsInt(string kvpString)
        {
            // Get the current value.
            int savedValue = GetResourceKvpInt($"{SETTINGS_PREFIX}{kvpString}");
            return savedValue;
        }

        private static void SetSavedSettingsInt(string kvpString, int newValue) => SetResourceKvpInt(SETTINGS_PREFIX + kvpString, newValue);

        #endregion

        /*#region Public Functions
        /// <summary>
        /// Saves all personal settings to the client storage.
        /// </summary>
        public static void SaveSettings()
        {
            FrameworkSalary = Main.FrameworkOptions.Salary;
            FrameworkCashHud = Main.FrameworkOptions.CashHud;
            FrameworkEconBlips = Main.FrameworkOptions.EconBlips;
            VoxtyMinimapLocked = Main.Options.MinimapLocked;

            Notify.Success("Your settings have been saved.");
        }

        #endregion*/
    }

}