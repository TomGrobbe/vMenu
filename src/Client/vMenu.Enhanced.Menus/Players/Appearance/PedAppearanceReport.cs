using System.Globalization;

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// Turns an appearance into lines a person can read in the console.
/// </summary>
/// <remarks>
/// Every line carries the raw ids, because those are what one player sends another when they want
/// the same outfit. The counts beside them come from the live ped rather than the appearance, so a
/// drawable that is out of range on this model shows up as such.
/// </remarks>
public static class PedAppearanceReport
{
    public static IEnumerable<string> Describe(PedAppearance appearance, int ped)
    {
        yield return $"Model: {(appearance.ModelName.Length > 0 ? appearance.ModelName : "unnamed")} ({appearance.ModelHash})";

        foreach (var slot in PedComponentSlots.All)
        {
            var available = Native.GetNumberOfPedDrawableVariations(ped, slot);

            if (appearance.ComponentAt(slot) is not { } component)
            {
                if (available > 0)
                {
                    yield return $"Component {Text(slot)} ({PedComponentSlots.TechnicalName(slot)}): not recorded, {Text(available)} available";
                }

                continue;
            }

            var textures = Native.GetNumberOfPedTextureVariations(ped, slot, component.Drawable);

            yield return
                $"Component {Text(slot)} ({PedComponentSlots.TechnicalName(slot)}): "
                + $"drawable {Text(component.Drawable)} of {Text(available)}, "
                + $"texture {Text(component.Texture)} of {Text(textures)}, "
                + $"palette {Text(component.Palette)}";
        }

        foreach (var slot in PedPropSlots.All)
        {
            var available = Native.GetNumberOfPedPropDrawableVariations(ped, slot);

            if (appearance.PropAt(slot) is not { } prop)
            {
                yield return $"Prop {Text(slot)} ({PedPropSlots.TechnicalName(slot)}): nothing worn, {Text(available)} available";

                continue;
            }

            var textures = Native.GetNumberOfPedPropTextureVariations(ped, slot, prop.Drawable);

            yield return
                $"Prop {Text(slot)} ({PedPropSlots.TechnicalName(slot)}): "
                + $"drawable {Text(prop.Drawable)} of {Text(available)}, "
                + $"texture {Text(prop.Texture)} of {Text(textures)}";
        }
    }

    private static string Text(int value) => value.ToString(CultureInfo.InvariantCulture);
}
