using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Appearance;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Menus.Players.Character;

public sealed class OnlineOutfit
{
    public required string Name { get; init; }

    public required string Pack { get; init; }

    public required PedOutfit Outfit { get; init; }
}

public sealed class OnlineOutfitPack
{
    public required string Name { get; init; }

    public List<OnlineOutfit> Outfits { get; } = [];
}

public static class OnlineOutfitCatalogue
{
    private const int MaleCharacter = 3;

    private const int FemaleCharacter = 4;

    private const int Shop = 0;

    private const int AnyLocate = -1;

    private const int OutfitApparel = 2;

    private const int AnyAnchor = -1;

    private const int AnyComponent = -1;

    private const int NoItem = 1849449579;

    private const int MaleInvisibleUndershirt = 15;

    private const int FemaleInvisibleUndershirt = 14;

    private static readonly List<OnlineOutfitPack> MalePacks = [];

    private static readonly List<OnlineOutfitPack> FemalePacks = [];

    private static bool _readingMale;

    private static bool _readingFemale;

    private static bool _readMale;

    private static bool _readFemale;

    private static int _baseGameDropped;

    private static int _forced;

    public static event Action? Changed;

    public static bool IsReady(bool male) => male ? _readMale : _readFemale;

    public static IReadOnlyList<OnlineOutfitPack> Packs(bool male) => male ? MalePacks : FemalePacks;

    public static void Forget()
    {
        MalePacks.Clear();
        FemalePacks.Clear();

        _readMale = false;
        _readFemale = false;
    }

    public static void Begin(bool male)
    {
        if (male ? _readMale || _readingMale : _readFemale || _readingFemale)
        {
            return;
        }

        _ = ReadAsync(male);
    }

    private static async Task ReadAsync(bool male)
    {
        if (male)
        {
            _readingMale = true;
        }
        else
        {
            _readingFemale = true;
        }

        _baseGameDropped = 0;
        _forced = 0;

        try
        {
            var packs = await GatherAsync(male, Native.PlayerPedId());

            var into = male ? MalePacks : FemalePacks;

            into.Clear();
            into.AddRange(packs);

            Log.Debug($"[Outfits] Read {Count(packs)} outfit(s) in {packs.Count} pack(s) for the freemode {(male ? "male" : "female")}.");

            Log.Debug($"[Outfits] {_forced} forced piece(s), {_baseGameDropped} base game piece(s) skipped.");
        }
        catch (Exception exception)
        {
            Log.Error($"[Outfits] The game's outfit list could not be read: {exception}");
        }
        finally
        {
            if (male)
            {
                _readingMale = false;
                _readMale = true;
            }
            else
            {
                _readingFemale = false;
                _readFemale = true;
            }

            Changed?.Invoke();
        }
    }

    private sealed class PieceRef
    {
        public uint Hash { get; init; }

        public int Slot { get; init; }

        public int Value { get; init; }
    }

    private sealed class Pending
    {
        public required ShopPedOutfitBuffer Outfit { get; init; }

        public List<PendingComponent> Components { get; } = [];

        public List<ShopPedPropBuffer> Props { get; } = [];
    }

    private sealed class PendingComponent
    {
        public ShopPedComponentBuffer? Buffer { get; init; }

        public int Slot { get; init; }

        public int Drawable { get; init; }
    }

