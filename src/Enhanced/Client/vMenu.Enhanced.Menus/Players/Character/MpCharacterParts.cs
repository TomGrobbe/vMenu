namespace vMenu.Enhanced.Menus.Players.Character;

public sealed class PedOverlayValue
{
    public int Slot { get; set; }

    public int Style { get; set; }

    public float Opacity { get; set; }

    public int Colour { get; set; }

    public PedOverlayValue Copy() => new()
    {
        Slot = Slot,
        Style = Style,
        Opacity = Opacity,
        Colour = Colour,
    };
}

public sealed class PedHeadBlend
{
    public int FirstShape { get; set; }

    public int SecondShape { get; set; }

    public int ThirdShape { get; set; }

    public int FirstSkin { get; set; }

    public int SecondSkin { get; set; }

    public int ThirdSkin { get; set; }

    public float ShapeMix { get; set; } = 0.5f;

    public float SkinMix { get; set; } = 0.5f;

    public float ThirdMix { get; set; }

    public bool IsParent { get; set; }

    public PedHeadBlend Copy() => new()
    {
        FirstShape = FirstShape,
        SecondShape = SecondShape,
        ThirdShape = ThirdShape,
        FirstSkin = FirstSkin,
        SecondSkin = SecondSkin,
        ThirdSkin = ThirdSkin,
        ShapeMix = ShapeMix,
        SkinMix = SkinMix,
        ThirdMix = ThirdMix,
        IsParent = IsParent,
    };
}

public sealed class TattooRef
{
    public string Collection { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public TattooRef Copy() => new() { Collection = Collection, Name = Name };
}

public sealed class PedTattooSet
{
    public List<TattooRef> Hair { get; set; } = [];

    public List<TattooRef> Head { get; set; } = [];

    public List<TattooRef> Torso { get; set; } = [];

    public List<TattooRef> LeftArm { get; set; } = [];

    public List<TattooRef> RightArm { get; set; } = [];

    public List<TattooRef> LeftLeg { get; set; } = [];

    public List<TattooRef> RightLeg { get; set; } = [];

    public List<TattooRef> Badges { get; set; } = [];

    public List<TattooRef> Addons { get; set; } = [];

    public IEnumerable<TattooRef> Everything()
    {
        foreach (var list in Lists())
        {
            foreach (var tattoo in list)
            {
                yield return tattoo;
            }
        }
    }

    public List<List<TattooRef>> Lists() =>
        [Hair, Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg, Badges, Addons];

    public void Clear()
    {
        foreach (var list in Lists())
        {
            list.Clear();
        }
    }

    public PedTattooSet Copy()
    {
        var copy = new PedTattooSet();

        var mine = Lists();
        var theirs = copy.Lists();

        for (var list = 0; list < mine.Count; list++)
        {
            foreach (var tattoo in mine[list])
            {
                theirs[list].Add(tattoo.Copy());
            }
        }

        return copy;
    }
}
