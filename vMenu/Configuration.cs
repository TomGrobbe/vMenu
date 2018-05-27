using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public static class Configuration
    {
        /// <summary>
        /// all this is still a wip, currently all unused.
        /// </summary>

        
        
        
        
        
        private static bool initialized = false;
        public static void InitializeConfig()
        {
            if (!initialized)
            {
                string file = LoadResourceFile("vMenu", "config/config.ini");
                if (file != null && !string.IsNullOrEmpty(file))
                {
                    ParseConfig(file);
                }
                else
                {
                    Debug.Write("\n\n[vMenu] Error loading config file! Please notify the server owner if you see this.\n\n");
                }
            }
            initialized = true;
        }

        private static void ParseConfig(string config)
        {

        }




        #region vehicle related settings
        public static bool keepSpawnedVehiclesPersistent = false;
        #endregion

    }
}
