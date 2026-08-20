using System.Collections;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Json;

// Writes language/example.json from the English table compiled into the menu framework, so the
// template a translator copies always lists every key the build actually has.
//
// Reflection over the built assembly rather than parsing EnglishStrings.cs: the keys there are Loc
// constants, so the source text alone does not say what any of them resolve to.
const string Header = """
// vMenu Enhanced language template.
//
// GENERATED. Rewritten from vMenu's own English strings on every build, so it always lists every
// key this version has. Any edit you make here is overwritten the next time vMenu is built.
//
// This file is never loaded, and renaming it will not change that. In particular, renaming it to
// en.json does NOT make it work: English is compiled into vMenu rather than read from a file, and
// the loader refuses the code 'en' outright. There is no way to override English with a file.
// The name 'example' is refused the same way, so this file cannot be loaded under its own name
// either. It exists to be copied.
//
// To add a language:
//   1. Copy this file to its language code, for example pl.json.
//   2. Set nativeName to the language's name in itself, so "Polski", not "Polish".
//   3. Translate the values on the right. Leave the keys on the left alone.
//   4. Add the code to the vMenu.Enhanced.Languages convar, for example "nl,de,pl".
//
// You do not have to translate everything. Any key you leave out falls back to English on its own,
// so a partial file is fine to ship and finish later.
//
// Comments like these are allowed anywhere in the file.
// Text in {braces} is substituted at runtime, so keep it exactly as it appears.


""";

if (args.Length is not (2 or 3))
{
    Console.Error.WriteLine("usage: LanguageTemplate <client-assembly-folder> <output-file> [coverage-file]");

    return 1;
}

var clientFolder = args[0];
var outputPath = args[1];
var coveragePath = args.Length == 3 ? args[2] : null;
var assemblyPath = Path.Combine(clientFolder, "vMenu.Enhanced.MenuFramework.dll");

if (!File.Exists(assemblyPath))
{
    Console.Error.WriteLine($"LanguageTemplate: {assemblyPath} does not exist.");

    return 1;
}

try
{
    // Its CitizenFX references sit beside it and are never invoked, only resolved.
    var context = new AssemblyLoadContext("vMenu.LanguageTemplate");

    context.Resolving += (loading, name) =>
    {
        var candidate = Path.Combine(clientFolder, name.Name + ".dll");

        return File.Exists(candidate) ? loading.LoadFromAssemblyPath(candidate) : null;
    };

    var assembly = context.LoadFromAssemblyPath(assemblyPath);
    var catalog = assembly.GetType("vMenu.Enhanced.MenuFramework.Localization.LanguageCatalog")
        ?? throw new InvalidOperationException("LanguageCatalog is missing.");

    var english = catalog.GetProperty("English")?.GetValue(null)
        ?? throw new InvalidOperationException("LanguageCatalog.English is missing.");

    var tableType = english.GetType();
    var nativeName = (string?)tableType.GetProperty("NativeName")?.GetValue(english) ?? "English";
    var keys = (IEnumerable?)tableType.GetProperty("Keys")?.GetValue(english)
        ?? throw new InvalidOperationException("LanguageTable.Keys is missing.");
    var tryGet = tableType.GetMethod("TryGet")
        ?? throw new InvalidOperationException("LanguageTable.TryGet is missing.");

    var strings = new SortedDictionary<string, string>(StringComparer.Ordinal);

    foreach (string key in keys)
    {
        var lookup = new object?[] { key, null };

        if ((bool)tryGet.Invoke(english, lookup)!)
        {
            strings[key] = (string)lookup[1]!;
        }
    }

    // camelCase to keep the field names the file has always had. It only renames properties, so the
    // localization keys inside strings, being dictionary keys, are left exactly as they are.
    var writeOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        // The least escaping available, matching ClientJson. The default encoder turns every
        // apostrophe, arrow and accent in here into an escape a translator cannot read.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    var body = JsonSerializer.Serialize(new { nativeName, strings }, writeOptions);

    var languageFolder = Path.GetDirectoryName(outputPath)!;

    Directory.CreateDirectory(languageFolder);
    File.WriteAllText(outputPath, Header + body + Environment.NewLine);

    Console.WriteLine($"LanguageTemplate: wrote {strings.Count} key(s) to {outputPath}");

    if (coveragePath is not null)
    {
        WriteCoverage(languageFolder, outputPath, strings.Keys, coveragePath);
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"LanguageTemplate: {exception.Message}");

    return 1;
}

