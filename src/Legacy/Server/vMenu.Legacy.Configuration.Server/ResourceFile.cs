using System.Diagnostics.CodeAnalysis;
using System.Text;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

using vMenu.Enhanced.BrokenNatives.Server;

namespace vMenu.Enhanced.Configuration.Server;

/// <summary>A config file in the resource folder that a server owner edits by hand. Every outcome is logged with
/// what it means for the server, so an owner reading the console knows why something is empty.</summary>
public static class ResourceFile
{
    public static string? Read(string file, string tag, string fallback)
    {
        var contents = API.LoadResourceFile(API.GetCurrentResourceName(), file);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Info($"[{tag}] No {file} found, so {fallback}.");

            return null;
        }

        return contents;
    }

    public static bool TryLoad<T>(
        string file,
        string tag,
        string fallback,
        string shape,
        [NotNullWhen(true)] out T? value)
        where T : class
    {
        value = null;

        if (Read(file, tag, fallback) is not { } contents)
        {
            return false;
        }

        if (!ServerJson.TryDeserialize<T>(contents, out var read, out var error))
        {
            Log.Error($"[{tag}] {file} could not be parsed, so {fallback}: {error}");

            return false;
        }

        if (read is null)
        {
            Log.Error($"[{tag}] {file} has to hold {shape}, so {fallback}.");

            return false;
        }

        value = read;

        return true;
    }

    /// <summary>Serializes inside the guard, and answers any throw as a failed write rather than leaving it to the
    /// caller, so the caller still undoes what it added. One that escaped would leave a list holding something
    /// that is not on disk.</summary>
    public static bool TryWrite(string file, string tag, object value, string fallback)
    {
        try
        {
            if (NativeFixer.SaveResourceFile(API.GetCurrentResourceName(), file, Encoding.UTF8.GetBytes(ServerJson.SerializeIndented(value))))
            {
                return true;
            }

            Log.Error($"[{tag}] {file} could not be written, so {fallback}.");
        }
        catch (Exception exception)
        {
            Log.Error($"[{tag}] Writing {file} threw, so {fallback}: {exception}");
        }

        return false;
    }
}
