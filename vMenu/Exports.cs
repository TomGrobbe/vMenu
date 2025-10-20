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

        #region Helper Methods

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

        /// <summary>
        /// Validates a string parameter is not null or empty
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="paramName">Parameter name for error message</param>
        /// <param name="context">Context for error messages</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateParameter(string value, string paramName, string context)
        {
            if (string.IsNullOrEmpty(value))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] {context}: {paramName} cannot be null or empty.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves a menu by ID from dynamic or built-in menus
        /// </summary>
        /// <param name="menuId">Menu identifier</param>
        /// <returns>Menu instance if found, null otherwise</returns>
        private Menu RetrieveMenu(string menuId)
        {
            if (DynamicMenus.TryGetValue(menuId, out Menu menu))
            {
                return menu;
            }
            return GetMenu(menuId);
        }

        /// <summary>
        /// Safely invokes a callback with exception handling
        /// </summary>
        /// <param name="callbackObj">Callback object to invoke</param>
        /// <param name="itemId">Item identifier for error logging</param>
        /// <param name="args">Arguments to pass to callback</param>
        private void SafeInvokeCallback(object callbackObj, string itemId, params object[] args)
        {
            try
            {
                if (callbackObj == null) return;

                if (callbackObj is CallbackDelegate callback)
                {
                    callback.Invoke(args);
                }
                else if (callbackObj is Func<object> luaCallback)
                {
                    luaCallback.Invoke();
                }
                else
                {
                    dynamic dynamicCallback = callbackObj;
                    // Dynamic invocation handles variable arguments automatically
                    switch (args?.Length ?? 0)
                    {
                        case 0:
                            dynamicCallback();
                            break;
                        case 1:
                            dynamicCallback(args[0]);
                            break;
                        case 2:
                            dynamicCallback(args[0], args[1]);
                            break;
                        case 3:
                            dynamicCallback(args[0], args[1], args[2]);
                            break;
                        case 4:
                            dynamicCallback(args[0], args[1], args[2], args[3]);
                            break;
                        default:
                            CitizenFX.Core.Debug.WriteLine($"[vMenu] Callback for {itemId} has unsupported number of arguments: {args.Length}");
                            break;
                    }
                }
            }
            catch (System.IO.EndOfStreamException)
            {
                // Silently ignore - callback was deleted/garbage collected
            }
            catch (Exception ex)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] Error invoking callback for {itemId}: {ex.GetType().Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// Attaches a callback to a menu item selection event
        /// </summary>
        private void AttachItemSelectCallback(Menu menu, MenuItem item, object callbackObj, string itemId)
        {
            if (callbackObj == null) return;

            if (callbackObj is CallbackDelegate callback)
            {
                menu.OnItemSelect += async (sender, menuItem, index) =>
                {
                    if (menuItem == item)
                    {
                        await BaseScript.Delay(0);
                        SafeInvokeCallback(callback, itemId);
                    }
                };
            }
            else
            {
                menu.OnItemSelect += async (sender, menuItem, index) =>
                {
                    if (menuItem == item)
                    {
                        await BaseScript.Delay(0);
                        SafeInvokeCallback(callbackObj, itemId);
                    }
                };
            }
        }

        /// <summary>
        /// Attaches a callback to a checkbox change event
        /// </summary>
        private void AttachCheckboxCallback(Menu menu, MenuCheckboxItem item, object callbackObj, string itemId)
        {
            if (callbackObj == null) return;

            menu.OnCheckboxChange += (sender, menuItem, index, _checked) =>
            {
                if (menuItem == item)
                {
                    SafeInvokeCallback(callbackObj, itemId, _checked);
                }
            };
        }

        /// <summary>
        /// Attaches callbacks to a list item events
        /// </summary>
        private void AttachListCallbacks(Menu menu, MenuListItem item, List<string> options, object callbackObj, string itemId)
        {
            if (callbackObj == null) return;

            // Listen for index changes (browsing through list)
            menu.OnListIndexChange += (sender, menuItem, oldIndex, newIndex, itemIndex) =>
            {
                if (menuItem == item)
                {
                    var currentValue = options[newIndex];
                    SafeInvokeCallback(callbackObj, itemId, false, currentValue, newIndex, oldIndex);
                }
            };

            // Listen for item selection (pressing enter/select)
            menu.OnListItemSelect += (sender, menuItem, listIndex, itemIndex) =>
            {
                if (menuItem == item)
                {
                    var selectedValue = options[listIndex];
                    SafeInvokeCallback(callbackObj, itemId, true, selectedValue, listIndex, listIndex);
                }
            };
        }

        /// <summary>
        /// Attaches a callback to a slider position change event
        /// </summary>
        private void AttachSliderCallback(Menu menu, MenuSliderItem item, object callbackObj, string itemId)
        {
            if (callbackObj == null) return;

            menu.OnSliderPositionChange += (sender, menuItem, oldPosition, newPosition, itemIndex) =>
            {
                if (menuItem == item)
                {
                    SafeInvokeCallback(callbackObj, itemId, oldPosition, newPosition);
                }
            };
        }

        /// <summary>
        /// Attaches a callback to a menu open event
        /// </summary>
        private void AttachMenuOpenCallback(Menu menu, object callbackObj, string menuId)
        {
            if (callbackObj == null) return;

            if (callbackObj is CallbackDelegate callback)
            {
                menu.OnMenuOpen += (sender) =>
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
                menu.OnMenuOpen += (sender) =>
                {
                    SafeInvokeCallback(callbackObj, null, menuId);
                };
            }
        }

        #endregion

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
            Exports.Add("GetMenu", new Func<string, object>(GetMenuExport));
            Exports.Add("IsMenuPermitted", new Func<string, bool>(IsMenuPermittedExport));
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
            if (!ValidateParameter(menuId, "menuId", "CreateMenu")) return;
            if (HasIdConflict(menuId, "CreateMenu")) return;

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

            AttachMenuOpenCallback(newMenu, callbackObj, menuId);
        }

        /// <summary>
        /// Displays notification to the player
        /// </summary>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type (error, info, success, or default)</param>
        private void notifExp(string message, string type)
        {
            switch (type)
            {
                case "error":
                    Notify.Error(message);
                    break;
                case "info":
                    Notify.Info(message);
                    break;
                case "success":
                    Notify.Success(message);
                    break;
                default:
                    Notify.Alert(message);
                    break;
            }
        }

        /// <summary>
        /// Opens a menu by ID
        /// </summary>
        /// <param name="menuId">Menu identifier</param>
        private void OpenMenu(string menuId)
        {
            if (!ValidateParameter(menuId, "menuId", "OpenMenu")) return;

            Menu menu = RetrieveMenu(menuId);

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

            if (!ValidateParameter(buttonId, "buttonId", "AddButton")) return;
            if (HasIdConflict(buttonId, "AddButton")) return;

            Menu menu = RetrieveMenu(menuId);
            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddButton: Menu with ID {menuId} not found.");
                return;
            }

            if (HasItemConflict(menu, buttonId, "AddButton")) return;

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

            AttachItemSelectCallback(menu, menuItem, callbackObj, buttonId);
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
            if (!ValidateParameter(listId, "listId", "AddList")) return;
            if (HasIdConflict(listId, "AddList")) return;

            Menu menu = RetrieveMenu(menuId);
            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddList: Menu with ID {menuId} not found.");
                return;
            }

            if (HasItemConflict(menu, listId, "AddList")) return;

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

            AttachListCallbacks(menu, menuListItem, optionsList, callbackObj, listId);
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
            if (!ValidateParameter(menuId, "menuId", "AddCheckbox")) return;
            if (!ValidateParameter(checkboxId, "checkboxId", "AddCheckbox")) return;

            Menu menu = RetrieveMenu(menuId);
            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddCheckbox: Menu with ID {menuId} not found.");
                return;
            }

            if (HasIdConflict(checkboxId, "AddCheckbox")) return;
            if (HasItemConflict(menu, checkboxId, "AddCheckbox")) return;

            // Apply defaults for optional parameters
            if (string.IsNullOrEmpty(checkboxLabel))
                checkboxLabel = "Checkbox";
            if (string.IsNullOrEmpty(description))
                description = "";

            var menuCheckboxItem = new MenuCheckboxItem(checkboxLabel, description, defaultValue)
            {
                ItemData = checkboxId
            };

            AttachCheckboxCallback(menu, menuCheckboxItem, callbackObj, checkboxId);
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
            if (!ValidateParameter(menuId, "menuId", "AddSlider")) return;
            if (!ValidateParameter(sliderId, "sliderId", "AddSlider")) return;

            Menu menu = RetrieveMenu(menuId);
            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSlider: Menu with ID {menuId} not found.");
                return;
            }

            if (HasIdConflict(sliderId, "AddSlider")) return;
            if (HasItemConflict(menu, sliderId, "AddSlider")) return;

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

            AttachSliderCallback(menu, menuSliderItem, callbackObj, sliderId);
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
            if (!ValidateParameter(menuId, "menuId", "AddSpacer")) return;
            if (!ValidateParameter(spacerId, "spacerId", "AddSpacer")) return;

            Menu menu = RetrieveMenu(menuId);
            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSpacer: Menu with ID {menuId} not found.");
                return;
            }

            if (HasIdConflict(spacerId, "AddSpacer")) return;
            if (HasItemConflict(menu, spacerId, "AddSpacer")) return;

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
            if (!ValidateParameter(menuId, "menuId", "RefreshMenu")) return;

            Menu menu = RetrieveMenu(menuId);
            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] RefreshMenu: Menu with ID {menuId} not found.");
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
            if (!ValidateParameter(menuId, "menuId", "CloseMenu")) return;

            Menu menu = RetrieveMenu(menuId);
            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] CloseMenu: Menu with ID {menuId} not found.");
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
            if (!ValidateParameter(parentMenuId, "parentMenuId", "AddSubmenuButton")) return;
            if (!ValidateParameter(buttonId, "buttonId", "AddSubmenuButton")) return;
            if (!ValidateParameter(submenuId, "submenuId", "AddSubmenuButton")) return;

            if (HasIdConflict(buttonId, "AddSubmenuButton")) return;

            // Verify submenu exists (either dynamic or built-in)
            if (!DynamicMenus.ContainsKey(submenuId) && GetMenu(submenuId) == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSubmenuButton: Submenu '{submenuId}' does not exist. Create it first.");
                return;
            }

            Menu parentMenu = RetrieveMenu(parentMenuId);
            Menu submenu = RetrieveMenu(submenuId);

            if (parentMenu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSubmenuButton: Parent menu {parentMenuId} not found.");
                return;
            }

            if (submenu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] AddSubmenuButton: Submenu {submenuId} not found.");
                return;
            }

            if (HasItemConflict(parentMenu, buttonId, "AddSubmenuButton")) return;

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

            AttachItemSelectCallback(parentMenu, submenuButton, callbackObj, buttonId);
            parentMenu.RefreshIndex();
        }

        /// <summary>
        /// Removes an item from a menu by ID (string) or index (int)
        /// </summary>
        /// <param name="menuId">Target menu ID</param>
        /// <param name="itemIdOrIndex">Item identifier (string) or index (int) to remove</param>
        private void RemoveItem(string menuId, object itemIdOrIndex)
        {
            if (!ValidateParameter(menuId, "menuId", "RemoveItem")) return;

            if (itemIdOrIndex == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] RemoveItem: itemIdOrIndex cannot be null.");
                return;
            }

            Menu menu = RetrieveMenu(menuId);
            if (menu == null)
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] RemoveItem: Menu with ID {menuId} not found.");
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
                    CitizenFX.Core.Debug.WriteLine($"[vMenu] RemoveItem: Item with ID {itemId} not found in menu {menuId}.");
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
            if (!ValidateParameter(menuId, "menuId", "ClearMenu")) return;

            Menu menu = RetrieveMenu(menuId);
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
            if (!ValidateParameter(menuId, "menuId", "CheckMenu")) return false;

            Menu menu = RetrieveMenu(menuId);
            return menu != null;
        }

        /// <summary>
        /// Deletes a dynamically created menu
        /// </summary>
        /// <param name="menuId">Menu ID to delete</param>
        private void DeleteMenu(string menuId)
        {
            if (!ValidateParameter(menuId, "menuId", "DeleteMenu")) return;

            if (!DynamicMenus.TryGetValue(menuId, out Menu menu))
            {
                CitizenFX.Core.Debug.WriteLine($"[vMenu] DeleteMenu: Menu with ID {menuId} not found.");
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
        /// Dynamically finds built-in vMenu menus by ID (without permission filtering)
        /// </summary>
        /// <param name="menuId">Menu identifier string</param>
        /// <returns>Menu instance if found, null otherwise</returns>
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
                    return menu;
                }
            }

            return null;
        }

        /// <summary>
        /// Export wrapper for GetMenu that returns an object (for export compatibility)
        /// </summary>
        /// <param name="menuId">Menu identifier</param>
        /// <returns>Menu instance as object if found, null otherwise</returns>
        private object GetMenuExport(string menuId)
        {
            return GetMenu(menuId);
        }

        /// <summary>
        /// Export function to get all available menu IDs (without permission filtering)
        /// </summary>
        /// <returns>Array of all menu IDs that exist</returns>
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

                var menuIdentifier = menu.MenuSubtitle ?? menu.MenuTitle ?? "";
                var normalizedTitle = menuIdentifier.ToLower().Replace(" ", "-");
                menuIds.Add(normalizedTitle);
            }

            // Remove duplicates and sort
            return menuIds.Distinct().OrderBy(id => id).ToArray();
        }

        /// <summary>
        /// Export function to check if a menu is permitted based on vMenu permissions
        /// </summary>
        /// <param name="menuId">Menu identifier</param>
        /// <returns>True if permitted, false otherwise</returns>
        private bool IsMenuPermittedExport(string menuId)
        {
            if (string.IsNullOrEmpty(menuId))
            {
                return false;
            }

            var normalizedId = menuId.ToLower().Replace(" ", "-");

            // Get the menu to check if it exists
            var menu = GetMenu(normalizedId);
            if (menu == null)
            {
                return false;
            }

            return IsMenuPermitted(menu, normalizedId);
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
