using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMenu.Shared.Enums
{
    public enum Permission
    {
        #region Everything else
        Everything,
        Staff,
        Open,
        NoClip,
        #endregion

        #region Player Options
        POAll,
        POMenu,
        #endregion

        #region Vehicle Options
        VOAll,
        VOMenu,
        #endregion

        #region Vehicle Spawner
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
        VSOpenWheel,
        #endregion

        #region World Related Options
        WRAll,
        WRMenu,
        #endregion

        #region Voice Chat
        VCAll,
        VCMenu,
        #endregion
    }
}
