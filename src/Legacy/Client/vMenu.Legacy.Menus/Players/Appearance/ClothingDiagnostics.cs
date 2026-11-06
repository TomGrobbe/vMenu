using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Logging;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// Temporary, for the add-on clothing reports where scrolling right stops at a certain number. Logged as
// warnings so it shows without debug mode. Remove once that report is settled.
internal static class ClothingDiagnostics
{
    private const string Prefix = "[ClothingDebug] ";

    private static readonly int[] PropSlots = [0, 1, 2, 6, 7];

    private static readonly int[] FollowUpMs = [100, 500, 1500];

    private static readonly Dictionary<int, Press> LastPress = [];

    private sealed class Press(bool left, int count, int current, int next)
    {
        internal bool Left { get; } = left;

        internal int Count { get; } = count;

        internal int Current { get; } = current;

        internal int Next { get; } = next;
    }

    internal static void Opened(PedVariationScope scope, string menu)
    {
        try
        {
            Report(scope, menu);
        }
        catch (Exception exception)
        {
            Log.Error(Prefix + "Open report failed: " + exception);
        }
    }

    internal static void Pressed(PedVariationScope scope, int slot, bool component, bool left, int count, int current, int next)
    {
        try
        {
            Check(scope, slot, component, left, count, current, next);
        }
        catch (Exception exception)
        {
            Log.Error(Prefix + "Press check failed: " + exception);
        }
    }

    private static void Report(PedVariationScope scope, string menu)
    {
        var ped = API.PlayerPedId();
        var model = NativeFixer.GetEntityModel(ped);
        var resource = API.GetCurrentResourceName();

        LastPress.Clear();

        Log.Warning(Prefix + $"=== opened '{menu}', scope {ScopeName(scope)}");
        Log.Warning(Prefix + $"flavor Legacy, vMenu {API.GetResourceMetadata(resource, "version", 0)}, build {API.GetGameBuildNumber()}, ped {ped}");
        Log.Warning(Prefix + $"model {model}, freemode male {model == PedSpawning.FreemodeModel(true)}, freemode female {model == PedSpawning.FreemodeModel(false)}");

        foreach (var slot in PedComponentSlots.All)
        {
            var drawable = API.GetPedDrawableVariation(ped, slot);
            var texture = API.GetPedTextureVariation(ped, slot);

            Log.Warning(Prefix + $"component {slot}: global count {API.GetNumberOfPedDrawableVariations(ped, slot)}, scope count {scope.DrawableCount(ped, slot)}, scope current {Show(scope.CurrentDrawable(ped, slot))}, "
                + $"worn {drawable} texture {texture}/{API.GetNumberOfPedTextureVariations(ped, slot, drawable)} palette {API.GetPedPaletteVariation(ped, slot)}, "
                + $"valid {API.IsPedComponentVariationValid(ped, slot, drawable, texture)}, collection '{API.GetPedDrawableVariationCollectionName(ped, slot)}' local {API.GetPedDrawableVariationCollectionLocalIndex(ped, slot)}");
        }

        foreach (var slot in PropSlots)
        {
            var worn = NativeFixer.GetPedPropIndex(ped, slot, false);
            var line = $"prop {slot}: global count {API.GetNumberOfPedPropDrawableVariations(ped, slot)}, scope count {scope.PropCount(ped, slot)}, scope current {Show(scope.CurrentProp(ped, slot))}, worn {worn}";

            if (worn >= 0)
            {
                line += $" texture {API.GetPedPropTextureIndex(ped, slot)}/{API.GetNumberOfPedPropTextureVariations(ped, slot, worn)}, collection '{API.GetPedPropCollectionName(ped, slot)}' local {API.GetPedPropCollectionLocalIndex(ped, slot)}";
            }

            Log.Warning(Prefix + line);
        }

        var collections = API.GetPedCollectionsCount(ped);
        var componentTotals = new int[PedComponentSlots.All.Length];
        var propTotals = new int[PropSlots.Length];

        Log.Warning(Prefix + $"collections: {collections}");

        for (var index = 0; index < collections; index++)
        {
            var name = API.GetPedCollectionName(ped, index) ?? string.Empty;
            var line = $"  [{index}] '{name}' components:";

            for (var at = 0; at < PedComponentSlots.All.Length; at++)
            {
                var count = API.GetNumberOfPedCollectionDrawableVariations(ped, PedComponentSlots.All[at], name);

                componentTotals[at] += count;
                line += $" {PedComponentSlots.All[at]}={count}";
            }

            line += " props:";

            for (var at = 0; at < PropSlots.Length; at++)
            {
                var count = API.GetNumberOfPedCollectionPropDrawableVariations(ped, PropSlots[at], name);

                propTotals[at] += count;
                line += $" {PropSlots[at]}={count}";
            }

            Log.Warning(Prefix + line);
        }

        for (var at = 0; at < PedComponentSlots.All.Length; at++)
        {
            var slot = PedComponentSlots.All[at];
            var global = API.GetNumberOfPedDrawableVariations(ped, slot);

            if (global != componentTotals[at])
            {
                Log.Warning(Prefix + $"MISMATCH component {slot}: global count {global}, collections add up to {componentTotals[at]}");
            }

            ComponentRanges(ped, slot, global);
        }

        for (var at = 0; at < PropSlots.Length; at++)
        {
            var slot = PropSlots[at];
            var global = API.GetNumberOfPedPropDrawableVariations(ped, slot);

            if (global != propTotals[at])
            {
                Log.Warning(Prefix + $"MISMATCH prop {slot}: global count {global}, collections add up to {propTotals[at]}");
            }

            PropRanges(ped, slot, global);
        }

        Log.Warning(Prefix + "=== end of open report");
    }

