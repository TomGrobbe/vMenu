using System.Text;

namespace vMenu.Enhanced.Webhooks.Server;

public static class WebhookText
{
    private const string Ellipsis = "...";

    private const string Markdown = "*_~`|";

    public static string Clean(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var text = new StringBuilder(value.Length);
        var lastWasSpace = false;

        foreach (var character in value)
        {
            if (char.IsControl(character) || character == ' ')
            {
                if (!lastWasSpace && text.Length > 0)
                {
                    text.Append(' ');
                    lastWasSpace = true;
                }

                continue;
            }

            lastWasSpace = false;

            if (Markdown.IndexOf(character) >= 0)
            {
                text.Append('\\');
            }

            text.Append(character);
        }

        return Truncate(text.ToString().TrimEnd(), maxLength);
    }

    public static string Truncate(string value, int maxLength)
    {
        if (maxLength <= 0 || value.Length <= maxLength)
        {
            return value;
        }

        return maxLength <= Ellipsis.Length
            ? value[..maxLength]
            : value[..(maxLength - Ellipsis.Length)] + Ellipsis;
    }
}
