namespace vMenu.Enhanced.Data.Appearance;

// Not a record: the client sandbox has no default equality comparer.
public class PedOutfit
{
    public List<PedComponentValue> Components { get; set; } = [];

    public List<PedPropValue> Props { get; set; } = [];

    public PedComponentValue? ComponentAt(int slot)
    {
        foreach (var component in Components)
        {
            if (component.Slot == slot)
            {
                return component;
            }
        }

        return null;
    }

    public PedOutfit Copy()
    {
        var copy = new PedOutfit();

        foreach (var component in Components)
        {
            copy.Components.Add(new PedComponentValue
            {
                Slot = component.Slot,
                Drawable = component.Drawable,
                Texture = component.Texture,
                Palette = component.Palette,
                Collection = component.Collection,
                LocalDrawable = component.LocalDrawable,
            });
        }

        foreach (var prop in Props)
        {
            copy.Props.Add(new PedPropValue
            {
                Slot = prop.Slot,
                Drawable = prop.Drawable,
                Texture = prop.Texture,
                Collection = prop.Collection,
                LocalDrawable = prop.LocalDrawable,
            });
        }

        return copy;
    }

    public PedPropValue? PropAt(int slot)
    {
        foreach (var prop in Props)
        {
            if (prop.Slot == slot)
            {
                return prop;
            }
        }

        return null;
    }
}
