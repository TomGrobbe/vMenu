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


        /*
         * example .ini file
         * 
        
            ; last modified 1 April 2001 by John Doe
            [owner]
            name=John Doe
            organization=Acme Widgets Inc.

            [database]
            ; use IP address in case network name resolution is not working
            server=192.0.2.62     
            port=143
            file="payroll.dat"

         *
         *
         */

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
                    SendErrorMessage("File could not be found or it's empty.");
                }
            }
            initialized = true;
        }

        private static void ParseConfig(string config)
        {
            if (!String.IsNullOrEmpty(config))
            {
                config = config.Trim('\r');
                foreach (string line in config.Split('\n'))
                {

                }
            }
            else
            {
                SendErrorMessage("The config was empty or the file does not exist.");
            }
        }


        private static void SendErrorMessage(string details = null)
        {
            if (String.IsNullOrEmpty(details))
            {
                Debug.WriteLine("\n\n[vMenu] Error loading config file! Please notify the server owner if you see this.\n\n");
            }
            else
            {
                Debug.WriteLine($"\n\n[vMenu] Error loading config file! Please notify the server owner if you see this.\n\nError details: {details}\n\n");
            }
        }



        #region vehicle related settings
        // temporary solution (editable in resource.lua file)
        public static bool keepSpawnedVehiclesPersistent = (GetResourceMetadata(GetCurrentResourceName(), "keep_spawned_vehicles_persistent", 0) ?? "false") == "true";
        #endregion

    }
}
