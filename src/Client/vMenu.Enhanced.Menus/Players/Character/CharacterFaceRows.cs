using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Menus.Players.Character;

internal static class CharacterFaceRows
{
    private const int Steps = 20;

    private const int Middle = Steps / 2;

    private static readonly (int Feature, string Name)[] Sliders =
    [
        (0, Loc.CharacterCreator.FaceNoseWidth),
        (1, Loc.CharacterCreator.FaceNoseHeight),
        (2, Loc.CharacterCreator.FaceNoseLength),
        (3, Loc.CharacterCreator.FaceNoseBridge),
        (4, Loc.CharacterCreator.FaceNoseTip),
        (5, Loc.CharacterCreator.FaceNoseTwist),
        (6, Loc.CharacterCreator.FaceBrowHeight),
        (7, Loc.CharacterCreator.FaceBrowDepth),
        (8, Loc.CharacterCreator.FaceCheekboneHeight),
        (9, Loc.CharacterCreator.FaceCheekboneWidth),
        (10, Loc.CharacterCreator.FaceCheekWidth),
        (11, Loc.CharacterCreator.FaceEyeOpening),
        (12, Loc.CharacterCreator.FaceLipThickness),
        (13, Loc.CharacterCreator.FaceJawWidth),
        (14, Loc.CharacterCreator.FaceJawLength),
        (15, Loc.CharacterCreator.FaceChinHeight),
        (16, Loc.CharacterCreator.FaceChinLength),
        (17, Loc.CharacterCreator.FaceChinWidth),
        (18, Loc.CharacterCreator.FaceChinDimple),
        (19, Loc.CharacterCreator.FaceNeckThickness),
    ];

    internal static void Attach(MenuBuilder menu)
    {

        foreach (var (feature, name) in Sliders)
        {
            menu.Entries.Add(Row(feature, name));
        }

        menu.OnOpened = _ => CharacterCamera.Page = CameraFocus.Head;
    }

    private static SliderEntry Row(int feature, string name)
    {
        return new SliderEntry
        {
            Text = MenuText.Key(name),
            Description = MenuText.Key(Loc.CharacterCreator.FaceRowDescription, ("name", MenuText.Key(name))),
            Gate = CharacterCreatorPermissions.Create,
            Min = 0,
            Max = Steps,
            ShowDivider = true,
            Configure = item => item.ItemData = CameraFocus.Head,
            ReadPosition = () => Position(feature),

            OnMoved = moved => Write(feature, moved.NewPosition),
        };
    }

    private static int Position(int feature)
    {
        if (CharacterEdit.Draft?.Core is not { } core || feature >= core.FaceFeatures.Count)
        {
            return Middle;
        }

        return Math.Clamp((int)Math.Round((core.FaceFeatures[feature] * Middle) + Middle), 0, Steps);
    }

    private static void Write(int feature, int position)
    {
        if (CharacterEdit.Draft?.Core is not { } core)
        {
            return;
        }

        while (core.FaceFeatures.Count <= feature)
        {
            core.FaceFeatures.Add(0f);
        }

        core.FaceFeatures[feature] = (position - Middle) / (float)Middle;

        CharacterEdit.ApplyFace();
    }
}
