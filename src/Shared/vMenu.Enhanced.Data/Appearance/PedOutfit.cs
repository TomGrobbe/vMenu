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
