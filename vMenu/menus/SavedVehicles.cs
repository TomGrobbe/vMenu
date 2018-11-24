using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    public class SavedVehicles
    {
        // Variables
        private UIMenu menu;
        private UIMenu selectedVehicleMenu = new UIMenu("Manage Vehicle", "Manage this saved vehicle.", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        private UIMenu unavailableVehiclesMenu = new UIMenu("Missing Vehicles", "Unavailable Saved Vehicles", true)
        {
            ScaleWithSafezone = false,
            MouseControlsEnabled = false,
            MouseEdgeEnabled = false,
            ControlDisablingEnabled = false
        };
        private CommonFunctions cf = MainMenu.Cf;
        private Dictionary<string, CommonFunctions.VehicleInfo> savedVehicles = new Dictionary<string, CommonFunctions.VehicleInfo>();
        private List<UIMenu> subMenus = new List<UIMenu>();
        private Dictionary<UIMenuItem, KeyValuePair<string, CommonFunctions.VehicleInfo>> svMenuItems = new Dictionary<UIMenuItem, KeyValuePair<string, CommonFunctions.VehicleInfo>>();
        private KeyValuePair<string, CommonFunctions.VehicleInfo> currentlySelectedVehicle = new KeyValuePair<string, CommonFunctions.VehicleInfo>();
        UIMenu lastMenu = null;
        private int deleteButtonPressedCount = 0;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            string menuTitle = "Saved Vehicles";
            #region Create menus and submenus
            // Create the menu.
            menu = new UIMenu(menuTitle, "Manage Saved Vehicles", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };



            UIMenuItem saveVehicle = new UIMenuItem("Save Current Vehicle", "Save the vehicle you are currently sitting in.");
            menu.AddItem(saveVehicle);
            saveVehicle.SetLeftBadge(UIMenuItem.BadgeStyle.Car);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == saveVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        cf.SaveVehicle();
                    }
                    else
                    {
                        Notify.Error("You are currently not in any vehicle. Please enter a vehicle before trying to save it.");
                    }
                }
            };


            for (int i = 0; i < 22; i++)
            {
                UIMenu categoryMenu = new UIMenu("Saved Vehicles", GetLabelText($"VEH_CLASS_{i}"), true)
                {
                    ScaleWithSafezone = false,
                    MouseControlsEnabled = false,
                    MouseEdgeEnabled = false,
                    ControlDisablingEnabled = false
                };

                UIMenuItem categoryButton = new UIMenuItem(GetLabelText($"VEH_CLASS_{i}"), $"All saved vehicles from the {(GetLabelText($"VEH_CLASS_{i}"))} category.");
                subMenus.Add(categoryMenu);
                MainMenu.Mp.Add(categoryMenu);
                menu.AddItem(categoryButton);
                categoryButton.SetRightLabel("→→→");
                menu.BindMenuToItem(categoryMenu, categoryButton);

                categoryMenu.OnMenuClose += (sender) =>
                {
                    UpdateMenuAvailableCategories();
                };

                categoryMenu.OnItemSelect += (sender, item, index) =>
                {
                    UpdateSelectedVehicleMenu(item);
                    lastMenu = categoryMenu;
                };
            }

            UIMenuItem unavailableModels = new UIMenuItem("Unavailable Saved Vehicles", "These vehicles are currently unavailable because the models are not present in the game. These vehicles are most likely not being streamed from the server.");

            unavailableModels.SetRightLabel("→→→");

            menu.AddItem(unavailableModels);
            menu.BindMenuToItem(unavailableVehiclesMenu, unavailableModels);
            MainMenu.Mp.Add(unavailableVehiclesMenu);


            MainMenu.Mp.Add(selectedVehicleMenu);
            UIMenuItem spawnVehicle = new UIMenuItem("Spawn Vehicle", "Spawn this saved vehicle.");
            UIMenuItem renameVehicle = new UIMenuItem("Rename Vehicle", "Rename your saved vehicle.");
            UIMenuItem replaceVehicle = new UIMenuItem("~r~Replace Vehicle", "Your saved vehicle will be replaced with the vehicle you are currently sitting in. ~r~Warning: this can NOT be undone!");
            UIMenuItem deleteVehicle = new UIMenuItem("~r~Delete Vehicle", "~r~This will delete your saved vehicle. Warning: this can NOT be undone!");
            selectedVehicleMenu.AddItem(spawnVehicle);
            selectedVehicleMenu.AddItem(renameVehicle);
            selectedVehicleMenu.AddItem(replaceVehicle);
            selectedVehicleMenu.AddItem(deleteVehicle);

            selectedVehicleMenu.OnMenuClose += (sender) =>
            {
                selectedVehicleMenu.RefreshIndex();
                MainMenu.Mp.CloseAllMenus();
                if (lastMenu != null)
                {
                    lastMenu.Visible = true;
                }
                else
                {
                    GetMenu().Visible = true;
                }

                deleteButtonPressedCount = 0;
                deleteVehicle.SetRightLabel("");
            };

            selectedVehicleMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnVehicle)
                {
                    if (MainMenu.VehicleSpawnerMenu != null)
                    {
                        cf.SpawnVehicle(currentlySelectedVehicle.Value.model, MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                    else
                    {
                        cf.SpawnVehicle(currentlySelectedVehicle.Value.model, true, true, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                }
                else if (item == renameVehicle)
                {
                    string newName = await cf.GetUserInput("Enter a new name for this vehicle.", "", 25);
                    if (string.IsNullOrEmpty(newName) || newName == "NULL")
                    {
                        Notify.Error("You entered an invalid name or you cancelled the action.");
                    }
                    else
                    {
                        if (StorageManager.SaveVehicleInfo("veh_" + newName, currentlySelectedVehicle.Value, false))
                        {
                            DeleteResourceKvp(currentlySelectedVehicle.Key);
                            while (!selectedVehicleMenu.Visible)
                            {
                                await BaseScript.Delay(0);
                            }
                            Notify.Success("Your vehicle has successfully been renamed.");
                            UpdateMenuAvailableCategories();
                            selectedVehicleMenu.GoBack();
                            currentlySelectedVehicle = new KeyValuePair<string, CommonFunctions.VehicleInfo>(); // clear the old info
                        }
                        else
                        {
                            Notify.Error("This name is already in use or something unknown failed. Contact the server owner if you believe something is wrong.");
                        }
                    }
                }
                else if (item == replaceVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        cf.SaveVehicle(currentlySelectedVehicle.Key.Substring(4));
                        selectedVehicleMenu.GoBack();
                        Notify.Success("Your saved vehicle has been replaced with your current vehicle.");
                    }
                    else
                    {
                        Notify.Error("You need to be in a vehicle before you can relplace your old vehicle.");
                    }
                }
                else if (item == deleteVehicle)
                {
                    if (deleteButtonPressedCount == 0)
                    {
                        deleteButtonPressedCount = 1;
                        item.SetRightLabel("Press again to confirm.");
                        Notify.Alert("Are you sure you want to delete this vehicle? Press the button again to confirm.");
                    }
                    else
                    {
                        deleteButtonPressedCount = 0;
                        item.SetRightLabel("");
                        DeleteResourceKvp(currentlySelectedVehicle.Key);
                        UpdateMenuAvailableCategories();
                        selectedVehicleMenu.GoBack();
                        Notify.Success("Your saved vehicle has been deleted.");
                    }
                }
                if (item != deleteVehicle) // if any other button is pressed, restore the delete vehicle button pressed count.
                {
                    deleteButtonPressedCount = 0;
                    deleteVehicle.SetRightLabel("");
                }
            };

            #endregion
        }



        private void UpdateSelectedVehicleMenu(UIMenuItem selectedItem)
        {
            var vehInfo = svMenuItems[selectedItem];
            selectedVehicleMenu.Subtitle.Caption = $"{vehInfo.Key.Substring(4)} ({vehInfo.Value.name})";
            currentlySelectedVehicle = vehInfo;
        }


        public void UpdateMenuAvailableCategories()
        {
            savedVehicles = cf.GetSavedVehicles();
            svMenuItems = new Dictionary<UIMenuItem, KeyValuePair<string, CommonFunctions.VehicleInfo>>();

            for (int i = 1; i < GetMenu().MenuItems.Count - 1; i++)
            {
                if (savedVehicles.Any(a => GetVehicleClassFromName(a.Value.model) == i - 1 && IsModelInCdimage(a.Value.model)))
                {
                    GetMenu().MenuItems[i].SetRightBadge(UIMenuItem.BadgeStyle.None);
                    GetMenu().MenuItems[i].SetRightLabel("→→→");
                    GetMenu().MenuItems[i].Enabled = true;
                    GetMenu().MenuItems[i].Description = $"All saved vehicles from the {GetMenu().MenuItems[i].Text} category.";
                }
                else
                {
                    GetMenu().MenuItems[i].SetRightLabel("");
                    GetMenu().MenuItems[i].SetRightBadge(UIMenuItem.BadgeStyle.Lock);
                    GetMenu().MenuItems[i].Enabled = false;
                    GetMenu().MenuItems[i].Description = $"You do not have any saved vehicles that belong to the {GetMenu().MenuItems[i].Text} category.";
                }
            }



            GetMenu().UpdateScaleform();

            foreach (UIMenu m in subMenus)
            {
                m.Clear();
            }

            unavailableVehiclesMenu.Clear();

            foreach (var sv in savedVehicles)
            {
                if (IsModelInCdimage(sv.Value.model))
                {
                    int vclass = GetVehicleClassFromName(sv.Value.model);
                    UIMenu menu = subMenus[vclass];

                    UIMenuItem savedVehicleBtn = new UIMenuItem(sv.Key.Substring(4), $"Manage this saved vehicle.");
                    savedVehicleBtn.SetRightLabel($"({sv.Value.name}) →→→");
                    menu.AddItem(savedVehicleBtn);

                    svMenuItems.Add(savedVehicleBtn, sv);

                    menu.BindMenuToItem(selectedVehicleMenu, savedVehicleBtn);
                }
                else
                {
                    UIMenuItem missingVehItem = new UIMenuItem(sv.Key.Substring(4), "This model could not be found in the game files. Most likely because this is an addon vehicle and it's currently not streamed by the server.");
                    missingVehItem.SetRightLabel("(" + sv.Value.name + ")");
                    missingVehItem.Enabled = false;
                    missingVehItem.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    unavailableVehiclesMenu.AddItem(missingVehItem);
                }

            }
            foreach (UIMenu m in subMenus)
            {
                m.MenuItems.Sort((UIMenuItem A, UIMenuItem B) =>
                {
                    return A.Text.ToLower().CompareTo(B.Text.ToLower());
                });

                m.UpdateScaleform();
                m.RefreshIndex();
            }
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
