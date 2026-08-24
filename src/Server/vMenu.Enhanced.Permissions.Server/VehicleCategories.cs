using System.Text.Json;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Permissions.Server;

// Vehicle categories a server owner defined, which take their models out of the game class they
// would otherwise fall in. Each one gets a permission of its own, registered next to the built in
// category permissions so it behaves exactly like them.
public static class VehicleCategories
{
    private const string ConfigFile = "config/vehicle-categories.json";

    // Tolerant on purpose: server owners hand-edit this file.
    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly Dictionary<string, string> CategoryByModel = new(StringComparer.OrdinalIgnoreCase);

    private static string[] _models = [];
    private static string[] _categories = [];

    // A missing or unreadable file just means every vehicle stays in its game class.
    public static void LoadAndRegister()
    {
        CategoryByModel.Clear();
        _models = [];
        _categories = [];

        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Warning($"[Permissions] No {ConfigFile} found. Every vehicle stays in its own game class.");
            return;
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(contents, ParseOptions);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Permissions] {ConfigFile} could not be parsed, so no custom categories exist: {exception.Message}");
            return;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                Log.Error($"[Permissions] {ConfigFile} has to hold a single object of categories, so no custom categories exist.");
                return;
            }

            Register(document.RootElement);
        }
    }

    // The category a model was moved into, or null when it is still in its game class.
    public static string? CategoryOfModel(string modelName) =>
        CategoryByModel.TryGetValue(modelName, out var category) ? category : null;

    public static string PermissionOfCategory(string categoryName) =>
        VehicleSpawnerCategories.ForCustom(CategoryName.ToPermissionSegment(categoryName));

    // Every categorised model, for sending to clients. Aligned with GetCategoryNames.
    public static string[] GetCategorisedModels() => _models;

    public static string[] GetCategoryNames() => _categories;

    private static void Register(JsonElement root)
    {
        var segments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var models = new List<string>();
        var categories = new List<string>();

        foreach (var property in root.EnumerateObject())
        {
            var name = property.Name.Trim();
            var segment = CategoryName.ToPermissionSegment(name);

            if (segment.Length == 0)
            {
                Log.Warning($"[Permissions] Skipping category '{property.Name}': its name has no letters or digits in it, so it could never be granted.");
                continue;
            }

            var permission = VehicleSpawnerCategories.ForCustom(segment);

            // A name matching one vMenu declares itself would quietly hijack that permission.
            if (PermissionRegistry.TryGet(permission, out _))
            {
                Log.Warning($"[Permissions] Skipping category '{name}': '{permission}' is a permission vMenu already declares, so pick a name that is not one of the game's own vehicle classes.");
                continue;
            }

            if (!segments.Add(segment))
            {
                Log.Warning($"[Permissions] Skipping category '{name}': another category already claims '{permission}'.");
                continue;
            }

            if (property.Value.ValueKind != JsonValueKind.Array)
            {
                Log.Warning($"[Permissions] Skipping category '{name}': its value has to be a list of vehicle model names.");
                continue;
            }

            var claimed = Claim(property.Value, name, models, categories);

            if (claimed == 0)
            {
                Log.Warning($"[Permissions] Skipping category '{name}': it has no vehicles in it, so it would show up empty.");
                continue;
            }

            PermissionRegistry.RegisterDynamic(permission, ConfigFile);

            Log.Info($"[Permissions] Category '{name}' holds {claimed} vehicle(s) and is granted by '{permission}'.");
        }

        _models = [.. models];
        _categories = [.. categories];
    }

    private static int Claim(JsonElement array, string category, List<string> models, List<string> categories)
    {
        var claimed = 0;

        foreach (var element in array.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.String || element.GetString()?.Trim() is not { Length: > 0 } model)
            {
                continue;
            }

            model = model.ToLowerInvariant();

            if (CategoryByModel.TryGetValue(model, out var owner))
            {
                Log.Warning($"[Permissions] '{model}' is listed under both '{owner}' and '{category}', so it stays in '{owner}'.");
                continue;
            }

            CategoryByModel[model] = category;
            models.Add(model);
            categories.Add(category);
            claimed++;
        }

        return claimed;
    }
}
