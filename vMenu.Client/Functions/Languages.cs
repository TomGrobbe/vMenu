// System Libraries //
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

// CitizenFX Libraries //
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

// vMenu Namespaces //
using vMenu.Client.Menus;
using ScaleformUI.Menu;
using vMenu.Client.Functions;
using static vMenu.Client.KeyMappings.Commands;
using vMenu.Shared.Objects;

namespace vMenu.Client.Functions
{
    public class Languages
    {
        private static readonly object _padlock = new();
        private static Languages _instance;

        public static Dictionary<string, Menu> Menus;

        private Languages()
        {
            string languageAbbrv = GetResourceMetadata(GetCurrentResourceName(), "language", 0);
            string JsonData = LoadResourceFile(GetCurrentResourceName(), $"languages/{languageAbbrv}.json") ?? LoadResourceFile(GetCurrentResourceName(), $"languages/en.json");
            LanguagesFile LanguagesJson = JsonConvert.DeserializeObject<LanguagesFile>(JsonData);

            // Menu Info Format //
            Menus = LanguagesJson.Menus;

            // LanguagesJson.Menus["MainMenu"].Subtitle;
            // LanguagesJson.Menus["MainMenu"].Items["OnlinePlayersItem"].Name;
            // LanguagesJson.Menus["MainMenu"].Items["OnlinePlayersItem"].Description;

            // Notifications Format //
            // Needs to be worked on //

            Debug.WriteLine("Languages Initialized");
        }

        internal static Languages Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new Languages();
                }
            }
        }
    }
}
