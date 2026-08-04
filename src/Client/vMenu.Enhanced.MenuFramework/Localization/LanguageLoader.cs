using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Serialization;

using LocalizationSetting = vMenu.Enhanced.Data.Configuration.Settings.Localization;

namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>Loads the languages named by the convar from <c>language/&lt;code&gt;.json</c>.</summary>
public static class LanguageLoader
{
    private const string Folder = "language";

    /// <summary>The generated template, which is a file to copy rather than a language to load.</summary>
    private const string Template = "example";

    /// <summary>
    /// Reads the convar and registers every language it names. English is not loaded from a file and
    /// is registered by <see cref="LanguageCatalog"/> itself.
    /// </summary>
    // Call after ClientConfig.Initialize and before the menus are built, since the picker's options
    // are fixed once its item exists.
    public static void Load()
    {
        var resource = Native.GetCurrentResourceName();

        foreach (var code in Codes())
        {
            if (Read(resource, code) is { } table)
            {
                LanguageCatalog.Register(table);
            }
        }
    }

    private static List<string> Codes()
    {
        var configured = ClientConfig.Value(LocalizationSetting.Languages);
        var codes = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var part in configured.Split(','))
        {
            var code = part.Trim().ToLowerInvariant();

            if (code.Length == 0)
            {
                continue;
            }

            if (string.Equals(code, LanguageId.English.Code, StringComparison.Ordinal))
            {
                API.Log.Warn(
                    $"[i18n] '{LocalizationSetting.Languages.Name}' lists 'en', which is being "
                    + "skipped. English is built into vMenu and is always available, so it does not "
                    + "come from a file and cannot be overridden by one. Renaming a file to "
                    + "en.json does not work either.");

                continue;
            }

            if (string.Equals(code, Template, StringComparison.Ordinal))
            {
                API.Log.Warn(
                    $"[i18n] '{LocalizationSetting.Languages.Name}' lists '{Template}', which is "
                    + $"being skipped. {Folder}/{Template}.json is a generated template to copy, "
                    + "and it is overwritten on every build.");

                continue;
            }

            if (!seen.Add(code))
            {
                API.Log.Warn($"[i18n] '{LocalizationSetting.Languages.Name}' lists '{code}' more than once.");

                continue;
            }

            codes.Add(code);
        }

        return codes;
    }

    private static LanguageTable? Read(string resource, string code)
    {
        var path = $"{Folder}/{code}.json";
        var raw = Native.LoadResourceFile(resource, path);

        if (string.IsNullOrWhiteSpace(raw))
        {
            API.Log.Error($"[i18n] '{code}' is listed but {path} is missing or empty, so it is not available.");

            return null;
        }

        if (!ClientJson.TryDeserialize<LanguageFile>(raw, out var file) || file is null)
        {
            API.Log.Error($"[i18n] {path} is not readable as JSON, so '{code}' is not available.");

            return null;
        }

        if (file.Strings.Count == 0)
        {
            API.Log.Error($"[i18n] {path} has no strings, so '{code}' is not available.");

            return null;
        }

        // Falling back to the code keeps the picker usable rather than showing it a blank row.
        var nativeName = string.IsNullOrWhiteSpace(file.NativeName) ? code : file.NativeName;

        return new LanguageTable(LanguageId.FromCode(code), nativeName, file.Strings);
    }
}
