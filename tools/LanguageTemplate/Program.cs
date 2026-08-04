using System.Collections;
using System.Runtime.Loader;

using Newtonsoft.Json;

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

if (args.Length != 2)
{
    Console.Error.WriteLine("usage: LanguageTemplate <client-assembly-folder> <output-file>");

    return 1;
}

var clientFolder = args[0];
var outputPath = args[1];
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

    var body = JsonConvert.SerializeObject(
        new { nativeName, strings },
        Formatting.Indented);

    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    File.WriteAllText(outputPath, Header + body + Environment.NewLine);

    Console.WriteLine($"LanguageTemplate: wrote {strings.Count} key(s) to {outputPath}");

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"LanguageTemplate: {exception.Message}");

    return 1;
}
