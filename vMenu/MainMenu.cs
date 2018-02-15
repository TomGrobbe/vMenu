using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenu
{
    public class MainMenu : BaseScript
    {
        private bool firstTick = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainMenu()
        {
            Tick += OnTick;
        }

        /// <summary>
        /// OnTick runs every game tick.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            if (firstTick)
            {
                firstTick = false;
                await Delay(0);
            }
        }


    }
}