    private static async Task<List<OnlineOutfitPack>> GatherAsync(bool male, int ped)
    {
        var character = male ? MaleCharacter : FemaleCharacter;

        var count = Native.SetupShopPedApparelQueryTu(
            character, Shop, AnyLocate, OutfitApparel, AnyAnchor, AnyComponent);

        if (count <= 0)
        {
            return [];
        }

        var outfits = new List<ShopPedOutfitBuffer>(count);

        for (var index = 0; index < count; index++)
        {
            var buffer = new ShopPedOutfitBuffer();

            Native.GetShopPedQueryOutfit(index, buffer);

            outfits.Add(buffer);
        }

        await NextFrameAsync();

        var wanted = new List<ShopPedOutfitBuffer>();
        var componentPieces = new List<List<PieceRef>>();
        var propHashes = new List<List<uint>>();

        foreach (var outfit in outfits)
        {
            if (outfit.NameHash == 0 || Native.IsContentItemLocked(outfit.LockHash))
            {
                continue;
            }

            wanted.Add(outfit);
            componentPieces.Add(ComponentsOf(outfit));
            propHashes.Add(PropsOf(outfit));
        }

        await NextFrameAsync();

        var pending = new List<Pending>(wanted.Count);

        for (var index = 0; index < wanted.Count; index++)
        {
            var entry = new Pending { Outfit = wanted[index] };

            foreach (var piece in componentPieces[index])
            {
                if (piece.Hash == 0)
                {
                    entry.Components.Add(new PendingComponent { Slot = piece.Slot, Drawable = piece.Value });

                    continue;
                }

                var component = new ShopPedComponentBuffer();

                Native.GetShopPedComponent(piece.Hash, component);

                entry.Components.Add(new PendingComponent { Buffer = component });
            }

            foreach (var hash in propHashes[index])
            {
                var prop = new ShopPedPropBuffer();

                Native.GetShopPedProp(hash, prop);

                entry.Props.Add(prop);
            }

            pending.Add(entry);
        }

        await NextFrameAsync();

        return Group(pending, ped, male);
    }

    private static List<PieceRef> ComponentsOf(ShopPedOutfitBuffer outfit)
    {
        var pieces = new List<PieceRef>(outfit.Components);
        var forced = new List<PieceRef>();

        for (var index = 0; index < outfit.Components; index++)
        {
            var variant = new OutfitVariantBuffer();

            if (!Native.GetShopPedOutfitComponentVariant(outfit.NameHash, index, variant))
            {
                continue;
            }

            if (variant.NameHash is 0 or NoItem)
            {
                if (variant.EnumValue < 0 || variant.Slot < 0)
                {
                    continue;
                }

                pieces.Add(new PieceRef { Slot = variant.Slot, Value = variant.EnumValue });

                continue;
            }

            pieces.Add(new PieceRef { Hash = variant.NameHash });

            Force(variant.NameHash, forced);
        }

        pieces.AddRange(forced);

        return pieces;
    }

    private static List<uint> PropsOf(ShopPedOutfitBuffer outfit)
    {
        var hashes = new List<uint>(outfit.Props);

        for (var index = 0; index < outfit.Props; index++)
        {
            var variant = new OutfitVariantBuffer();

            if (Native.GetShopPedOutfitPropVariant(outfit.NameHash, index, variant)
                && variant.NameHash is not (0 or NoItem))
            {
                hashes.Add(variant.NameHash);
            }
        }

        return hashes;
    }

    private static void Force(uint piece, List<PieceRef> into)
    {
        var count = Native.GetShopPedApparelForcedComponentCount(piece);

        for (var index = 0; index < count; index++)
        {
            Native.GetForcedComponent(piece, index, out var hash, out var value, out var type);

            if (hash is 0 or NoItem)
            {
                if (value < 0 || type < 0)
                {
                    continue;
                }

                _forced++;

                into.Add(new PieceRef { Slot = type, Value = value });

                continue;
            }

            _forced++;

            into.Add(new PieceRef { Hash = (uint)hash });
        }
    }

