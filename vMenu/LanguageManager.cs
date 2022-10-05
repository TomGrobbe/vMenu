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

        //private bool 

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
            TriggerServerEvent("vMenu:DumpLanguageTemplate:Server", Newtonsoft.Json.JsonConvert.SerializeObject(OriginalLanguage));
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

                    #region Translate item parent menu title

                    // Reset the item parent menu title to the original language
                    item.ParentMenu.MenuTitle = OriginalLanguage.ContainsValue(item.ParentMenu.MenuTitle) ? OriginalLanguage.First(x => x.Value == item.ParentMenu.MenuTitle).Key : item.ParentMenu.MenuTitle;

                    // Check if the languages dictionary contains the key matching the item parent menu title
                    if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(item.ParentMenu.MenuTitle))
                    {
                        // Update the original language value with the new language translation
                        OriginalLanguage[item.ParentMenu.MenuTitle] = Languages[MiscSettings.CurrentLanguage][item.ParentMenu.MenuTitle];

                        // Set the new language
                        item.ParentMenu.MenuTitle = Languages[MiscSettings.CurrentLanguage][item.ParentMenu.MenuTitle];
                    }
                    else
                    {
                        // These are the duplicate words that lost their way in the dictionary, we can recover them like this
                        foreach (var lang in Languages)
                        {
                            if (lang.Value.ContainsValue(item.ParentMenu.MenuTitle))
                            {
                                // Default the item parent menu title
                                item.ParentMenu.MenuTitle = lang.Value.First(x => x.Value.Equals(item.ParentMenu.MenuTitle)).Key;

                                // Update it
                                if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(item.ParentMenu.MenuTitle))
                                {
                                    // Set the new language
                                    item.ParentMenu.MenuTitle = Languages[MiscSettings.CurrentLanguage][item.ParentMenu.MenuTitle];
                                }
                            }
                        }
                    }

                    #endregion

                    #region Translate item parent menu subtitle

                    // item parent menu subtitle isn't always available so we check for it being null or empty
                    if (!string.IsNullOrEmpty(item.ParentMenu.MenuSubtitle))
                    {
                        // Reset the item parent menu subtitle to the original language
                        item.ParentMenu.MenuSubtitle = OriginalLanguage.ContainsValue(item.ParentMenu.MenuSubtitle) ? OriginalLanguage.First(x => x.Value == item.ParentMenu.MenuSubtitle).Key : item.ParentMenu.MenuSubtitle;

                        // Check if the languages dictionary contains the key matching the item parent menu subtitle
                        if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(item.ParentMenu.MenuSubtitle))
                        {
                            // Update the original language value with the new language translation
                            OriginalLanguage[item.ParentMenu.MenuSubtitle] = Languages[MiscSettings.CurrentLanguage][item.ParentMenu.MenuSubtitle];

                            // Set the new language
                            item.ParentMenu.MenuSubtitle = Languages[MiscSettings.CurrentLanguage][item.ParentMenu.MenuSubtitle];
                        }
                        else
                        {
                            // These are the duplicate words that lost their way in the dictionary, we can recover them like this
                            foreach (var lang in Languages)
                            {
                                if (lang.Value.ContainsValue(item.ParentMenu.MenuSubtitle))
                                {
                                    // Default the item parent menu subtitle
                                    item.ParentMenu.MenuSubtitle = lang.Value.First(x => x.Value.Equals(item.ParentMenu.MenuSubtitle)).Key;

                                    // Update it
                                    if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(item.ParentMenu.MenuSubtitle))
                                    {
                                        // Set the new language
                                        item.ParentMenu.MenuSubtitle = Languages[MiscSettings.CurrentLanguage][item.ParentMenu.MenuSubtitle];
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Translate item text

                    // Reset the item text to the original language
                    item.Text = OriginalLanguage.ContainsValue(item.Text) ? OriginalLanguage.First(x => x.Value == item.Text).Key : item.Text;

                    // Check if the languages dictionary contains the key matching the item text
                    if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(item.Text))
                    {
                        // Update the original language value with the new language translation
                        OriginalLanguage[item.Text] = Languages[MiscSettings.CurrentLanguage][item.Text];

                        // Set the new language
                        item.Text = Languages[MiscSettings.CurrentLanguage][item.Text];
                    }
                    else
                    {
                        // These are the duplicate words that lost their way in the dictionary, we can recover them like this
                        foreach (var lang in Languages)
                        {
                            if (lang.Value.ContainsValue(item.Text))
                            {
                                // Default the text
                                item.Text = lang.Value.First(x => x.Value.Equals(item.Text)).Key;

                                // Update it
                                if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(item.Text))
                                {
                                    // Set the new language
                                    item.Text = Languages[MiscSettings.CurrentLanguage][item.Text];
                                }
                            }
                        }
                    }

                    #endregion

                    #region Translate item description

                    // Description isn't always available so we check for it being null or empty
                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        // Reset the item description to the original language
                        item.Description = OriginalLanguage.ContainsValue(item.Description) ? OriginalLanguage.First(x => x.Value == item.Description).Key : item.Description;

                        // Check if the languages dictionary contains the key matching the item description
                        if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(item.Description))
                        {
                            // Update the original language value with the new language translation
                            OriginalLanguage[item.Description] = Languages[MiscSettings.CurrentLanguage][item.Description];

                            // Set the new language
                            item.Description = Languages[MiscSettings.CurrentLanguage][item.Description];
                        }
                        else
                        {
                            // These are the duplicate words that lost their way in the dictionary, we can recover them like this
                            foreach (var lang in Languages)
                            {
                                if (lang.Value.ContainsValue(item.Description))
                                {
                                    // Default the description
                                    item.Description = lang.Value.First(x => x.Value.Equals(item.Description)).Key;

                                    // Update it
                                    if (Languages.ContainsKey(MiscSettings.CurrentLanguage) && Languages[MiscSettings.CurrentLanguage].ContainsKey(item.Description))
                                    {
                                        // Set the new language
                                        item.Description = Languages[MiscSettings.CurrentLanguage][item.Description];
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    // Solves resource time warning spiking so high when building the menu
                    if (slowDownLoop)
                    {
                        await Delay(10);
                    }
                }
            }
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
