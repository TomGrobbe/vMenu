using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json;

using vMenuClient.menus;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class EventManager : BaseScript
    {
        public static WeatherOptions WeatherOptionsMenu { get; private set; }
        public static bool IsSnowEnabled => GetSettingsBool(Setting.vmenu_enable_snow);
        public static int GetServerMinutes => MathUtil.Clamp(GetSettingsInt(Setting.vmenu_current_minute), 0, 59);
        public static int GetServerHours => MathUtil.Clamp(GetSettingsInt(Setting.vmenu_current_hour), 0, 23);
        public static int GetServerMinuteDuration => GetSettingsInt(Setting.vmenu_ingame_minute_duration);
        public static bool IsServerTimeFrozen => GetSettingsBool(Setting.vmenu_freeze_time);
        public static bool IsServerTimeSyncedWithMachineTime => GetSettingsBool(Setting.vmenu_sync_to_machine_time);
        public static string GetServerWeather => GetSettingsString(Setting.vmenu_current_weather, "CLEAR");
        public static bool DynamicWeatherEnabled => GetSettingsBool(Setting.vmenu_enable_dynamic_weather);
        public static bool IsBlackoutEnabled => GetSettingsBool(Setting.vmenu_blackout_enabled);
        public static bool IsVehicleLightsEnabled => GetSettingsBool(Setting.vmenu_vehicle_blackout_enabled);
        public static int WeatherChangeTime => MathUtil.Clamp(GetSettingsInt(Setting.vmenu_weather_change_duration), 0, 45);

        /// <summary>
        /// Constructor.
        /// </summary>
        public EventManager()
        {
            // Add event handlers.
            EventHandlers.Add("vMenu:SetAddons", new Action(SetConfigOptions)); // DEPRECATED: Backwards-compatible event handler; use 'vMenu:SetConfigOptions' instead
            EventHandlers.Add("vMenu:SetConfigOptions", new Action(SetConfigOptions));
            EventHandlers.Add("vMenu:SetPermissions", new Action<string>(MainMenu.SetPermissions));
            EventHandlers.Add("vMenu:GoToPlayer", new Action<string>(SummonPlayer));
            EventHandlers.Add("vMenu:KillMe", new Action<string>(KillMe));
            EventHandlers.Add("vMenu:Notify", new Action<string>(NotifyPlayer));
            EventHandlers.Add("vMenu:SetClouds", new Action<float, string>(SetClouds));
            EventHandlers.Add("vMenu:GoodBye", new Action(GoodBye));
            EventHandlers.Add("vMenu:SetBanList", new Action<string>(UpdateBanList));
            EventHandlers.Add("vMenu:ClearArea", new Action<float, float, float>(ClearAreaNearPos));
            EventHandlers.Add("vMenu:updatePedDecors", new Action(UpdatePedDecors));
            EventHandlers.Add("playerSpawned", new Action(SetAppearanceOnFirstSpawn));
            EventHandlers.Add("vMenu:GetOutOfCar", new Action<int, int>(GetOutOfCar));
            EventHandlers.Add("vMenu:PrivateMessage", new Action<string, string>(PrivateMessage));
            EventHandlers.Add("vMenu:UpdateTeleportLocations", new Action<string>(UpdateTeleportLocations));

            if (GetSettingsBool(Setting.vmenu_enable_weather_sync))
            {
                Tick += WeatherSync;
            }

            if (GetSettingsBool(Setting.vmenu_enable_time_sync))
            {
                Tick += TimeSync;
            }

            RegisterNuiCallbackType("disableImportExportNUI");
            RegisterNuiCallbackType("importData");
        }

        [EventHandler("__cfx_nui:importData")]
        internal void ImportData(IDictionary<string, object> data, CallbackDelegate cb)
        {
            SetNuiFocus(false, false);
            Notify.Info("Debug info: This feature is not yet available, check back later.");
            cb(JsonConvert.SerializeObject(new { ok = true }));
        }

        [EventHandler("__cfx_nui:disableImportExportNUI")]
        internal void DisableImportExportNUI(IDictionary<string, object> data, CallbackDelegate cb)
        {
            SetNuiFocus(false, false);
            Notify.Info("Debug info: Closing import/export NUI window.");
            cb(JsonConvert.SerializeObject(new { ok = true }));
        }

        private bool firstSpawn = true;
        /// <summary>
        /// Sets the saved character whenever the player first spawns.
        /// </summary>
        private async void SetAppearanceOnFirstSpawn()
        {
            if (firstSpawn)
            {
                firstSpawn = false;
                if (MainMenu.MiscSettingsMenu != null && MainMenu.MpPedCustomizationMenu != null && MainMenu.MiscSettingsMenu.MiscRespawnDefaultCharacter && !string.IsNullOrEmpty(GetResourceKvpString("vmenu_default_character")) && !GetSettingsBool(Setting.vmenu_disable_spawning_as_default_character))
                {
                    await MainMenu.MpPedCustomizationMenu.SpawnThisCharacter(GetResourceKvpString("vmenu_default_character"), false);
                }
                while (!IsScreenFadedIn() || IsPlayerSwitchInProgress() || IsPauseMenuActive() || GetIsLoadingScreenActive())
                {
                    await Delay(0);
                }
                if (MainMenu.WeaponLoadoutsMenu != null && MainMenu.WeaponLoadoutsMenu.WeaponLoadoutsSetLoadoutOnRespawn && IsAllowed(Permission.WLEquipOnRespawn))
                {
                    var saveName = GetResourceKvpString("vmenu_string_default_loadout");
                    if (!string.IsNullOrEmpty(saveName))
                    {
                        await SpawnWeaponLoadoutAsync(saveName, true, false, true);
                    }

                }
            }
        }

        /// <summary>
        /// Sets the addon models from the addons.json file.
        /// </summary>
        private void SetConfigOptions()
        {
            SetAddons();
            SetExtras();

            MainMenu.ConfigOptionsSetupComplete = true;
        }

        /// <summary>
        /// Sets the addon models from the addons.json file.
        /// </summary>
        private void SetAddons()
        {
            // reset addons
            VehicleSpawner.AddonVehicles = new Dictionary<string, uint>();
            WeaponOptions.AddonWeapons = new Dictionary<string, uint>();
            PlayerAppearance.AddonPeds = new Dictionary<string, uint>();

            var jsonData = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
            try
            {
                // load new addons.
                var addons = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonData);

                // load vehicles
                if (addons.ContainsKey("vehicles"))
                {
                    foreach (var addon in addons["vehicles"])
                    {
                        if (!VehicleSpawner.AddonVehicles.ContainsKey(addon))
                        {
                            VehicleSpawner.AddonVehicles.Add(addon, (uint)GetHashKey(addon));
                        }
                        else
                        {
                            Debug.WriteLine($"[vMenu] [Error] Your addons.json file contains 2 or more entries with the same vehicle name! ({addon}) Please remove duplicate lines!");
                        }
                    }
                }

                // load weapons
                if (addons.ContainsKey("weapons"))
                {
                    foreach (var addon in addons["weapons"])
                    {
                        if (!WeaponOptions.AddonWeapons.ContainsKey(addon))
                        {
                            WeaponOptions.AddonWeapons.Add(addon, (uint)GetHashKey(addon));
                        }
                        else
                        {
                            Debug.WriteLine($"[vMenu] [Error] Your addons.json file contains 2 or more entries with the same weapon name! ({addon}) Please remove duplicate lines!");
                        }
                    }
                }

                // load peds.
                if (addons.ContainsKey("peds"))
                {
                    foreach (var addon in addons["peds"])
                    {
                        if (!PlayerAppearance.AddonPeds.ContainsKey(addon))
                        {
                            PlayerAppearance.AddonPeds.Add(addon, (uint)GetHashKey(addon));
                        }
                        else
                        {
                            Debug.WriteLine($"[vMenu] [Error] Your addons.json file contains 2 or more entries with the same ped name! ({addon}) Please remove duplicate lines!");
                        }
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine($"\n\n^1[vMenu] [ERROR] ^7Your addons.json file contains a problem! Error details: {ex.Message}\n\n");
            }
        }

        /// <summary>
        /// Sets the extras labels from the extras.json file.
        /// </summary>
        private void SetExtras()
        {
            // reset addons
            VehicleOptions.VehicleExtras = new Dictionary<uint, Dictionary<int, string>>();

            string jsonData = LoadResourceFile(GetCurrentResourceName(), "config/extras.json") ?? "{}";

            try
            {
                // load new extras.
                var extras = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, string>>>(jsonData);

                foreach (string model in extras.Keys)
                {
                    uint modelHash = (uint)GetHashKey(model);

                    if (extras[model] != null && extras[model].Count > 0)
                    {
                        if (!VehicleOptions.VehicleExtras.ContainsKey(modelHash) || VehicleOptions.VehicleExtras[modelHash] == null)
                            VehicleOptions.VehicleExtras.Add(modelHash, extras[model]);
                        else
                        {
                            foreach(int extra in extras[model].Keys)
                            {
                                if(!VehicleOptions.VehicleExtras[modelHash].ContainsKey(extra))
                                    VehicleOptions.VehicleExtras[modelHash].Add(extra, extras[model][extra]);
                                else
                                    Debug.WriteLine($"[vMenu] [Warning] Your extras.json file contains 2 or more entries with the same extra index! ({model}, Extra {extra}) Please remove duplicate!");
                            }
                        }
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine($"\n\n^1[vMenu] [ERROR] ^7Your extras.json file contains a problem! Error details: {ex.Message}\n\n");
            }
        }

        /// <summary>
        /// Update ban list.
        /// </summary>
        /// <param name="list"></param>
        private void UpdateBanList(string list)
        {
            MainMenu.BannedPlayersMenu?.UpdateBanList(list);
        }

        /// <summary>
        /// Used for cheaters.
        /// </summary>
        private void GoodBye()
        {
            ForceSocialClubUpdate();
        }

        /// <summary>
        /// Loads/unloads the snow fx particles if needed.
        /// </summary>
        private async Task UpdateWeatherParticles()
        {
            ForceSnowPass(IsSnowEnabled);
            SetForceVehicleTrails(IsSnowEnabled);
            SetForcePedFootstepsTracks(IsSnowEnabled);
            if (IsSnowEnabled)
            {
                if (!HasNamedPtfxAssetLoaded("core_snow"))
                {
                    RequestNamedPtfxAsset("core_snow");
                    while (!HasNamedPtfxAssetLoaded("core_snow"))
                    {
                        await Delay(0);
                    }
                }
                UseParticleFxAssetNextCall("core_snow");
            }
            else
            {
                RemoveNamedPtfxAsset("core_snow");
            }
        }

        /// <summary>
        /// OnTick loop to keep the weather synced.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherSync()
        {
            await UpdateWeatherParticles();
            SetArtificialLightsState(IsBlackoutEnabled);
            SetArtificialLightsStateAffectsVehicles(!IsVehicleLightsEnabled);

            if (GetNextWeatherType() != GetHashKey(GetServerWeather))
            {
                SetWeatherTypeOvertimePersist(GetServerWeather, (float)WeatherChangeTime);
                await Delay((WeatherChangeTime * 1000) + 2000);

                TriggerEvent("vMenu:WeatherChangeComplete", GetServerWeather);
            }

            await Delay(1000);
        }


        /// <summary>
        /// This function will take care of time sync. It'll be called once, and never stop.
        /// </summary>
        /// <returns></returns>
        private async Task TimeSync()
        {
            NetworkOverrideClockTime(GetServerHours, GetServerMinutes, 0);
            if (IsServerTimeFrozen || IsServerTimeSyncedWithMachineTime)
            {
                await Delay(5);
            }
            else
            {
                await Delay(MathUtil.Clamp(GetServerMinuteDuration, 100, 2000));
            }
        }

        /// <summary>
        /// Set the cloud hat type.
        /// </summary>
        /// <param name="opacity"></param>
        /// <param name="cloudsType"></param>
        private void SetClouds(float opacity, string cloudsType)
        {
            if (opacity == 0f && cloudsType == "removed")
            {
                ClearCloudHat();
            }
            else
            {
                SetCloudHatOpacity(opacity);
                SetCloudHatTransition(cloudsType, 4f);
            }
        }

        /// <summary>
        /// Used by events triggered from the server to notify a user.
        /// </summary>
        /// <param name="message"></param>
        private void NotifyPlayer(string message)
        {
            Notify.Custom(message, true, true);
        }

        /// <summary>
        /// Kill this player, poor thing, someone wants you dead... R.I.P.
        /// </summary>
        private void KillMe(string sourceName)
        {
            Notify.Alert($"You have been killed by <C>{GetSafePlayerName(sourceName)}</C>~s~ using the ~r~Kill Player~s~ option in vMenu.");
            SetEntityHealth(Game.PlayerPed.Handle, 0);
        }

        /// <summary>
        /// Teleport to the specified player.
        /// </summary>
        /// <param name="targetPlayer"></param>
        private async void SummonPlayer(string targetPlayer)
        {
            // ensure the player list is requested in case of Infinity
            MainMenu.PlayersList.RequestPlayerList();
            await MainMenu.PlayersList.WaitRequested();

            var player = MainMenu.PlayersList.FirstOrDefault(a => a.ServerId == int.Parse(targetPlayer));

            if (player != null)
            {
                _ = TeleportToPlayer(player);
            }
        }

        /// <summary>
        /// Clear the area around the provided x, y, z coordinates. Clears everything like (destroyed) objects, peds, (ai) vehicles, etc.
        /// Also restores broken streetlights, etc.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void ClearAreaNearPos(float x, float y, float z)
        {
            ClearAreaOfEverything(x, y, z, 100f, false, false, false, false);
        }

        /// <summary>
        /// Kicks the current player from the specified vehicle if they're inside and don't own the vehicle themselves.
        /// </summary>
        /// <param name="vehNetId"></param>
        /// <param name="vehicleOwnedBy"></param>
        private async void GetOutOfCar(int vehNetId, int vehicleOwnedBy)
        {
            if (NetworkDoesNetworkIdExist(vehNetId))
            {
                var veh = NetToVeh(vehNetId);
                if (DoesEntityExist(veh))
                {
                    var vehicle = new Vehicle(veh);

                    if (vehicle == null || !vehicle.Exists())
                    {
                        return;
                    }

                    if (Game.PlayerPed.IsInVehicle(vehicle) && vehicleOwnedBy != Game.Player.ServerId)
                    {
                        if (!vehicle.IsStopped)
                        {
                            Notify.Alert("The owner of this vehicle is reclaiming their personal vehicle. You will be kicked from this vehicle in about 10 seconds. Stop the vehicle now to avoid taking damage.", false, true);
                        }

                        // Wait for the vehicle to come to a stop, or 10 seconds, whichever is faster.
                        var timer = GetGameTimer();
                        while (vehicle != null && vehicle.Exists() && !vehicle.IsStopped)
                        {
                            await Delay(0);
                            if (GetGameTimer() - timer > (10 * 1000)) // 10 second timeout
                            {
                                break;
                            }
                        }

                        // just to make sure they're actually still inside the vehicle and the vehicle still exists.
                        if (vehicle != null && vehicle.Exists() && Game.PlayerPed.IsInVehicle(vehicle))
                        {
                            // Make the ped jump out because the car isn't stopped yet.
                            if (!vehicle.IsStopped)
                            {
                                Notify.Info("You were warned, now you'll have to suffer the consequences!");
                                TaskLeaveVehicle(Game.PlayerPed.Handle, vehicle.Handle, 4160);
                            }
                            // Make the ped exit gently.
                            else
                            {
                                TaskLeaveVehicle(Game.PlayerPed.Handle, vehicle.Handle, 0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates ped decorators for the clothing animation when players have joined.
        /// </summary>
        private async void UpdatePedDecors()
        {
            await Delay(1000);
            var backup = PlayerAppearance.ClothingAnimationType;
            PlayerAppearance.ClothingAnimationType = -1;
            await Delay(100);
            PlayerAppearance.ClothingAnimationType = backup;
        }

        /// <summary>
        /// Updates the teleports locations data from the server side locations.json, because that doesn't update client side on change.
        /// </summary>
        /// <param name="jsonData"></param>
        private void UpdateTeleportLocations(string jsonData)
        {
            MiscSettings.TpLocations = JsonConvert.DeserializeObject<List<vMenuShared.ConfigManager.TeleportLocation>>(jsonData);
        }
    }
}
