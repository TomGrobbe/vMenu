using System.Text;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.Plugins;

/// <summary>
/// Named <c>{placeholder}</c> substitution for plugin texts, following the same rules as the
/// framework's own: <c>{{</c> and <c>}}</c> escape braces, an unknown name renders loudly and an
/// unbalanced template still renders. Reimplemented here because the framework's walker is
/// internal and resolves framework arguments rather than payload ones.
/// </summary>
internal static class PluginPlaceholders
{
    internal static string Substitute(string template, Dictionary<string, TextRef>? arguments, PluginState plugin)
    {
        if (string.IsNullOrEmpty(template) || !template.Contains('{'))
        {
            return template;
        }

        var builder = new StringBuilder(template.Length + 16);

        for (var index = 0; index < template.Length; index++)
        {
            var character = template[index];

            if (character == '}')
            {
                if (index + 1 < template.Length && template[index + 1] == '}')
                {
                    index++;
                }

                builder.Append('}');
                continue;
            }

            if (character != '{')
            {
                builder.Append(character);
                continue;
            }

            if (index + 1 < template.Length && template[index + 1] == '{')
            {
                builder.Append('{');
                index++;
                continue;
            }

            var end = template.IndexOf('}', index + 1);

            if (end < 0)
            {
                builder.Append(template, index, template.Length - index);
                break;
            }

            builder.Append(Resolve(template[(index + 1)..end], arguments, plugin));
            index = end;
        }

        return builder.ToString();
    }

    private static string Resolve(string name, Dictionary<string, TextRef>? arguments, PluginState plugin)
    {
        if (arguments is not null)
        {
            foreach (var pair in arguments)
            {
                if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    return plugin.Resolve(pair.Value);
                }
            }
        }

        Log.Warning($"[Plugins] '{plugin.Resource}' used placeholder '{{{name}}}' without supplying a value for it.");

        return "!{" + name + "}!";
    }
}
