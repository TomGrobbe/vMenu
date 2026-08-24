using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Vehicles.Appearance;
using vMenu.Enhanced.Menus.Vehicles.Saved;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Vehicles;

// Both of these re-read the vehicle through VehicleAppearanceReader, which asks the game rather than
// remembering what vMenu set. That is the point of them: a report built from vMenu's own memory
// would agree with itself no matter what the game actually did.
public static class VehicleDumpCommands
{
    private const string DumpCommand = "vmenu_vehicle";

    private const string DiffCommand = "vmenu_vehicle_diff";

    private const string LabelsCommand = "vmenu_vehicle_labels";

    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(DumpCommand, false, DebugCommands.Gate(Dump));
        SharedAPI.Commands.RegisterCommand(DiffCommand, false, DebugCommands.Gate<string?>(Diff));
        SharedAPI.Commands.RegisterCommand(LabelsCommand, false, DebugCommands.Gate(Labels));
    }

    // The vehicle the player is sitting in, driving or not, or null after saying so.
    internal static int? CurrentVehicle()
    {
        var ped = API.Players.Local.Ped;

        if (ped is null)
        {
            return null;
        }

        var target = VehicleTargeting.Current(ped);

        if (!target.Found)
        {
            Log.Info("[Vehicle] You have to be in a vehicle for this command to report anything.");

            return null;
        }

        return target.Handle;
    }

    private static void Dump()
    {
        if (CurrentVehicle() is not { } handle)
        {
            return;
        }

        var appearance = VehicleAppearanceReader.Read(handle);

        Log.Info("[Vehicle] Live state, read from the game:");

        foreach (var line in VehicleAppearanceReport.Describe(appearance, handle))
        {
            Log.Info("[Vehicle] " + line);
        }

        Log.Info("[Vehicle] As stored:");
        Log.Info(ClientJson.SerializeIndented(appearance));
    }

    private static void Labels()
    {
        if (CurrentVehicle() is not { } handle)
        {
            return;
        }

        Native.SetVehicleModKit(handle, 0);

        Log.Debug($"[Vehicle] Mod kit {Native.GetVehicleModKit(handle)}, type {Native.GetVehicleModKitType(handle)}.");
        Log.Debug($"[Vehicle] Mods streamed in: {Native.HaveVehicleModsStreamedIn(handle)}.");

        foreach (var slot in VehicleModSlots.All)
        {
            var count = Native.GetNumVehicleMods(handle, (int)slot);

            if (count <= 0 && !VehicleModSlots.IsToggle(slot))
            {
                continue;
            }

            var slotKey = Native.GetModSlotName(handle, (int)slot);

            Log.Debug(
                $"[Vehicle] Slot {(int)slot} ({VehicleModSlots.TechnicalName(slot)}): {count} part(s), "
                + $"slot name key '{slotKey}' {Reports(slotKey)}");

            for (var index = 0; index < count; index++)
            {
                var raw = Native.GetModTextLabel(handle, (int)slot, index);
                var used = VehicleModLabels.NameKey(handle, slot, index, count);

                // The identifier hash is what names a horn, and is the only handle a developer has on a part whose
                // artist supplied no label at all. Unsigned, so it reads the same way round as the hashes
                // VehicleHornLabels matches on.
                var identifier = (uint)Native.GetVehicleModIdentifierHash(handle, (int)slot, index);

                Log.Debug(
                    $"[Vehicle]   [{index}] id {identifier}, GetModTextLabel '{raw}' {Reports(raw)}"
                    + $", vMenu uses '{used ?? "<numbered fallback>"}' {Reports(used ?? string.Empty)}");
            }
        }
    }

    private static string Reports(string key) =>
        GameLabels.Exists(key) ? $"-> \"{GameLabels.Text(key, string.Empty)}\"" : "-> no text";

    // Says how the vehicle being driven differs from a saved one, which is what proves a restore was
    // faithful rather than merely plausible.
    private static void Diff(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Log.Info($"[Vehicle] Usage: {DiffCommand} <saved vehicle name>");

            return;
        }

        if (SavedVehicleStore.Load(name.Trim()) is not { } entry)
        {
            Log.Info($"[Vehicle] There is no saved vehicle called '{name}'.");

            return;
        }

        if (CurrentVehicle() is not { } handle)
        {
            return;
        }

        if (entry.IsFromNewerBuild)
        {
            Log.Warning(
                $"[Vehicle] '{entry.Vehicle.Name}' was saved by a newer version of vMenu (version "
                + $"{entry.StoredVersion}, this build understands {SavedVehicle.SchemaVersion}). Anything "
                + "that version added is not in the comparison below.");
        }

        var differences = VehicleAppearanceDiff.Compare(entry.Vehicle.Appearance, VehicleAppearanceReader.Read(handle));

        if (differences.Count == 0)
        {
            Log.Info($"[Vehicle] The vehicle you are in is identical to '{entry.Vehicle.Name}'.");

            return;
        }

        Log.Info($"[Vehicle] {differences.Count} difference(s) from '{entry.Vehicle.Name}':");

        foreach (var difference in differences)
        {
            Log.Info("[Vehicle]   " + difference);
        }
    }
}
