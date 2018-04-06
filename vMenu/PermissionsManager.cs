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

        // Misc Settings
        MSMenu,
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
            //if (allowed)
            //    MainMenu.Cf.Log("Permission: " + permissionName.ToString() + " is ALLOWED.");
            //else
            //    MainMenu.Cf.Log("Permission: " + permissionName.ToString() + " is DENIED.");
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
