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

                // create a UUID for this server to check versions and keep track of unique servers.
                string UUID;
                var existingUuid = LoadResourceFile(GetCurrentResourceName(), "uuid");
                if (existingUuid != null && existingUuid != "")
                {
                    UUID = existingUuid;
                }
                else
                {
                    Guid uuid = Guid.NewGuid();
                    UUID = uuid.ToString();
                    SaveResourceFile(GetCurrentResourceName(), "uuid", UUID, -1);
                }
                // sets the UUID convar.
                ExecuteCommand($"sets vMenuUUID {UUID.Substring(0, UUID.LastIndexOf('-'))}");
                ExecuteCommand($"sets vMenuVersion {MainServer.Version}");


                // Get a response from the specified url.
                RequestResponse result = await r.Http($"https://vespura.com/vmenu/version?id={UUID}&version={MainServer.Version}");


                Debug.WriteLine("\r\n[vMenu] Checking for updates.");

                // If the result status = 200 (status code OK) then continue.
                switch (result.status)
                {
                    case System.Net.HttpStatusCode.OK:
                        dynamic UpdateData = JsonConvert.DeserializeObject(result.content);
                        if ((bool)UpdateData["up_to_date"])
                        {
                            Debug.WriteLine("[vMenu] Your version of vMenu is up to date! Good job!");
                        }
                        else
                        {
                            Debug.WriteLine("[vMenu] WARNING: Your version of vMenu is OUTDATED!");
                            Debug.WriteLine("[vMenu] WARNING: Your version: " + MainServer.Version);
                            Debug.WriteLine("[vMenu] WARNING: Latest version: " + UpdateData["latest_version"]);
                            Debug.WriteLine("[vMenu] WARNING: Release date: " + UpdateData["release_date"]);
                            Debug.WriteLine("[vMenu] WARNING: Changelog summary: " + UpdateData["update_message"]);
                            Debug.WriteLine("[vMenu] WARNING: Please update as soon as possible!");
                            Debug.WriteLine("[vMenu] WARNING: Download: https://github.com/tomgrobbe/vMenu/releases/");
                            Debug.WriteLine("\n");
                            MainServer.UpToDate = false;
                        }
                        break;
                    default:
                        Debug.WriteLine("[vMenu] An error occurred while checking for the latest version. Please try again in a few hours.");
                        Debug.Write($"[vMenu] Error details: {result.content}\n");
                        break;
                }
            }
            // Aw damn! An exception. :(
            catch (Exception e)
            {
                Debug.WriteLine("\r\n\r\n[vMenu] An error occurred while checking for updates. If you require immediate assistance email: contact@vespura.com.");
                Debug.Write($"[vMenu] Error info: {e.Message.ToString()}\r\n\r\n");
            }
            CheckedForUpdates = true;
        }

        //private async void CheckUpdates()
        //{
        //    // Create a new request object.
        //    Request r = new Request();

        //    // Try to request a response.
        //    try
        //    {
        //        await Delay(500);
        //        // Get a response from the specified url.
        //        RequestResponse result = await r.Http("https://vespura.com/vMenu-version.json");
        //        // TODO: create webserver api for proper version checking.

        //        Debug.WriteLine("\r\n[vMenu] Checking for updates.");

        //        // If the result status = 200 (status code OK) then continue.
        //        switch (result.status)
        //        {
        //            case System.Net.HttpStatusCode.OK:
        //                // Get the results
        //                var currentVersion = GetResourceMetadata(GetCurrentResourceName(), "version", 0);
        //                dynamic output = JsonConvert.DeserializeObject<dynamic>(result.content);
        //                string version = output["version"].ToString();
        //                string date = output["date"].ToString();
        //                string changes = output["changes"].ToString() ?? "N/A";

        //                // Output the info.
        //                Debug.WriteLine($"[vMenu] Current version: {currentVersion}");
        //                Debug.WriteLine($"[vMenu] Latest version: {version}");
        //                Debug.WriteLine($"[vMenu] Release Date: {date}");

        //                // If up to date :)
        //                if (currentVersion == version)
        //                {
        //                    // yay up to date! :) Snail is happy.
        //                    Debug.WriteLine("\r\n[vMenu] You are currently using the latest version, good job!");
        //                }
        //                // If not up to date :(
        //                else
        //                {
        //                    MainServer.UpToDate = false;

        //                    // Snail is sad :(
        //                    Debug.WriteLine("\r\n[vMenu] You are NOT using the latest version. Please update to the latest version as soon as possible.");
        //                    Debug.WriteLine("[vMenu] Download the latest version here: https://github.com/tomgrobbe/vMenu/releases/ !");
        //                    Debug.WriteLine($"[vMenu] New in version {version}: \t{changes}\r\n");
        //                }

        //                break;
        //            default:
        //                Debug.WriteLine("[vMenu] An error occurred while checking for the latest version. Please try again in a few hours.");
        //                break;
        //        }
        //    }
        //    // Awww an exception. RIP.
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine("\r\n\r\n[vMenu] An error occurred while checking for updates. If you require immediate assistance email: contact@vespura.com.");
        //        Debug.WriteLine($"[vMenu] Error info: {e.Message.ToString()}\r\n\r\n");
        //    }
        //    CheckedForUpdates = true;
        //}
    }
}
