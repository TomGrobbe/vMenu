using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Menus.Players.Saved;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Players;

// Both of these re-read the ped through PedAppearanceReader, which asks the game rather than
// remembering what vMenu set. That is the point of them: a report built from vMenu's own memory
// would agree with itself no matter what the game actually did.
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
        API.RunOnMainThread(() =>
        {
            var ped = Native.PlayerPedId();
            var appearance = PedAppearanceReader.Read(ped);

            Log.Info("[Ped] Live state, read from the game:");

            foreach (var line in PedAppearanceReport.Describe(appearance, ped))
            {
                Log.Info("[Ped] " + line);
            }

            Log.Info("[Ped] As stored:");
            Log.Info(ClientJson.SerializeIndented(appearance));
        });
    }

    // Says how the ped being worn differs from a saved one, which is what proves a restore was faithful
    // rather than merely plausible.
    private static void Diff(string? name)
    {
        API.RunOnMainThread(() =>
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Log.Info($"[Ped] Usage: {DiffCommand} <saved ped name>");

                return;
            }

            if (SavedPedStore.Load(name.Trim()) is not { } entry)
            {
                Log.Info($"[Ped] There is no saved ped called '{name}'.");

                return;
            }

            if (entry.IsFromNewerBuild)
            {
                Log.Warning(
                    $"[Ped] '{entry.Ped.Name}' was saved by a newer version of vMenu (version "
                    + $"{entry.StoredVersion}, this build understands {SavedPed.SchemaVersion}). Anything "
                    + "that version added is not in the comparison below.");
            }

            var differences = PedAppearanceDiff.Compare(
                entry.Ped.Appearance,
                PedAppearanceReader.Read(Native.PlayerPedId()));

            if (differences.Count == 0)
            {
                Log.Info($"[Ped] The ped you are wearing is identical to '{entry.Ped.Name}'.");

                return;
            }

            Log.Info($"[Ped] {differences.Count} difference(s) from '{entry.Ped.Name}':");

            foreach (var difference in differences)
            {
                Log.Info("[Ped]   " + difference);
            }
        });
    }
}
