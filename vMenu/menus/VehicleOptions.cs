using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    class VehicleOptions
    {
        // Menu variable, will be defined in CreateMenu()
        private UIMenu menu;
        private static Notification Notify = new Notification();
        private static Subtitles Subtitle = new Subtitles();

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = false;
    }
}
