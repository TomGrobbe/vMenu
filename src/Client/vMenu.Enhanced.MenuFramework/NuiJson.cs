using System.Globalization;
using System.Text;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// The little bit of JSON the NUI prompt needs, written and read by hand because
/// <c>System.Text.Json</c> does not load in the FiveM client sandbox.
/// </summary>
internal static class NuiJson
{
    internal static StringBuilder AppendString(this StringBuilder builder, string text)
    {
        builder.Append('"');

        foreach (var character in text)
        {
            switch (character)
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (character < ' ')
                    {
                        builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        return builder.Append('"');
    }

    /// <summary>A raw NUI callback hands the posted body over still quoted and escaped.</summary>
    internal static string Unquote(string raw)
    {
        if (raw.Length < 2 || raw[0] != '"' || raw[^1] != '"')
        {
            return raw;
        }

        var builder = new StringBuilder(raw.Length - 2);

        for (var index = 1; index < raw.Length - 1; index++)
        {
            var character = raw[index];

            if (character != '\\' || index + 1 >= raw.Length - 1)
            {
                builder.Append(character);
                continue;
            }

            var escaped = raw[++index];

            switch (escaped)
            {
                case 'n':
                    builder.Append('\n');
                    break;
                case 'r':
                    builder.Append('\r');
                    break;
                case 't':
                    builder.Append('\t');
                    break;
                case 'b':
                    builder.Append('\b');
                    break;
                case 'f':
                    builder.Append('\f');
                    break;
                // Substring rather than AsSpan: the sandbox refuses MemoryExtensions, so the span
                // overload of TryParse throws a SecurityException on the first escape it parses.
                case 'u' when index + 4 < raw.Length - 1
                    && ushort.TryParse(raw.Substring(index + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code):
                    builder.Append((char)code);
                    index += 4;
                    break;
                default:
                    builder.Append(escaped);
                    break;
            }
        }

        return builder.ToString();
    }
}
