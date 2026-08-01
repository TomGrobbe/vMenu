using System.Globalization;
using System.Numerics;

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
    internal static NativeApi nativeApi = BaseEntrypoint.NativeApi;
    private static readonly TextInfo _textInfo = new CultureInfo("en-US", false).TextInfo;

    public async Task Initialize()
    {
        var menu = new Menu("Vehicle Spawner", "Vehicle Spawner Menu");
        MenuController.AddSubmenu(MenuController.MainMenu, menu);
        var linkBtn = new MenuItem("VehicleSpawner Menu", "Temporary vehicle spawner menu.") { Label = "→" };
        MenuController.MainMenu.AddMenuItem(linkBtn);
        MenuController.BindMenuItem(MenuController.MainMenu, menu, linkBtn);

        MenuItem spawnByClassBtn = new MenuItem("Spawn Vehicle By Class", "Spawn a vehicle from a list of vehicle classes.") { Label = "→" };
        Menu spawnByClassMenu = new Menu("Vehicle Spawner", "Spawn vehicles by class");
        menu.AddMenuItem(spawnByClassBtn);
        MenuController.AddSubmenu(menu, spawnByClassMenu);
        MenuController.BindMenuItem(menu, spawnByClassMenu, spawnByClassBtn);

        var vehicles = BrokenNatives.NativeFixer.GetAllVehicleModels();

        if (vehicles is null)
        {
            return;
        }

        var vehiclesPerCategory = vehicles
            .Where(veh => !string.IsNullOrWhiteSpace(veh))
            .Select(veh => veh.Trim())
            .OrderBy(vehicle => vehicle)
            .GroupBy(vehicle => Native.GetVehicleClassFromName(API.Hash(vehicle)))
            .OrderBy(vehicle => Native.GetLabelText($"VEH_CLASS_{vehicle.Key}"));

        // These are the max speed, acceleration, braking and traction values per vehicle class.
        var speedValues = new float[23]
        {
                44.9374657f,
                50.0000038f,
                48.862133f,
                48.1321335f,
                50.7077942f,
                51.3333359f,
                52.3922348f,
                53.86687f,
                52.03867f,
                49.2241631f,
                39.6176529f,
                37.5559425f,
                42.72843f,
                21.0f,
                45.0f,
                65.1952744f,
                109.764259f,
                42.72843f,
                56.5962219f,
                57.5398865f,
                43.3140678f,
                26.66667f,
                53.0537224f
        };
        var accelerationValues = new float[23]
        {
                0.34f,
                0.29f,
                0.335f,
                0.28f,
                0.395f,
                0.39f,
                0.66f,
                0.42f,
                0.425f,
                0.475f,
                0.21f,
                0.3f,
                0.32f,
                0.17f,
                18.0f,
                5.88f,
                21.0700016f,
                0.33f,
                14.0f,
                6.86f,
                0.32f,
                0.2f,
                0.76f
        };
        var brakingValues = new float[23]
        {
                0.72f,
                0.95f,
                0.85f,
                0.9f,
                1.0f,
                1.0f,
                1.3f,
                1.25f,
                1.52f,
                1.1f,
                0.6f,
                0.7f,
                0.8f,
                3.0f,
                0.4f,
                3.5920403f,
                20.58f,
                0.9f,
                2.93960738f,
                3.9472363f,
                0.85f,
                5.0f,
                1.3f
        };
        var tractionValues = new float[23]
        {
                2.3f,
                2.55f,
                2.3f,
                2.6f,
                2.625f,
                2.65f,
                2.8f,
                2.782f,
                2.9f,
                2.95f,
                2.0f,
                3.3f,
                2.175f,
                2.05f,
                0.0f,
                1.6f,
                2.15f,
                2.55f,
                2.57f,
                3.7f,
                2.05f,
                2.5f,
                3.2925f
        };

        foreach (var cat in vehiclesPerCategory)
        {
            var className = Native.GetLabelText($"VEH_CLASS_{cat.Key}");

            var vehicleClassSubMenu = new Menu(className, "Vehicle Spawner Menu");
            foreach (var vehicle in cat)
            {
                var hash = API.Hash(vehicle);
                var topSpeed = Map(Native.GetVehicleModelEstimatedMaxSpeed(hash), 0f, speedValues[cat.Key], 0f, 1f);
                var acceleration = Map(Native.GetVehicleModelAcceleration(hash), 0f, accelerationValues[cat.Key], 0f, 1f);
                var maxBraking = Map(Native.GetVehicleModelMaxBraking(hash), 0f, brakingValues[cat.Key], 0f, 1f);
                var maxTraction = Map(Native.GetVehicleModelMaxTraction(hash), 0f, tractionValues[cat.Key], 0f, 1f);

                var vehicleDisplayName = GetVehicleDisplayName(hash);
                var vehicleSpawnButton = new MenuItem(vehicleDisplayName)
                {
                    Label = vehicle,
                    ItemData = new float[4] { topSpeed, acceleration, maxBraking, maxTraction }
                };

                vehicleClassSubMenu.AddMenuItem(vehicleSpawnButton);
            }

            static void HandleStatsPanel(Menu openedMenu, MenuItem currentItem)
            {
                if (currentItem != null)
                {
                    if (currentItem.ItemData is float[] data)
                    {
                        openedMenu.ShowVehicleStatsPanel = true;
                        openedMenu.SetVehicleStats(data[0], data[1], data[2], data[3]);
                        openedMenu.SetVehicleUpgradeStats(0f, 0f, 0f, 0f);
                    }
                    else
                    {
                        openedMenu.ShowVehicleStatsPanel = false;
                    }
                }
            }

            vehicleClassSubMenu.OnMenuOpen += (m) =>
            {
                HandleStatsPanel(m, m.GetCurrentMenuItem());
            };

            vehicleClassSubMenu.OnIndexChange += (m, oldItem, newItem, oldIndex, newIndex) =>
            {
                HandleStatsPanel(m, newItem);
            };

            vehicleClassSubMenu.OnItemSelect += Submenu_OnItemSelect;

            var btn = new MenuItem(className, $"Spawn a vehicle from the ~y~{className}~s~ class.") { Label = $"({cat.Count()}) →" };
            spawnByClassMenu.AddMenuItem(btn);
            MenuController.AddSubmenu(spawnByClassMenu, vehicleClassSubMenu);
            MenuController.BindMenuItem(spawnByClassMenu, vehicleClassSubMenu, btn);
        }
    }

    private static string GetVehicleDisplayName(uint hash)
    {
        var displayName = Native.GetDisplayNameFromVehicleModel(hash);
        var labelText = Native.GetLabelText(displayName);
        if (labelText == "NULL")
        {
            return _textInfo.ToTitleCase(displayName);
        }

        return _textInfo.ToTitleCase(labelText);
    }

    /// <summary>
    /// Maps the <paramref name="value"/> (which is a value between <paramref name="min_in"/> and <paramref name="max_in"/>) to a new value in the range of <paramref name="min_out"/> and <paramref name="max_out"/>.
    /// </summary>
    /// <param name="value">The value to map.</param>
    /// <param name="min_in">The minimum range value of the value.</param>
    /// <param name="max_in">The max range value of the value.</param>
    /// <param name="min_out">The min output range value.</param>
    /// <param name="max_out">The max output range value.</param>
    /// <returns></returns>
    public static float Map(float value, float min_in, float max_in, float min_out, float max_out)
    {
        return ((value - min_in) * (max_out - min_out) / (max_in - min_in)) + min_out;
    }


    private async void Submenu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
    {
        var hash = API.Hash(menuItem.Label);

        // Manually checking and requesting model because API.Vehicles.RequestAndCreate uses datetime which is currently broken and crashes the game.
        // https://github.com/citizenfx/rfc/discussions/328
        if (!Native.IsModelValid(hash))
        {
            return;
        }

        Native.RequestModel(hash);

        while (!Native.HasModelLoaded(hash))
        {
            await API.Delay(0);
        }

        var ped = API.Players.Local.Ped!;

        var position = ped.Position;
        Vector3? velocity = null;
        if (ped.IsPedInAnyVehicle())
        {
            var currentVehicle = ped.Vehicle!;

            BrokenNatives.NativeFixer.GetModelDimensions(currentVehicle.Model, out var p1, out var p2);
            BrokenNatives.NativeFixer.GetModelDimensions(hash, out var p3, out var p4);

            var yOffset = (Math.Abs((p1 - p2).Y) / 2) + (Math.Abs((p3 - p4).Y) / 2) + 1f;
            position = Native.GetOffsetFromEntityInWorldCoords(currentVehicle.Handle, 0f, yOffset, 0f);

            velocity = currentVehicle.Velocity;
        }

        var newVehicle = await API.Vehicles.RequestAndCreate(hash, position, (int)ped.Heading, true, true, true);

        Native.SetModelAsNoLongerNeeded(hash);

        if (newVehicle is null)
        {
            return;
        }
        Native.SetVehicleEngineOn(VehicleIndex: newVehicle.Handle, EngineOnFlag: true, bNoDelay: true, bOnlyStartWithPlayerInput: false);

        if ((Native.IsThisModelAHeli(hash) is bool isHeli && isHeli) || Native.IsThisModelAPlane(hash))
        {
            newVehicle.HeliBladesSpeed = 1f;

            if (isHeli)
            {
                Native.SetHeliTurbulenceScalar(newVehicle.Handle, 0f);
            }
            else
            {
                Native.SetPlaneTurbulenceMultiplier(newVehicle.Handle, 0f);
            }
        }

        if (velocity.HasValue)
        {
            newVehicle.Velocity = velocity.Value;
        }

        ped.SetPedIntoVehicle(newVehicle!.Handle, -1);
    }
}
