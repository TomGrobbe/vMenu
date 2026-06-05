using System.Collections.Generic;

using static CitizenFX.Core.Native.API;
using  CitizenFX.Core;

namespace vMenuClient.data
{
    public static class BlipInfo
    {
        public static int GetBlipSpriteForVehicle(int vehicle)
        {
            var model = (uint)GetEntityModel(vehicle);
            var sprites = new Dictionary<uint, int>()
            {
                { Game.GenerateHashASCII("taxi"), 56 },
                //
                { Game.GenerateHashASCII("nightshark"), 225 },
                //
                { Game.GenerateHashASCII("rhino"), 421 },
                //
                { Game.GenerateHashASCII("lazer"), 424 },
                { Game.GenerateHashASCII("besra"), 424 },
                { Game.GenerateHashASCII("hydra"), 424 },
                //
                { Game.GenerateHashASCII("insurgent"), 426 },
                { Game.GenerateHashASCII("insurgent2"), 426 },
                { Game.GenerateHashASCII("insurgent3"), 426 },
                //
                { Game.GenerateHashASCII("limo2"), 460 },
                //
                { Game.GenerateHashASCII("blazer5"), 512 },
                //
                { Game.GenerateHashASCII("phantom2"), 528 },
                { Game.GenerateHashASCII("boxville5"), 529 },
                { Game.GenerateHashASCII("ruiner2"), 530 },
                { Game.GenerateHashASCII("dune4"), 531 },
                { Game.GenerateHashASCII("dune5"), 531 },
                { Game.GenerateHashASCII("wastelander"), 532 },
                { Game.GenerateHashASCII("voltic2"), 533 },
                { Game.GenerateHashASCII("technical2"), 534 },
                { Game.GenerateHashASCII("technical3"), 534 },
                { Game.GenerateHashASCII("technical"), 534 },
                //
                { Game.GenerateHashASCII("apc"), 558 },
                { Game.GenerateHashASCII("oppressor"), 559 },
                { Game.GenerateHashASCII("oppressor2"), 559 },
                { Game.GenerateHashASCII("halftrack"), 560 },
                { Game.GenerateHashASCII("dune3"), 561 },
                { Game.GenerateHashASCII("tampa3"), 562 },
                { Game.GenerateHashASCII("trailersmall2"), 563 },
                //
                { Game.GenerateHashASCII("alphaz1"), 572 },
                { Game.GenerateHashASCII("bombushka"), 573 },
                { Game.GenerateHashASCII("havok"), 574 },
                { Game.GenerateHashASCII("howard"), 575 },
                { Game.GenerateHashASCII("hunter"), 576 },
                { Game.GenerateHashASCII("microlight"), 577 },
                { Game.GenerateHashASCII("mogul"), 578 },
                { Game.GenerateHashASCII("molotok"), 579 },
                { Game.GenerateHashASCII("nokota"), 580 },
                { Game.GenerateHashASCII("pyro"), 581 },
                { Game.GenerateHashASCII("rogue"), 582 },
                { Game.GenerateHashASCII("starling"), 583 },
                { Game.GenerateHashASCII("seabreeze"), 584 },
                { Game.GenerateHashASCII("tula"), 585 },
                //
                { Game.GenerateHashASCII("avenger"), 589 },
                //
                { Game.GenerateHashASCII("stromberg"), 595 },
                { Game.GenerateHashASCII("deluxo"), 596 },
                { Game.GenerateHashASCII("thruster"), 597 },
                { Game.GenerateHashASCII("khanjali"), 598 },
                { Game.GenerateHashASCII("riot2"), 599 },
                { Game.GenerateHashASCII("volatol"), 600 },
                { Game.GenerateHashASCII("barrage"), 601 },
                { Game.GenerateHashASCII("akula"), 602 },
                { Game.GenerateHashASCII("chernobog"), 603 },
            };

            if (sprites.ContainsKey(model))
            {
                return sprites[model];
            }
            else if (IsThisModelABike(model))
            {
                return 348;
            }
            else if (IsThisModelABoat(model))
            {
                return 427;
            }
            else if (IsThisModelAHeli(model))
            {
                return 422;
            }
            else if (IsThisModelAPlane(model))
            {
                return 423;
            }
            return 225;
        }
    }
}
