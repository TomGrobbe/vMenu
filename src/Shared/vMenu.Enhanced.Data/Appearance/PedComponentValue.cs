namespace vMenu.Enhanced.Data.Appearance;

public sealed class PedComponentValue
{
    public int Slot { get; set; }

    public int Drawable { get; set; }

    public int Texture { get; set; }

    public int Palette { get; set; }

    public string Collection { get; set; } = string.Empty;

    public int LocalDrawable { get; set; } = -1;
}
