using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json;

using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus.VehicleSubmenus
{
    public class VehicleSpawner
    {
        private static UIMenu VehicleSpawnMenu = null;

        public VehicleSpawner()
        {
            var MenuLanguage = Languages.Menus["TimeOptionsMenu"];

            VehicleSpawnMenu = new Objects.vMenu("Vehicle Spawner").Create();

            UIMenuItem SpawnByName = new UIMenuItem("Spawn Vehicle By Model Name", "Enter the name of a vehicle to spawn.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            SpawnByName.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            SpawnByName.SetRightLabel(">>>");
            UIMenuCheckboxItem replacevehicle = new UIMenuCheckboxItem("Replace Previous Vehicle", UIMenuCheckboxStyle.Tick, true, "This will automatically delete your previously spawned vehicle when you spawn a new vehicle.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            UIMenuCheckboxItem spawninside = new UIMenuCheckboxItem("Spawn Inside Vehicle", UIMenuCheckboxStyle.Tick, true, "This will teleport you into the vehicle when you spawn it.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);


            SpawnByName.Activated += async (sender, i) =>
            {
                Main.Instance.AttachTick(DisableControls);
                await EntitySpawner.SpawnVehicle("custom", spawninside.Checked, replacevehicle.Checked);
                Main.Instance.DetachTick(DisableControls);

            };

            VehicleSpawnMenu.AddItem(SpawnByName);
            VehicleSpawnMenu.AddItem(replacevehicle);
            VehicleSpawnMenu.AddItem(spawninside);

            var jsonData = LoadResourceFile(GetCurrentResourceName(), "config/Default.jsonc") ?? "{}";
            var defaultcars = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonData);
            var modellist = new Dictionary<string, uint>();

            if (defaultcars.ContainsKey("vehicles"))
            {
                foreach (var defaultveh in defaultcars["vehicles"])
                {
                    if (!modellist.ContainsKey(defaultveh))
                    {
                        modellist.Add(defaultveh, (uint)GetHashKey(defaultveh));
                    }
                    else
                    {
                        Debug.WriteLine($"[vMenu] [Error] Your addons.json file contains 2 or more entries with the same vehicle name! ({defaultveh}) Please remove duplicate lines!");
                    }
                }
            }

            for (var cat = 0; cat < 23; cat++)
            {
                var categoryMenu = new Objects.vMenu(GetLabelText($"VEH_CLASS_{cat}")).Create();

                UIMenuItem categoryBtn = new UIMenuItem(GetLabelText($"VEH_CLASS_{cat}"), $"Spawn an addon vehicle from the {GetLabelText($"VEH_CLASS_{cat}")} class.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
                categoryBtn.SetRightLabel(">>>");
                VehicleSpawnMenu.AddItem(categoryBtn);
                categoryBtn.Activated += (sender, i) =>
                {
                    sender.SwitchTo(categoryMenu, inheritOldMenuParams: true);
                };

                if (!VehicleOptionsMenu.allowedCategories[cat])
                {
                    categoryBtn.Description = "This vehicle class is disabled by the server.";
                    categoryBtn.Enabled = false;
                    categoryBtn.SetRightLabel("");
                    continue;
                }
                // Loop through all addon vehicles in this class.
                foreach (KeyValuePair<string, uint> veh in modellist.Where(v => GetVehicleClassFromName(v.Value) == cat))
                {
                    string localizedName = GetLabelText(GetDisplayNameFromVehicleModel(veh.Value));

                    string name = localizedName != "NULL" ? localizedName : GetDisplayNameFromVehicleModel(veh.Value);

                    name = name != "CARNOTFOUND" ? name : veh.Key;

                    var carBtn = new UIMenuItem(name, $"Click to spawn {name}.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
                    {
                        ItemData = veh.Key
                    };   
                    carBtn.SetRightLabel($"({veh.Key})");

                    
                    carBtn.Activated += async (sender, i) =>
                    {
                        await EntitySpawner.SpawnVehicle(i.ItemData, spawninside.Checked, replacevehicle.Checked);
                    };
                    // This should be impossible to be false, but we check it anyway.
                    if (IsModelInCdimage(veh.Value))
                    {
                        categoryMenu.AddItem(carBtn);
                    }
                    else
                    {
                        carBtn.Enabled = false;
                        carBtn.Description = "This vehicle is not available. Please ask the server owner to check if the vehicle is being streamed correctly.";
                    }
                }

             
                if (categoryMenu.Size > 0)
                {
                    Main.Menus.Add(categoryMenu);
                }
                else
                {
                    categoryBtn.Description = "There are no addon cars available in this category.";
                    categoryBtn.Enabled = false;
                    categoryBtn.SetRightLabel("");

                }
            }


            Main.Menus.Add(VehicleSpawnMenu);
        }

        private async Task DisableControls()
        {
            DisableAllControlActions(0);
            await Task.FromResult(0);  
        }

        public static UIMenu Menu()
        {
            return VehicleSpawnMenu;
        }
    }
}