    private static void ComponentRanges(int ped, int slot, int count)
    {
        if (count <= 0)
        {
            return;
        }

        var invalid = new List<int>();

        for (var drawable = 0; drawable < count; drawable++)
        {
            if (!API.IsPedComponentVariationValid(ped, slot, drawable, 0))
            {
                invalid.Add(drawable);
            }
        }

        var start = 0;
        var name = API.GetPedCollectionNameFromDrawable(ped, slot, 0) ?? string.Empty;

        for (var drawable = 1; drawable <= count; drawable++)
        {
            var next = drawable < count ? API.GetPedCollectionNameFromDrawable(ped, slot, drawable) ?? string.Empty : null;

            if (next is not null && string.Equals(next, name, StringComparison.Ordinal))
            {
                continue;
            }

            Log.Warning(Prefix + $"  component {slot} global {start}-{drawable - 1} '{name}' local {API.GetPedCollectionLocalIndexFromDrawable(ped, slot, start)}-{API.GetPedCollectionLocalIndexFromDrawable(ped, slot, drawable - 1)}, textures of first {API.GetNumberOfPedTextureVariations(ped, slot, start)}");

            start = drawable;
            name = next ?? string.Empty;
        }

        if (invalid.Count > 0)
        {
            Log.Warning(Prefix + $"  component {slot} invalid with texture 0: {Ranges(invalid)}");
        }
    }

    private static void PropRanges(int ped, int slot, int count)
    {
        if (count <= 0)
        {
            return;
        }

        var start = 0;
        var name = API.GetPedCollectionNameFromProp(ped, slot, 0) ?? string.Empty;

        for (var drawable = 1; drawable <= count; drawable++)
        {
            var next = drawable < count ? API.GetPedCollectionNameFromProp(ped, slot, drawable) ?? string.Empty : null;

            if (next is not null && string.Equals(next, name, StringComparison.Ordinal))
            {
                continue;
            }

            Log.Warning(Prefix + $"  prop {slot} global {start}-{drawable - 1} '{name}' local {API.GetPedCollectionLocalIndexFromProp(ped, slot, start)}-{API.GetPedCollectionLocalIndexFromProp(ped, slot, drawable - 1)}, textures of first {API.GetNumberOfPedPropTextureVariations(ped, slot, start)}");

            start = drawable;
            name = next ?? string.Empty;
        }
    }

