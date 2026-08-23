namespace vMenu.Enhanced.Menus.Misc;

// A class, not a record: the sandbox refuses the generated EqualityComparer<T>.Default.
public sealed class LocationBlipFile
{
    public List<LocationBlip> AlwaysOn { get; set; } = [];

    public List<LocationBlip> Toggleable { get; set; } = [];
}

public sealed class LocationBlip
{
    public string Name { get; set; } = string.Empty;

    public int Sprite { get; set; }

    public int Colour { get; set; }

    public float ScaleOffset { get; set; }

    public bool ShortRange { get; set; } = true;

    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }
}
