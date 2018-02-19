using GHMatti.Http;
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
        private bool firstTick = true;
        public UpdateChecker()
        {
            Tick += CheckUpdates;
        }
        private async Task CheckUpdates()
        {
            if (firstTick)
            {
                firstTick = false;
                Request r = new Request();
                try
                {
                    RequestResponse result = await r.Http("https://vespura.com/vMenu-version.json");
                    if (result.status == System.Net.HttpStatusCode.OK)
                    {
                        var currentVersion = GetResourceMetadata(GetCurrentResourceName(), "version", 0);
                        dynamic output = JsonConvert.DeserializeObject<dynamic>(result.content);
                        string version = output.version.ToString();
                        string date = output.date.ToString();
                        Debug.WriteLine("\r\n  +----------------- [ vMenu ] -----------------+");
                        Debug.WriteLine("  |       Current version: \t" + currentVersion + "        |");
                        Debug.WriteLine("  |       Latest version: \t" + version + "        |");
                        Debug.WriteLine("  |       Release date: \t" + date + "      |");
                        Debug.WriteLine("  |                                             |");
                        if (currentVersion == version)
                        {
                            Debug.WriteLine("  |       You are using the latest version!     |");
                        }
                        else
                        {
                            Debug.WriteLine("  | A new version is available, please update!  |");
                            Debug.WriteLine("  |  >> https://github.com/tomgrobbe/vMenu <<   |");
                        }

                        Debug.WriteLine("  +---------------------------------------------+\r\n");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }
    }
}
