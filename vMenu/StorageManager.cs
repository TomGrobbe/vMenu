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
        /// Save a dictionary to the client's storage.
        /// </summary>
        /// <param name="saveName">The key used to save this dictionary.</param>
        /// <param name="data">The dictionary to save.</param>
        /// <param name="overrideExistingData">If the key/dictionary already exists, do you want to override it?</param>
        /// <returns>True when the save was successful, false if it was not.</returns>
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
        /// Get a saved dictionary.
        /// </summary>
        /// <param name="name">The key for the dictionary to get.</param>
        /// <returns>The requested dictionary.</returns>
        public Dictionary<string, string> GetSavedDictionary(string name)
        {
            //MainMenu.Cf.Log("Name: " + name);
            //if (name.Length < 5)
            //{
            //    MainMenu.Cf.Log("Invalid save name: name too short.");
            //    return new Dictionary<string, string>();
            //}
            string json;
            //if (name.Substring(name.Length - 3).Contains("_v2"))
            //{
            json = GetResourceKvpString(name);
            MainMenu.Cf.Log("Existing v2 save is being loaded: Name: " + name + " Dict: " + json.ToString());
            var dict = MainMenu.Cf.JsonToDictionary(json);
            return dict ?? new Dictionary<string, string>();
            //}
            //else if (GetResourceKvpString(name + "_v2") != null)
            //{
            //    json = GetResourceKvpString(name + "_v2");
            //    var dict = MainMenu.Cf.JsonToDictionary(json, true);
            //    MainMenu.Cf.Log("Existing v2 save is being loaded (loading v2, but original name provided was v1): Name: " + name + " Dict: " + json.ToString());
            //    return dict ?? new Dictionary<string, string>();
            //}
            //else
            //{
            //    json = GetResourceKvpString(name);
            //    MainMenu.Cf.Log("Existing v1 save is being loaded: Name: " + name + " Dict: " + json.ToString());
            //    var dict = MainMenu.Cf.JsonToDictionary(json);
            //    MainMenu.Cf.Log("Attempting conversion to v2.");
            //    if (!SaveDictionary(name, dict, true))
            //    {
            //        MainMenu.Cf.Log("Save to v2 was not successfull.");
            //    }
            //    else
            //    {
            //        MainMenu.Cf.Log("Save to v2 was successfull.");
            //    }
            //    return GetSavedDictionary(name + "_v2");
            //}
        }

        /// <summary>
        /// Delete the specified dictionary from local storage.
        /// </summary>
        /// <param name="saveName"></param>
        public void DeleteSavedDictionary(string saveName)
        {
            DeleteResourceKvp(saveName);
        }



    }
}
