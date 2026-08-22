using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Menus.Players.Character;

internal static class CharacterInheritanceRows
{
    private const int MixSteps = 10;

    internal static void Attach(MenuBuilder menu)
    {
        CharacterCamera.AddButtons(menu);

        menu.Entries.Add(FaceRow(
            Loc.CharacterCreator.ParentOne,
            Loc.CharacterCreator.ParentRowDescription,
            core => core.Blend.FirstShape,
            (core, index) => core.Blend.FirstShape = index));

        menu.Entries.Add(FaceRow(
            Loc.CharacterCreator.ParentOneSkin,
            Loc.CharacterCreator.SkinRowDescription,
            core => core.Blend.FirstSkin,
            (core, index) => core.Blend.FirstSkin = index));

        menu.Entries.Add(FaceRow(
            Loc.CharacterCreator.ParentTwo,
            Loc.CharacterCreator.ParentRowDescription,
            core => core.Blend.SecondShape,
            (core, index) => core.Blend.SecondShape = index));

        menu.Entries.Add(FaceRow(
            Loc.CharacterCreator.ParentTwoSkin,
            Loc.CharacterCreator.SkinRowDescription,
            core => core.Blend.SecondSkin,
            (core, index) => core.Blend.SecondSkin = index));

        menu.Entries.Add(MixRow(
            Loc.CharacterCreator.ShapeMix,
            Loc.CharacterCreator.ShapeMixDescription,
            core => core.Blend.ShapeMix,
            (core, mix) => core.Blend.ShapeMix = mix));

        menu.Entries.Add(MixRow(
            Loc.CharacterCreator.SkinMix,
            Loc.CharacterCreator.SkinMixDescription,
            core => core.Blend.SkinMix,
            (core, mix) => core.Blend.SkinMix = mix));

        menu.OnOpened = _ => CharacterCamera.Page = CameraFocus.Head;
    }

    private static DynamicListEntry FaceRow(
        string name,
        string description,
        Func<MpCharacterCore, int> read,
        Action<MpCharacterCore, int> write)
    {
        return new DynamicListEntry
        {
            Text = MenuText.Key(name),
            Description = MenuText.From(() => Describe(description, read)),
            Gate = CharacterCreatorPermissions.Create,
            Configure = item => item.ItemData = CameraFocus.Head,
            ReadValue = () => Value(read),

            Change = changing =>
            {
                if (CharacterEdit.Draft?.Core is { } core)
                {
                    var position = CharacterEdit.Step(
                        CharacterParents.PositionOf(read(core)),
                        CharacterParents.All().Count,
                        changing.Left);

                    write(core, CharacterParents.IndexAt(position));

                    CharacterEdit.ApplyBlend();
                }

                changing.Item.Description = Describe(description, read);

                return Value(read);
            },
        };
    }

    private static SliderEntry MixRow(
        string name,
        string description,
        Func<MpCharacterCore, float> read,
        Action<MpCharacterCore, float> write)
    {
        return new SliderEntry
        {
            Text = MenuText.Key(name),
            Description = MenuText.Key(description),
            Gate = CharacterCreatorPermissions.Create,
            Min = 0,
            Max = MixSteps,
            Configure = item => item.ItemData = CameraFocus.Head,
            ReadPosition = () => CharacterEdit.Draft?.Core is { } core
                ? (int)Math.Round(read(core) * MixSteps)
                : MixSteps / 2,

            OnMoved = moved =>
            {
                if (CharacterEdit.Draft?.Core is { } core)
                {
                    write(core, moved.NewPosition / (float)MixSteps);

                    CharacterEdit.ApplyBlend();
                }
            },
        };
    }

    private static string Value(Func<MpCharacterCore, int> read) =>
        CharacterEdit.Draft?.Core is { } core
            ? CharacterParents.NameAt(CharacterParents.PositionOf(read(core)))
            : string.Empty;

    private static string Describe(string description, Func<MpCharacterCore, int> read) =>
        CharacterEdit.Resolve(MenuText.Key(description, ("name", MenuText.Literal(Value(read)))));
}
