using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuShared
{
    public static class PermissionsManager
    {
        public enum Permission
        {
            // Global
            Everything,
            DontKickMe,
            DontBanMe,
            NoClip,
            Staff,

            // Online Players
            OPMenu,
            OPAll,
            OPTeleport,
            OPWaypoint,
            OPSpectate,
            OPIdentifiers,
            OPSummon,
            OPKill,
            OPKick,
            OPPermBan,
            OPTempBan,
            OPUnban,
            OPViewBannedPlayers,

            // Player Options
            POMenu,
            POAll,
            POGod,
            POInvisible,
            POFastRun,
            POFastSwim,
            POSuperjump,
            PONoRagdoll,
            PONeverWanted,
            POSetWanted,
            POIgnored,
            POMaxHealth,
            POMaxArmor,
            POCleanPlayer,
            PODryPlayer,
            POWetPlayer,
            POVehicleAutoPilotMenu,
            POFreeze,
            POScenarios,
            POUnlimitedStamina,

            // Vehicle Options
            VOMenu,
            VOAll,
            VOGod,
            VOSpecialGod,
            VOKeepClean,
            VORepair,
            VOWash,
            VOEngine,
            VOSpeedLimiter,
            VOChangePlate,
            VOMod,
            VOColors,
            VOLiveries,
            VOComponents,
            VODoors,
            VOWindows,
            VOFreeze,
            VOTorqueMultiplier,
            VOPowerMultiplier,
            VOFlip,
            VOAlarm,
            VOCycleSeats,
            VOEngineAlwaysOn,
            VONoSiren,
            VONoHelmet,
            VOLights,
            VODelete,
            VOUnderglow,
            VOFlashHighbeamsOnHonk,
            VODisableTurbulence,
            VOFlares,
            VOPlaneBombs,


            // Vehicle Spawner
            VSMenu,
            VSAll,
            VSDisableReplacePrevious,
            VSSpawnByName,
            VSAddon,
            VSCompacts,
            VSSedans,
            VSSUVs,
            VSCoupes,
            VSMuscle,
            VSSportsClassic,
            VSSports,
            VSSuper,
            VSMotorcycles,
            VSOffRoad,
            VSIndustrial,
            VSUtility,
            VSVans,
            VSCycles,
            VSBoats,
            VSHelicopters,
            VSPlanes,
            VSService,
            VSEmergency,
            VSMilitary,
            VSCommercial,
            VSTrains,

            // Saved Vehicles
            SVMenu,
            SVAll,
            SVSpawn,

            // Player Appearance
            PAMenu,
            PAAll,
            PACustomize,
            PASpawnSaved,
            PASpawnNew,

            // Time Options
            TOMenu,
            TOAll,
            TOFreezeTime,
            TOSetTime,

            // Weather Options
            WOMenu,
            WOAll,
            WODynamic,
            WOBlackout,
            WOSetWeather,
            WORemoveClouds,
            WORandomizeClouds,

            // Weapon Options
            WPMenu,
            WPAll,
            WPGetAll,
            WPRemoveAll,
            WPUnlimitedAmmo,
            WPNoReload,
            WPSpawn,
            WPSpawnByName,
            WPSetAllAmmo,

            //Weapons Permissions
            WPAPPistol,
            WPAdvancedRifle,
            WPAssaultRifle,
            WPAssaultRifleMk2,
            WPAssaultSMG,
            WPAssaultShotgun,
            WPBZGas,
            WPBall,
            WPBat,
            WPBattleAxe,
            WPBottle,
            WPBullpupRifle,
            WPBullpupRifleMk2,
            WPBullpupShotgun,
            WPCarbineRifle,
            WPCarbineRifleMk2,
            WPCombatMG,
            WPCombatMGMk2,
            WPCombatPDW,
            WPCombatPistol,
            WPCompactGrenadeLauncher,
            WPCompactRifle,
            WPCrowbar,
            WPDagger,
            WPDoubleAction,
            WPDoubleBarrelShotgun,
            WPFireExtinguisher,
            WPFirework,
            WPFlare,
            WPFlareGun,
            WPFlashlight,
            WPGolfClub,
            WPGrenade,
            WPGrenadeLauncher,
            WPGrenadeLauncherSmoke,
            WPGusenberg,
            WPHammer,
            WPHatchet,
            WPHeavyPistol,
            WPHeavyShotgun,
            WPHeavySniper,
            WPHeavySniperMk2,
            WPHomingLauncher,
            WPKnife,
            WPKnuckleDuster,
            WPMG,
            WPMachete,
            WPMachinePistol,
            WPMarksmanPistol,
            WPMarksmanRifle,
            WPMarksmanRifleMk2,
            WPMicroSMG,
            WPMiniSMG,
            WPMinigun,
            WPMolotov,
            WPMusket,
            WPNightVision,
            WPNightstick,
            WPParachute,
            WPPetrolCan,
            WPPipeBomb,
            WPPistol,
            WPPistol50,
            WPPistolMk2,
            WPPoolCue,
            WPProximityMine,
            WPPumpShotgun,
            WPPumpShotgunMk2,
            WPRPG,
            WPRailgun,
            WPRevolver,
            WPRevolverMk2,
            WPSMG,
            WPSMGMk2,
            WPSNSPistol,
            WPSNSPistolMk2,
            WPSawnOffShotgun,
            WPSmokeGrenade,
            WPSniperRifle,
            WPSnowball,
            WPSpecialCarbine,
            WPSpecialCarbineMk2,
            WPStickyBomb,
            WPStunGun,
            WPSweeperShotgun,
            WPSwitchBlade,
            WPUnarmed,
            WPVintagePistol,
            WPWrench,
            WPPlasmaPistol, // xmas 2018 dlc (1604)
            WPPlasmaCarbine, // xmas 2018 dlc (1604)
            WPPlasmaMinigun, // xmas 2018 dlc (1604)

            // Weapon Loadouts Menu
            WLMenu,
            WLAll,
            WLEquip,
            WLEquipOnRespawn,

            // Misc Settings
            MSAll,
            MSClearArea,
            MSTeleportToWp,
            MSShowCoordinates,
            MSShowLocation,
            MSJoinQuitNotifs,
            MSDeathNotifs,
            MSNightVision,
            MSThermalVision,
            MSLocationBlips,
            MSPlayerBlips,
            MSTeleportLocations,
            MSConnectionMenu,
            MSRestoreAppearance,
            MSRestoreWeapons,
            MSDriftMode,

            // Voice Chat
            VCMenu,
            VCAll,
            VCEnable,
            VCShowSpeaker,
            VCStaffChannel,
        };

        public static Dictionary<Permission, bool> Permissions { get; private set; } = new Dictionary<Permission, bool>();

        /// <summary>
        /// Public function to check if a permission is allowed.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsAllowed(Permission permission, Player source = null) => IsDuplicityVersion() ? IsAllowedServer(permission, source) : IsAllowedClient(permission);

        private static Dictionary<Permission, bool> allowedPerms = new Dictionary<Permission, bool>();
        /// <summary>
        /// Private function that handles client side permission requests.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        private static bool IsAllowedClient(Permission permission)
        {
            if (allowedPerms.ContainsKey(permission))
            {
                return allowedPerms[permission];
            }

            allowedPerms[permission] = false;

            // Get a list of all permissions that are (parents) of the current permission, including the current permission.
            List<Permission> permissionsToCheck = GetPermissionAndParentPermissions(permission);

            // Check if any of those permissions is allowed, if so, return true.
            if (permissionsToCheck.Any(p => Permissions.ContainsKey(p) && Permissions[p]))
            {
                allowedPerms[permission] = true;
                return true;
            }


            // Return false if nothing is allowed.
            return false;
        }

        /// <summary>
        /// Checks if the player is allowed that specific permission.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private static bool IsAllowedServer(Permission permission, Player source)
        {
            if (source == null)
            {
                return false;
            }

            if (IsPlayerAceAllowed(source.Handle, GetAceName(permission)))
            {
                return true;
            }


            return false;
        }

        private static Dictionary<Permission, List<Permission>> parentPermissions = new Dictionary<Permission, List<Permission>>();

        /// <summary>
        /// Gets the current permission and all parent permissions.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        private static List<Permission> GetPermissionAndParentPermissions(Permission permission)
        {

            if (parentPermissions.ContainsKey(permission))
            {
                return parentPermissions[permission];
            }
            else
            {
                var list = new List<Permission>() { Permission.Everything, permission };
                string permStr = permission.ToString();

                // if the first 2 characters are both uppercase
                if (permStr.Substring(0, 2).ToUpper() == permStr.Substring(0, 2))
                {
                    if (!(permStr.Substring(2) == "All" || permStr.Substring(2) == "Menu"))
                    {
                        list.AddRange(Enum.GetValues(typeof(Permission)).Cast<Permission>().Where(a => a.ToString() == permStr.Substring(0, 2) + "All"));
                    }
                }
                else // it's one of the .Everything, .DontKickMe, DontBanMe, NoClip, Staff, etc perms that are not menu specific.
                {
                    // do nothing
                }
                Console.Write($"Returning all these: {Newtonsoft.Json.JsonConvert.SerializeObject(list)}\n");
                parentPermissions[permission] = list;
                return list;
            }


        }

        /// <summary>
        /// Sets the permissions for a specific player (checks server side, sends event to client side).
        /// </summary>
        /// <param name="player"></param>
        public static void SetPermissionsForPlayer([FromSource]Player player)
        {
            if (player == null)
            {
                return;
            }

            Dictionary<Permission, bool> perms = new Dictionary<Permission, bool>();

            // Add all permissions if the vMenu.Dev permission is added for Vespura only. Can be disable in the permissions.cfg
            // This is only used in case I need to debug an issue on your server related to vMenu. It only works for me, and does not give me any access outside of
            // vMenu at all! Feel free to remove it (in the permissions.cfg) if you don't want this, however I will not be able to help you without this.
            if (player.Identifiers.ToList().Any(id => id == "4510587c13e0b645eb8d24bc104601792277ab98") && IsPlayerAceAllowed(player.Handle, "vMenu.Dev"))
            {
                perms.Add(Permission.Everything, true);
            }

            if (!ConfigManager.GetSettingsBool(ConfigManager.Setting.vmenu_use_permissions))
            {
                foreach (var p in Enum.GetValues(typeof(Permission)))
                {
                    Permission permission = (Permission)p;
                    switch (permission)
                    {
                        // don't allow any of the following permissions if perms are ignored.
                        case Permission.Everything:
                        case Permission.OPAll:
                        case Permission.OPKick:
                        case Permission.OPKill:
                        case Permission.OPPermBan:
                        case Permission.OPTempBan:
                        case Permission.OPUnban:
                        case Permission.OPIdentifiers:
                        case Permission.OPViewBannedPlayers:
                            break;
                        // do allow the rest
                        default:
                            perms.Add(permission, true);
                            break;
                    }
                }
            }
            else
            {
                // Loop through all permissions and check if they're allowed.
                foreach (var p in Enum.GetValues(typeof(Permission)))
                {
                    Permission permission = (Permission)p;
                    if (!perms.ContainsKey(permission))
                        perms.Add(permission, IsAllowed(permission, player)); // triggers IsAllowedServer
                }
            }

            // Send the permissions to the client.
            player.TriggerEvent("vMenu:SetPermissions", Newtonsoft.Json.JsonConvert.SerializeObject(perms));

            // Also tell the client to do the addons setup.
            player.TriggerEvent("vMenu:SetAddons");
        }

        /// <summary>
        /// Sets the permission (client side event handler).
        /// </summary>
        /// <param name="permissions"></param>
        public static void SetPermissions(string permissions)
        {
            if (!IsDuplicityVersion())
            {
                Permissions = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<Permission, bool>>(permissions);
                // if debug logging.
                if (GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true")
                {
                    Debug.WriteLine("[vMenu] [Permissions] " + Newtonsoft.Json.JsonConvert.SerializeObject(Permissions, Newtonsoft.Json.Formatting.None));
                }

            }
        }

        /// <summary>
        /// Gets the full permission ace name for the specific <see cref="Permission"/> enum.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        private static string GetAceName(Permission permission)
        {
            string name = permission.ToString();

            string prefix = "vMenu.";

            switch (name.Substring(0, 2))
            {
                case "OP":
                    prefix += "OnlinePlayers";
                    break;
                case "PO":
                    prefix += "PlayerOptions";
                    break;
                case "VO":
                    prefix += "VehicleOptions";
                    break;
                case "VS":
                    prefix += "VehicleSpawner";
                    break;
                case "SV":
                    prefix += "SavedVehicles";
                    break;
                case "PA":
                    prefix += "PlayerAppearance";
                    break;
                case "TO":
                    prefix += "TimeOptions";
                    break;
                case "WO":
                    prefix += "WeatherOptions";
                    break;
                case "WP":
                    prefix += "WeaponOptions";
                    break;
                case "WL":
                    prefix += "WeaponLoadouts";
                    break;
                case "MS":
                    prefix += "MiscSettings";
                    break;
                case "VC":
                    prefix += "VoiceChat";
                    break;
                default:
                    return prefix + name;
            }

            return prefix + "." + name.Substring(2);
        }
    }
}
