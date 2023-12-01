using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json;

using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;

using static vMenu.Client.Functions.MenuFunctions;
using vMenu.Shared.Enums;

using vMenu.Client.Functions;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus.VehicleSubmenus
{
    public class VehicleSpawnerMenu
    {
        private static UIMenu VehicleSpawnMenu = null;

        public VehicleSpawnerMenu()
        {
           // var MenuLanguage = Languages.Menus["VehicleSpawnMenu"];

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
            var defaultcars = JsonConvert.DeserializeObject<Dictionary<string,Dictionary<string, Dictionary<string,string>>>>(jsonData);
            var modellist = new Dictionary<string, string>();

            if (defaultcars.ContainsKey("vehicles"))
            {
                foreach (var listvalues in defaultcars["vehicles"])
                {
                    if (!modellist.ContainsKey(listvalues.Key))
                    {
                        modellist.Add(listvalues.Key, listvalues.Value["Name"] == "" ? GetDisplayNameFromVehicleModel((uint)GetHashKey(listvalues.Key)) : listvalues.Value["Name"]);
                    }
                    else
                    {
                        Debug.WriteLine($"[vMenu] [Error] Your addons.json file contains 2 or more entries with the same vehicle name! ({listvalues.Key}) Please remove duplicate lines!");
                    }
                }

            }

            var speedValues = new float[23]
            {
                44.9374657f,
                50.0000038f,
                48.862133f,
                48.1321335f,
                50.7077942f,
                51.3333359f,
                52.3922348f,
                53.86687f,
                52.03867f,
                49.2241631f,
                39.6176529f,
                37.5559425f,
                42.72843f,
                21.0f,
                45.0f,
                65.1952744f,
                109.764259f,
                42.72843f,
                56.5962219f,
                57.5398865f,
                43.3140678f,
                26.66667f,
                53.0537224f
            };

            var accelerationValues = new float[23]
            {
                0.34f,
                0.29f,
                0.335f,
                0.28f,
                0.395f,
                0.39f,
                0.66f,
                0.42f,
                0.425f,
                0.475f,
                0.21f,
                0.3f,
                0.32f,
                0.17f,
                18.0f,
                5.88f,
                21.0700016f,
                0.33f,
                0.33f,
                6.86f,
                0.32f,
                0.2f,
                0.76f
            };

            var brakingValues = new float[23]
            {
                0.72f,
                0.95f,
                0.85f,
                0.9f,
                1.0f,
                1.0f,
                1.3f,
                1.25f,
                1.52f,
                1.1f,
                0.6f,
                0.7f,
                0.8f,
                3.0f,
                0.4f,
                3.5920403f,
                20.58f,
                0.9f,
                2.93960738f,
                3.9472363f,
                0.85f,
                5.0f,
                1.3f
            };

            var tractionValues = new float[23]
            {
                2.3f,
                2.55f,
                2.3f,
                2.6f,
                2.625f,
                2.65f,
                2.8f,
                2.782f,
                2.9f,
                2.95f,
                2.0f,
                3.3f,
                2.175f,
                2.05f,
                0.0f,
                1.6f,
                2.15f,
                2.55f,
                2.57f,
                3.7f,
                2.05f,
                2.5f,
                3.2925f
            };
            List<bool> allowedCategories = new List<bool>()
            {
                IsAllowed(Permission.VSCompacts),
                IsAllowed(Permission.VSSedans),
                IsAllowed(Permission.VSSUVs),
                IsAllowed(Permission.VSCoupes),
                IsAllowed(Permission.VSMuscle),
                IsAllowed(Permission.VSSportsClassic),
                IsAllowed(Permission.VSSports),
                IsAllowed(Permission.VSSuper),
                IsAllowed(Permission.VSMotorcycles),
                IsAllowed(Permission.VSOffRoad),
                IsAllowed(Permission.VSIndustrial),
                IsAllowed(Permission.VSUtility),
                IsAllowed(Permission.VSVans),
                IsAllowed(Permission.VSCycles),
                IsAllowed(Permission.VSBoats),
                IsAllowed(Permission.VSHelicopters),
                IsAllowed(Permission.VSPlanes),
                IsAllowed(Permission.VSService),
                IsAllowed(Permission.VSEmergency),
                IsAllowed(Permission.VSMilitary),
                IsAllowed(Permission.VSCommercial),
                IsAllowed(Permission.VSTrains),
                IsAllowed(Permission.VSOpenWheel)
            };
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

                if (!allowedCategories[cat])
                {
                    categoryBtn.Description = "This vehicle class is disabled by the server.";
                    categoryBtn.Enabled = false;
                    categoryBtn.SetRightLabel("");
                    continue;
                }

                // Loop through all addon vehicles in this class.
                foreach (KeyValuePair<string, string> veh in modellist.Where(v => GetVehicleClassFromName((uint)GetHashKey(v.Key)) == cat))
                {
                    uint vehuint = (uint)GetHashKey(veh.Key);
                    string localizedName = GetLabelText(GetDisplayNameFromVehicleModel(vehuint));

                    string name = veh.Value;

                    name = name != "CARNOTFOUND" ? name : veh.Key;

                    var carBtn = new UIMenuItem(name, $"Click to spawn {name}.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
                    {
                        ItemData = veh.Key
                    };   
                    if (Convar.GetSettingsBool("vmenu_see_vehicle_spawncode"))
                    {
                        carBtn.SetRightLabel($"({veh.Key})");
                    } 
                    UIMenuStatisticsPanel vehstatistics = new UIMenuStatisticsPanel();
                    carBtn.AddPanel(vehstatistics);

                    var topSpeed = Map(GetVehicleModelEstimatedMaxSpeed(vehuint), 0f, speedValues[cat], 0f, 1f)*100;
                    var acceleration = Map(GetVehicleModelAcceleration(vehuint), 0f, accelerationValues[cat], 0f, 1f)*100;
                    var maxBraking = Map(GetVehicleModelMaxBraking(vehuint), 0f, brakingValues[cat], 0f, 1f)*100;
                    var maxTraction = Map(GetVehicleModelMaxTraction(vehuint), 0f, tractionValues[cat], 0f, 1f)*100;

                    vehstatistics.AddStatistics("~b~Top Speed", topSpeed);
                    vehstatistics.AddStatistics("~y~Acceleration", acceleration);
                    vehstatistics.AddStatistics("~g~Max Braking", maxBraking);
                    vehstatistics.AddStatistics("~q~Max Traction", maxTraction);


                   
                    carBtn.Activated += async (sender, i) =>
                    {
                        await EntitySpawner.SpawnVehicle(i.ItemData, spawninside.Checked, replacevehicle.Checked);
                    };

                    // This should be impossible to be false, but we check it anyway.
                    if (IsModelInCdimage(vehuint))
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

        #region Map (math util) function
        /// <summary>
        /// Maps the <paramref name="value"/> (which is a value between <paramref name="min_in"/> and <paramref name="max_in"/>) to a new value in the range of <paramref name="min_out"/> and <paramref name="max_out"/>.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="min_in">The minimum range value of the value.</param>
        /// <param name="max_in">The max range value of the value.</param>
        /// <param name="min_out">The min output range value.</param>
        /// <param name="max_out">The max output range value.</param>
        /// <returns></returns>
        public static float Map(float value, float min_in, float max_in, float min_out, float max_out)
        {
            return ((value - min_in) * (max_out - min_out) / (max_in - min_in)) + min_out;
        }

        /// <summary>
        /// Maps the <paramref name="value"/> (which is a value between <paramref name="min_in"/> and <paramref name="max_in"/>) to a new value in the range of <paramref name="min_out"/> and <paramref name="max_out"/>.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="min_in">The minimum range value of the value.</param>
        /// <param name="max_in">The max range value of the value.</param>
        /// <param name="min_out">The min output range value.</param>
        /// <param name="max_out">The max output range value.</param>
        /// <returns></returns>
        public static double Map(double value, double min_in, double max_in, double min_out, double max_out)
        {
            return ((value - min_in) * (max_out - min_out) / (max_in - min_in)) + min_out;
        }
        #endregion
    }
}