    private static void Check(PedVariationScope scope, int slot, bool component, bool left, int count, int current, int next)
    {
        var ped = API.PlayerPedId();
        var key = component ? slot : 100 + slot;
        var what = $"{(component ? "component" : "prop")} {slot} ({ScopeName(scope)})";
        var direction = left ? "left" : "right";
        var landed = Read(scope, ped, slot, component);

        if (LastPress.TryGetValue(key, out var previous))
        {
            if (previous.Count != count)
            {
                Log.Warning(Prefix + $"COUNT CHANGED {what}: was {previous.Count}, now {count}");
            }

            if (previous.Next != current)
            {
                Log.Warning(Prefix + $"READ BACK DIFFERS {what}: previous press asked {previous.Next}, this press read {current}");
            }

            if (previous.Left == left && previous.Current == current)
            {
                Log.Warning(Prefix + $"STUCK {what}: pressed {direction} twice and both presses read {current}");
            }
        }

        LastPress[key] = new Press(left, count, current, next);

        var wrapped = left ? next > current : next < current;

        if (wrapped)
        {
            Log.Warning(Prefix + $"WRAPPED {what}: pressed {direction} on {current} of count {count}, went to {next}");
        }

        if (landed == next)
        {
            return;
        }

        Log.Warning(Prefix + $"DID NOT CHANGE {what}: pressed {direction}, read {current}, asked {next} of count {count}, immediately reads {Show(landed)}, {Detail(scope, ped, slot, component, next)}");

        _ = FollowUpAsync(scope, slot, component, next, what);
    }

    private static async Task FollowUpAsync(PedVariationScope scope, int slot, bool component, int asked, string what)
    {
        try
        {
            var waited = 0;

            foreach (var delay in FollowUpMs)
            {
                await BaseScript.Delay(delay - waited);

                waited = delay;

                var ped = API.PlayerPedId();
                var value = Read(scope, ped, slot, component);

                Log.Warning(Prefix + $"  follow up {what} after {delay} ms: asked {asked}, reads {Show(value)}, {Detail(scope, ped, slot, component, asked)}");

                if (value == asked)
                {
                    return;
                }
            }
        }
        catch (Exception exception)
        {
            Log.Error(Prefix + "Follow up failed: " + exception);
        }
    }

    private static int? Read(PedVariationScope scope, int ped, int slot, bool component)
    {
        if (component)
        {
            return scope.CurrentDrawable(ped, slot);
        }

        return scope.CurrentProp(ped, slot) ?? -1;
    }

    private static string Detail(PedVariationScope scope, int ped, int slot, bool component, int asked)
    {
        if (component)
        {
            var drawable = API.GetPedDrawableVariation(ped, slot);

            return $"global worn {drawable} texture {API.GetPedTextureVariation(ped, slot)}, collection '{API.GetPedDrawableVariationCollectionName(ped, slot)}' local {API.GetPedDrawableVariationCollectionLocalIndex(ped, slot)}, "
                + $"asked valid {(asked >= 0 && scope.IsUsable(ped, slot, asked, 0))}, asked textures {(asked >= 0 ? scope.TextureCount(ped, slot, asked) : 0)}, "
                + $"global count {API.GetNumberOfPedDrawableVariations(ped, slot)}, scope count {scope.DrawableCount(ped, slot)}";
        }

        var worn = NativeFixer.GetPedPropIndex(ped, slot, false);
        var line = $"global worn {worn}";

        if (worn >= 0)
        {
            line += $" texture {API.GetPedPropTextureIndex(ped, slot)}, collection '{API.GetPedPropCollectionName(ped, slot)}' local {API.GetPedPropCollectionLocalIndex(ped, slot)}";
        }

        return line + $", asked textures {(asked >= 0 ? scope.PropTextureCount(ped, slot, asked) : 0)}, global count {API.GetNumberOfPedPropDrawableVariations(ped, slot)}, scope count {scope.PropCount(ped, slot)}";
    }

    private static string Ranges(List<int> values)
    {
        var parts = new List<string>();
        var start = values[0];
        var end = values[0];

        for (var at = 1; at <= values.Count; at++)
        {
            if (at < values.Count && values[at] == end + 1)
            {
                end = values[at];

                continue;
            }

            parts.Add(start == end ? $"{start}" : $"{start}-{end}");

            if (at < values.Count)
            {
                start = values[at];
                end = values[at];
            }
        }

        return string.Join(", ", parts);
    }

    private static string ScopeName(PedVariationScope scope) =>
        scope.IsCollection ? $"collection '{scope.CollectionName}'" : "global";

    private static string Show(int? value) => value is { } number ? $"{number}" : "none";
}