    private static List<OnlineOutfitPack> Group(List<Pending> pending, int ped, bool male)
    {
        var packs = new List<OnlineOutfitPack>();

        foreach (var entry in pending)
        {
            var outfit = new PedOutfit();

            foreach (var component in entry.Components)
            {
                if (Wearable(ped, component) is not { } piece)
                {
                    continue;
                }

                outfit.Components.Add(piece);
            }

            if (outfit.Components.Count == 0)
            {
                continue;
            }

            Cover(ped, outfit, male);

            var pack = PackOf(outfit);

            foreach (var prop in entry.Props)
            {
                outfit.Props.Add(new PedPropValue
                {
                    Slot = prop.Slot,
                    Drawable = prop.Drawable,
                    Texture = prop.Texture,
                    Collection = Native.GetPedCollectionNameFromProp(ped, prop.Slot, prop.Drawable) ?? string.Empty,
                    LocalDrawable = Native.GetPedCollectionLocalIndexFromProp(ped, prop.Slot, prop.Drawable),
                });
            }

            Into(packs, pack).Outfits.Add(new OnlineOutfit
            {
                Name = Name(entry.Outfit),
                Pack = pack,
                Outfit = outfit,
            });
        }

        packs.Sort(static (left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

        return packs;
    }

    private static PedComponentValue? Wearable(int ped, PendingComponent pending)
    {
        int slot;
        int drawable;
        int texture;

        if (pending.Buffer is { } buffer)
        {
            slot = buffer.Slot;
            drawable = buffer.Drawable;
            texture = buffer.Texture;
        }
        else
        {
            _baseGameDropped++;

            Log.Debug($"[Outfits] Skipping base game piece {pending.Drawable} for slot {pending.Slot}: only the game can say which drawable that is.");

            return null;
        }

        return new PedComponentValue
        {
            Slot = slot,
            Drawable = drawable,
            Texture = texture,

            Collection = Native.GetPedCollectionNameFromDrawable(ped, slot, drawable) ?? string.Empty,
            LocalDrawable = Native.GetPedCollectionLocalIndexFromDrawable(ped, slot, drawable),
        };
    }

    private static void Cover(int ped, PedOutfit outfit, bool male)
    {
        if (outfit.ComponentAt(PedComponentSlots.Undershirt) is not null)
        {
            return;
        }

        var drawable = male ? MaleInvisibleUndershirt : FemaleInvisibleUndershirt;

        if (drawable >= Native.GetNumberOfPedDrawableVariations(ped, PedComponentSlots.Undershirt))
        {
            Log.Debug($"[Outfits] This ped has no drawable {drawable} for the undershirt, so one was not added.");

            return;
        }

        outfit.Components.Add(new PedComponentValue
        {
            Slot = PedComponentSlots.Undershirt,
            Drawable = drawable,
            Texture = 0,
            Collection = Native.GetPedCollectionNameFromDrawable(ped, PedComponentSlots.Undershirt, drawable) ?? string.Empty,
            LocalDrawable = Native.GetPedCollectionLocalIndexFromDrawable(ped, PedComponentSlots.Undershirt, drawable),
        });
    }

    private static string PackOf(PedOutfit outfit)
    {
        foreach (var piece in outfit.Components)
        {
            if (piece.Collection.Length > 0)
            {
                return piece.Collection;
            }
        }

        foreach (var prop in outfit.Props)
        {
            if (prop.Collection.Length > 0)
            {
                return prop.Collection;
            }
        }

        return string.Empty;
    }

    private static OnlineOutfitPack Into(List<OnlineOutfitPack> packs, string name)
    {
        foreach (var pack in packs)
        {
            if (string.Equals(pack.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return pack;
            }
        }

        var added = new OnlineOutfitPack { Name = name };

        packs.Add(added);

        return added;
    }

    private static string Name(ShopPedOutfitBuffer outfit)
    {
        var label = outfit.Label;

        if (label.Length == 0)
        {
            return string.Empty;
        }

        var text = Native.GetLabelText(label);

        return string.IsNullOrWhiteSpace(text) || string.Equals(text, "NULL", StringComparison.Ordinal)
            ? label
            : text;
    }

    private static int Count(List<OnlineOutfitPack> packs)
    {
        var total = 0;

        foreach (var pack in packs)
        {
            total += pack.Outfits.Count;
        }

        return total;
    }

    private static async Task NextFrameAsync()
    {
        var asked = Native.GetFrameCount();

        while (Native.GetFrameCount() == asked)
        {
            await API.Delay(0);
        }
    }
}
