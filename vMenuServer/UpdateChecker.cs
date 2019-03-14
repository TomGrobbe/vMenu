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

                string path = GetResourcePath(GetCurrentResourceName());
                int count = path.Substring(path.LastIndexOf("resources")).ToCharArray().Count(c => c == '/');
                string newPath = "";
                for (var i = 0; i < count; i++) newPath += "../";
                string file = LoadResourceFile(GetCurrentResourceName(), newPath + "server.cfg");
                bool zap = !string.IsNullOrEmpty(file) && file.ToLower().Contains("hosted by zap-hosting.com");
                if (existingUuid != null && existingUuid != "")
                {
                    UUID = existingUuid;
                }
                else
                {
                    Guid uuid = Guid.NewGuid();
                    UUID = uuid.ToString();
                }

                //if (!UUID.Contains("_zap_server") && zap) UUID += "_zap_server";

                SaveResourceFile(GetCurrentResourceName(), "uuid", UUID, -1);

                // sets the UUID convar.
                if (!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_disable_server_info_convars))
                {
                    SetConvarServerInfo("vMenuUUID", UUID.Substring(0, UUID.IndexOf('-')));
                    SetConvarServerInfo("vMenuVersion", MainServer.Version);
                }

                // Get a response from the specified url.
                RequestResponse result = await r.Http($"https://www.vespura.com/vmenu/version?id={UUID}&version={MainServer.Version}{(zap ? "&zap=true" : "")}");


                Debug.WriteLine("\r\n^5[vMenu] Checking for updates.^7");

                // If the result status = 200 (status code OK) then continue.
                switch (result.status)
                {
                    case System.Net.HttpStatusCode.OK:
                        dynamic UpdateData = JsonConvert.DeserializeObject(result.content);
                        if (!string.IsNullOrEmpty(UpdateData["message"].ToString() ?? ""))
                        {
                            Debug.WriteLine($"^5[vMenu] {UpdateData["message"]}^0");
                        }
                        if ((bool)UpdateData["up_to_date"])
                        {
                            Debug.WriteLine("^2[vMenu] Your version of vMenu is up to date! Good job!^7\n");
                        }
                        else
                        {
                            Debug.WriteLine("^3[vMenu] WARNING:^7 Your version of vMenu does ^1NOT ^7match the latest version!");
                            Debug.WriteLine($"^3[vMenu] WARNING:^7 Your version: {MainServer.Version}^7");
                            Debug.WriteLine($"^3[vMenu] WARNING:^7 Latest version: {UpdateData["latest_version"]}^7");
                            Debug.WriteLine($"^3[vMenu] WARNING:^7 Release date: {UpdateData["release_date"]}^7");
                            Debug.WriteLine($"^3[vMenu] WARNING:^7 Changelog summary: {UpdateData["update_message"]}^7");
                            Debug.WriteLine("^3[vMenu] WARNING:^7 Please update as soon as possible!");
                            Debug.WriteLine("^3[vMenu] WARNING:^7 Download: https://github.com/tomgrobbe/vMenu/releases/");
                            Debug.WriteLine("\n");
                            MainServer.UpToDate = false;
                            MainServer.UpdaterVersion = UpdateData["latest_version"].ToString();
                            MainServer.UpdateMessage = UpdateData["update_message"].ToString();
                        }
                        break;
                    default:
                        Debug.WriteLine("^3[vMenu] [WARNING] ^7An error occurred while checking for the latest version. This is not a problem, vMenu will still work correctly. Please check your version manually.");
                        Debug.WriteLine($"^3[vMenu] [WARNING] ^7Error details: {result.content}.");
                        break;
                }
            }
            // Aw damn! An exception. :(
            catch (Exception e)
            {
                Debug.WriteLine("^3[vMenu] [WARNING] ^7An error occurred while checking for the latest version. This is not a problem, vMenu will still work correctly. Please check your version manually.");
                Debug.Write($"^3[vMenu] ^7Error info: {e.Message.ToString()}\r\n\r\n");
            }

            CheckedForUpdates = true;

            DateTime currentDateTime = DateTime.Now;
            if (!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_disable_daily_update_checks))
            {
                while (true)
                {
                    // once every 10 minutes.
                    //            ms  sec  min
                    await Delay(1000 * 60 * 10);

                    if (DateTime.Now.Subtract(currentDateTime).TotalHours >= 24)
                    {
                        CheckUpdates();
                        return;
                    }
                }

            }
        }
    }
}
