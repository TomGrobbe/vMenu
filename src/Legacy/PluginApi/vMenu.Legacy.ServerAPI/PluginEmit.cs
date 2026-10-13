using CitizenFX.Core;

namespace vMenu.Enhanced.ServerAPI;

internal static class PluginEmit
{
    internal static void Local(string eventName, params object?[] args) => BaseScript.TriggerEvent(eventName, args);
}
