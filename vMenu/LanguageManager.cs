using CitizenFX.Core;
using MenuAPI;
using System.Collections.Generic;
using System.Linq;

/*
 * TODO:
 * • Notifications
*/

namespace vMenuClient
{
    class LanguageManager : BaseScript
    {
        #region Properties

        /// <summary>
        /// The list of languages from the server.
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> Languages { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// The original language of the menus.
        /// </summary>
        private static Dictionary<string, string> OriginalLanguage { get; set; } = new Dictionary<string, string>();

        #endregion

        #region Fields

        /// <summary>
        /// The list of menus extracted by the GetMenu() method.
        /// </summary>
        private static readonly List<Menu> ExtractedListOfMenus = new List<Menu>();

        /// <summary>
        /// To indicate whether the original language has been stored yet.
        /// </summary>
        private static bool IsOriginalLanguageStored = false;

        #endregion

        #region Constructor

        public LanguageManager()
        {
            TranslateMenus(true);
        }

        #endregion

        #region Events

        #region Dump a language template

        [EventHandler("vMenu:DumpLanguageTamplate:Client")]
        void DumpLanguageTemplate()
        {
            // Just to make sure
            OriginalLanguage.Remove(Game.Player.Name);
            OriginalLanguage.Remove("vMenu");

            // Send to the server
            TriggerServerEvent("vMenu:DumpLanguageTemplate:Server", Newtonsoft.Json.JsonConvert.SerializeObject(OriginalLanguage, Newtonsoft.Json.Formatting.Indented));
        }

        #endregion

        #endregion

        #region Tools

        #region Translate menus

        /// <summary>
        /// Translate all of the menus' text and descriptions.
        /// </summary>
        public async static void TranslateMenus(bool slowDownLoop = false)
        {
            // Wait for it to be completed
            while (!IsOriginalLanguageStored)
            {
                await Delay(500);
            }

            for (int i = 0; i < ExtractedListOfMenus.Count; i++)
            {
                Menu m = ExtractedListOfMenus[i];
                List<MenuItem> list = m.GetMenuItems();

                for (int i1 = 0; i1 < list.Count; i1++)
                {
                    MenuItem item = list[i1];

                    if (!string.IsNullOrEmpty(item.ParentMenu.MenuTitle))
                        item.ParentMenu.MenuTitle = TranslateHelper(item.ParentMenu.MenuTitle);

                    if (!string.IsNullOrEmpty(item.ParentMenu.MenuSubtitle))
                        item.ParentMenu.MenuSubtitle = TranslateHelper(item.ParentMenu.MenuSubtitle);

                    if (!string.IsNullOrEmpty(item.Text))
                        item.Text = TranslateHelper(item.Text);

                    if (!string.IsNullOrEmpty(item.Description))
                        item.Description = TranslateHelper(item.Description);

                    // Solves resource time warning spiking so high when building the menu
                    if (slowDownLoop)
                    {
                        await Delay(10);
                    }
                }
            }
        }

        #endregion

        #region Translate helper

        private static string TranslateHelper(string text)
        {
            // Reset the text to the original language.
            text = OriginalLanguage.ContainsValue(text) ? OriginalLanguage.First(x => x.Value == text).Key : text;

            // Check if the languages dictionary contains the key matching the text.
            if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(text))
            {
                // Update the original language value with the new language translation.
                OriginalLanguage[text] = Languages[MiscSettings.CurrentLanguage][text];

                // Return the new translation.
                return Languages[MiscSettings.CurrentLanguage][text];
            }
            else
            {
                // These are the duplicate words that lost their way in the dictionary, we can recover them like this.
                foreach (var lang in Languages)
                {
                    if (lang.Value.ContainsValue(text))
                    {
                        // Default the text.
                        text = lang.Value.First(x => x.Value.Equals(text)).Key;

                        // Update it.
                        if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(text))
                        {
                            // Return the new translation.
                            return Languages[MiscSettings.CurrentLanguage][text];
                        }
                    }
                }
            }

            return text;
        }

        #endregion

        #region Update original langauge

        public static void UpdateOriginalLanguage()
        {
            for (int i = 0; i < ExtractedListOfMenus.Count; i++)
            {
                Menu m = ExtractedListOfMenus[i];
                List<MenuItem> list = m.GetMenuItems();

                for (int i1 = 0; i1 < list.Count; i1++)
                {
                    MenuItem item = list[i1];

                    if (!OriginalLanguage.ContainsKey(item.ParentMenu.MenuTitle))
                    {
                        if (!item.ParentMenu.MenuTitle.Equals(Game.Player.Name) && !item.ParentMenu.MenuTitle.Equals("vMenu"))
                        {
                            OriginalLanguage.Add(item.ParentMenu.MenuTitle, item.ParentMenu.MenuTitle);
                        }
                    }
                    if (!string.IsNullOrEmpty(item.ParentMenu.MenuSubtitle) && !OriginalLanguage.ContainsKey(item.ParentMenu.MenuSubtitle))
                    {
                        if (!item.ParentMenu.MenuSubtitle.Equals(Game.Player.Name))
                        {
                            OriginalLanguage.Add(item.ParentMenu.MenuSubtitle, item.ParentMenu.MenuSubtitle);
                        }
                    }
                    if (!OriginalLanguage.ContainsKey(item.Text))
                    {
                        OriginalLanguage.Add(item.Text, item.Text);
                    }
                    if (!string.IsNullOrEmpty(item.Description) && !OriginalLanguage.ContainsKey(item.Description))
                    {
                        OriginalLanguage.Add(item.Description, item.Description);
                    }
                }
            }

            IsOriginalLanguageStored = true;
        }

        #endregion

        #region Get menu

        /// <summary>
        /// Get the menu and store it to a list to be used to translate.
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public Menu GetMenu(Menu menu)
        {
            ExtractedListOfMenus.Add(menu);
            return menu;
        }

        #endregion

        #endregion
    }
}
