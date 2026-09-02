using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Players.Character;

public static class CharacterDumpCommands
{
    private const string DumpCommand = "vmenu_character";

    private const string TattooCommand = "vmenu_character_tattoos";

    private const string OutfitCommand = "vmenu_character_outfits";

    private const string OneOutfitCommand = "vmenu_character_outfit";

    private const string StoreCommand = "vmenu_characters";

    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(DumpCommand, false, DebugCommands.Gate(() => _ = DumpAsync()));
        SharedAPI.Commands.RegisterCommand(TattooCommand, false, DebugCommands.Gate(Tattoos));
        SharedAPI.Commands.RegisterCommand(OutfitCommand, false, DebugCommands.Gate(() => _ = OutfitsAsync(null)));
        SharedAPI.Commands.RegisterCommand(OneOutfitCommand, false, DebugCommands.Gate<string?>(name => _ = OutfitsAsync(name)));
        SharedAPI.Commands.RegisterCommand(StoreCommand, false, DebugCommands.Gate(Store));
    }

    private static async Task DumpAsync()
    {
        await API.JumpToMainThread();

        var ped = Native.PlayerPedId();

        if (await FreemodeReader.ReadCoreAsync(ped) is not { } core)
        {
            Log.Info("[Character] You are not wearing one of the two freemode models, so there is nothing to report.");

            return;
        }

        var character = new MpCharacter
        {
            Name = "live",
            Core = core,
            Styles = [FreemodeReader.ReadStyle(ped)],
            Outfits = [new MpCharacterOutfit { Name = "live", Outfit = FreemodeReader.ReadOutfit(ped) }],
        };

        Log.Info($"[Character] Live state, read from the game ({(core.IsMale ? "male" : "female")}):");
        Log.Info(ClientJson.SerializeIndented(character));
    }

    private static void Store()
    {
        API.RunOnMainThread(() =>
        {
            Log.Info($"[Character] Keys under '{MpCharacterStore.CharacterPrefix}':");

            foreach (var line in KvpStore.Describe(MpCharacterStore.CharacterPrefix))
            {
                Log.Info("[Character]   " + line);
            }

            Log.Info($"[Character] Keys under '{MpCharacterStore.CategoryPrefix}':");

            foreach (var line in KvpStore.Describe(MpCharacterStore.CategoryPrefix))
            {
                Log.Info("[Character]   " + line);
            }

            Log.Info($"[Character] Read back: {MpCharacterStore.All().Count} character(s), {MpCharacterStore.Categories().Count} category/categories.");
        });
    }

    private static async Task OutfitsAsync(string? filter)
    {
        await API.JumpToMainThread();

        var ped = Native.PlayerPedId();

        if (!PedSpawning.IsWearingFreemode())
        {
            Log.Info("[Outfits] You are not wearing a freemode model, so the game has no outfits for you.");

            return;
        }

        var male = PedSpawning.IsFreemodeMale((uint)Native.GetEntityModel(ped));

        OnlineOutfitCatalogue.Forget();
        OnlineOutfitCatalogue.Begin(male);

        while (!OnlineOutfitCatalogue.IsReady(male))
        {
            await API.Delay(50);
        }

        var packs = OnlineOutfitCatalogue.Packs(male);

        Log.Info($"[Outfits] {packs.Count} pack(s) for the freemode {(male ? "male" : "female")}:");

        var found = false;

        foreach (var pack in packs)
        {
            var name = pack.Name.Length == 0 ? "base game" : pack.Name;

            if (string.IsNullOrWhiteSpace(filter))
            {
                Log.Info($"[Outfits]   {name}: {pack.Outfits.Count} outfit(s)");

                continue;
            }

            var matched = false;

            foreach (var outfit in pack.Outfits)
            {
                if (outfit.Name.IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (!matched)
                {
                    Log.Info($"[Outfits]   {name}:");

                    matched = true;
                    found = true;
                }

                Describe(outfit);
            }
        }

        if (string.IsNullOrWhiteSpace(filter))
        {
            Log.Info($"[Outfits] Run {OneOutfitCommand} <name> to see what one of them is made of.");

            return;
        }

        if (!found)
        {
            Log.Info($"[Outfits] Nothing matched '{filter.Trim()}'.");
        }
    }

    private static void Describe(OnlineOutfit outfit)
    {
        Log.Info($"[Outfits]     {outfit.Name}");

        foreach (var piece in outfit.Outfit.Components)
        {
            var collection = piece.Collection.Length == 0 ? "base game" : piece.Collection;

            Log.Info(
                $"[Outfits]       component {piece.Slot} ({PedComponentSlots.TechnicalName(piece.Slot)}): "
                + $"drawable {piece.Drawable}, texture {piece.Texture}, {collection} #{piece.LocalDrawable}");
        }

        foreach (var prop in outfit.Outfit.Props)
        {
            var collection = prop.Collection.Length == 0 ? "base game" : prop.Collection;

            Log.Info(
                $"[Outfits]       prop {prop.Slot} ({PedPropSlots.TechnicalName(prop.Slot)}): "
                + $"drawable {prop.Drawable}, texture {prop.Texture}, {collection} #{prop.LocalDrawable}");
        }

        var missing = new List<string>();

        foreach (var slot in PedComponentSlots.All)
        {
            if (outfit.Outfit.ComponentAt(slot) is null)
            {
                missing.Add($"{slot} ({PedComponentSlots.TechnicalName(slot)})");
            }
        }

        if (missing.Count > 0)
        {
            Log.Info($"[Outfits]       says nothing about: {string.Join(", ", missing)}");
        }
    }

    private static void Tattoos()
    {
        API.RunOnMainThread(() =>
        {
            var ped = Native.PlayerPedId();
            var decorations = BrokenNatives.NativeFixer.GetPedDecorations(ped);

            if (decorations.Count == 0)
            {
                Log.Info("[Character] This ped has no decorations on it at all.");

                return;
            }

            Log.Info($"[Character] {decorations.Count} decoration(s) on this ped:");

            foreach (var (collection, overlay) in decorations)
            {
                if (TattooCatalogue.Resolve(collection, overlay) is { } known)
                {
                    Log.Info($"[Character]   {known.Collection} / {known.Name} ({known.Zone})");

                    continue;
                }

                Log.Info($"[Character]   unknown, collection hash {collection}, overlay hash {overlay}");
            }
        });
    }
}
