using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using MenuAPI;
using Newtonsoft.Json;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;
using static vMenuShared.ConfigManager;

namespace vMenuClient
{
    /// <summary>
    /// Handles all dynamic menu exports for plugin usage
    /// </summary>
    public partial class MainMenu
    {
        /// <summary>
        /// Callback delegate for export functions
        /// </summary>
        /// <param name="args">Optional arguments passed to the callback</param>
        private delegate void CallbackDelegate(params object[] args);

        /// <summary>
        /// Dictionary to store dynamically created menus
        /// </summary>
        private static Dictionary<string, Menu> DynamicMenus = new Dictionary<string, Menu>();

        /// <summary>
        /// Checks if an ID conflicts with any existing menu or item
        /// </summary>
        /// <param name="id">ID to check</param>
        /// <param name="context">Context for error messages</param>
        /// <returns>True if there's a conflict</returns>
        private bool HasIdConflict(string id, string context)
        {
            // Check against dynamic menus
            if (DynamicMenus.ContainsKey(id))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] {context}: ID '{id}' conflicts with existing dynamic menu.");
                return true;
            }

            // Check against core built-in menus only (strict matching)
            if (IsBuiltInMenuId(id))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] {context}: ID '{id}' conflicts with built-in vMenu menu.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if an ID matches a core built-in menu (strict matching only)
        /// </summary>
        /// <param name="id">ID to check</param>
        /// <returns>True if it matches a built-in menu ID</returns>
        private bool IsBuiltInMenuId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var normalizedId = id.ToLower().Replace(" ", "-");

            // Get all built-in menu IDs based on their subtitles (same logic as GetAllMenuIds)
            var uniqueMenus = MenuController.Menus.GroupBy(m => m.MenuSubtitle ?? m.MenuTitle).Select(g => g.First()).ToList();
            foreach (var menu in uniqueMenus)
            {
                // Skip dynamic menus
                if (DynamicMenus.ContainsValue(menu)) continue;

                // Check if menu identifier matches
                var menuIdentifier = menu.MenuSubtitle ?? menu.MenuTitle ?? "";
                var normalizedTitle = menuIdentifier.ToLower().Replace(" ", "-");

                if (normalizedTitle == normalizedId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if an item ID conflicts within a specific menu
        /// </summary>
        /// <param name="menu">Menu to check</param>
        /// <param name="itemId">Item ID to check</param>
        /// <param name="context">Context for error messages</param>
        /// <returns>True if there's a conflict</returns>
        private bool HasItemConflict(Menu menu, string itemId, string context)
        {
            if (menu.GetMenuItems().Any(item => item.ItemData as string == itemId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] {context}: Item with ID '{itemId}' already exists in this menu.");
                return true;
            }
            return false;
        }

        #region Export Registration and Initialization
        /// <summary>
        /// Initialize dynamic menu exports for plugin usage
        /// </summary>
        public void InitializeDynamicMenuExports()
        {
            RegisterDynamicMenuExports();
        }

        /// <summary>
        /// Registers all dynamic menu exports
        /// </summary>
        private void RegisterDynamicMenuExports()
        {
            // Core Menu Management
            Exports.Add("CreateMenu", new Action<string, string, string, object>(CreateMenu));
            Exports.Add("OpenMenu", new Action<string>(OpenMenu));
            Exports.Add("CloseMenu", new Action<string>(CloseMenu));
            Exports.Add("CloseAllMenus", new Action(CloseAllMenus));
            Exports.Add("CheckMenu", new Func<string, bool>(CheckMenu));
            Exports.Add("ClearMenu", new Action<string>(ClearMenu));
            Exports.Add("RefreshMenu", new Action<string>(RefreshMenu));
            Exports.Add("DeleteMenu", new Action<string>(DeleteMenu));

            // Add Menu Items
            Exports.Add("AddButton", new Action<string, string, string, string, object, object>(AddButton));
            Exports.Add("AddList", new Action<string, string, string, object, int, string, object>(AddList));
            Exports.Add("AddCheckbox", new Action<string, string, string, string, bool, object>(AddCheckbox));
            Exports.Add("AddSlider", new Action<string, string, string, string, int, object>(AddSlider));
            Exports.Add("AddSpacer", new Action<string, string, string, string>(AddSpacer));
            Exports.Add("AddSubmenuButton", new Action<string, string, string, string, string, object>(AddSubmenuButton));

            // Modify Menu Items
            Exports.Add("RemoveItem", new Action<string, object>(RemoveItem));

            // Notifications
            Exports.Add("Notify", new Action<string, string>(notifExp));

            // Menu Information
            Exports.Add("GetAllMenuIds", new Func<string[]>(GetAllMenuIds));
        }
        #endregion

        #region Export Implementation Methods

        /// <summary>
        /// Creates a new dynamic menu
        /// </summary>
        /// <param name="menuId">Unique identifier for the menu</param>
        /// <param name="menuTitle">Display title for the menu</param>
        /// <param name="menuDescription">Menu description/subtitle</param>
        /// <param name="callbackObj">Optional callback for menu open events</param>
        private void CreateMenu(string menuId, string menuTitle, string menuDescription, object callbackObj)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] CreateMenu: menuId cannot be null or empty.");
                return;
            }

            // Check for any ID conflicts
            if (HasIdConflict(menuId, "CreateMenu"))
            {
                return;
            }

            // Apply defaults for optional parameters
            if (string.IsNullOrEmpty(menuTitle))
                menuTitle = "Menu";
            if (string.IsNullOrEmpty(menuDescription))
                menuDescription = "";

            // Cache player name at creation time to prevent dynamic changes
            var playerName = Game.Player.Name;
            var newMenu = new Menu(playerName, menuTitle)
            {
                MenuSubtitle = menuDescription
            };
            MenuController.AddMenu(newMenu);
            DynamicMenus[menuId] = newMenu;

            if (callbackObj != null)
            {
                // Handle both C# delegates and Lua functions
                if (callbackObj is CallbackDelegate callback)
                {
                    newMenu.OnMenuOpen += (sender) =>
                    {
                        try
                        {
                            BaseScript.TriggerEvent("vMenu:DelayedCallback", new Action(() => callback.Invoke()));
                        }
                        catch (System.IO.EndOfStreamException)
                        {
                            // Silently ignore - callback was deleted/garbage collected
                        }
                        catch (Exception ex)
                        {
                            CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking menu open callback for {menuId}: {ex.Message}");
                            if (ex.InnerException != null)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Inner exception: {ex.InnerException.Message}");
                            }
                        }
                    };
                }
                else
                {
                    // Handle Lua functions and dynamic callbacks
                    newMenu.OnMenuOpen += (sender) =>
                    {
                        try
                        {
                            // Validate callback before invoking
                            if (callbackObj == null)
                            {
                                return;
                            }

                            dynamic dynamicCallback = callbackObj;
                            dynamicCallback();
                        }
                        catch (System.IO.EndOfStreamException)
                        {
                            // Silently ignore - callback was deleted/garbage collected
                        }
                        catch (Exception ex)
                        {
                            CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking menu open callback for {menuId}: {ex.GetType().Name} - {ex.Message}");
                            if (ex.InnerException != null)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Inner exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                            }
                            CitizenFX.Core.Debug.WriteLine($"[vMenu] Stack trace: {ex.StackTrace}");
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Displays notification to the player
        /// </summary>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type (error, info, success, or default)</param>
        private void notifExp(string message, string type)
        {
            if (type == "error")
            {
                Notify.Error(message);
            }
            else if (type == "info")
            {
                Notify.Info(message);
            }
            else if (type == "success")
            {
                Notify.Success(message);
            }
            else
            {
                Notify.Alert(message);
            }
        }

        /// <summary>
        /// Opens a menu by ID
        /// </summary>
        /// <param name="menuId">Menu identifier</param>
        private void OpenMenu(string menuId)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] OpenMenu: menuId cannot be null or empty.");
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] OpenMenu: Menu with ID '{menuId}' not found. Available menus: {string.Join(", ", GetAllMenuIds())}");
                return;
            }

            // Close all currently open menus before opening the new one
            MenuController.CloseAllMenus();

            // Open the requested menu
            menu.OpenMenu();
        }

        /// <summary>
        /// Adds a button item to a menu
        /// </summary>
        /// <param name="menuId">Target menu ID</param>
        /// <param name="buttonId">Unique button identifier</param>
        /// <param name="buttonLabel">Button label</param>
        /// <param name="buttonDescription">Button description</param>
        /// <param name="param5">Either rightLabel (string) or callback (object)</param>
        /// <param name="param6">Optional callback if param5 is a string</param>
        private void AddButton(string menuId, string buttonId, string buttonLabel, string buttonDescription, object param5 = null, object param6 = null)
        {
            // Handle backwards compatibility: param5 can be either rightLabel (string) or callback (object)
            string rightLabel = null;
            object callbackObj = null;

            if (param5 is string label)
            {
                // New signature: AddButton(menuId, buttonId, label, desc, rightLabel, callback)
                rightLabel = label;
                callbackObj = param6;
            }
            else
            {
                // Old signature: AddButton(menuId, buttonId, label, desc, callback)
                callbackObj = param5;
            }
            // Validate required parameters
            if (string.IsNullOrEmpty(buttonId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddButton: buttonId cannot be null or empty.");
                return;
            }

            // Check for ID conflicts (menu names, other items)
            if (HasIdConflict(buttonId, "AddButton"))
            {
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Menu with ID {menuId} not found.");
                return;
            }

            // Check for item conflicts within this menu
            if (HasItemConflict(menu, buttonId, "AddButton"))
            {
                return;
            }

            // Apply defaults for optional parameters
            if (string.IsNullOrEmpty(buttonLabel))
                buttonLabel = "Button";
            if (string.IsNullOrEmpty(buttonDescription))
                buttonDescription = "";

            var menuItem = new MenuItem(buttonLabel, buttonDescription)
            {
                ItemData = buttonId,
                Label = rightLabel ?? ""
            };

            if (callbackObj != null)
            {
                // Handle both C# delegates and Lua functions
                if (callbackObj is CallbackDelegate callback)
                {
                    menu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item == menuItem)
                        {
                            await BaseScript.Delay(0);
                            callback.Invoke();
                        }
                    };
                }
                else if (callbackObj is Func<object> luaCallback)
                {
                    menu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item == menuItem)
                        {
                            await BaseScript.Delay(0);
                            try
                            {
                                luaCallback.Invoke();
                            }
                            catch (System.IO.EndOfStreamException)
                            {
                                // Silently ignore - callback was deleted/garbage collected
                            }
                            catch (Exception ex)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking callback for button {buttonId}: {ex.Message}");
                            }
                        }
                    };
                }
                else
                {
                    // Try to invoke as dynamic for other callback types
                    menu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item == menuItem)
                        {
                            await BaseScript.Delay(0);
                            try
                            {
                                dynamic dynamicCallback = callbackObj;
                                dynamicCallback();
                            }
                            catch (System.IO.EndOfStreamException)
                            {
                                // Silently ignore - callback was deleted/garbage collected
                            }
                            catch (Exception ex)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking dynamic callback for button {buttonId}: {ex.Message}");
                            }
                        }
                    };
                }
            }

            menu.AddMenuItem(menuItem);
            menu.RefreshIndex();
        }

        /// <summary>
        /// Adds a list item to a menu
        /// </summary>
        /// <param name="menuId">Target menu ID</param>
        /// <param name="listId">Unique list identifier</param>
        /// <param name="listLabel">List display text</param>
        /// <param name="options">List options as object array</param>
        /// <param name="defaultIndex">Default selected index</param>
        /// <param name="description">List description</param>
        /// <param name="callbackObj">Optional callback for list changes</param>
        private void AddList(string menuId, string listId, string listLabel, object options, int defaultIndex, string description, object callbackObj = null)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(listId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddList: listId cannot be null or empty.");
                return;
            }

            // Check for ID conflicts (menu names, other items)
            if (HasIdConflict(listId, "AddList"))
            {
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Menu with ID {menuId} not found.");
                return;
            }

            // Check for item conflicts within this menu
            if (HasItemConflict(menu, listId, "AddList"))
            {
                return;
            }

            // Apply defaults for optional parameters
            if (string.IsNullOrEmpty(listLabel))
                listLabel = "List";
            if (string.IsNullOrEmpty(description))
                description = "";

            List<string> optionsList;

            // Handle different option input types
            if (options is string jsonString)
            {
                // Legacy support for JSON string
                optionsList = JsonConvert.DeserializeObject<List<string>>(jsonString);
            }
            else if (options is object[] objectArray)
            {
                // Convert object array to string list
                optionsList = objectArray.Select(o => o?.ToString() ?? "").ToList();
            }
            else if (options is List<object> objectList)
            {
                // Convert object list to string list
                optionsList = objectList.Select(o => o?.ToString() ?? "").ToList();
            }
            else if (options == null)
            {
                // Provide default empty list if options is null
                optionsList = new List<string> { "Option 1", "Option 2" };
                CitizenFX.Core.Debug.WriteLine($"[vMenu] No options provided for list {listId}, using default options.");
            }
            else
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Invalid options type for list {listId}. Expected array or JSON string.");
                return;
            }

            // Validate defaultIndex
            if (defaultIndex < 0 || defaultIndex >= optionsList.Count)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Invalid defaultIndex {defaultIndex} for list {listId}. Using 0 instead.");
                defaultIndex = 0;
            }

            var menuListItem = new MenuListItem(listLabel, optionsList, defaultIndex, description)
            {
                ItemData = listId
            };

            if (callbackObj != null)
            {
                // Handle both C# delegates and Lua functions
                if (callbackObj is CallbackDelegate callback)
                {
                    // Listen for index changes (browsing through list)
                    menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
                    {
                        if (item == menuListItem)
                        {
                            var currentValue = optionsList[newIndex];
                            var oldValue = optionsList[oldIndex];
                            callback.Invoke(false, currentValue, newIndex, oldIndex);
                        }
                    };

                    // Listen for item selection (pressing enter/select)
                    menu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
                    {
                        if (item == menuListItem)
                        {
                            var selectedValue = optionsList[listIndex];
                            callback.Invoke(true, selectedValue, listIndex, listIndex);
                        }
                    };
                }
                else
                {
                    // Handle Lua functions and dynamic callbacks
                    menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
                    {
                        if (item == menuListItem)
                        {
                            try
                            {
                                var currentValue = optionsList[newIndex];
                                var oldValue = optionsList[oldIndex];
                                dynamic dynamicCallback = callbackObj;
                                dynamicCallback(false, currentValue, newIndex, oldIndex);
                            }
                            catch (System.IO.EndOfStreamException)
                            {
                                // Silently ignore - callback was deleted/garbage collected
                            }
                            catch (Exception ex)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking callback for list {listId}: {ex.Message}");
                            }
                        }
                    };

                    menu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
                    {
                        if (item == menuListItem)
                        {
                            try
                            {
                                var selectedValue = optionsList[listIndex];
                                dynamic dynamicCallback = callbackObj;
                                dynamicCallback(true, selectedValue, listIndex, listIndex);
                            }
                            catch (System.IO.EndOfStreamException)
                            {
                                // Silently ignore - callback was deleted/garbage collected
                            }
                            catch (Exception ex)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking callback for list {listId}: {ex.Message}");
                            }
                        }
                    };
                }
            }

            menu.AddMenuItem(menuListItem);
            menu.RefreshIndex();
        }

        /// <summary>
        /// Adds a checkbox to a menu
        /// </summary>
        /// <param name="menuId">Target menu ID</param>
        /// <param name="checkboxId">Unique checkbox identifier</param>
        /// <param name="checkboxLabel">Checkbox display text</param>
        /// <param name="description">Checkbox description</param>
        /// <param name="defaultValue">Default checked state</param>
        /// <param name="callbackObj">Optional callback for checkbox changes</param>
        private void AddCheckbox(string menuId, string checkboxId, string checkboxLabel, string description, bool defaultValue, object callbackObj = null)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddCheckbox: menuId cannot be null or empty.");
                return;
            }
            if (string.IsNullOrEmpty(checkboxId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddCheckbox: checkboxId cannot be null or empty.");
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Menu with ID {menuId} not found.");
                return;
            }

            // Check for ID conflicts (menu names, other items)
            if (HasIdConflict(checkboxId, "AddCheckbox"))
            {
                return;
            }

            // Check for item conflicts within this menu
            if (HasItemConflict(menu, checkboxId, "AddCheckbox"))
            {
                return;
            }

            // Apply defaults for optional parameters
            if (string.IsNullOrEmpty(checkboxLabel))
                checkboxLabel = "Checkbox";
            if (string.IsNullOrEmpty(description))
                description = "";

            var menuCheckboxItem = new MenuCheckboxItem(checkboxLabel, description, defaultValue)
            {
                ItemData = checkboxId
            };

            if (callbackObj != null)
            {
                // Handle both C# delegates and Lua functions
                if (callbackObj is CallbackDelegate callback)
                {
                    menu.OnCheckboxChange += (sender, item, index, _checked) =>
                    {
                        if (item == menuCheckboxItem)
                        {
                            callback.Invoke(_checked);
                        }
                    };
                }
                else if (callbackObj is Func<object> luaCallback)
                {
                    menu.OnCheckboxChange += (sender, item, index, _checked) =>
                    {
                        if (item == menuCheckboxItem)
                        {
                            try
                            {
                                dynamic dynamicCallback = luaCallback;
                                dynamicCallback(_checked);
                            }
                            catch (System.IO.EndOfStreamException)
                            {
                                // Silently ignore - callback was deleted/garbage collected
                            }
                            catch (Exception ex)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking callback for checkbox {checkboxId}: {ex.Message}");
                            }
                        }
                    };
                }
                else
                {
                    // Try to invoke as dynamic for other callback types
                    menu.OnCheckboxChange += (sender, item, index, _checked) =>
                    {
                        if (item == menuCheckboxItem)
                        {
                            try
                            {
                                dynamic dynamicCallback = callbackObj;
                                dynamicCallback(_checked);
                            }
                            catch (System.IO.EndOfStreamException)
                            {
                                // Silently ignore - callback was deleted/garbage collected
                            }
                            catch (Exception ex)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking dynamic callback for checkbox {checkboxId}: {ex.Message}");
                            }
                        }
                    };
                }
            }

            menu.AddMenuItem(menuCheckboxItem);
            menu.RefreshIndex();
        }

        /// <summary>
        /// Adds a slider to a menu (0-100% range)
        /// </summary>
        /// <param name="menuId">Target menu ID</param>
        /// <param name="sliderId">Unique slider identifier</param>
        /// <param name="sliderLabel">Slider display text</param>
        /// <param name="description">Slider description</param>
        /// <param name="defaultValue">Default slider value (0-100)</param>
        /// <param name="callbackObj">Optional callback for slider changes</param>
        private void AddSlider(string menuId, string sliderId, string sliderLabel, string description, int defaultValue, object callbackObj = null)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSlider: menuId cannot be null or empty.");
                return;
            }
            if (string.IsNullOrEmpty(sliderId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSlider: sliderId cannot be null or empty.");
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Menu with ID {menuId} not found.");
                return;
            }

            // Check for ID conflicts (menu names, other items)
            if (HasIdConflict(sliderId, "AddSlider"))
            {
                return;
            }

            // Check for item conflicts within this menu
            if (HasItemConflict(menu, sliderId, "AddSlider"))
            {
                return;
            }

            // Apply defaults for optional parameters
            if (string.IsNullOrEmpty(sliderLabel))
                sliderLabel = "Slider";
            if (string.IsNullOrEmpty(description))
                description = "";

            // Validate defaultValue range (assuming 0-10 range)
            if (defaultValue < 0 || defaultValue > 10)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Invalid defaultValue {defaultValue} for slider {sliderId}. Using 0 instead.");
                defaultValue = 0;
            }

            var menuSliderItem = new MenuSliderItem(sliderLabel, description, 0, 10, defaultValue, false)
            {
                ItemData = sliderId
            };

            if (callbackObj != null)
            {
                // Handle both C# delegates and Lua functions
                if (callbackObj is CallbackDelegate callback)
                {
                    // Listen for position changes
                    menu.OnSliderPositionChange += (sender, item, oldPosition, newPosition, itemIndex) =>
                    {
                        if (item == menuSliderItem)
                        {
                            callback.Invoke(oldPosition, newPosition);
                        }
                    };
                }
                else
                {
                    // Handle Lua functions and dynamic callbacks
                    menu.OnSliderPositionChange += (sender, item, oldPosition, newPosition, itemIndex) =>
                    {
                        if (item == menuSliderItem)
                        {
                            try
                            {
                                dynamic dynamicCallback = callbackObj;
                                dynamicCallback(oldPosition, newPosition);
                            }
                            catch (System.IO.EndOfStreamException)
                            {
                                // Silently ignore - callback was deleted/garbage collected
                            }
                            catch (Exception ex)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking callback for slider {sliderId}: {ex.Message}");
                            }
                        }
                    };
                }
            }

            menu.AddMenuItem(menuSliderItem);
            menu.RefreshIndex();
        }

        /// <summary>
        /// Adds a spacer/separator to a menu
        /// </summary>
        /// <param name="menuId">Target menu ID</param>
        /// <param name="spacerId">Unique spacer identifier</param>
        /// <param name="spacerText">Spacer display text</param>
        /// <param name="description">Optional spacer description</param>
        private void AddSpacer(string menuId, string spacerId, string spacerText, string description)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSpacer: menuId cannot be null or empty.");
                return;
            }
            if (string.IsNullOrEmpty(spacerId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSpacer: spacerId cannot be null or empty.");
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Menu with ID {menuId} not found.");
                return;
            }

            // Check for ID conflicts (menu names, other items)
            if (HasIdConflict(spacerId, "AddSpacer"))
            {
                return;
            }

            // Check for item conflicts within this menu
            if (HasItemConflict(menu, spacerId, "AddSpacer"))
            {
                return;
            }

            // Apply defaults for optional parameters
            if (string.IsNullOrEmpty(spacerText))
                spacerText = "---";
            if (string.IsNullOrEmpty(description))
                description = "";

            var spacerItem = CommonFunctions.GetSpacerMenuItem(spacerText, description);
            spacerItem.ItemData = spacerId;
            menu.AddMenuItem(spacerItem);
        }

        /// <summary>
        /// Refreshes a menu's display
        /// </summary>
        /// <param name="menuId">Target menu ID</param>
        private void RefreshMenu(string menuId)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] RefreshMenu: menuId cannot be null or empty.");
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Menu with ID {menuId} not found.");
                return;
            }

            menu.RefreshIndex();
        }

        /// <summary>
        /// Closes all open menus
        /// </summary>
        private void CloseAllMenus()
        {
            MenuController.CloseAllMenus();
        }

        /// <summary>
        /// Closes a specific menu
        /// </summary>
        /// <param name="menuId">Menu ID to close</param>
        private void CloseMenu(string menuId)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] CloseMenu: menuId cannot be null or empty.");
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Menu with ID {menuId} not found.");
                return;
            }

            if (menu.Visible)
            {
                menu.CloseMenu();
            }
        }

        /// <summary>
        /// Adds a submenu button to link a parent menu to a submenu
        /// </summary>
        /// <param name="parentMenuId">Parent menu ID</param>
        /// <param name="buttonId">Unique button identifier</param>
        /// <param name="submenuId">Submenu ID to link to</param>
        /// <param name="buttonLabel">Button display text</param>
        /// <param name="buttonDescription">Button description</param>
        /// <param name="callbackObj">Optional callback for button selection</param>
        private void AddSubmenuButton(string parentMenuId, string buttonId, string submenuId, string buttonLabel, string buttonDescription, object callbackObj = null)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(parentMenuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSubmenuButton: parentMenuId cannot be null or empty.");
                return;
            }



            if (string.IsNullOrEmpty(buttonId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSubmenuButton: buttonId cannot be null or empty.");
                return;
            }
            if (string.IsNullOrEmpty(submenuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSubmenuButton: submenuId cannot be null or empty.");
                return;
            }

            // Check for ID conflicts for buttonId
            if (HasIdConflict(buttonId, "AddSubmenuButton"))
            {
                return;
            }

            // Verify submenu exists (either dynamic or built-in)
            if (!DynamicMenus.ContainsKey(submenuId) && GetMenu(submenuId) == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSubmenuButton: Submenu '{submenuId}' does not exist. Create it first.");
                return;
            }


            Menu parentMenu = null;
            if (!DynamicMenus.TryGetValue(parentMenuId, out parentMenu))
            {
                parentMenu = GetMenu(parentMenuId);
            }

            Menu submenu = null;
            if (!DynamicMenus.TryGetValue(submenuId, out submenu))
            {
                submenu = GetMenu(submenuId);
            }

            if (parentMenu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Parent menu {parentMenuId} not found.");
                return;
            }

            if (submenu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Submenu {submenuId} not found.");
                return;
            }

            // Check for item conflicts within parent menu
            if (HasItemConflict(parentMenu, buttonId, "AddSubmenuButton"))
            {
                return;
            }

            // Apply defaults for optional parameters
            if (string.IsNullOrEmpty(buttonLabel))
                buttonLabel = "Submenu";
            if (string.IsNullOrEmpty(buttonDescription))
                buttonDescription = "";

            var submenuButton = new MenuItem(buttonLabel, buttonDescription)
            {
                Label = "→→→",
                ItemData = buttonId
            };

            parentMenu.AddMenuItem(submenuButton);
            MenuController.AddSubmenu(parentMenu, submenu);
            MenuController.BindMenuItem(parentMenu, submenu, submenuButton);
            submenu.RefreshIndex();

            if (callbackObj != null)
            {
                // Handle both C# delegates and Lua functions
                if (callbackObj is CallbackDelegate callback)
                {
                    parentMenu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item == submenuButton)
                        {
                            await BaseScript.Delay(0);
                            callback.Invoke();
                        }
                    };
                }
                else if (callbackObj is Func<object> luaCallback)
                {
                    parentMenu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item == submenuButton)
                        {
                            await BaseScript.Delay(0);
                            try
                            {
                                luaCallback.Invoke();
                            }
                            catch (System.IO.EndOfStreamException)
                            {
                                // Silently ignore - callback was deleted/garbage collected
                            }
                            catch (Exception ex)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking callback for submenu button {buttonLabel}: {ex.Message}");
                            }
                        }
                    };
                }
                else
                {
                    // Try to invoke as dynamic for other callback types
                    parentMenu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item == submenuButton)
                        {
                            await BaseScript.Delay(0);
                            try
                            {
                                dynamic dynamicCallback = callbackObj;
                                dynamicCallback();
                            }
                            catch (System.IO.EndOfStreamException)
                            {
                                // Silently ignore - callback was deleted/garbage collected
                            }
                            catch (Exception ex)
                            {
                                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking dynamic callback for submenu button {buttonLabel}: {ex.Message}");
                            }
                        }
                    };
                }
            }

            parentMenu.RefreshIndex();
        }

        /// <summary>
        /// Removes an item from a menu by ID (string) or index (int)
        /// </summary>
        /// <param name="menuId">Target menu ID</param>
        /// <param name="itemIdOrIndex">Item identifier (string) or index (int) to remove</param>
        private void RemoveItem(string menuId, object itemIdOrIndex)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] RemoveItem: menuId cannot be null or empty.");
                return;
            }
            if (itemIdOrIndex == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] RemoveItem: itemIdOrIndex cannot be null.");
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Menu with ID {menuId} not found.");
                return;
            }

            var items = menu.GetMenuItems();

            // Check if it's an int (index-based removal)
            if (itemIdOrIndex is int index)
            {
                if (index < 0 || index >= items.Count)
                {
                    // Silent fail for out of range index (used for clearing menus in loops)
                    return;
                }
                menu.RemoveMenuItem(items[index]);
            }
            // Otherwise treat as string (ID-based removal)
            else if (itemIdOrIndex is string itemId)
            {
                if (string.IsNullOrEmpty(itemId))
                {
                    CitizenFX.Core.Debug.WriteLine($"[vMenu] RemoveItem: itemId cannot be empty.");
                    return;
                }

                // Find the item to remove by ItemData
                var itemToRemove = items.Find(item => item.ItemData as string == itemId);

                if (itemToRemove != null)
                {
                    menu.RemoveMenuItem(itemToRemove);
                }
                else
                {
                    CitizenFX.Core.Debug.WriteLine($"[vMenu] Item with ID {itemId} not found in menu {menuId}.");
                }
            }
            else
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] RemoveItem: itemIdOrIndex must be a string or int.");
            }
        }

        /// <summary>
        /// Clears all items from a menu
        /// </summary>
        /// <param name="menuId">Target menu ID</param>
        private void ClearMenu(string menuId)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] ClearMenu: menuId cannot be null or empty.");
                return;
            }

            Menu menu = null;

            // First check dynamic menus
            if (DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu.ClearMenuItems();
                return;
            }

            // Then check built-in menus using GetMenu
            menu = GetMenu(menuId);

            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] ClearMenu: Menu with ID '{menuId}' not found. Available menus: {string.Join(", ", GetAllMenuIds())}");
                return;
            }

            menu.ClearMenuItems();
        }

        /// <summary>
        /// Checks if a menu exists
        /// </summary>
        /// <param name="menuId">Menu ID to check</param>
        /// <returns>True if menu exists, false otherwise</returns>
        private bool CheckMenu(string menuId)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] CheckMenu: menuId cannot be null or empty.");
                return false;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                menu = GetMenu(menuId);
            }

            if (menu == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deletes a dynamically created menu
        /// </summary>
        /// <param name="menuId">Menu ID to delete</param>
        private void DeleteMenu(string menuId)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(menuId))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] DeleteMenu: menuId cannot be null or empty.");
                return;
            }

            Menu menu = null;
            if (!DynamicMenus.TryGetValue(menuId, out menu))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Menu with ID {menuId} not found.");
                return;
            }

            // Close the menu if it's currently open
            if (menu.Visible)
            {
                menu.CloseMenu();
            }

            // Clear all menu items (handles all item cleanup including submenu buttons)
            menu.ClearMenuItems();

            // Remove from MenuController's menu collection
            if (MenuController.Menus.Contains(menu))
            {
                MenuController.Menus.Remove(menu);
            }

            // Remove from dynamic menus dictionary
            DynamicMenus.Remove(menuId);

            // Set menu reference to null for garbage collection
            menu = null;
        }


        /// <summary>
        /// Dynamically finds built-in vMenu menus by ID with permission checking
        /// </summary>
        /// <param name="menuId">Menu identifier string</param>
        /// <returns>Menu instance if found and permitted, null otherwise</returns>
        private Menu GetMenu(string menuId)
        {
            // First check dynamic menus by exact ID
            if (DynamicMenus.TryGetValue(menuId, out Menu dynamicMenu))
            {
                return dynamicMenu;
            }

            // Search built-in menus by normalized subtitle
            var uniqueMenus = MenuController.Menus.GroupBy(m => m.MenuSubtitle ?? m.MenuTitle).Select(g => g.First()).ToList();
            var normalizedId = menuId.ToLower().Replace(" ", "-");

            foreach (var menu in uniqueMenus)
            {
                // Skip dynamic menus (already checked above)
                if (DynamicMenus.ContainsValue(menu)) continue;

                // Try to match by menu subtitle (or title if subtitle is null)
                var menuIdentifier = menu.MenuSubtitle ?? menu.MenuTitle ?? "";
                var normalizedTitle = menuIdentifier.ToLower().Replace(" ", "-");

                // Exact match only
                if (normalizedTitle == normalizedId)
                {
                    // Basic permission check for known menu patterns
                    if (IsMenuPermitted(menu, menuId.ToLower()))
                    {
                        return menu;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Export function to get all available menu IDs
        /// </summary>
        /// <returns>Array of menu IDs that can be accessed through GetMenu</returns>
        private string[] GetAllMenuIds()
        {
            var menuIds = new List<string>();

            // Add dynamic menu IDs
            foreach (var kvp in DynamicMenus)
            {
                menuIds.Add(kvp.Key);
            }

            // Add built-in menu IDs based on their subtitles (avoid duplicates)
            var uniqueMenus = MenuController.Menus.GroupBy(m => m.MenuSubtitle ?? m.MenuTitle).Select(g => g.First()).ToList();
            foreach (var menu in uniqueMenus)
            {
                // Skip dynamic menus (already added above)
                if (DynamicMenus.ContainsValue(menu)) continue;

                // Check if menu is permitted
                var menuIdentifier = menu.MenuSubtitle ?? menu.MenuTitle ?? "";
                var normalizedTitle = menuIdentifier.ToLower().Replace(" ", "-");
                if (IsMenuPermitted(menu, normalizedTitle))
                {
                    menuIds.Add(normalizedTitle);
                }
            }

            // Remove duplicates and sort
            return menuIds.Distinct().OrderBy(id => id).ToArray();
        }

        /// <summary>
        /// Checks if a menu is permitted based on vMenu permissions
        /// </summary>
        /// <param name="menu">Menu to check</param>
        /// <param name="menuId">Menu identifier</param>
        /// <returns>True if permitted, false otherwise</returns>
        private bool IsMenuPermitted(Menu menu, string menuId)
        {
            // Common permission patterns based on menu titles/IDs
            return menuId switch
            {
                "player-options" => IsAllowed(Permission.POMenu),
                "online-players" => IsAllowed(Permission.OPMenu),
                var id when id.Contains("banned-players") => IsAllowed(Permission.OPUnban) || IsAllowed(Permission.OPViewBannedPlayers),
                "personal-vehicle-options" => IsAllowed(Permission.PVMenu),
                "vehicle-options" => IsAllowed(Permission.VOMenu),
                "vehicle-spawner" => IsAllowed(Permission.VSMenu),
                var id when id.Contains("saved") && id.Contains("vehicle") => IsAllowed(Permission.SVMenu),
                "player-appearance" or "character-appearance-options" => IsAllowed(Permission.PAMenu),
                "mp-ped-customization" => IsAllowed(Permission.PAMenu),
                var id when id.Contains("time") => IsAllowed(Permission.TOMenu) && GetSettingsBool(Setting.vmenu_enable_time_sync),
                var id when id.Contains("weather") => IsAllowed(Permission.WOMenu) && GetSettingsBool(Setting.vmenu_enable_weather_sync),
                var id when id == "world" || id.Contains("world") => (IsAllowed(Permission.TOMenu) && GetSettingsBool(Setting.vmenu_enable_time_sync)) ||
                                                                     (IsAllowed(Permission.WOMenu) && GetSettingsBool(Setting.vmenu_enable_weather_sync)),
                "weapon-options" => IsAllowed(Permission.WPMenu),
                var id when id.Contains("weapon-loadouts") => IsAllowed(Permission.WLMenu),
                "voice-chat-settings" => IsAllowed(Permission.VCMenu),
                // Allow access to menus without specific restrictions
                var id when id.Contains("recording-options") || id.Contains("misc-settings") || id.Contains("about-vmenu") => true,
                // Default to allowing access for unrecognized menus
                _ => true
            };
        }

        #endregion
    }
}
