using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Players.Character;

public sealed class ParentFace
{
    public required string Name { get; init; }

    public required int Index { get; init; }

    public bool IsFemale { get; init; }
}

public static class CharacterParents
{
    private const int OrdinaryMale = 0;

    private const int OrdinaryFemale = 1;

    private const int SpecialMale = 2;

    private const int SpecialFemale = 3;

    private static readonly (int Type, string Prefix, bool Female)[] Lists =
    [
        (OrdinaryMale, "Male_", false),
        (SpecialMale, "Special_Male_", false),
        (OrdinaryFemale, "Female_", true),
        (SpecialFemale, "Special_Female_", true),
    ];

    private static List<ParentFace>? _faces;

    public static List<ParentFace> All()
    {
        if (_faces is not null)
        {
            return _faces;
        }

        var faces = new List<ParentFace>();

        foreach (var (type, prefix, female) in Lists)
        {
            var first = Native.GetPedHeadBlendFirstIndex(type);
            var count = Native.GetPedHeadBlendNumHeads(type);

            for (var offset = 0; offset < count; offset++)
            {
                faces.Add(new ParentFace
                {
                    Name = Label(prefix, offset, first + offset),
                    Index = first + offset,
                    IsFemale = female,
                });
            }
        }

        _faces = faces;

        return faces;
    }

    public static int PositionOf(int index)
    {
        var faces = All();

        for (var position = 0; position < faces.Count; position++)
        {
            if (faces[position].Index == index)
            {
                return position;
            }
        }

        return 0;
    }

    public static int IndexAt(int position)
    {
        var faces = All();

        if (faces.Count == 0)
        {
            return 0;
        }

        return faces[Math.Clamp(position, 0, faces.Count - 1)].Index;
    }

    public static string NameAt(int position)
    {
        var faces = All();

        if (position < 0 || position >= faces.Count)
        {
            return Text(Loc.CharacterCreator.ParentUnnamed, ("number", (position + 1).ToString(CultureInfo.InvariantCulture)));
        }

        var face = faces[position];

        return Text(
            face.IsFemale ? Loc.CharacterCreator.ParentFemale : Loc.CharacterCreator.ParentMale,
            ("name", face.Name));
    }

    private static string Label(string prefix, int offset, int index)
    {
        var key = prefix + offset.ToString(CultureInfo.InvariantCulture);
        var label = Native.GetLabelText(key);

        if (string.IsNullOrWhiteSpace(label)
            || string.Equals(label, "NULL", StringComparison.Ordinal)
            || string.Equals(label, key, StringComparison.Ordinal))
        {
            return Text(
                Loc.CharacterCreator.ParentUnnamed,
                ("number", (index + 1).ToString(CultureInfo.InvariantCulture)));
        }

        return label;
    }

    private static string Text(string key, params (string Name, string Value)[] values)
    {
        var text = MenuText.Key(key, Array.ConvertAll(values, pair => (pair.Name, MenuText.Literal(pair.Value))));

        return text.Resolve(Localizer.Current);
    }
}
