using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace vMenuClient
{

    public class StorageManager : BaseScript
    {
        /// <summary>
        /// Save Dictionary(string, string) to local storage.
        /// </summary>
        /// <param name="saveName">Name (including prefix) to save.</param>
        /// <param name="data">Data (dictionary) to save.</param>
        /// <param name="overrideExistingData">When true, will override existing save data with the same name. 
        /// If false, it will cancel the save if existing data is found and return false.</param>
        /// <returns>A boolean value indicating if the save was successful.</returns>
        public bool SaveDictionary(string saveName, Dictionary<string, string> data, bool overrideExistingData)
        {
            // If the savename doesn't exist yet or we're allowed to override it.
            if (GetResourceKvpString(saveName) == null || overrideExistingData)
            {
                // Get the json string from the dictionary.
                //string jsonString = MainMenu.Cf.DictionaryToJson(data);
                string jsonString = JsonConvert.SerializeObject(data);
                MainMenu.Cf.Log($"Saving: [name: {saveName}, json:{jsonString}]");

                // Save the kvp.
                SetResourceKvp(saveName, jsonString);

                // Return true if the kvp was set successfully, false if it wasn't set successfully.
                return GetResourceKvpString(saveName) == jsonString;
            }
            // If the data already exists and we are not allowed to override it.
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get a saved dictionary. (Used for saving peds)
        /// </summary>
        /// <param name="name">The key for the dictionary to get.</param>
        /// <returns>The requested dictionary.</returns>
        public Dictionary<string, string> GetSavedDictionary(string name)
        {
            string json;
            json = GetResourceKvpString(name);
            MainMenu.Cf.Log("Existing v2 save is being loaded: Name: " + name + " Dict: " + json.ToString());
            var dict = MainMenu.Cf.JsonToDictionary(json);
            return dict ?? new Dictionary<string, string>();
        }

        public CommonFunctions.PedInfo GetSavedPedInfo(string name)
        {
            return MainMenu.Cf.JsonToPedInfo(GetResourceKvpString(name), name);
        }

        public bool SavePedInfo(string saveName, CommonFunctions.PedInfo pedData, bool overrideExisting)
        {
            if (overrideExisting || (GetResourceKvpString(saveName) ?? "NULL") == "NULL")
            {
                SetResourceKvp(saveName, JsonConvert.SerializeObject(pedData));
                return GetResourceKvpString(saveName) == JsonConvert.SerializeObject(pedData);
            }
            return false;

        }

        /// <summary>
        /// Delete the specified saved item from local storage.
        /// </summary>
        /// <param name="saveName">The full name of the item to remove.</param>
        public void DeleteSavedStorageItem(string saveName)
        {
            DeleteResourceKvp(saveName);
        }

        /// <summary>
        /// New function used to save vehicle info to local storage.
        /// </summary>
        /// <param name="saveName"></param>
        /// <param name="vehicleInfo"></param>
        /// <param name="overrideOldVersion"></param>
        /// <returns></returns>
        public static bool SaveVehicleInfo(string saveName, CommonFunctions.VehicleInfo vehicleInfo, bool overrideOldVersion)
        {
            if ((GetResourceKvpString(saveName) ?? "NULL") == "NULL" || overrideOldVersion)
            {
                if ((saveName ?? "NULL") != "NULL" && saveName.Length > 4)
                {
                    // convert
                    string json = JsonConvert.SerializeObject(vehicleInfo);

                    // log
                    MainMenu.Cf.Log($"[vMenu] Saving!\nName: {saveName}\nVehicle Data: {json}\n");

                    // save
                    SetResourceKvp(saveName, json);
                    //Debug.WriteLine(GetResourceKvpString(saveName).ToString());
                    //Debug.WriteLine(saveName);

                    // confirm
                    return GetResourceKvpString(saveName) == json;
                }
            }
            // if something isn't right, then the save is aborted and return false ("failed" state).
            return false;
        }

        /// <summary>
        /// New function to get vehicle information from a saved vehicle.
        /// </summary>
        /// <param name="saveName">Saved vehicle name to get info from. (name includes "veh_")</param>
        /// <returns></returns>
        public CommonFunctions.VehicleInfo GetSavedVehicleInfo(string saveName)
        {
            string json = GetResourceKvpString(saveName);
            var vi = new CommonFunctions.VehicleInfo() { };
            dynamic data = JsonConvert.DeserializeObject(json);
            if (data.ContainsKey("version"))
            {
                //MainMenu.Cf.Log("New Version: " + data["version"] + "\n");
                var colors = new Dictionary<string, int>();
                foreach (Newtonsoft.Json.Linq.JProperty c in data["colors"])
                {
                    colors.Add(c.Name, (int)c.Value);
                }
                vi.colors = colors;
                vi.customWheels = (bool)data["customWheels"];
                var extras = new Dictionary<int, bool>();
                foreach (Newtonsoft.Json.Linq.JProperty e in data["extras"])
                {
                    extras.Add(int.Parse(e.Name), (bool)e.Value);
                }
                vi.extras = extras;
                vi.livery = (int)data["livery"];
                vi.model = (uint)data["model"];
                var mods = new Dictionary<int, int>();
                foreach (Newtonsoft.Json.Linq.JProperty m in data["mods"])
                {
                    mods.Add(int.Parse(m.Name.ToString()), (int)m.Value);
                }
                vi.mods = mods;
                vi.name = (string)data["name"];
                vi.neonBack = (bool)data["neonBack"];
                vi.neonFront = (bool)data["neonFront"];
                vi.neonLeft = (bool)data["neonLeft"];
                vi.neonRight = (bool)data["neonRight"];
                vi.plateStyle = (int)data["plateStyle"];
                vi.plateText = (string)data["plateText"];
                vi.turbo = (bool)data["turbo"];
                vi.tyreSmoke = (bool)data["tyreSmoke"];
                vi.version = (int)data["version"];
                vi.wheelType = (int)data["wheelType"];
                vi.windowTint = (int)data["windowTint"];
                vi.xenonHeadlights = (bool)data["xenonHeadlights"];
            }
            else
            {
                //MainMenu.Cf.Log("Old: " + json + "\n");
                var dict = MainMenu.Cf.JsonToDictionary(json);
                var colors = new Dictionary<string, int>()
                {
                    ["primary"] = int.Parse(dict["primaryColor"]),
                    ["secondary"] = int.Parse(dict["secondaryColor"]),
                    ["pearlescent"] = int.Parse(dict["pearlescentColor"]),
                    ["wheels"] = int.Parse(dict["wheelsColor"]),
                    ["dash"] = int.Parse(dict["dashboardColor"]),
                    ["trim"] = int.Parse(dict["interiorColor"]),
                    ["neonR"] = 255,
                    ["neonG"] = 255,
                    ["neonB"] = 255,
                    ["tyresmokeR"] = int.Parse(dict["tireSmokeR"]),
                    ["tyresmokeG"] = int.Parse(dict["tireSmokeG"]),
                    ["tyresmokeB"] = int.Parse(dict["tireSmokeB"]),
                };
                var extras = new Dictionary<int, bool>();
                for (int i = 0; i < 15; i++)
                {
                    if (dict["extra" + i] == "true")
                    {
                        extras.Add(i, true);
                    }
                    else
                    {
                        extras.Add(i, false);
                    }
                }

                var mods = new Dictionary<int, int>();
                int skip = 8 + 24 + 2 + 1;
                foreach (var mod in dict)
                {
                    skip--;
                    if (skip < 0)
                    {
                        var key = int.Parse(mod.Key);
                        var val = int.Parse(mod.Value);
                        mods.Add(key, val);
                    }
                }

                vi.colors = colors;
                vi.customWheels = dict["customWheels"] == "true";
                vi.extras = extras;
                vi.livery = int.Parse(dict["oldLivery"]);
                vi.model = (uint)Int64.Parse(dict["model"]);
                vi.mods = mods;
                vi.name = dict["name"];
                vi.neonBack = false;
                vi.neonFront = false;
                vi.neonLeft = false;
                vi.neonRight = false;
                vi.plateStyle = int.Parse(dict["plateStyle"]);
                vi.plateText = dict["plate"];
                vi.turbo = dict["turbo"] == "true";
                vi.tyreSmoke = dict["tireSmoke"] == "true";
                vi.version = 1;
                vi.wheelType = int.Parse(dict["wheelType"]);
                vi.windowTint = int.Parse(dict["windowTint"]);
                vi.xenonHeadlights = dict["xenonHeadlights"] == "true";
                SaveVehicleInfo(saveName, vi, true);
            }
            //MainMenu.Cf.Log(json + "\n");
            return vi;
        }

        /// <summary>
        /// Save json data. Returns true if save was successfull.
        /// </summary>
        /// <param name="saveName">Name to store the data under.</param>
        /// <param name="jsonData">The data to store.</param>
        /// <param name="overrideExistingData">If the saveName is already in use, can we override it?</param>
        /// <returns>Whether or not the data was saved successfully.</returns>
        public bool SaveJsonData(string saveName, string jsonData, bool overrideExistingData)
        {
            if (!string.IsNullOrEmpty(saveName) && !string.IsNullOrEmpty(jsonData))
            {
                string existingData = GetResourceKvpString(saveName); // check for existing data.

                if (!string.IsNullOrEmpty(existingData)) // data already exists for this save name.
                {
                    if (!overrideExistingData)
                    {
                        return false; // data already exists, and we are not allowed to override it.
                    }
                }

                // write data.
                SetResourceKvp(saveName, jsonData);

                // return true if the data is successfully written, otherwise return false.
                return (GetResourceKvpString(saveName) ?? "") == jsonData;
            }
            return false; // input parameters are invalid.
        }

        /// <summary>
        /// Returns the saved json data for the provided save name. Returns null if no data exists.
        /// </summary>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public string GetJsonData(string saveName)
        {
            if (!string.IsNullOrEmpty(saveName))
            {
                //Debug.WriteLine("not null");
                string data = GetResourceKvpString(saveName);
                //Debug.Write(data + "\n");
                if (!string.IsNullOrEmpty(data))
                {
                    return data;
                }
            }
            return null;
        }
    }
}
