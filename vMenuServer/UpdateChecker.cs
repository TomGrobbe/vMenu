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


                Debug.WriteLine("\r\n^5[vMenu] Checking for updates.^0");

                // If the result status = 200 (status code OK) then continue.
                switch (result.status)
                {
                    case System.Net.HttpStatusCode.OK:
                        dynamic UpdateData = JsonConvert.DeserializeObject(result.content);
                        if ((bool)UpdateData["up_to_date"])
                        {
                            Debug.WriteLine("^2[vMenu] Your version of vMenu is up to date! Good job!^0\n");
                        }
                        else
                        {
                            Debug.WriteLine("^3[vMenu] WARNING:^0 Your version of vMenu is OUTDATED!");
                            Debug.WriteLine("^3[vMenu] WARNING:^0 Your version: " + MainServer.Version);
                            Debug.WriteLine("^3[vMenu] WARNING:^0 Latest version: " + UpdateData["latest_version"]);
                            Debug.WriteLine("^3[vMenu] WARNING:^0 Release date: " + UpdateData["release_date"]);
                            Debug.WriteLine("^3[vMenu] WARNING:^0 Changelog summary: " + UpdateData["update_message"]);
                            Debug.WriteLine("^3[vMenu] WARNING:^0 Please update as soon as possible!");
                            Debug.WriteLine("^3[vMenu] WARNING:^0 Download: https://github.com/tomgrobbe/vMenu/releases/");
                            Debug.WriteLine("\n");
                            MainServer.UpToDate = false;
                        }
                        break;
                    default:
                        Debug.WriteLine("^3[vMenu] [WARNING] ^0An error occurred while checking for the latest version. This is not a problem, vMenu will still work correctly. Please check your version manually.");
                        Debug.WriteLine($"^3[vMenu] [WARNING] ^0Error details: {result.content}.");
                        break;
                }
            }
            // Aw damn! An exception. :(
            catch (Exception e)
            {
                Debug.WriteLine("^3[vMenu] [WARNING] ^0An error occurred while checking for the latest version. This is not a problem, vMenu will still work correctly. Please check your version manually.");
                Debug.Write($"^3[vMenu] ^0Error info: {e.Message.ToString()}\r\n\r\n");
            }
            CheckedForUpdates = true;
        }
    }
}
