using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Storage;

public static class KvpTransfer
{
    private const string PendingLoadoutKey = "vmenu_pendingweaponloadout";

    private const string VehiclePrefix = "vmenu_vehicle_";

    private const string PedPrefix = "vmenu_ped_";

    private const string CharacterPrefix = "vmenu_mpchar_";

    private const string LoadoutPrefix = "vmenu_weaponloadout_";

    public static KvpBundle Export()
    {
        var entries = new List<KvpBundleEntry>();

        foreach (var key in Owned())
        {
            var raw = ReadEnvelope(key);

            if (raw is null)
            {
                continue;
            }

            entries.Add(new KvpBundleEntry { Key = key, Raw = raw });
        }

        // CreatedAt is left for JS to fill out, because C# has no access to DateTime thanks to the
        // overly protective sandboxing and in-game Natives return the wrong UTC time :(
        return new KvpBundle
        {
            Format = KvpBundle.FormatName,
            Version = KvpBundle.CurrentVersion,
            Entries = entries,
        };
    }

    public static KvpImportResult Import(KvpBundle bundle, KvpImportMode mode)
    {
        var result = new KvpImportResult();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        if (mode == KvpImportMode.Replace)
        {
            foreach (var key in Owned())
            {
                KvpStore.Delete(key);
                result.Deleted++;
            }
        }

        // Backwards, so a code holding the same key twice keeps the last one rather than the first.
        for (var index = bundle.Entries.Count - 1; index >= 0; index--)
        {
            Apply(bundle.Entries[index], mode, seen, result);
        }

        KvpStore.InvalidateCache();

        return result;
    }

    public static KvpInventory Measure()
    {
        var inventory = new KvpInventory();

        foreach (var key in Owned())
        {
            inventory.Total++;

            if (key.StartsWith(VehiclePrefix, StringComparison.Ordinal))
            {
                inventory.Vehicles++;
            }
            else if (key.StartsWith(PedPrefix, StringComparison.Ordinal))
            {
                inventory.Peds++;
            }
            else if (key.StartsWith(CharacterPrefix, StringComparison.Ordinal))
            {
                inventory.Characters++;
            }
            else if (key.StartsWith(LoadoutPrefix, StringComparison.Ordinal))
            {
                inventory.Loadouts++;
            }
            else if (key.StartsWith(UserDefault.KeyPrefix, StringComparison.Ordinal))
            {
                inventory.Settings++;
            }
        }

        return inventory;
    }

    private static void Apply(KvpBundleEntry entry, KvpImportMode mode, HashSet<string> seen, KvpImportResult result)
    {
        var key = entry.Key;

        if (string.IsNullOrEmpty(key) || !key.StartsWith(KvpStore.Prefix, StringComparison.Ordinal))
        {
            Log.Warning($"[Transfer] '{key}' is not a key vMenu owns, so it is being skipped.");
            result.SkippedMalformed++;

            return;
        }

        if (!seen.Add(key))
        {
            result.SkippedDuplicate++;

            return;
        }

        if (!KvpStore.TryReadHeader(entry.Raw, out var named, out _, out var version))
        {
            Log.Warning($"[Transfer] '{key}' does not hold a vMenu envelope, so it is being skipped.");
            result.SkippedMalformed++;

            return;
        }

        if (!string.Equals(named, key, StringComparison.Ordinal))
        {
            Log.Warning($"[Transfer] '{key}' holds an envelope naming itself '{named}', so it is being skipped.");
            result.SkippedMalformed++;

            return;
        }

        if (mode == KvpImportMode.Merge && KvpStore.VersionOf(key) is { } stored && stored > version)
        {
            Log.Warning(
                $"[Transfer] '{key}' is newer here (version {stored}) than the one in the code "
                + $"(version {version}), so it is being left alone.");

            result.SkippedNewer++;

            return;
        }

        KvpStore.WriteRaw(key, entry.Raw);
        result.Applied++;
    }

    private static List<string> Owned()
    {
        var keys = KvpStore.Keys(KvpStore.Prefix);
        var owned = new List<string>(keys.Count);

        foreach (var key in keys)
        {
            if (!string.Equals(key, PendingLoadoutKey, StringComparison.Ordinal))
            {
                owned.Add(key);
            }
        }

        return owned;
    }

    private static string? ReadEnvelope(string key)
    {
        var raw = KvpStore.ReadRaw(key);

        if (!string.IsNullOrEmpty(raw) && KvpStore.TryReadHeader(raw, out _, out _, out _))
        {
            return raw;
        }

        Log.Debug($"[Transfer] '{key}' does not hold a vMenu envelope, so it is being left out.");

        return null;
    }
}
