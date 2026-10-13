using System.Globalization;

using vMenu.Enhanced.Data.Appearance;
using vMenu.Enhanced.Menus.Appearance;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// Compares what a ped was asked to wear against what it actually has on. Written out slot by slot on
// purpose: reflection would drift out of step with the model quietly, and the names in the output are
// meant to be read by a person rather than to match a property.
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

            // Two different peds have two different wardrobes, so comparing slot against slot below would list
            // every one of them and say nothing the line above has not already said.
            return differences;
        }

        CompareComponents(expected, actual, differences);
        CompareProps(expected, actual, differences);

        return differences;
    }

    private static void CompareComponents(
        PedOutfit expected,
        PedOutfit actual,
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

            if (!Matches(component, worn))
            {
                differences.Add(new AppearanceDifference(field, ComponentText(component), ComponentText(worn)));
            }
        }

        // A slot this ped has but the save said nothing about. Worth saying, since it means the two peds do
        // not have the same set of slots.
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
        PedOutfit expected,
        PedOutfit actual,
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

            if (!Matches(prop, worn))
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

    private static bool Matches(PedComponentValue expected, PedComponentValue worn) =>
        expected.Texture == worn.Texture
        && expected.Palette == worn.Palette
        && (SharesCollection(expected.LocalDrawable, worn.LocalDrawable)
            ? string.Equals(expected.Collection, worn.Collection, StringComparison.Ordinal)
                && expected.LocalDrawable == worn.LocalDrawable
            : expected.Drawable == worn.Drawable);

    private static bool Matches(PedPropValue expected, PedPropValue worn) =>
        expected.Texture == worn.Texture
        && (SharesCollection(expected.LocalDrawable, worn.LocalDrawable)
            ? string.Equals(expected.Collection, worn.Collection, StringComparison.Ordinal)
                && expected.LocalDrawable == worn.LocalDrawable
            : expected.Drawable == worn.Drawable);

    private static bool SharesCollection(int expected, int worn) => expected >= 0 && worn >= 0;

    private static string ComponentField(int slot) =>
        $"Component {Text(slot)} ({PedComponentSlots.TechnicalName(slot)})";

    private static string PropField(int slot) =>
        $"Prop {Text(slot)} ({PedPropSlots.TechnicalName(slot)})";

    private static string ComponentText(PedComponentValue component) =>
        $"drawable {Drawable(component.Drawable, component.Collection, component.LocalDrawable)}, "
        + $"texture {Text(component.Texture)}, palette {Text(component.Palette)}";

    private static string PropText(PedPropValue prop) =>
        $"drawable {Drawable(prop.Drawable, prop.Collection, prop.LocalDrawable)}, texture {Text(prop.Texture)}";

    private static string Drawable(int drawable, string collection, int local)
    {
        if (local < 0)
        {
            return Text(drawable);
        }

        return collection.Length == 0
            ? $"{Text(drawable)} (base game #{Text(local)})"
            : $"{Text(drawable)} ({collection} #{Text(local)})";
    }

    private static string Named(string name, uint hash) => name.Length > 0 ? name : hash.ToString();

    private static string Text(int value) => value.ToString(CultureInfo.InvariantCulture);
}
