using System;

using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using static CitizenFX.Core.Native.API;
using CitizenFX.Core.Native;

namespace vMenuShared
{
    public static class SupplementaryPermissionManager
    {
        public static List<string> Permission = new()
        {
            "VWAll",
            "PWAll",
            "WWAll",
        };

        public static Dictionary<string, bool> Permissions { get; private set; } = new Dictionary<string, bool>();
        public static bool ArePermissionsSetup { get; set; } = false;


#if SERVER
        /// <summary>
        /// Public function to check if a permission is allowed.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsAllowed(string permission, Player source) => IsAllowedServer(permission, source);

        /// <summary>
        /// Public function to check if a permission is allowed.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="playerHandle"></param>
        /// <returns></returns>
        public static bool IsAllowed(string permission, string playerHandle) => IsAllowedServer(permission, playerHandle);
#endif

#if CLIENT
        /// <summary>
        /// Public function to check if a permission is allowed.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="checkAnyway">if true, then the permissions will be checked even if they aren't setup yet.</param>
        /// <returns></returns>
        public static bool IsAllowed(string permission, bool checkAnyway = false) => IsAllowedClient(permission, checkAnyway);

        private static readonly Dictionary<string, bool> allowedPerms = new();
        /// <summary>
        /// Private function that handles client side permission requests.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        private static bool IsAllowedClient(string permission, bool checkAnyway)
        {
            if (ArePermissionsSetup || checkAnyway)
            {
                if (allowedPerms.ContainsKey(permission) && allowedPerms[permission])
                {
                    return true;
                }
                else if (!allowedPerms.ContainsKey(permission))
                {
                    allowedPerms[permission] = false;
                }

                // Get a list of all permissions that are (parents) of the current permission, including the current permission.
                var permissionsToCheck = GetPermissionAndParentPermissions(permission);

                // Check if any of those permissions is allowed, if so, return true.
                if (permissionsToCheck.Any(p => Permissions.ContainsKey(p) && Permissions[p]))
                {
                    allowedPerms[permission] = true;
                    return true;
                }
            }
            switch (permission.Substring(0, 2))
            {
                case "VW":
                    allowedPerms[permission] = PermissionsManager.IsAllowed(PermissionsManager.Permission.VSAll);
                    return PermissionsManager.IsAllowed(PermissionsManager.Permission.VSAll);
                case "PW":
                    allowedPerms[permission] = PermissionsManager.IsAllowed(PermissionsManager.Permission.PAAll);
                    return PermissionsManager.IsAllowed(PermissionsManager.Permission.PAAll);
                case "WW":
                    allowedPerms[permission] = PermissionsManager.IsAllowed(PermissionsManager.Permission.WPAll);
                    return PermissionsManager.IsAllowed(PermissionsManager.Permission.WPAll);
            }
            return false;
        }
#endif
#if SERVER
        /// <summary>
        /// Checks if the player is allowed that specific permission.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private static bool IsAllowedServer(string permission, Player source)
        {
            if (source == null)
            {
                return false;
            }

            return IsAllowedServer(permission, source.Handle);
        }

        /// <summary>
        /// Checks if the player is allowed that specific permission.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="playerHandle"></param>
        /// <returns></returns>
        private static bool IsAllowedServer(string permission, string playerHandle)
        {
            if (!DoesPlayerExist(playerHandle))
            {
                return false;
            }

            return GetPermissionAndParentPermissions(permission).Any(p => IsPlayerAceAllowed(playerHandle, GetAceName(p)));
        }
#endif

        private static readonly Dictionary<string, List<string>> parentPermissions = new();

        /// <summary>
        /// Gets the current permission and all parent permissions.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static List<string> GetPermissionAndParentPermissions(string permission)
        {
            if (parentPermissions.ContainsKey(permission))
            {
                return parentPermissions[permission];
            }
            else
            {
                var list = new List<string>() { "Everything", permission };

                // if the first 2 characters are both uppercase
                if (permission.Substring(0, 2).ToUpper() == permission.Substring(0, 2))
                {
                    if (permission.Substring(2) is not "All")
                    {
                        list.AddRange(Permission.Where(a => a.ToString() == permission.Substring(0, 2) + "All"));
                    }
                }
                //else // it's one of the .Everything, .DontKickMe, DontBanMe, NoClip, Staff, etc perms that are not menu specific.
                //{
                //    // do nothing
                //}
                parentPermissions[permission] = list;
                return list;
            }
        }

#if SERVER


        /// <summary>
        /// Sets the permissions for a specific player (checks server side, sends event to client side).
        /// </summary>
        /// <param name="player"></param>
        public static void SetPermissionsForPlayer([FromSource] Player player)
        {
            if (player == null)
            {
                return;
            }

            var perms = new Dictionary<string, bool>();

            // Loop through all permissions and check if they're allowed.
            foreach (string permission in Permission)
            {
                if (!perms.ContainsKey(permission))
                {
                    perms.Add(permission, IsAllowed(permission, player)); // triggers IsAllowedServer
                }
            }
            // Send the permissions to the client.
            player.TriggerEvent("vMenu:SetSupplementaryPermissions", Newtonsoft.Json.JsonConvert.SerializeObject(perms));
        }
#endif
#if CLIENT
        /// <summary>
        /// Sets the permission (client side event handler).
        /// </summary>
        /// <param name="permissions"></param>
        public static void SetPermissions(string permissions)
        {
            Permissions = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, bool>>(permissions);
            
            // if debug logging.
            if (GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true")
            {
                Debug.WriteLine("[vMenu] [Permissions] " + Newtonsoft.Json.JsonConvert.SerializeObject(Permissions, Newtonsoft.Json.Formatting.None));
            }
        }
#endif
#if SERVER
        /// <summary>
        /// Gets the full permission ace name for the specific <see cref="Permission"/> enum.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static string GetAceName(string permission)
        {
            var name = permission.ToString();

            var prefix = "vMenu.";

            switch (name.Substring(0, 2))
            {
                case "VW":
                    prefix += "VehicleSpawner.WhitelistedModels";
                    break;
                case "PW":
                    prefix += "PlayerAppearance.WhitelistedModels";
                    break;
                case "WW":
                    prefix += "WeaponOptions.WhitelistedModels";
                    break;
                default:
                    return prefix + name;
            }

            return prefix + "." + name.Substring(2);
        }
#endif
    }
}
