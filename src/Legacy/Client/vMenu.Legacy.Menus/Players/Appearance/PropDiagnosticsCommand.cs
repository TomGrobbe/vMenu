using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Logging;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// Temporary, to diagnose add-on hats that cannot be scrolled past. Not gated, so a reporter can run it
// without changing their config. Remove once that report is settled.
public static class PropDiagnosticsCommand
{
    private const string Command = "vmenu_debug_props";

    private const string Prefix = "[PropDebug] ";

    private const int Hats = 0;

    private const int TimeoutMs = 5000;

    private const int PressGapMs = 150;

    private static bool _running;

    public static void Initialize()
    {
        Commands.Register(Command, false, () => _ = RunAsync());
    }

    private static async Task RunAsync()
    {
        if (_running)
        {
            Log.Info(Prefix + "Already running, wait for it to finish.");

            return;
        }

        _running = true;

        try
        {
            await ReportAsync();
        }
        catch (Exception exception)
        {
            Log.Error(Prefix + "Failed: " + exception);
        }
        finally
        {
            _running = false;
        }
    }

    private static async Task ReportAsync()
    {
        var ped = API.PlayerPedId();
        var model = NativeFixer.GetEntityModel(ped);
        var resource = API.GetCurrentResourceName();

        Log.Info(Prefix + "Start. This changes your hat for up to a minute, stand still until it says Done.");
        Log.Info(Prefix + $"flavor Legacy, vMenu {API.GetResourceMetadata(resource, "version", 0)}, build {API.GetGameBuildNumber()}");
        Log.Info(Prefix + $"model {model}, freemode male {model == PedSpawning.FreemodeModel(true)}, freemode female {model == PedSpawning.FreemodeModel(false)}");

        int[] slots = [0, 1, 2, 6, 7];

        foreach (var slot in slots)
        {
            var worn = NativeFixer.GetPedPropIndex(ped, slot, false);
            var line = $"slot {slot}: count {API.GetNumberOfPedPropDrawableVariations(ped, slot)}, worn {worn}, texture {API.GetPedPropTextureIndex(ped, slot)}";

            if (worn >= 0)
            {
                line += $", collection '{API.GetPedPropCollectionName(ped, slot)}' local {API.GetPedPropCollectionLocalIndex(ped, slot)}";
            }

            Log.Info(Prefix + line);
        }

        var collections = API.GetPedCollectionsCount(ped);

        Log.Info(Prefix + $"collections: {collections}");

        for (var index = 0; index < collections; index++)
        {
            var name = API.GetPedCollectionName(ped, index) ?? string.Empty;
            var line = $"  [{index}] '{name}' props:";

            foreach (var slot in slots)
            {
                line += $" {slot}={API.GetNumberOfPedCollectionPropDrawableVariations(ped, slot, name)}";
            }

            Log.Info(Prefix + line);
        }

        var count = API.GetNumberOfPedPropDrawableVariations(ped, Hats);

        if (count <= 0)
        {
            Log.Info(Prefix + "This ped has no hats, nothing more to test. Done.");

            return;
        }

        var lastGroupStart = HatRanges(ped, count);

        var originalHat = NativeFixer.GetPedPropIndex(ped, Hats, false);
        var originalTexture = API.GetPedPropTextureIndex(ped, Hats);

        try
        {
            await TimingAsync(ped, TestIndices(count, lastGroupStart));

            await ReplayAsync(ped, count, lastGroupStart);
        }
        finally
        {
            if (originalHat >= 0)
            {
                NativeFixer.SetPedPropIndex(ped, Hats, originalHat, Math.Max(0, originalTexture), true, false);
            }
            else
            {
                NativeFixer.ClearPedProp(ped, Hats, false);
            }
        }

        Log.Info(Prefix + "Done.");
    }

    private static int HatRanges(int ped, int count)
    {
        Log.Info(Prefix + $"hat ranges ({count} hats):");

        var start = 0;
        var name = API.GetPedCollectionNameFromProp(ped, Hats, 0) ?? string.Empty;
        var localStart = API.GetPedCollectionLocalIndexFromProp(ped, Hats, 0);
        var lastGroupStart = 0;

        for (var drawable = 1; drawable <= count; drawable++)
        {
            var next = drawable < count ? API.GetPedCollectionNameFromProp(ped, Hats, drawable) ?? string.Empty : null;

            if (next is not null && string.Equals(next, name, StringComparison.Ordinal))
            {
                continue;
            }

            var localEnd = API.GetPedCollectionLocalIndexFromProp(ped, Hats, drawable - 1);

            Log.Info(Prefix + $"  global {start}-{drawable - 1} '{name}' local {localStart}-{localEnd}, textures of first {API.GetNumberOfPedPropTextureVariations(ped, Hats, start)}");

            if (next is null)
            {
                break;
            }

            lastGroupStart = drawable;
            start = drawable;
            name = next;
            localStart = API.GetPedCollectionLocalIndexFromProp(ped, Hats, drawable);
        }

        return lastGroupStart;
    }

