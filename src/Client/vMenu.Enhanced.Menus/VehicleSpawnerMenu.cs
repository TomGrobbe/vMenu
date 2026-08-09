using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Data;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Permissions;

using VehicleSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleSpawner;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Spawns vehicles, grouped by category: the game's own vehicle classes, plus whatever the server
/// owner defined in <c>config/vehicle-categories.json</c>.
/// </summary>
[VMenu(
    TitleKey = Loc.VehicleSpawner.Title,
    SubtitleKey = Loc.VehicleSpawner.Subtitle,
    DescriptionKey = Loc.VehicleSpawner.LinkDescription,
    Permission = VehicleSpawnerPermissions.Menu)]
public sealed class VehicleSpawnerMenu : MenuDefinition
{
    private VehicleCategory[] _categories = [];

    private (string Model, int Class, string Label, string CategoryName, string Icon)[] _describedVehicles = [];

    public override Task PrepareAsync()
    {
        var vehicles = BrokenNatives.NativeFixer.GetAllVehicleModels();

        if (vehicles is null)
        {
            // The menu still exists, it just has no categories in it. Leaving it half-built would be
            // worse than leaving it empty.
            return Task.CompletedTask;
        }

        var described = vehicles
            .Where(vehicle => !string.IsNullOrWhiteSpace(vehicle))
            .Select(vehicle =>
            {
                var model = vehicle.Trim();
                var hash = API.Hash(model);

                return (
                    Model: model,
                    Class: Native.GetVehicleClassFromName(hash),
                    Custom: ClientVehiclePermissions.CategoryOfModel(model),
                    Label: GetVehicleDisplayName(hash));
            })
            .OrderBy(vehicle => vehicle.Label)
                .ThenBy(vehicle => vehicle.Model)
            .ToArray();

        // A game class every model left behind produces no group at all, so it gets no submenu.
        var gameClasses = described
            .Where(vehicle => vehicle.Custom is null)
            .GroupBy(vehicle => vehicle.Class)
            .Select(group => new VehicleCategory
            {
                Name = ClassName(group.Key),
                Title = MenuText.From(() => ClassName(group.Key)),
                Gate = () => ClientVehiclePermissions.CanSpawnVehicleClass(group.Key),
                Vehicles = [.. group.Select(vehicle => (vehicle.Model, vehicle.Class, vehicle.Label))],
            });

        var custom = described
            .Where(vehicle => vehicle.Custom is not null)
            .GroupBy(vehicle => vehicle.Custom!, StringComparer.OrdinalIgnoreCase)
            .Select(group => new VehicleCategory
            {
                Name = group.Key,
                Title = MenuText.Literal(group.Key),
                Gate = () => ClientVehiclePermissions.CanSpawnCustomCategory(group.Key),
                Vehicles = [.. group.Select(vehicle => (vehicle.Model, vehicle.Class, vehicle.Label))],
            });

        _categories = [.. gameClasses.Concat(custom).OrderBy(category => category.Name)];

        _describedVehicles = [.. _categories.SelectMany(category => category.Vehicles.Select(vehicle =>
            (vehicle.Model, vehicle.Class, vehicle.Label, category.Name, VehicleIcon(API.Hash(vehicle.Model), vehicle.Class))))];

        return Task.CompletedTask;
    }

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.VehicleSpawner.SpawnByCategory),
            Description = MenuText.Key(Loc.VehicleSpawner.SpawnByCategoryDescription),
            MenuTitle = MenuText.Key(Loc.VehicleSpawner.Title),
            MenuSubtitle = MenuText.Key(Loc.VehicleSpawner.SpawnByCategorySubtitle),
            Build = BuildCategoryList,
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleSpawner.SpawnByName),
            Description = MenuText.Key(Loc.VehicleSpawner.SpawnByNameDescription),
            Gate = VehicleSpawnerPermissions.SpawnByName,
            OnSelectedAsync = _ => SpawnByNameAsync(),
        });
    }

    private void BuildCategoryList(MenuBuilder byCategory)
    {
        foreach (var category in _categories)
        {
            // Copied out of the loop variable so each entry's callbacks capture its own category.
            var current = category;

            byCategory.Entries.Add(new SubmenuEntry
            {
                Text = current.Title,
                Description = MenuText.Key(
                    Loc.VehicleSpawner.CategoryDescription,
                    ("category", current.Title)),
                Label = MenuText.Literal("→"),
                MenuTitle = current.Title,
                MenuSubtitle = MenuText.Key(Loc.VehicleSpawner.CategorySubtitle),
                Gate = MenuGate.When(current.Gate),
                Build = categoryMenu => BuildCategoryMenu(categoryMenu, current.Vehicles),
            });
        }
    }

    /// <summary>
    /// Vehicles keep their own class id here, whichever category they ended up in, because the stats
    /// and the icon are read off the game's class rather than off the grouping.
    /// </summary>
    private static void BuildCategoryMenu(MenuBuilder categoryMenu, (string Model, int Class, string Label)[] vehicles)
    {
        foreach (var vehicle in vehicles)
        {
            var modelName = vehicle.Model;
            var classId = vehicle.Class;
            var stats = VehicleClassStats.Normalise(API.Hash(modelName), classId);

            categoryMenu.Entries.Add(new ButtonEntry
            {
                // Model names are data, not prose, so they are never looked up as a key.
                Text = MenuText.Literal(vehicle.Label),
                Label = MenuText.Literal(modelName),
                Gate = MenuGate.When(() => ClientVehiclePermissions.CanSpawnVehicle(modelName, classId)),
                VehicleStats = () => stats,
                OnSelectedAsync = _ => SpawnVehicleAsync(modelName, classId),
            });
        }
    }

    private async Task SpawnByNameAsync()
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.VehicleSpawner.SpawnByNamePrompt),
            maxLength: 30,
            suggestions: SpawnableSuggestions());

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var modelName = typed.Trim();
        var hash = API.Hash(modelName);

        if (!Native.IsModelValid(hash) || !Native.IsModelAVehicle(hash))
        {
            Notifications.Error(MenuText.Key(Loc.VehicleSpawner.SpawnByNameInvalid, ("model", MenuText.Literal(modelName))));
            return;
        }

        var vehicleClass = Native.GetVehicleClassFromName(hash);

        if (!ClientVehiclePermissions.CanSpawnVehicle(modelName, vehicleClass))
        {
            Notifications.Warning(MenuText.Key(Loc.VehicleSpawner.SpawnByNameDenied, ("model", MenuText.Literal(modelName))));
            return;
        }

        await SpawnVehicleAsync(modelName, vehicleClass);
    }

    /// <summary>Built per opening: a permission refresh in between changes what belongs in it.</summary>
    private IReadOnlyList<InputSuggestion> SpawnableSuggestions() =>
        [.. _describedVehicles
            .Where(vehicle => ClientVehiclePermissions.CanSpawnVehicle(vehicle.Model, vehicle.Class))
            .Select(vehicle => new InputSuggestion
            {
                Value = vehicle.Model,
                Label = vehicle.Label,
                Icon = vehicle.Icon,
                Detail = vehicle.CategoryName,
            })];

    /// <summary>
    /// These natives answer for a model hash, unlike <c>GET_VEHICLE_TYPE_RAW</c>, which needs a
    /// vehicle that exists.
    /// </summary>
    private static string VehicleIcon(uint hash, int vehicleClass)
    {
        if (Native.IsThisModelATrain(hash))
        {
            return "train";
        }

        if (Native.IsThisModelAPlane(hash))
        {
            return "plane";
        }

        if (Native.IsThisModelAHeli(hash))
        {
            return "heli";
        }

        if (Native.IsThisModelABicycle(hash))
        {
            return "bicycle";
        }

        if (Native.IsThisModelABike(hash))
        {
            return "motorcycle";
        }

        if (Native.IsThisModelAQuadbike(hash) || Native.IsThisModelAnAmphibiousQuadbike(hash))
        {
            return "quad";
        }

        if (Native.IsThisModelASubmersible(hash))
        {
            return "submarine";
        }

        if (Native.IsThisModelABoat(hash) || Native.IsThisModelAnEmergencyBoat(hash) || Native.IsThisModelAJetski(hash))
        {
            return "boat";
        }

        // A blimp is its own type, a trailer is none of these: what the natives have no word for
        // follows its class instead.
        return vehicleClass switch
        {
            8 => "motorcycle",
            13 => "bicycle",
            14 => "boat",
            15 => "heli",
            16 => "plane",
            21 => "train",
            10 or 11 or 20 => "truck",
            12 => "van",
            _ => "car",
        };
    }

    /// <summary>The game already returns these in the player's game language.</summary>
    private static string ClassName(int vehicleClass) => Native.GetLabelText($"VEH_CLASS_{vehicleClass}");

    /// <summary>
    /// One submenu's worth of vehicles, either a game class or one the server owner defined.
    /// </summary>
    // A plain class rather than a record: the generated equality would route through
    // EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class VehicleCategory
    {
        /// <summary>Resolved once, for the alphabetical ordering and the by name suggestions.</summary>
        public required string Name { get; init; }

        public required MenuText Title { get; init; }

        public required Func<bool> Gate { get; init; }

        public required (string Model, int Class, string Label)[] Vehicles { get; init; }
    }

    private static string GetVehicleDisplayName(uint hash) => VehicleSpawning.DisplayName(hash);

    private static async Task SpawnVehicleAsync(string modelName, int vehicleClass)
    {
        // Re-checked because a permission refresh can land between drawing and selecting. The server
        // decides for real; this only avoids doing the work.
        if (!ClientVehiclePermissions.CanSpawnVehicle(modelName, vehicleClass))
        {
            return;
        }

        if (await VehicleSpawning.SpawnAsync(modelName) is null)
        {
            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.VehicleSpawner.Spawned,
            ("vehicle", MenuText.Literal(GetVehicleDisplayName(API.Hash(modelName))))));
    }
}
