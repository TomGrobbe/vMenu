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

        public static void SetPermission(string permissionName, bool allowed)
        {
            if (allowed)
            {
                Permissions.Add(permissionName);
            }
        }

        public static bool IsAllowed(Permission permission)
        {
            if (Permissions.Contains("Everything"))
            {
                MainMenu.Cf.Log("Everything allowed, breaking.");
                return true;
            }
            else
            {
                var allowed = false;
                if (Permissions.Contains(permission.ToString().Substring(0, 2) + "All"))
                {
                    allowed = true;
                }
                if (!allowed)
                {
                    allowed = Permissions.Contains(permission.ToString());
                }
                return allowed;
            }
        }
    }
}
