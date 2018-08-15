using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace vMenuClient
{

    public static class PermissionsManager
    {
        public static List<string> Permissions { get; private set; } = new List<string>() { };

        private static bool allowEverything = false;

        public static void SetPermission(string permissionName, bool allowed)
        {
            if (allowed)
            {
                if (permissionName == "Everything")
                {
                    allowEverything = true;
                }
                Permissions.Add(permissionName);
            }
        }

        public static bool IsAllowed(Permission permission)
        {

            //if (Permissions.Contains("Everything"))
            if (allowEverything)
            {
                //MainMenu.Cf.Log($"Everything is allowed, no need to check for \"{permission.ToString()}\" specifically.");
                return true;
            }
            else
            {
                //var allowed = false;
                if (Permissions.Contains(permission.ToString().Substring(0, 2) + "All"))
                {
                    //MainMenu.Cf.Log(".All was allowed.");
                    //allowed = true;
                    return true;
                }
                //if (!allowed)
                //{
                //allowed = Permissions.Contains(permission.ToString());
                //}
                //return allowed;
                return Permissions.Contains(permission.ToString());
            }
        }
    }
}
