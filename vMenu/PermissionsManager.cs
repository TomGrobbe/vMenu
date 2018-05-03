using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace vMenuClient
{
    public enum Permission
    {
        // Global
        Everything,
        DontKickMe,
        NoClip,

        // Online Players
        OPMenu,
        OPAll,
        OPTeleport,
        OPWaypoint,
        OPSpectate,
        OPSummon,
        OPKill,
        OPKick,
        OPPermBan,
        OPTempBan,
        OPUnban,

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
        POFunctions,
        POFreeze,
        POScenarios,

        // Vehicle Options
        VOMenu,
        VOAll,
        VOGod,
        VOSpecialGod,
        VORepair,
        VOWash,
        VOEngine,
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
        WPSetAllAmmo,

        //Weapons Permissions
        WPSniperRifle,
        WPFireExtinguisher,
        WPCompactGrenadeLauncher,
        WPSnowball,
        WPVintagePistol,
        WPCombatPDW,
        WPHeavySniperMk2,
        WPHeavySniper,
        WPSweeperShotgun,
        WPMicroSMG,
        WPWrench,
        WPPistol,
        WPPumpShotgun,
        WPAPPistol,
        WPBall,
        WPMolotov,
        WPSMG,
        WPStickyBomb,
        WPPetrolCan,
        WPStunGun,
        WPAssaultRifleMk2,
        WPHeavyShotgun,
        WPMinigun,
        WPGolfClub,
        WPFlareGun,
        WPFlare,
        WPGrenadeLauncherSmoke,
        WPHammer,
        WPCombatPistol,
        WPGusenberg,
        WPCompactRifle,
        WPHomingLauncher,
        WPNightstick,
        WPRailgun,
        WPSawnOffShotgun,
        WPSMGMk2,
        WPBullpupRifle,
        WPFirework,
        WPCombatMG,
        WPCarbineRifle,
        WPCrowbar,
        WPFlashlight,
        WPDagger,
        WPGrenade,
        WPPoolCue,
        WPBat,
        WPPistol50,
        WPKnife,
        WPMG,
        WPBullpupShotgun,
        WPBZGas,
        WPUnarmed,
        WPGrenadeLauncher,
        WPNightVision,
        WPMusket,
        WPProximityMine,
        WPAdvancedRifle,
        WPRPG,
        WPPipeBomb,
        WPMiniSMG,
        WPSNSPistol,
        WPPistolMk2,
        WPAssaultRifle,
        WPSpecialCarbine,
        WPRevolver,
        WPMarksmanRifle,
        WPBattleAxe,
        WPHeavyPistol,
        WPKnuckleDuster,
        WPMachinePistol,
        WPCombatMGMk2,
        WPMarksmanPistol,
        WPMachete,
        WPSwitchBlade,
        WPAssaultShotgun,
        WPDoubleBarrelShotgun,
        WPAssaultSMG,
        WPHatchet,
        WPBottle,
        WPCarbineRifleMk2,
        WPParachute,
        WPSmokeGrenade,

        // Misc Settings
        //MSMenu, (removed because this menu should always be allowed).
        MSAll,
        MSClearArea,
        MSTeleportToWp,
        MSShowCoordinates,
        MSShowLocation,
        MSJoinQuitNotifs,
        MSDeathNotifs,
        MSNightVision,
        MSThermalVision,

        // Voice Chat
        VCMenu,
        VCAll,
        VCEnable,
        VCShowSpeaker,
        VCStaffChannel,

    };

    public static class PermissionsManager
    {
        public static List<string> Permissions { get; private set; } = new List<string>() { };

        public static void SetPermission(string permissionName, bool allowed)
        {
            if (allowed)
            {
                Permissions.Add(permissionName);
            }
        }

        public static bool IsAllowed(Permission permission)
        {
            if (Permissions.Contains("Everything"))
            {
                MainMenu.Cf.Log("Everything allowed, breaking.");
                return true;
            }
            else
            {
                var allowed = false;
                if (Permissions.Contains(permission.ToString().Substring(0, 2) + "All"))
                {
                    allowed = true;
                }
                if (!allowed)
                {
                    allowed = Permissions.Contains(permission.ToString());
                }
                return allowed;
            }
        }
    }
}
