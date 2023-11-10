using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json;

namespace vMenu.Server.Functions
{
    internal class Json : BaseScript
    {
        #region Stringify
        public static string Stringify(object data)
        {
            bool flag = data == null;
            string result;

            if (flag)
            {
                result = null;
            }
            else
            {
                string text = null;

                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    text = JsonConvert.SerializeObject(data, settings);
                }
                catch (Exception ex)
                {
                    text = null;
                    Debug.WriteLine($"[Error] {ex}");
                }

                result = text;
            }

            return result;
        }
        #endregion
    }
}
