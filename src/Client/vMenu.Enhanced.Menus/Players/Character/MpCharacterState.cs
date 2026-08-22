using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Character;

public static class MpCharacterState
{
    public static MpCharacterCore? Worn { get; private set; }

    public static MpCharacterStyle? Style { get; private set; }

    public static MpCharacterOutfit? Outfit { get; private set; }

    public static MpCharacterEntry? From { get; private set; }

    public static bool IsEditing { get; private set; }

    public static MpCharacter? Draft { get; private set; }

    public static void Wearing(MpCharacter character, MpCharacterStyle? style, MpCharacterOutfit? outfit, MpCharacterEntry? from)
    {
        Worn = character.Core;
        Style = style;
        Outfit = outfit;
        From = from;
    }

    public static void BeginEditing(MpCharacter draft, MpCharacterEntry? from)
    {
        Draft = draft;
        From = from;
        Worn = draft.Core;
        Style = draft.Styles.Count > 0 ? draft.Styles[0] : null;
        Outfit = draft.Outfits.Count > 0 ? draft.Outfits[0] : null;
        IsEditing = true;
    }

    public static void StopEditing()
    {
        IsEditing = false;
        Draft = null;
    }

    public static async Task<bool> AdoptAsync(int ped)
    {
        if (IsEditing)
        {
            return Worn is not null;
        }

        if (await FreemodeReader.ReadCoreAsync(ped) is not { } core)
        {
            Forget();

            return false;
        }

        Worn = core;
        Style = FreemodeReader.ReadStyle(ped);
        Outfit = null;
        From = null;

        return true;
    }

    public static void Forget()
    {
        Worn = null;
        Style = null;
        Outfit = null;
        From = null;
    }

    public static bool MatchesPlayer()
    {
        if (Worn is null)
        {
            return false;
        }

        var model = (uint)Native.GetEntityModel(Native.PlayerPedId());

        return PedSpawning.IsFreemode(model) && PedSpawning.IsFreemodeMale(model) == Worn.IsMale;
    }
}
