using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

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
                string jsonString = MainMenu.Cf.DictionaryToJson(data);

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
            string json = GetResourceKvpString(name);
            var dict = MainMenu.Cf.JsonToDictionary(json);
            return dict ?? new Dictionary<string, string>();
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
