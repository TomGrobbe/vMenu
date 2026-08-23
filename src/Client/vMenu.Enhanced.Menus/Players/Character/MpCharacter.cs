using vMenu.Enhanced.Data.Appearance;

namespace vMenu.Enhanced.Menus.Players.Character;

// Not a record: the client sandbox has no default equality comparer.
public sealed class MpCharacter
{
    public const int SchemaVersion = 2;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public MpCharacterCore Core { get; set; } = new();

    public MpCharacterStyle? CurrentStyle { get; set; }

    public MpCharacterOutfit? CurrentOutfit { get; set; }

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

    public MpCharacter Copy()
    {
        var copy = new MpCharacter
        {
            Name = Name,
            Description = Description,
            Category = Category,
            Core = Core.Copy(),
            CurrentStyle = CurrentStyle?.Copy(),
            CurrentOutfit = CurrentOutfit?.Copy(),
            LastStyle = LastStyle,
            LastOutfit = LastOutfit,
            FacialExpression = FacialExpression,
            MovementClipset = MovementClipset,
        };

        foreach (var style in Styles)
        {
            copy.Styles.Add(style.Copy());
        }

        foreach (var outfit in Outfits)
        {
            copy.Outfits.Add(outfit.Copy());
        }

        return copy;
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

    public MpCharacterCore Copy()
    {
        var copy = new MpCharacterCore
        {
            IsMale = IsMale,
            Blend = Blend.Copy(),
            FaceFeatures = new List<float>(FaceFeatures),
            EyeColour = EyeColour,
            Tattoos = Tattoos.Copy(),
        };

        foreach (var overlay in Overlays)
        {
            copy.Overlays.Add(overlay.Copy());
        }

        return copy;
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

    public MpCharacterStyle Copy()
    {
        var copy = new MpCharacterStyle
        {
            Name = Name,
            Description = Description,
            HairStyle = HairStyle,
            HairColour = HairColour,
            HairHighlight = HairHighlight,
            HairDecorationCollection = HairDecorationCollection,
            HairDecorationName = HairDecorationName,
        };

        foreach (var overlay in Overlays)
        {
            copy.Overlays.Add(overlay.Copy());
        }

        return copy;
    }
}

public sealed class MpCharacterOutfit
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public PedOutfit Outfit { get; set; } = new();

    public MpCharacterOutfit Copy() => new()
    {
        Name = Name,
        Description = Description,
        Outfit = Outfit.Copy(),
    };
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
