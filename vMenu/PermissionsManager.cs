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

        // Onlie Players
        OPMenu,
        OPAll,
        OPTeleport,
        OPWaypoint,
        OPSpectate,
        OPSummon,
        OPKill,
        OPKick,

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

        // Vehicle Spawner
        VSMenu,
        VSAll,
        VSSpawnByName,
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

        // Misc Settings
        MSMenu,
        MSAll,
        MSTeleportToWp,
        MSShowCoordinates,
        MSShowLocation,
        MSJoinQuitNotifs,
        MSDeathNotifs,

        // Voice Chat
        VCMenu,
        VCAll,
        VCEnable,
        VCShowSpeaker,
        VCStaffChannel,
    };

    public static class PermissionsManager
    {
        private static List<string> perms = new List<string>() { };

        public static void SetPermission(string permissionName, bool allowed)
        {
            if (allowed)
            {
                perms.Add(permissionName);
            }
        }


        public static bool IsAllowed(Permission permission)
        {
            if (perms.Contains("Everything"))
            {
                if (MainMenu.DebugMode)
                {
                    MainMenu.Cf.Log("Everything allowed, breaking.");
                }
                return true;
            }
            else
            {
                var allowed = false;
                if (perms.Contains(permission.ToString().Substring(0,2) + "All"))
                {
                    allowed = true;
                }

                if (!allowed)
                {
                    allowed = perms.Contains(permission.ToString());
                }
                
                return allowed;
            }
            

        }
    }
}
