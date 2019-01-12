using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public static class IplManager
    {
        /// <summary>
        /// Checks if the dependency for the ipl menu is present and enabled via the config option in the __resource.lua of the ipl resource.
        /// </summary>
        /// <returns></returns>
        internal static bool IsDependencyPresentAndEnabled()
        {
            var resCount = GetNumResources();
            bool found = false;
            for (int i = 0; i < resCount; i++)
            {
                // Get the resource name.
                string name = GetResourceByFindIndex(i);

                // resource name is invalid or missing, skip this resource.
                if (string.IsNullOrEmpty(name))
                    continue;

                // resource is not started, skip this resource.
                if (GetResourceState(name) != "started")
                    continue;

                // we don't want to check for vMenu itself.
                if (GetCurrentResourceName() == name)
                    continue;

                // Get the metadata to see if this is bob's IPL resource and if it's integration is enabled or disabled.
                string metadata = GetResourceMetadata(name, "enable_vmenu_bob74_ipl_integration", 0);

                // invalid or no meta data present.
                if (string.IsNullOrEmpty(metadata))
                    continue;

                // found and enabled.
                if (metadata.ToLower() == "true")
                {
                    found = true;
                    break;
                }
                // found, but not enabled.
                else if (metadata.ToLower() == "false")
                {
                    break;
                }
            }

            // If it's not found or not enabled by the IPL's config, then return false.
            if (!found)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the ipl integration with bob74's ipl resource is enabled.
        /// </summary>
        /// <returns></returns>
        internal static bool IsIplIntegrationEnabled()
        {
            // Check if the dependency is present and enabled.
            if (IsDependencyPresentAndEnabled())
            {
                // Check if the setting in vMenu is enabled.
                if (GetSettingsBool(Setting.vmenu_enable_bob74_ipl_integration))
                {
                    return true;
                }
            }
            return false;
        }



    }
}
