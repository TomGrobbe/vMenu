using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// Reads what a ped is wearing back out of the game.
/// </summary>
/// <remarks>
/// Every value here is asked of the game the moment it is wanted. Nothing is remembered from when it
/// was set, which is the whole point: this is what the dump command reports and what the writer
/// checks its own work against, so a value that came from vMenu's memory would prove nothing.
/// </remarks>
public static class PedAppearanceReader
{
    public static PedAppearance Read(int ped)
    {
        var model = (uint)Native.GetEntityModel(ped);

        return new PedAppearance
        {
            ModelHash = model,
            ModelName = PedModelNames.Resolve(model),
            Components = ReadComponents(ped),
            Props = ReadProps(ped),
        };
    }

    private static List<PedComponentValue> ReadComponents(int ped)
    {
        var components = new List<PedComponentValue>(PedComponentSlots.All.Length);

        foreach (var slot in PedComponentSlots.All)
        {
            // A model that has nothing for this slot, such as most animals. Recording a value the
            // model does not have would make every restore report a difference it cannot fix.
            if (Native.GetNumberOfPedDrawableVariations(ped, slot) <= 0)
            {
                continue;
            }

            components.Add(new PedComponentValue
            {
                Slot = slot,
                Drawable = Native.GetPedDrawableVariation(ped, slot),

                // The game answers -1 for a slot whose texture it has not decided on. Stored as zero,
                // because that is the texture it will draw, and -1 is not a value anything can set.
                Texture = Math.Max(0, Native.GetPedTextureVariation(ped, slot)),
                Palette = Math.Max(0, Native.GetPedPaletteVariation(ped, slot)),
            });
        }

        return components;
    }

    private static List<PedPropValue> ReadProps(int ped)
    {
        var props = new List<PedPropValue>(PedPropSlots.All.Length);

        foreach (var slot in PedPropSlots.All)
        {
            // The false is the dead check the enhanced natives added. Off, because a ped is wearing
            // what it is wearing whether or not it happens to be dead.
            var drawable = Native.GetPedPropIndex(ped, slot, false);

            // Nothing worn. Left out of the list entirely rather than stored as legacy's -1, so an
            // empty slot needs no special case anywhere downstream.
            if (drawable < 0)
            {
                continue;
            }

            props.Add(new PedPropValue
            {
                Slot = slot,
                Drawable = drawable,
                Texture = Math.Max(0, Native.GetPedPropTextureIndex(ped, slot)),
            });
        }

        return props;
    }
}
