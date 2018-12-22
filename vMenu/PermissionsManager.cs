using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;

namespace vMenuClient
{

    public static class PermissionsManager
    {
        public static List<string> Permissions { get; private set; } = new List<string>() { };

        private static bool allowEverything = false;

        /// <summary>
        /// Sets the permission locally.
        /// </summary>
        /// <param name="permissionName"></param>
        /// <param name="allowed"></param>
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

        /// <summary>
        /// Returns true if the specific permission, or a parent permission, is allowed.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static bool IsAllowed(Permission permission)
        {
            if (vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_use_permissions))
            {
                if (allowEverything)
                {
                    return true;
                }
                else
                {
                    //var allowed = false;
                    if (Permissions.Contains(permission.ToString().Substring(0, 2) + "All"))
                    {
                        return true;
                    }
                    return Permissions.Contains(permission.ToString());
                }
            }
            else // if permissions are not used...
            {   // then check for .everything and some specific admin stuff and disable that, but for everything else return true (allowed)
                if (permission == Permission.Everything || permission == Permission.OPAll || permission == Permission.OPKick || permission == Permission.OPKill || permission == Permission.OPPermBan || permission == Permission.OPTempBan || permission == Permission.OPUnban || permission == Permission.OPIdentifiers || permission == Permission.OPViewBannedPlayers)
                {
                    return false;
                }
                return true;
            }

        }
    }
}
