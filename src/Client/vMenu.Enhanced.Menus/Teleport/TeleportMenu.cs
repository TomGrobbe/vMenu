using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Entities;
using CitizenFX.FiveM.Client.Extensions;

using MenuAPI;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Ticks;

using Newtonsoft.Json;
using TeleportMenuPermissions = vMenu.Enhanced.Data.Permissions.Menus.TeleportMenu;

namespace vMenu.Enhanced.Menus;


[VMenu(
    TitleKey = Loc.TeleportMenu.Title,
    SubtitleKey = Loc.TeleportMenu.Subtitle,
    DescriptionKey = Loc.TeleportMenu.LinkDescription,
    Permission = TeleportMenuPermissions.Menu)]
public sealed class TeleportMenu : MenuDefinition
{
    public record JsonVec4(float x, float y, float z);
    public record TeleportLoc(string name, string description, JsonVec4 position);
    public record TeleportCat(string name, string description, List<TeleportLoc> locations);
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.TeleportMenu.TeleportToWaypoint),
            Description = MenuText.Key(Loc.TeleportMenu.TeleportToWaypointDescription),
            OnSelectedAsync = async _ =>
            {
                if (Native.IsWaypointActive())
                {
                    var blip = GetWaypointBlip();
                    if (!(blip?.Position == null))
                    {
                        await PlayerTeleport.ToCoordsAsync(new Vector3(blip.Position.X, blip.Position.Y, -2000));
                    }
                    else
                    {
                        Notifications.Error(MenuText.Key(Loc.TeleportMenu.WaypointInvalid));
                    }
                }
                else
                {
                    Notifications.Error(MenuText.Key(Loc.TeleportMenu.NoWaypoint));
                }
            },
        });
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.TeleportMenu.TeleportToCoords),
            Description = MenuText.Key(Loc.TeleportMenu.TeleportToCoordsDescription),
            OnSelectedAsync = async _ =>
            {
                string? typed = await UserInput.GetTextAsync(
                    MenuText.Key(Loc.TeleportMenu.CoordsPrompt),
                    50,
                    "0, 0, 0");
                
                if (string.IsNullOrWhiteSpace(typed))
                {
                    return;
                }
                
                string[] parts = typed.Split(',');
                Vector3 position = new Vector3(
                    float.Parse(parts[0].Trim()),
                    float.Parse(parts[1].Trim()),
                    float.Parse(parts[2].Trim())
                );
                
                await PlayerTeleport.ToCoordsAsync(position);
            },
        });
        
        var actions = menu.AddDetachedMenu(
            MenuText.Key(Loc.TeleportMenu.TeleportCategories), 
            MenuText.Key(Loc.TeleportMenu.TeleportCategoriesDescription),
            _ => {}
        );
        actions.Builder.OnOpened = async _ =>
        {
            actions.Builder.ClearEntries();
            var result = await ServerActions.InvokeAsync(ActionIds.TeleportMenu.TeleportCategories);
            var loc = JsonConvert.DeserializeObject<List<TeleportCat>>(result.Data[0]);
            if (loc == null || loc.Count == 0)
            {
                return;
            }
            
            foreach (var location in loc)
            {
                var action = actions.Builder.AddDetachedMenu(
                    MenuText.From(() => location?.name ?? string.Empty), 
                    MenuText.From(() => location?.description ?? string.Empty), 
                    _ => {}
                );
                actions.Builder.Add(new ButtonEntry
                {
                    Text = MenuText.From(() => location?.name ?? string.Empty),
                    Description = MenuText.From(() => location?.description ?? string.Empty), 
                    OnSelected = _ => action.Open()
                });
                foreach (var locs in location.locations)
                {
                    action.Builder.Add(new ButtonEntry
                    {
                        Text = MenuText.From(() => locs?.name ?? string.Empty),
                        Description = MenuText.From(() => locs?.description ?? string.Empty), 
                        OnSelectedAsync = async _ =>
                        {
                            await PlayerTeleport.ToCoordsAsync(new Vector3(locs.position.x, locs.position.y, locs.position.z));
                        }
                    });
                }
                action.Builder.Add(new ButtonEntry
                {
                    Text = MenuText.Key(Loc.TeleportMenu.CreatePosition),
                    Description = MenuText.Key(Loc.TeleportMenu.CreatePositionDescription),
                    OnSelectedAsync = async _ =>
                    {
                        string? name = await UserInput.GetTextAsync(
                            MenuText.Key(Loc.TeleportMenu.PositionName),
                            50);
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            return;
                        }

                        await API.Delay(500);
                        
                        string? description = await UserInput.GetTextAsync(
                            MenuText.Key(Loc.TeleportMenu.PositionDescription),
                            50);
                        if (string.IsNullOrWhiteSpace(description))
                        {
                            return;
                        }
                        var position = API.Players.Local.Position;
                        location.locations.Add(new (name, description, new JsonVec4(position.X, position.Y, position.Z)));
                        API.Log.Debug(JsonConvert.SerializeObject(loc));
                    },
                });
            }
            actions.Builder.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.TeleportMenu.CreateCategory),
                Description = MenuText.Key(Loc.TeleportMenu.CreateCategoryDescription),
                OnSelectedAsync = async _ =>
                {
                    string? name = await UserInput.GetTextAsync(
                        MenuText.Key(Loc.TeleportMenu.CategoryName),
                        50);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        return;
                    }

                    await API.Delay(500);
                    
                    string? description = await UserInput.GetTextAsync(
                        MenuText.Key(Loc.TeleportMenu.CategoryDescription),
                        50);
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        return;
                    }
                    var position = API.Players.Local.Position;
                    loc.Add(new (name, description, new ()));
                    API.Log.Debug(JsonConvert.SerializeObject(loc));
                },
            });
        };
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.TeleportMenu.TeleportCategories),
            Description = MenuText.Key(Loc.TeleportMenu.TeleportCategoriesDescription),
            OnSelected = _ => actions.Open()
        });
    }
    private static Blip? GetWaypointBlip()
    {
        if (!Native.IsWaypointActive())
        {
            return null;
        }

        for (int it = Native.GetBlipInfoIdIterator(), blip = Native.GetFirstBlipInfoId(it); Native.DoesBlipExist(blip); blip = Native.GetNextBlipInfoId(it))
        {
            if (Native.GetBlipInfoIdType(blip) == 4)
            {
                return new Blip(blip);
            }
        }

        return null;
    }
}
