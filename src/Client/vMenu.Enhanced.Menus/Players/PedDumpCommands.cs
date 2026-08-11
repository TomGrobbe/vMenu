using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Players.Saved;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>
/// Console commands that report on the ped the player is wearing.
/// </summary>
/// <remarks>
/// Both of these re-read the ped through <see cref="PedAppearanceReader"/>, which asks the game
/// rather than remembering what vMenu set. That is the point of them: a report built from vMenu's
/// own memory would agree with itself no matter what the game actually did.
/// </remarks>
public static class PedDumpCommands
{
    private const string DumpCommand = "vmenu_ped";

    private const string DiffCommand = "vmenu_ped_diff";

    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(DumpCommand, false, DebugCommands.Gate(Dump));
        SharedAPI.Commands.RegisterCommand(DiffCommand, false, DebugCommands.Gate<string?>(Diff));
    }

    private static void Dump()
    {
        var ped = Native.PlayerPedId();
        var appearance = PedAppearanceReader.Read(ped);

        API.Log.Info("[Ped] Live state, read from the game:");

        foreach (var line in PedAppearanceReport.Describe(appearance, ped))
        {
            API.Log.Info("[Ped] " + line);
        }

        API.Log.Info("[Ped] As stored:");
        API.Log.Info(ClientJson.SerializeIndented(appearance));
    }

    /// <summary>
    /// Says how the ped being worn differs from a saved one, which is what proves a restore was
    /// faithful rather than merely plausible.
    /// </summary>
    private static void Diff(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            API.Log.Info($"[Ped] Usage: {DiffCommand} <saved ped name>");

            return;
        }

        if (SavedPedStore.Load(name.Trim()) is not { } entry)
        {
            API.Log.Info($"[Ped] There is no saved ped called '{name}'.");

            return;
        }

        if (entry.IsFromNewerBuild)
        {
            API.Log.Warn(
                $"[Ped] '{entry.Ped.Name}' was saved by a newer version of vMenu (version "
                + $"{entry.StoredVersion}, this build understands {SavedPed.SchemaVersion}). Anything "
                + "that version added is not in the comparison below.");
        }

        var differences = PedAppearanceDiff.Compare(
            entry.Ped.Appearance,
            PedAppearanceReader.Read(Native.PlayerPedId()));

        if (differences.Count == 0)
        {
            API.Log.Info($"[Ped] The ped you are wearing is identical to '{entry.Ped.Name}'.");

            return;
        }

        API.Log.Info($"[Ped] {differences.Count} difference(s) from '{entry.Ped.Name}':");

        foreach (var difference in differences)
        {
            API.Log.Info("[Ped]   " + difference);
        }
    }
}
