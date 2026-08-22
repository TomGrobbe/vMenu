using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

internal static class TorsoTags
{
    internal const string Gloves = "GLOVES";

    internal const string Jacket = "JACKET";

    internal const string JacketOnly = "JACKET_ONLY";

    internal const string BikerVest = "BIKER_VEST";

    internal const string VestShirt = "VEST_SHIRT";

    internal const string SweatVest = "SWEAT_VEST";

    internal const string TuxJacket = "TUX_JACKET";

    internal const string TailsJacket = "TAILS_JACKET";

    internal const string SilkRobe = "SILK_ROBE";

    internal const string SilkPyjamas = "SILK_PYJAMAS";

    internal const string LongSleeve = "LONG_SLEEVE";

    internal const string ShirtBraces = "SHIRT_BRACES";

    internal const string OpenCollar = "OPEN_COLLAR";

    internal const string ClosedCollar = "CLOSED_COLLAR";

    internal const string OpenShort = "OPEN_SHORT";

    internal const string OpenShortTwo = "OPEN_SHORT_2";

    internal const uint UnnamedShirtTagHash = 4176592416;

    internal const string OvercoatAccessory = "OVERCOAT_ACCS";

    internal const string HighWaist = "HIGH_WAIST";

    internal const string MorphSuit = "MORPH_SUIT";

    internal const string GorkaSuit = "GORKA_SUIT";

    internal const string ScubaGear = "SCUBA_GEAR";

    internal const string LowriderOpenCheck = "LOW2_OPEN_CHECK";

    internal const string X17Draw6 = "X17_DRAW_6";

    internal const string SmugglerDraw0 = "SMUG_DRAW_0";

    internal const string SmugglerDraw1 = "SMUG_DRAW_1";

    internal const string SmugglerDraw6 = "SMUG_DRAW_6";

    internal const string AirDraw3 = "AIR_DRAW_3";

    private const int ComponentApparel = 0;

    private const int NoDrawGroup = -1;

    private const int DrawGroupCount = 16;

    private static readonly string[] DrawGroupTags =
    [
        "DRAW_0", "DRAW_1", "DRAW_2", "DRAW_3", "DRAW_4", "DRAW_5", "DRAW_6", "DRAW_7",
        "DRAW_8", "DRAW_9", "DRAW_10", "DRAW_11", "DRAW_12", "DRAW_13", "DRAW_14", "DRAW_15",
    ];

    private static readonly Dictionary<string, uint> HashOfTag = new(StringComparer.Ordinal);

    private static readonly Dictionary<ulong, bool> AnswerForItemAndTag = [];

    private static readonly Dictionary<uint, int> DrawGroupOfItem = [];

    internal static string ApartmentDraw(int index) => $"APART_DRAW_{index}";

    internal static string BikerDraw(int index) => $"BIKER_DRAW_{index}";

    internal static string HeistDraw(int index) => $"HEIST_DRAW_{index}";

    internal static string LowriderDraw(int index) => $"LOW_DRAW_{index}";

    internal static string LowriderTwoDraw(int index) => $"LOW2_DRAW_{index}";

    internal static string LuxeDraw(int index) => $"LUXE_DRAW_{index}";

    internal static string LuxeTwoDraw(int index) => $"LUXE2_DRAW_{index}";

    internal static string StuntDraw(int index) => $"STUNT_DRAW_{index}";

    internal static void Forget()
    {
        AnswerForItemAndTag.Clear();
        DrawGroupOfItem.Clear();
    }

    internal static bool Has(uint item, string tag) => HasHash(item, HashOf(tag));

    internal static bool HasHash(uint item, uint tagHash)
    {
        if (!TorsoItems.IsRealItem(item))
        {
            return false;
        }

        var key = ((ulong)item << 32) | tagHash;

        if (AnswerForItemAndTag.TryGetValue(key, out var known))
        {
            return known;
        }

        var answer = Native.DoesShopPedApparelHaveRestrictionTag(item, tagHash, ComponentApparel);

        AnswerForItemAndTag[key] = answer;

        return answer;
    }

    internal static bool HasAny(uint item, params string[] tags)
    {
        foreach (var tag in tags)
        {
            if (Has(item, tag))
            {
                return true;
            }
        }

        return false;
    }

    internal static int DrawGroup(uint item)
    {
        if (!TorsoItems.IsRealItem(item))
        {
            return NoDrawGroup;
        }

        if (DrawGroupOfItem.TryGetValue(item, out var known))
        {
            return known;
        }

        var group = NoDrawGroup;

        for (var index = 0; index < DrawGroupCount; index++)
        {
            if (Has(item, DrawGroupTags[index]))
            {
                group = index;

                break;
            }
        }

        DrawGroupOfItem[item] = group;

        return group;
    }

    private static uint HashOf(string tag)
    {
        if (HashOfTag.TryGetValue(tag, out var known))
        {
            return known;
        }

        var hash = (uint)Native.GetHashKey(tag);

        HashOfTag[tag] = hash;

        return hash;
    }
}
