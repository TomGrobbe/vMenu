using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;


namespace vMenu.Enhanced.Menus;
[VMenu(                     
    TitleKey = Loc.PlayerOptions.Title,                         
    SubtitleKey = Loc.PlayerOptions.Subtitle,                       
    DescriptionKey = Loc.PlayerOptions.LinkDescription  
)]  
public sealed class PlayerOptions : MenuDefinition  
{   
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PlayerOptions.EmptyMenu),
            Description = MenuText.Key(Loc.PlayerOptions.EmptyMenu),
            OnSelected = _ => { },
        });
    }
}
