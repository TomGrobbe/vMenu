using vMenu.Enhanced.Data.Appearance;

namespace vMenu.Enhanced.Menus.Players.Character;

// Not a record: the client sandbox has no default equality comparer.
public sealed class MpCharacter
{
    public const int SchemaVersion = 1;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public MpCharacterCore Core { get; set; } = new();

    public List<MpCharacterStyle> Styles { get; set; } = [];

    public List<MpCharacterOutfit> Outfits { get; set; } = [];

    public string LastStyle { get; set; } = string.Empty;

    public string LastOutfit { get; set; } = string.Empty;

    public string FacialExpression { get; set; } = string.Empty;

    public string MovementClipset { get; set; } = string.Empty;

    public MpCharacterStyle? StyleNamed(string name)
    {
        foreach (var style in Styles)
        {
            if (string.Equals(style.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return style;
            }
        }

        return null;
    }

    public MpCharacterOutfit? OutfitNamed(string name)
    {
        foreach (var outfit in Outfits)
        {
            if (string.Equals(outfit.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return outfit;
            }
        }

        return null;
    }
}

public sealed class MpCharacterCore
{
    public bool IsMale { get; set; } = true;

    public PedHeadBlend Blend { get; set; } = new();

    public List<float> FaceFeatures { get; set; } = [];

    public int EyeColour { get; set; }

    public List<PedOverlayValue> Overlays { get; set; } = [];

    public PedTattooSet Tattoos { get; set; } = new();

    public PedOverlayValue? OverlayAt(int slot)
    {
        foreach (var overlay in Overlays)
        {
            if (overlay.Slot == slot)
            {
                return overlay;
            }
        }

        return null;
    }
}

public sealed class MpCharacterStyle
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int HairStyle { get; set; }

    public int HairColour { get; set; }

    public int HairHighlight { get; set; }

    public string HairDecorationCollection { get; set; } = string.Empty;

    public string HairDecorationName { get; set; } = string.Empty;

    public List<PedOverlayValue> Overlays { get; set; } = [];

    public PedOverlayValue? OverlayAt(int slot)
    {
        foreach (var overlay in Overlays)
        {
            if (overlay.Slot == slot)
            {
                return overlay;
            }
        }

        return null;
    }
}

public sealed class MpCharacterOutfit
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public PedOutfit Outfit { get; set; } = new();
}

public sealed class MpCharacterCategory
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public sealed class MpCharacterEntry(MpCharacter character, int storedVersion)
{
    public MpCharacter Character { get; } = character;

    public int StoredVersion { get; } = storedVersion;

    public bool IsFromNewerBuild => StoredVersion > MpCharacter.SchemaVersion;
}
