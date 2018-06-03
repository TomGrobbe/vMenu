using GHMatti.Http;
using vMenuServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace vMenuServer
{
    public class UpdateChecker : BaseScript
    {

        public static bool CheckedForUpdates = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public UpdateChecker()
        {
            CheckUpdates();
        }

        private async void CheckUpdates()
        {
            // Create a new request object.
            Request r = new Request();

            // Try to request a response.
            try
            {
                await Delay(500);
                // Get a response from the specified url.
                RequestResponse result = await r.Http("https://vespura.com/vMenu-version.json");
                // TODO: create webserver api for proper version checking.

                Debug.WriteLine("\r\n[vMenu] Checking for updates.");

                // If the result status = 200 (status code OK) then continue.
                if (result.status == System.Net.HttpStatusCode.OK)
                {
                    // Get the results
                    var currentVersion = GetResourceMetadata(GetCurrentResourceName(), "version", 0);
                    dynamic output = JsonConvert.DeserializeObject<dynamic>(result.content);
                    string version = output["version"].ToString();
                    string date = output["date"].ToString();
                    string changes = output["changes"].ToString() ?? "N/A";

                    // Output the info.
                    Debug.WriteLine($"[vMenu] Current version: {currentVersion}");
                    Debug.WriteLine($"[vMenu] Latest version: {version}");
                    Debug.WriteLine($"[vMenu] Release Date: {date}");

                    // If up to date :)
                    if (currentVersion == version)
                    {
                        // yay up to date! :) Snail is happy.
                        Debug.WriteLine("\r\n[vMenu] You are currently using the latest version, good job!");

                    }
                    // If not up to date :(
                    else
                    {
                        MainServer.UpToDate = false;

                        // Snail is sad :(
                        Debug.WriteLine("\r\n[vMenu] You are NOT using the latest version. Please update to the latest version as soon as possible.");
                        Debug.WriteLine("[vMenu] Download the latest version here: https://github.com/tomgrobbe/vMenu/releases/ !");
                        Debug.WriteLine($"[vMenu] New in version {version}: \t{changes}\r\n");
                    }
                }
                // An error occured, most likely: Vespura's VPS is down.
                else
                {
                    Debug.WriteLine("[vMenu] An error occurred while checking for the latest version. Please try again in a few hours.");
                }
            }
            // Awww an exception. RIP.
            catch (Exception e)
            {
                Debug.WriteLine("\r\n\r\n[vMenu] An error occurred while checking for updates. If you require immediate assistance email: contact@vespura.com.");
                Debug.WriteLine($"[vMenu] Error info: {e.Message.ToString()}\r\n\r\n");
            }
            CheckedForUpdates = true;
        }
    }
}
