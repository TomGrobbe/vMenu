using System.Text;

using CitizenFX.Base;
using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Extensions;

using MenuAPI;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Placeholder so the project produces a valid assembly. Replace with the
/// client core (bootstrap, common functions, controllers) as the port lands.
/// </summary>
public sealed class VehicleSpawnerMenu
{



    public async Task<Menu> GetMenu()
    {

        var menu = new Menu("Vehicle Spawner", "Vehicle Spawner Menu");
        MenuController.AddSubmenu(MenuController.MainMenu, menu);
        var linkBtn = new MenuItem("VehicleSpawner Menu", "Temporary vehicle spawner menu.") { Label = "»»»" };
        MenuController.MainMenu.AddMenuItem(linkBtn);
        MenuController.BindMenuItem(MenuController.MainMenu, menu, linkBtn);

        API.Log.Info("Calling something.GetVehicleList now");
        var data = API.Exports["something"].Call<string>("GetVehicleList");

        // This is broken due to crash: https://github.com/citizenfx/rfc/discussions/328
        //var vehicles = System.Text.Json.JsonSerializer.Deserialize<string[]>(data);

        API.Log.Info("Got vehicle list data: " + data);
        var vehicles = data.Replace("[", "").Replace("]", "").Replace("\"", "").Split(',');

        if (vehicles is null)
        {
            API.Log.Warn("Vehicle list is null");
            return menu;
        }

        var vehicleSperCategory = vehicles
            .Where(veh => !string.IsNullOrWhiteSpace(veh))
            .Select(veh => veh.Trim())
            .OrderBy(vehicle => vehicle)
            .GroupBy(vehicle => Native.GetVehicleClassFromName(API.Hash(vehicle)));

        API.Log.Info("Vehicle list categorized and cleaned up");



        foreach (var cat in vehicleSperCategory)
        {
            var submenu = new Menu("Class: " + cat.Key.ToString(), "Vehicle Spawner Menu");
            foreach (var vehicle in cat)
            {
                submenu.AddMenuItem(new MenuItem(vehicle));
            }

            submenu.OnItemSelect += Submenu_OnItemSelect;
            MenuController.AddSubmenu(menu, submenu);
            var btn = new MenuItem("Veh Class: " + cat.Key.ToString());
            menu.AddMenuItem(btn);
            MenuController.BindMenuItem(menu, submenu, btn);
            API.Log.Info("Added submenu for vehicle class: {0}", cat.Key.ToString());
        }

        return menu;
    }


    private async void Submenu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
    {
        var veh = await API.Vehicles.RequestAndCreate(API.Hash(menuItem.Text), API.Players.Local.Ped!.Position, (int)API.Players.Local.Ped.Heading, true, true, true);
        if (veh is not null)
        {
            API.Players.Local.Ped.SetPedIntoVehicle(veh!.Handle, -1);
        }
    }
}
