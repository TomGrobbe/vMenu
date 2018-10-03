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
            if (vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.SettingsCategory.permissions, vMenuShared.ConfigManager.Setting.use_permissions))
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
            else // if permissions are not used...
            {   // then check for .everything and some specific admin stuff and disable that, but for everything else return true (allowed)
                if (permission == Permission.Everything || permission == Permission.OPAll || permission == Permission.OPKick || permission == Permission.OPKill || permission == Permission.OPPermBan || permission == Permission.OPTempBan || permission == Permission.OPUnban)
                {
                    return false;
                }
                return true;
            }

        }
    }
}
