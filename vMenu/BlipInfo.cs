using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public static class BlipInfo
    {
        public static int GetBlipSpriteForVehicle(int vehicle)
        {
            uint model = (uint)GetEntityModel(vehicle);

            if (IsThisModelABicycle(model) || IsThisModelABike(model))
            {
                return 226; // (motor)bike
            }
            if (model == (uint)VehicleHash.Marquis)
            {
                return 410; // sailboat
            }
            if (model == (uint)VehicleHash.Submersible || model == (uint)VehicleHash.Submersible2)
            {
                return 308;
            }
            if (IsThisModelABoat(model) || IsThisModelAJetski(model))
            {
                return 427; // speed boat
            }
            if (IsThisModelAHeli(model))
            {
                
                return 422; // animated helicopter
            }
            if (IsThisModelACar(model))
            {
                return 225; // regular car
            }
            return 225; // regular car
        }
    }
}