// How much of the English table each shipped language actually covers. Written to a file as well as
// logged, so a build that skips this tool because nothing localizable changed can still echo the
// last known numbers without paying to start a process. A JSON copy goes next to it for CI, which
// wants the numbers rather than the sentence.
static void WriteCoverage(string languageFolder, string templatePath, IEnumerable<string> englishKeys, string coveragePath)
{
    var keys = englishKeys.ToArray();
    var lines = new List<string>();
    var report = new List<CoverageRow>();

    var writeOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        // The least escaping available, so a native name such as Français survives into the report.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    // Case insensitive and comment tolerant, because every one of these files opens with a comment
    // and was written with camelCase field names.
    var readOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    var files = Directory.EnumerateFiles(languageFolder, "*.json")
        .Where(file => !string.Equals(Path.GetFullPath(file), Path.GetFullPath(templatePath), StringComparison.OrdinalIgnoreCase))
        .OrderBy(Path.GetFileName, StringComparer.Ordinal);

    foreach (var file in files)
    {
        var code = Path.GetFileNameWithoutExtension(file);

        LanguageFile? read;

        try
        {
            read = JsonSerializer.Deserialize<LanguageFile>(File.ReadAllText(file), readOptions);
        }
        catch (JsonException exception)
        {
            lines.Add($"{code}: could not be read, {exception.Message}");
            report.Add(new CoverageRow { Code = code, NativeName = code, Translated = 0, Missing = keys.Length, Orphans = 0, Unreadable = true });
            continue;
        }

        var translated = read?.Strings ?? [];
        var missing = keys.Count(key => !translated.ContainsKey(key));
        var orphans = translated.Keys.Count(key => !keys.Contains(key));

        var native = string.IsNullOrWhiteSpace(read?.NativeName) ? code : read!.NativeName;
        var orphanNote = orphans > 0 ? $", {orphans} key(s) no longer in English" : string.Empty;

        lines.Add($"{code} ({native}): {keys.Length - missing}/{keys.Length} translated, {missing} missing{orphanNote}");
        report.Add(new CoverageRow { Code = code, NativeName = native, Translated = keys.Length - missing, Missing = missing, Orphans = orphans, Unreadable = false });
    }

    foreach (var line in lines)
    {
        Console.WriteLine($"LanguageTemplate: {line}");
    }

    Directory.CreateDirectory(Path.GetDirectoryName(coveragePath)!);
    File.WriteAllLines(coveragePath, lines);

    File.WriteAllText(
        Path.ChangeExtension(coveragePath, ".json"),
        JsonSerializer.Serialize(new { totalKeys = keys.Length, languages = report }, writeOptions));
}

internal sealed class LanguageFile
{
    public string NativeName { get; init; } = string.Empty;

    // Populate, or System.Text.Json leaves this get-only dictionary empty and every language reads as
    // 0% translated. See the same note on the runtime LanguageFile.
    [System.Text.Json.Serialization.JsonObjectCreationHandling(System.Text.Json.Serialization.JsonObjectCreationHandling.Populate)]
    public Dictionary<string, string> Strings { get; } = new(StringComparer.Ordinal);
}

// A concrete type rather than an anonymous one: System.Text.Json serializes a List<object> by each
// element's declared type, which for object is nothing, so anonymous rows would come out empty.
internal sealed class CoverageRow
{
    public string Code { get; init; } = string.Empty;

    public string NativeName { get; init; } = string.Empty;

    public int Translated { get; init; }

    public int Missing { get; init; }

    public int Orphans { get; init; }

    public bool Unreadable { get; init; }
}