    private static List<int> TestIndices(int count, int lastGroupStart)
    {
        int[] wanted =
        [
            188, 189, 190, 191,
            lastGroupStart - 2, lastGroupStart - 1, lastGroupStart, lastGroupStart + 1, lastGroupStart + 2,
            count - 2, count - 1,
        ];

        var indices = new List<int>();

        foreach (var index in wanted)
        {
            if (index < 0 || index >= count || Has(indices, index))
            {
                continue;
            }

            var at = indices.Count;

            while (at > 0 && indices[at - 1] > index)
            {
                at--;
            }

            indices.Insert(at, index);
        }

        return indices;
    }

    private static bool Has(List<int> values, int value)
    {
        foreach (var existing in values)
        {
            if (existing == value)
            {
                return true;
            }
        }

        return false;
    }

    private static async Task TimingAsync(int ped, List<int> indices)
    {
        Log.Info(Prefix + "timing (set a hat, then read it back every frame):");

        foreach (var index in indices)
        {
            NativeFixer.SetPedPropIndex(ped, Hats, index, 0, true, false);

            var immediate = NativeFixer.GetPedPropIndex(ped, Hats, false);
            var result = await WaitForAsync(ped, index);

            Log.Info(Prefix + $"  set {index}: immediate {immediate}, {result}");
        }
    }

    private static async Task<string> WaitForAsync(int ped, int index)
    {
        var started = API.GetGameTimer();
        var frames = 0;
        var seen = string.Empty;
        var last = int.MinValue;
        var repeats = 0;

        while (true)
        {
            var value = NativeFixer.GetPedPropIndex(ped, Hats, false);

            if (value != last)
            {
                if (last != int.MinValue)
                {
                    seen += $"{last}x{repeats} ";
                }

                last = value;
                repeats = 0;
            }

            repeats++;

            var elapsed = API.GetGameTimer() - started;

            if (value == index)
            {
                return $"matched after {frames} frames / {elapsed} ms, seen {seen}{last}";
            }

            if (elapsed >= TimeoutMs)
            {
                return $"NEVER matched in {TimeoutMs} ms, seen {seen}{last}x{repeats}";
            }

            frames++;

            await BaseScript.Delay(0);
        }
    }

    private static async Task ReplayAsync(int ped, int count, int lastGroupStart)
    {
        var start = count > 190 ? 189 : Math.Max(0, lastGroupStart - 1);

        NativeFixer.SetPedPropIndex(ped, Hats, start, 0, true, false);

        Log.Info(Prefix + $"replay right, menu logic, starting on {start}: {await WaitForAsync(ped, start)}");

        for (var press = 1; press <= 5; press++)
        {
            await PressAsync(ped, press, left: false);
        }

        NativeFixer.ClearPedProp(ped, Hats, false);

        await BaseScript.Delay(PressGapMs);

        Log.Info(Prefix + $"replay left, menu logic, starting on none (read {NativeFixer.GetPedPropIndex(ped, Hats, false)}):");

        for (var press = 1; press <= 3; press++)
        {
            await PressAsync(ped, press, left: true);
        }
    }

    private static async Task PressAsync(int ped, int press, bool left)
    {
        var scope = PedVariationScope.Global;
        var count = scope.PropCount(ped, Hats);
        var current = scope.CurrentProp(ped, Hats) ?? -1;
        var next = current + (left ? -1 : 1);

        if (next < -1)
        {
            next = count - 1;
        }
        else if (next >= count)
        {
            next = -1;
        }

        if (next < 0)
        {
            PedVariationScope.ClearProp(ped, Hats);
        }
        else
        {
            scope.SetProp(ped, Hats, next, 0);
        }

        var immediate = NativeFixer.GetPedPropIndex(ped, Hats, false);

        await BaseScript.Delay(PressGapMs);

        Log.Info(Prefix + $"  press {press}: read {current}, asked {next}, immediately {immediate}, after {PressGapMs} ms {NativeFixer.GetPedPropIndex(ped, Hats, false)}");
    }
}
