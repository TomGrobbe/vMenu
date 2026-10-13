using System.Collections;
using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json.Linq;

namespace vMenu.Enhanced.BrokenNatives;

public static partial class NativeFixer
{
    private static bool _scriptMissingReported;

    public static string[] GetAllVehicleModels() => Convert<string[]>((object)API.GetAllVehicleModels()) ?? [];

    public static int[] GetGamePool(string poolName) => Convert<int[]>((object)API.GetGamePool(poolName)) ?? [];

    public static List<(uint Collection, uint Overlay)> GetPedDecorations(int ped)
    {
        var decorations = new List<(uint, uint)>();

        if ((object)API.GetPedDecorations(ped) is not IEnumerable raw)
        {
            return decorations;
        }

        foreach (var entry in raw)
        {
            if (entry is IList { Count: >= 2 } pair)
            {
                decorations.Add((
                    unchecked((uint)System.Convert.ToInt64(pair[0], CultureInfo.InvariantCulture)),
                    unchecked((uint)System.Convert.ToInt64(pair[1], CultureInfo.InvariantCulture))));
            }
        }

        return decorations;
    }

    public static int TestVerticalProbeAgainstAllWater(float x, float y, float z, int blockingFlags, out float height)
    {
        var found = new OutputArgument();
        var result = Function.Call<int>(Hash.TEST_VERTICAL_PROBE_AGAINST_ALL_WATER, x, y, z, blockingFlags, found);

        height = result == 0 ? default : found.GetResult<float>();

        return result;
    }

    public static bool IsClosestDoorOfTypeLocked(uint model, float x, float y, float z)
    {
        var locked = false;
        var heading = 0f;

        API.GetStateOfClosestDoorOfType(model, x, y, z, ref locked, ref heading);

        return locked;
    }

    public static void SetFacialIdleAnimOverride(int ped, string clipName) =>
        API.SetFacialIdleAnimOverride(ped, clipName, null);

    public static void EmitLocal(string eventName, params object?[] args) => BaseScript.TriggerEvent(eventName, args);

    public static int AddConvarChangeListener(string convar, Action<string, object?> handler) =>
        API.AddConvarChangeListener(convar, new Action<object, object>((name, reserved) => handler(name?.ToString() ?? string.Empty, reserved)));

    public static T? GetStateBagValue<T>(string bagName, string keyName) => Convert<T>((object)API.GetStateBagValue(bagName, keyName));

    public static int AddStateBagChangeHandler(string? keyName, string? bagName, Delegate handler)
    {
        var parameters = handler.Method.GetParameters().Length;

        return API.AddStateBagChangeHandler(
            keyName!,
            bagName!,
            new Action<object, object, object, object, object>((bag, key, value, reserved, replicated) =>
                handler.DynamicInvoke(new object?[] { bag?.ToString(), key?.ToString(), value, reserved, replicated }.Take(parameters).ToArray())));
    }

    public static bool GetPedHeadBlendData(int ped, PedHeadBlendBuffer buffer)
    {
        var data = new Ped(ped).GetHeadBlendData();

        buffer.Load(
        [
            Slot(data.FirstFaceShape),
            Slot(data.SecondFaceShape),
            Slot(data.ThirdFaceShape),
            Slot(data.FirstSkinTone),
            Slot(data.SecondSkinTone),
            Slot(data.ThirdSkinTone),
            Slot(data.ParentFaceShapePercent),
            Slot(data.ParentSkinTonePercent),
            Slot(data.ParentThirdUnkPercent),
            data.IsParentInheritance ? 1UL : 0UL,
        ]);

        return true;
    }

    public static bool GetWeaponHudStats(uint weaponHash, WeaponHudStatsData buffer)
    {
        var stats = default(Game.WeaponHudStats);
        var found = Game.GetWeaponHudStats(weaponHash, ref stats);

        buffer.Load([Slot(stats.hudDamage), Slot(stats.hudSpeed), Slot(stats.hudCapacity), Slot(stats.hudAccuracy), Slot(stats.hudRange)]);

        return found;
    }

    public static bool GetWeaponComponentHudStats(uint componentHash, WeaponHudStatsData buffer)
    {
        var stats = default(Game.WeaponComponentHudStats);
        var found = Game.GetWeaponComponentHudStats(componentHash, ref stats);

        buffer.Load([Slot(stats.hudDamage), Slot(stats.hudSpeed), Slot(stats.hudCapacity), Slot(stats.hudAccuracy), Slot(stats.hudRange)]);

        return found;
    }

    public static void GetShopPedComponent(uint nameHash, ShopPedComponentBuffer buffer) =>
        ViaScript(Hash.GET_SHOP_PED_COMPONENT, ShopPedComponentBuffer.Slots, buffer.Load, nameHash);

    public static void GetShopPedProp(uint nameHash, ShopPedPropBuffer buffer) =>
        ViaScript(Hash.GET_SHOP_PED_PROP, ShopPedPropBuffer.Slots, buffer.Load, nameHash);

    public static void GetShopPedQueryOutfit(int index, ShopPedOutfitBuffer buffer) =>
        ViaScript(Hash.GET_SHOP_PED_QUERY_OUTFIT, ShopPedOutfitBuffer.Slots, buffer.Load, index);

    public static bool GetShopPedOutfitComponentVariant(uint nameHash, int componentIndex, OutfitVariantBuffer buffer) =>
        ViaScript(Hash.GET_SHOP_PED_OUTFIT_COMPONENT_VARIANT, OutfitVariantBuffer.Slots, buffer.Load, nameHash, componentIndex);

    public static bool GetShopPedOutfitPropVariant(uint nameHash, int propIndex, OutfitVariantBuffer buffer) =>
        ViaScript(Hash.GET_SHOP_PED_OUTFIT_PROP_VARIANT, OutfitVariantBuffer.Slots, buffer.Load, nameHash, propIndex);

    private static ulong Slot(int value) => unchecked((uint)value);

    private static ulong Slot(float value) => unchecked((uint)BitConverter.ToInt32(BitConverter.GetBytes(value), 0));

    private static bool ViaScript(Hash hash, int size, Action<ulong[]> load, params object[] leading)
    {
        long result = 0;
        ulong[]? slots = null;

        BaseScript.TriggerEvent(
            "vMenu.Enhanced:StructNative",
            unchecked((long)(ulong)hash),
            size,
            leading,
            new Action<object, object>((returned, values) =>
            {
                result = System.Convert.ToInt64(returned, CultureInfo.InvariantCulture);
                slots = (values as IEnumerable)?.Cast<object>().Select(value => unchecked((ulong)System.Convert.ToInt64(value, CultureInfo.InvariantCulture))).ToArray();
            }));

        if (slots is null || slots.Length < size)
        {
            if (!_scriptMissingReported)
            {
                _scriptMissingReported = true;
                Debug.WriteLine("[vMenu] client/struct_natives.lua did not answer, so shop outfit data is unavailable.");
            }

            load(new ulong[size]);
            return false;
        }

        load(slots);
        return result != 0;
    }

    public static T? Convert<T>(object? value) => value is null ? default : JToken.FromObject(value).ToObject<T>();
}
