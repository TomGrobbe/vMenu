using System.Text;

using vMenu.Enhanced.Data;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;

const string DefaultConfigFolder = @"assets\enhanced\config";

var ready = args.Contains("--ready", StringComparer.OrdinalIgnoreCase);
var positional = args.Where(static argument => !argument.StartsWith("--", StringComparison.Ordinal)).ToArray();

if (positional.Length is not (1 or 2))
{
    Console.Error.WriteLine("usage: ConfigDefaults <output-folder> [config-folder] [--ready]");
    Console.Error.WriteLine("    Writes permissions.cfg.example and configuration.cfg.example, exactly as a clean server would.");
    Console.Error.WriteLine("    --ready writes permissions.cfg and configuration.cfg instead, for handing out ready to use.");
    Console.Error.WriteLine($"    config-folder holds the shipped JSON files. Default {DefaultConfigFolder}.");

    return 1;
}

var outputFolder = positional[0];
var configFolder = positional.Length == 2 ? positional[1] : DefaultConfigFolder;

try
{
    PermissionRegistry.Build(typeof(Global).Assembly);
    ModelWhitelist.Load(Read(ModelWhitelist.ConfigFile));
    VehicleCategories.Load(Read(VehicleCategories.ConfigFile));
    PedCategories.Load(Read(PedCategories.ConfigFile));
    WeaponCatalog.Load(Read(WeaponCatalog.ConfigFile));

    var suffix = ready ? string.Empty : ExampleFile.Extension;

    Directory.CreateDirectory(outputFolder);

    Write(PermissionsExample.CopyName + suffix, PermissionsExampleFile.Render(ready));
    Write(ConfigurationExample.CopyName + suffix, ConfigurationExample.Render(ready));

    Console.WriteLine($"ConfigDefaults: wrote {PermissionRegistry.Count} permission(s) and every setting to {outputFolder}");

    return Log.Errors > 0 ? 1 : 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"ConfigDefaults: {exception.Message}");

    return 1;
}

string? Read(string resourcePath)
{
    var path = Path.Combine(configFolder, Path.GetFileName(resourcePath));

    return File.Exists(path) ? File.ReadAllText(path) : null;
}

void Write(string name, string contents) =>
    File.WriteAllText(Path.Combine(outputFolder, name), contents, new UTF8Encoding(false));
