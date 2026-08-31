using System.Collections;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Json;

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
// To see exactly which keys a language is still missing, rather than only how many, build vMenu
// once and then run this from the repository root:
//
//   dotnet run --project tools/LanguageTemplate -- check
//   dotnet run --project tools/LanguageTemplate -- check --language pl
//
// Comments like these are allowed anywhere in the file.
// Text in {braces} is substituted at runtime, so keep it exactly as it appears.

""";

const string DefaultClientFolder = @"build\intermediate\client";
const string DefaultLanguageFolder = @"assets\enhanced\language";
const string TemplateName = "example";

if (args.Length > 0 && string.Equals(args[0], "check", StringComparison.OrdinalIgnoreCase))
{
    return Check(args[1..]);
}

if (args.Length is not (2 or 3))
{
    Usage();

    return 1;
}

var clientFolder = args[0];
var outputPath = args[1];
var coveragePath = args.Length == 3 ? args[2] : null;

try
{
    var english = EnglishStrings(clientFolder);

    var writeOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        // The least escaping available, matching ClientJson. The default encoder turns every apostrophe,
        // arrow and accent in here into an escape a translator cannot read.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    var body = JsonSerializer.Serialize(new { nativeName = "English", strings = english }, writeOptions);

    var languageFolder = Path.GetDirectoryName(outputPath)!;

    Directory.CreateDirectory(languageFolder);
    File.WriteAllText(outputPath, Header + body + Environment.NewLine);

    Console.WriteLine($"LanguageTemplate: wrote {english.Count} key(s) to {outputPath}");

    if (coveragePath is not null)
    {
        WriteCoverage(Compare(languageFolder, english.Keys), english.Count, coveragePath);
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"LanguageTemplate: {exception.Message}");

    return 1;
}

static void Usage()
{
    Console.Error.WriteLine("usage:");
    Console.Error.WriteLine("  LanguageTemplate <client-assembly-folder> <output-file> [coverage-file]");
    Console.Error.WriteLine("      Rewrites the translator's template and, given a third path, the coverage report.");
    Console.Error.WriteLine();
    Console.Error.WriteLine("  LanguageTemplate check [options]");
    Console.Error.WriteLine("      Names the keys each language is missing, and the ones English no longer has.");
    Console.Error.WriteLine("      Writes nothing.");
    Console.Error.WriteLine();
    Console.Error.WriteLine("      --language <code>      Only this language. Repeatable.");
    Console.Error.WriteLine("      --limit <n>            Name at most n key(s) per list. 0 means all, the default.");
    Console.Error.WriteLine("      --quiet                Counts only, naming no keys.");
    Console.Error.WriteLine("      --strict               Exit 1 when anything is missing, stale or unreadable.");
    Console.Error.WriteLine($"      --client <folder>      Where the client assemblies are. Default {DefaultClientFolder}.");
    Console.Error.WriteLine($"      --languages <folder>   Where the language files are. Default {DefaultLanguageFolder}.");
}

static int Check(string[] args)
{
    var clientFolder = DefaultClientFolder;
    var languageFolder = DefaultLanguageFolder;
    var wanted = new List<string>();
    var limit = 0;
    var quiet = false;
    var strict = false;

    for (var index = 0; index < args.Length; index++)
    {
        var option = args[index];

        switch (option)
        {
            case "--quiet":
                quiet = true;
                continue;

            case "--strict":
                strict = true;
                continue;
        }

        if (option is not ("--client" or "--languages" or "--language" or "--limit"))
        {
            Console.Error.WriteLine($"LanguageTemplate: {option} is not an option this tool has.");
            Usage();

            return 1;
        }

        if (index + 1 >= args.Length)
        {
            Console.Error.WriteLine($"LanguageTemplate: {option} needs a value.");

            return 1;
        }

        var value = args[++index];

        switch (option)
        {
            case "--client":
                clientFolder = value;
                break;

            case "--languages":
                languageFolder = value;
                break;

            case "--language":
                wanted.Add(value);
                break;

            case "--limit" when int.TryParse(value, out var parsed) && parsed >= 0:
                limit = parsed;
                break;

            default:
                Console.Error.WriteLine($"LanguageTemplate: {option} needs a whole number of 0 or more, not '{value}'.");

                return 1;
        }
    }

    IReadOnlyList<LanguageComparison> rows;
    int total;

    try
    {
        var english = EnglishStrings(clientFolder);

        total = english.Count;
        rows = Compare(languageFolder, english.Keys);
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"LanguageTemplate: {exception.Message}");

        return 1;
    }

    if (wanted.Count > 0)
    {
        rows = rows.Where(row => wanted.Contains(row.Code, StringComparer.OrdinalIgnoreCase)).ToArray();

        if (rows.Count == 0)
        {
            Console.Error.WriteLine($"LanguageTemplate: no language file in {languageFolder} matches {string.Join(", ", wanted)}.");

            return 1;
        }
    }

    Console.WriteLine($"{total} English key(s), {rows.Count} language file(s) in {languageFolder}");

    foreach (var row in rows)
    {
        Console.WriteLine();

        if (row.Unreadable is { } problem)
        {
            Console.WriteLine($"{row.Code}: could not be read, {problem}");

            continue;
        }

        var stale = row.Orphans.Count > 0 ? $", {row.Orphans.Count} no longer in English" : string.Empty;

        Console.WriteLine($"{row.Code} ({row.NativeName}): {total - row.Missing.Count}/{total} translated, {row.Missing.Count} missing{stale}");

        if (quiet)
        {
            continue;
        }

        Name("missing", row.Missing, limit);
        Name("no longer in English", row.Orphans, limit);
    }

    var behind = rows.Count(row => row.Unreadable is not null || row.Missing.Count > 0 || row.Orphans.Count > 0);

    Console.WriteLine();
    Console.WriteLine(behind == 0
        ? $"Every language covers all {total} key(s)."
        : $"{behind} of {rows.Count} language(s) are behind. Missing keys fall back to English, so nothing is broken by a gap.");

    return strict && behind > 0 ? 1 : 0;

    static void Name(string label, IReadOnlyList<string> keys, int limit)
    {
        if (keys.Count == 0)
        {
            return;
        }

        var shown = limit == 0 ? keys.Count : Math.Min(limit, keys.Count);

        Console.WriteLine($"  {label} ({keys.Count}):");

        for (var index = 0; index < shown; index++)
        {
            Console.WriteLine($"    {keys[index]}");
        }

        if (shown < keys.Count)
        {
            Console.WriteLine($"    ... and {keys.Count - shown} more");
        }
    }
}

// Reflection over the built assembly rather than parsing EnglishStrings.cs: the keys there are Loc
// constants, so the source text alone does not say what any of them resolve to.
static SortedDictionary<string, string> EnglishStrings(string clientFolder)
{
    clientFolder = Path.GetFullPath(clientFolder);

    var assemblyPath = Path.Combine(clientFolder, "vMenu.Enhanced.MenuFramework.dll");

    if (!File.Exists(assemblyPath))
    {
        throw new FileNotFoundException($"{assemblyPath} does not exist. Build vMenu first, or pass --client.");
    }

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

    return strings;
}

static IReadOnlyList<LanguageComparison> Compare(string languageFolder, IEnumerable<string> englishKeys)
{
    var keys = englishKeys.ToArray();
    var known = new HashSet<string>(keys, StringComparer.Ordinal);
    var rows = new List<LanguageComparison>();

    // Case insensitive and comment tolerant, because every one of these files opens with a comment and
    // was written with camelCase field names.
    var readOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    if (!Directory.Exists(languageFolder))
    {
        throw new DirectoryNotFoundException($"{languageFolder} does not exist. Pass --languages.");
    }

    var files = Directory.EnumerateFiles(languageFolder, "*.json")
        .Where(file => !string.Equals(Path.GetFileNameWithoutExtension(file), TemplateName, StringComparison.OrdinalIgnoreCase))
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
            rows.Add(new LanguageComparison(code, code, [], [], exception.Message));

            continue;
        }

        var translated = read?.Strings ?? [];
        var missing = keys.Where(key => !translated.ContainsKey(key)).ToArray();
        var orphans = translated.Keys.Where(key => !known.Contains(key)).Order(StringComparer.Ordinal).ToArray();
        var native = string.IsNullOrWhiteSpace(read?.NativeName) ? code : read!.NativeName;

        rows.Add(new LanguageComparison(code, native, missing, orphans, null));
    }

    return rows;
}

// Written to a file as well as logged, so a build that skips this tool because nothing localizable
// changed can still echo the last known numbers without paying to start a process. A JSON copy goes
// next to it for CI, which wants the numbers and the key names rather than the sentence.
static void WriteCoverage(IReadOnlyList<LanguageComparison> rows, int total, string coveragePath)
{
    var lines = new List<string>();
    var report = new List<CoverageRow>();

    var writeOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        // The least escaping available, so a native name with accents in it survives into the report.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    foreach (var row in rows)
    {
        if (row.Unreadable is { } problem)
        {
            lines.Add($"{row.Code}: could not be read, {problem}");

            report.Add(new CoverageRow
            {
                Code = row.Code,
                NativeName = row.Code,
                Translated = 0,
                Missing = total,
                Orphans = 0,
                Unreadable = true,
            });

            continue;
        }

        var orphanNote = row.Orphans.Count > 0 ? $", {row.Orphans.Count} key(s) no longer in English" : string.Empty;

        lines.Add($"{row.Code} ({row.NativeName}): {total - row.Missing.Count}/{total} translated, {row.Missing.Count} missing{orphanNote}");

        report.Add(new CoverageRow
        {
            Code = row.Code,
            NativeName = row.NativeName,
            Translated = total - row.Missing.Count,
            Missing = row.Missing.Count,
            Orphans = row.Orphans.Count,
            Unreadable = false,
            MissingKeys = row.Missing,
            OrphanKeys = row.Orphans,
        });
    }

    foreach (var line in lines)
    {
        Console.WriteLine($"LanguageTemplate: {line}");
    }

    Directory.CreateDirectory(Path.GetDirectoryName(coveragePath)!);
    File.WriteAllLines(coveragePath, lines);

    File.WriteAllText(
        Path.ChangeExtension(coveragePath, ".json"),
        JsonSerializer.Serialize(new { totalKeys = total, languages = report }, writeOptions));
}

internal sealed record LanguageComparison(
    string Code,
    string NativeName,
    IReadOnlyList<string> Missing,
    IReadOnlyList<string> Orphans,
    string? Unreadable);

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

    public IReadOnlyList<string> MissingKeys { get; init; } = [];

    public IReadOnlyList<string> OrphanKeys { get; init; } = [];
}
