using System.Text;

namespace vMenu.Enhanced.MenuFramework.Localization;

// Named {placeholder} substitution. Named rather than positional, so a translator editing another
// language sees what the slot holds. Reordering, repeating and omitting one are free consequences.
internal static class Placeholders
{
    internal static string Substitute(string template, (string Name, MenuText Value)[]? arguments, ILocalizer localizer)
    {
        // The overwhelming majority of strings have no placeholders at all.
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
                // "}}" is an escaped brace. A lone '}' is passed through rather than treated as an error, so an
                // unbalanced string still renders.
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
                // Unterminated placeholder: emit the remainder verbatim so the row still renders.
                builder.Append(template, index, template.Length - index);
                break;
            }

            builder.Append(Resolve(template[(index + 1)..end], template, arguments, localizer));
            index = end;
        }

        return builder.ToString();
    }

    private static string Resolve(string name, string template, (string Name, MenuText Value)[]? arguments, ILocalizer localizer)
    {
        if (arguments is not null)
        {
            foreach (var (Name, Value) in arguments)
            {
                if (string.Equals(Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return Value.Resolve(localizer);
                }
            }
        }

        LocalizationLog.UnknownPlaceholder(name, template);

        // Same policy as a missing key: loud in game, but the row still draws.
        return $"!{{{name}}}!";
    }
}
