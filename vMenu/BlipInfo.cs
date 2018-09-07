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

            if (model == (uint)GetHashKey("rhino"))
            {
                return 421; // tank
            }
            if (model == (uint)GetHashKey("KHANJALI"))
            {
                return 598; // new tank
            }
            if (model == (uint)GetHashKey("limo2"))
            {
                return 562; // armored limo
            }
            if (model == (uint)GetHashKey("TAMPA3") || model == (uint)GetHashKey("BARRAGE") || model == (uint)GetHashKey("HALFTRACK") || model == (uint)GetHashKey("HALFTRACK"))
            {
                return 562; // weaponized tampa (and similar small armored/weaponized vehicles)
            }
            if (model == (uint)GetHashKey("APC"))
            {
                return 558; // apc
            }
            if (model == (uint)GetHashKey("DUNE3"))
            {
                return 561; // weaponized dune
            }
            if (model == (uint)GetHashKey("INSURGENT") || model == (uint)GetHashKey("INSURGENT2") || model == (uint)GetHashKey("INSURGENT3") || model == (uint)GetHashKey("CARACARA"))
            {
                return 613; // inurgent pickup
            }
            if (model == (uint)GetHashKey("TRAILERSMALL2"))
            {
                return 563; // anti aircraft trailer
            }
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
                return 308; // submarine
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
            if (model == (uint)VehicleHash.Lazer || model == (uint)VehicleHash.Hydra || model == (uint)VehicleHash.Besra)
            {
                return 424; // (fighter)jet
            }
            if (IsThisModelAPlane(model))
            {
                return 423; // regular plane
            }
            return 225; // regular car
        }
    }
}
