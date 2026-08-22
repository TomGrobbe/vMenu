using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Players.Character;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.CharacterCreator.Title,
    SubtitleKey = Loc.CharacterCreator.Subtitle,
    DescriptionKey = Loc.CharacterCreator.LinkDescription,
    Permission = CharacterCreatorPermissions.Menu)]
public sealed class CharacterCreatorMenu : MenuDefinition
{
    private readonly CharacterBuilder _builder = new();

    private readonly SavedCharacters _saved = new();

    protected override void Build(MenuBuilder menu)
    {
        _builder.Attach(menu);
        _saved.Attach(menu, _builder);

        _builder.Finished = _saved.ShowAfterEditing;

        menu.Entries.Add(CreateRow(Loc.CharacterCreator.CreateMale, Loc.CharacterCreator.CreateMaleDescription, male: true));
        menu.Entries.Add(CreateRow(Loc.CharacterCreator.CreateFemale, Loc.CharacterCreator.CreateFemaleDescription, male: false));

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.SaveCurrent),
            Description = MenuText.Key(Loc.CharacterCreator.SaveCurrentDescription),
            LockedDescription = MenuText.Key(Loc.CharacterCreator.InVehicle),
            Gate = MenuGate.Permission(CharacterCreatorPermissions.Save) & MenuGate.When(OnFoot),
            Behaviour = GateBehaviour.Lock,
            OnSelectedAsync = _ => AdoptAsync(),
        });

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.SavedCharacters),
            Description = MenuText.Key(Loc.CharacterCreator.SavedCharactersDescription),
            MenuSubtitle = MenuText.Key(Loc.CharacterCreator.SavedCharactersSubtitle),
            Gate = MenuGate.Permission(CharacterCreatorPermissions.Spawn)
                | MenuGate.Permission(CharacterCreatorPermissions.Manage),
            Build = _saved.BuildRoot,
        });

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.Presets),
            Description = MenuText.Key(Loc.CharacterCreator.PresetsDescription),
            MenuSubtitle = MenuText.Key(Loc.CharacterCreator.PresetsSubtitle),
            Gate = MenuGate.Permission(CharacterCreatorPermissions.Presets)
                | MenuGate.Permission(CharacterCreatorPermissions.OnlineOutfits),
            Build = OutfitPresetsMenu.Build,
        });

        menu.OnOpened = _ => MenuRegistry.Refresh(menu.Menu);
    }

    private ButtonEntry CreateRow(string name, string description, bool male) => new()
    {
        Text = MenuText.Key(name),
        Description = MenuText.Key(description),
        LockedDescription = MenuText.Key(Loc.CharacterCreator.InVehicle),
        Label = MenuText.Literal("→"),
        Gate = MenuGate.Permission(CharacterCreatorPermissions.Create) & MenuGate.When(OnFoot),
        Behaviour = GateBehaviour.Lock,
        OnSelectedAsync = _ => _builder.OpenAsync(CharacterDraft.New(male), from: null, restore: true),
    };

    private async Task AdoptAsync()
    {
        if (await CharacterDraft.FromPlayerAsync(Native.PlayerPedId()) is not { } character)
        {
            Notifications.Warning(MenuText.Key(Loc.CharacterCreator.NotFreemode));

            return;
        }

        await _builder.OpenAsync(character, from: null, restore: false);
    }

    private static bool OnFoot() => !Native.IsPedInAnyVehicle(Native.PlayerPedId(), false);
}
