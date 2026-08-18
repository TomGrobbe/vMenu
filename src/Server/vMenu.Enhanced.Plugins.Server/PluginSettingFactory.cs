using System.Globalization;

using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.Plugins.Server;

/// <summary>Turns declared <see cref="SettingNode"/>s into vMenu <see cref="Setting"/> objects.</summary>
public static class PluginSettingFactory
{
    /// <param name="fullName">The composed convar name, already validated.</param>
    /// <param name="problem">Why the node was unusable, or <see langword="null"/> when it converted.</param>
    public static Setting? Create(SettingNode node, string fullName, out string? problem)
    {
        var description = string.IsNullOrWhiteSpace(node.Description)
            ? "No description was provided by the plugin."
            : node.Description;

        switch (node.Type)
        {
            case SettingTypes.Bool:
                if (!TryParseBool(node.Default, out var boolDefault))
                {
                    problem = BadDefault(node, "true or false");
                    return null;
                }

                problem = null;
                return new BoolSetting(fullName) { Description = description, Default = boolDefault };

            case SettingTypes.Int:
                if (!int.TryParse(node.Default, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intDefault))
                {
                    problem = BadDefault(node, "a whole number");
                    return null;
                }

                problem = null;
                return new IntSetting(fullName) { Description = description, Default = intDefault };

            case SettingTypes.Float:
                if (!float.TryParse(node.Default, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatDefault))
                {
                    problem = BadDefault(node, "a number");
                    return null;
                }

                problem = null;
                return new FloatSetting(fullName) { Description = description, Default = floatDefault };

            case SettingTypes.String:
                problem = null;
                return new StringSetting(fullName) { Description = description, Default = node.Default };

            default:
                problem = $"Setting '{node.Name}' has unknown type '{node.Type}'.";
                return null;
        }
    }

    private static bool TryParseBool(string text, out bool value)
    {
        if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(text))
        {
            value = false;
            return true;
        }

        value = false;
        return false;
    }

    private static string BadDefault(SettingNode node, string expected) =>
        $"Setting '{node.Name}' has default '{node.Default}' where {expected} was expected.";
}
