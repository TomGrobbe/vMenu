using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class LanguageManager : BaseScript
    {
        public string Get (string originalText) {
            
            string transText = originalText;

            if (!MainMenu.DumpedData.ContainsKey(originalText))
            {
                MainMenu.DumpedData.Add(originalText, "");
            }

            if (MainMenu.CurrentLanguage.ContainsKey(originalText)) {
                try {
                    transText = (string)MainMenu.CurrentLanguage[originalText];
                    if (transText == "")
                    {
                        transText = originalText;
                    }
                } catch (Exception ex)
                {
                    // Not need this because it will generate lot of logs and fill your disk xD
                    // Debug.WriteLine($"Cannot found translate: {originalText}");
                }
            }

            return transText;
        }
    }
}