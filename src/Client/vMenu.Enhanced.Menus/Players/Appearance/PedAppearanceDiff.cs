using System.Globalization;

using vMenu.Enhanced.Menus.Appearance;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// Compares what a ped was asked to wear against what it actually has on.
/// </summary>
/// <remarks>
/// Written out slot by slot on purpose. Reflection would drift out of step with the model quietly,
/// and the names in the output are meant to be read by a person rather than to match a property.
/// </remarks>
public static class PedAppearanceDiff
{
    public static List<AppearanceDifference> Compare(PedAppearance expected, PedAppearance actual)
    {
        var differences = new List<AppearanceDifference>();

        if (expected.ModelHash != actual.ModelHash)
        {
            differences.Add(new AppearanceDifference(
                "Model",
                Named(expected.ModelName, expected.ModelHash),
                Named(actual.ModelName, actual.ModelHash)));

            // Two different peds have two different wardrobes, so comparing slot against slot below
            // would list every one of them and say nothing the line above has not already said.
            return differences;
        }

        CompareComponents(expected, actual, differences);
        CompareProps(expected, actual, differences);

        return differences;
    }

    private static void CompareComponents(
        PedAppearance expected,
        PedAppearance actual,
        List<AppearanceDifference> differences)
    {
        foreach (var component in expected.Components)
        {
            var field = ComponentField(component.Slot);
            var worn = actual.ComponentAt(component.Slot);

            if (worn is null)
            {
                differences.Add(new AppearanceDifference(field, ComponentText(component), "no such slot on this ped"));

                continue;
            }

            if (worn.Drawable != component.Drawable
                || worn.Texture != component.Texture
                || worn.Palette != component.Palette)
            {
                differences.Add(new AppearanceDifference(field, ComponentText(component), ComponentText(worn)));
            }
        }

        // A slot this ped has but the save said nothing about. Worth saying, since it means the two
        // peds do not have the same set of slots.
        foreach (var component in actual.Components)
        {
            if (expected.ComponentAt(component.Slot) is null)
            {
                differences.Add(new AppearanceDifference(
                    ComponentField(component.Slot),
                    "not recorded",
                    ComponentText(component)));
            }
        }
    }

    private static void CompareProps(
        PedAppearance expected,
        PedAppearance actual,
        List<AppearanceDifference> differences)
    {
        foreach (var prop in expected.Props)
        {
            var field = PropField(prop.Slot);
            var worn = actual.PropAt(prop.Slot);

            if (worn is null)
            {
                differences.Add(new AppearanceDifference(field, PropText(prop), "nothing worn"));

                continue;
            }

            if (worn.Drawable != prop.Drawable || worn.Texture != prop.Texture)
            {
                differences.Add(new AppearanceDifference(field, PropText(prop), PropText(worn)));
            }
        }

        foreach (var prop in actual.Props)
        {
            if (expected.PropAt(prop.Slot) is null)
            {
                differences.Add(new AppearanceDifference(PropField(prop.Slot), "nothing worn", PropText(prop)));
            }
        }
    }

    private static string ComponentField(int slot) =>
        $"Component {Text(slot)} ({PedComponentSlots.TechnicalName(slot)})";

    private static string PropField(int slot) =>
        $"Prop {Text(slot)} ({PedPropSlots.TechnicalName(slot)})";

    private static string ComponentText(PedComponentValue component) =>
        $"drawable {Text(component.Drawable)}, texture {Text(component.Texture)}, palette {Text(component.Palette)}";

    private static string PropText(PedPropValue prop) =>
        $"drawable {Text(prop.Drawable)}, texture {Text(prop.Texture)}";

    private static string Named(string name, uint hash) => name.Length > 0 ? name : hash.ToString();

    private static string Text(int value) => value.ToString(CultureInfo.InvariantCulture);
}
