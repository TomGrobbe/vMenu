using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Extensions;
using CitizenFX.FiveM.Shared.Data;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Data;
using vMenu.Enhanced.Permissions;

using VehicleSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleSpawner;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Spawns vehicles, grouped by the game's own vehicle classes.
/// </summary>
[VMenu(
    TitleKey = Loc.VehicleSpawner.Title,
    SubtitleKey = Loc.VehicleSpawner.Subtitle,
    DescriptionKey = Loc.VehicleSpawner.LinkDescription,
    Permission = VehicleSpawnerPermissions.Menu)]
public sealed class VehicleSpawnerMenu : MenuDefinition
{
    private static readonly TextInfo TitleCase = new CultureInfo("en-US", false).TextInfo;

    private IGrouping<int, string>[] _vehiclesPerClass = [];

    public override Task PrepareAsync()
    {
        var vehicles = BrokenNatives.NativeFixer.GetAllVehicleModels();

        if (vehicles is null)
        {
            // The menu still exists, it just has no classes in it. Leaving it half-built would be
            // worse than leaving it empty.
            return Task.CompletedTask;
        }

        _vehiclesPerClass = [.. vehicles
            .Where(vehicle => !string.IsNullOrWhiteSpace(vehicle))
            .Select(vehicle => vehicle.Trim())
            .OrderBy(vehicle => vehicle)
            .GroupBy(vehicle => Native.GetVehicleClassFromName(API.Hash(vehicle)))
            .OrderBy(category => ClassName(category.Key))];

        return Task.CompletedTask;
    }

    protected override void Build(MenuBuilder menu) =>
        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.VehicleSpawner.SpawnByClass),
            Description = MenuText.Key(Loc.VehicleSpawner.SpawnByClassDescription),
            MenuTitle = MenuText.Key(Loc.VehicleSpawner.Title),
            MenuSubtitle = MenuText.Key(Loc.VehicleSpawner.SpawnByClassSubtitle),
            Build = BuildClassList,
        });

    private void BuildClassList(MenuBuilder byClass)
    {
        foreach (var category in _vehiclesPerClass)
        {
            // Copied out of the loop variable so each entry's callbacks capture its own class.
            var classId = category.Key;
            var models = category.ToArray();

            byClass.Entries.Add(new SubmenuEntry
            {
                Text = MenuText.From(() => ClassName(classId)),
                Description = MenuText.Key(
                    Loc.VehicleSpawner.ClassDescription,
                    ("class", MenuText.From(() => ClassName(classId)))),
                Label = MenuText.Literal($"({models.Length}) →"),
                MenuTitle = MenuText.From(() => ClassName(classId)),
                MenuSubtitle = MenuText.Key(Loc.VehicleSpawner.ClassSubtitle),
                Gate = MenuGate.When(() => ClientVehiclePermissions.CanSpawnVehicleClass(classId)),
                Build = classMenu => BuildClassMenu(classMenu, classId, models),
            });
        }
    }

    private static void BuildClassMenu(MenuBuilder classMenu, int classId, string[] models)
    {
        foreach (var model in models)
        {
            var modelName = model;
            var hash = API.Hash(modelName);
            var stats = VehicleClassStats.Normalise(hash, classId);

            classMenu.Entries.Add(new ButtonEntry
            {
                // Model names are data, not prose, so they are never looked up as a key.
                Text = MenuText.Literal(GetVehicleDisplayName(hash)),
                Label = MenuText.Literal(modelName),
                Gate = MenuGate.When(() => ClientVehiclePermissions.CanSpawnVehicle(modelName, classId)),
                VehicleStats = () => stats,
                OnSelectedAsync = _ => SpawnVehicleAsync(modelName, classId),
            });
        }
    }

    /// <summary>The game already returns these in the player's game language.</summary>
    private static string ClassName(int vehicleClass) => Native.GetLabelText($"VEH_CLASS_{vehicleClass}");

    private static string GetVehicleDisplayName(uint hash)
    {
        var displayName = Native.GetDisplayNameFromVehicleModel(hash);
        var labelText = Native.GetLabelText(displayName);

        return TitleCase.ToTitleCase(labelText == "NULL" ? displayName : labelText);
    }

    private static async Task SpawnVehicleAsync(string modelName, int vehicleClass)
    {
        var hash = API.Hash(modelName);

        // Manually checking and requesting the model because API.Vehicles.RequestAndCreate uses
        // DateTime, which is currently broken and crashes the game.
        // https://github.com/citizenfx/rfc/discussions/328
        if (!Native.IsModelValid(hash))
        {
            return;
        }

        // Re-checked because a permission refresh can land between drawing and selecting. The server
        // decides for real; this only avoids doing the work.
        if (!ClientVehiclePermissions.CanSpawnVehicle(modelName, vehicleClass))
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
        var rpm = 100f;
        var speed = 0f;

        if (ped.IsPedInAnyVehicle())
        {
            var currentVehicle = ped.Vehicle!;

            BrokenNatives.NativeFixer.GetModelDimensions(currentVehicle.Model, out var currentMin, out var currentMax);
            BrokenNatives.NativeFixer.GetModelDimensions(hash, out var spawnedMin, out var spawnedMax);

            var yOffset = (Math.Abs((currentMin - currentMax).Y) / 2) + (Math.Abs((spawnedMin - spawnedMax).Y) / 2) + 1f;
            position = Native.GetOffsetFromEntityInWorldCoords(currentVehicle.Handle, 0f, yOffset, 0f);

            velocity = currentVehicle.Velocity;
            speed = Native.GetEntitySpeedVector(currentVehicle.Handle, true).Y;
            rpm = Native.GetVehicleCurrentRpm(currentVehicle.Handle);

            var handle = currentVehicle.Handle;
            Native.SetEntityAsNoLongerNeeded(new Ref<int>(ref handle));
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

        Native.SetVehicleForwardSpeed(newVehicle.Handle, speed);

        if (velocity.HasValue)
        {
            newVehicle.Velocity = velocity.Value;
        }

        Native.SetVehicleCurrentRpm(newVehicle.Handle, rpm);

        ped.SetPedIntoVehicle(newVehicle.Handle, -1);
    }
}
