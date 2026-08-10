using System.Text.Json;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

namespace vMenu.Enhanced.Permissions.Server;

/// <summary>
/// Models held back from the class permissions and given their own permission instead.
/// </summary>
public static class ModelWhitelist
{
    private const string ConfigFile = "config/model-whitelists.json";

    // Tolerant on purpose: server owners hand-edit this file.
    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>Wiring up weapons later means supplying a factory here.</summary>
    private static readonly KindDescriptor[] Descriptors =
    [
        new(SupplementalModelKind.Vehicle, "vehicles", VehicleModels.ForModel),
        new(SupplementalModelKind.Ped, "peds", Peds.ForModel),
        new(SupplementalModelKind.Weapon, "weapons", null),
    ];

    private static readonly Dictionary<SupplementalModelKind, HashSet<string>> Whitelists = [];
    private static readonly Dictionary<SupplementalModelKind, string[]> Ordered = [];

    /// <summary>
    /// Reads the config file and registers a permission for every whitelisted model. A missing or
    /// unreadable file just means nothing is held back.
    /// </summary>
    public static void LoadAndRegister()
    {
        Whitelists.Clear();
        Ordered.Clear();

        foreach (var descriptor in Descriptors)
        {
            Whitelists[descriptor.Kind] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Ordered[descriptor.Kind] = [];
        }

        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            API.Log.Info($"[Permissions] No {ConfigFile} found. Every model is governed by its class permission.");
            return;
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(contents, ParseOptions);
        }
        catch (JsonException exception)
        {
            API.Log.Error($"[Permissions] {ConfigFile} could not be parsed, so no models are whitelisted: {exception.Message}");
            return;
        }

        using (document)
        {
            foreach (var descriptor in Descriptors)
            {
                Register(descriptor, ReadModels(document, descriptor.JsonProperty));
            }
        }
    }

    public static bool IsWhitelisted(SupplementalModelKind kind, string modelName) =>
        Whitelists.TryGetValue(kind, out var models) && models.Contains(modelName);

    public static bool IsWhitelistedVehicle(string modelName) =>
        IsWhitelisted(SupplementalModelKind.Vehicle, modelName);

    /// <summary>
    /// The whitelisted models of a kind, for sending to clients so they can tell which models
    /// their class permissions do not cover.
    /// </summary>
    public static string[] GetModels(SupplementalModelKind kind) =>
        Ordered.TryGetValue(kind, out var models) ? models : [];

    private static void Register(KindDescriptor descriptor, List<string> models)
    {
        var accepted = Whitelists[descriptor.Kind];

        foreach (var model in models)
        {
            // A name with a space or a dot would produce an ACE nobody could write.
            if (!PermissionPath.IsValidSegment(model))
            {
                API.Log.Warn($"[Permissions] Skipping whitelisted {descriptor.JsonProperty} entry '{model}': only letters, digits and underscores are usable in a permission.");
                continue;
            }

            if (!accepted.Add(model))
            {
                API.Log.Warn($"[Permissions] '{model}' is listed more than once under '{descriptor.JsonProperty}'.");
                continue;
            }

            if (descriptor.PermissionFactory is not null)
            {
                PermissionRegistry.RegisterDynamic(descriptor.PermissionFactory(model), ConfigFile);
            }
        }

        Ordered[descriptor.Kind] = [.. accepted.OrderBy(static model => model, StringComparer.Ordinal)];

        if (accepted.Count > 0)
        {
            API.Log.Info($"[Permissions] Loaded {accepted.Count} whitelisted model(s) from '{descriptor.JsonProperty}'.");
        }
    }

    private static List<string> ReadModels(JsonDocument document, string propertyName)
    {
        if (!document.RootElement.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var models = new List<string>(array.GetArrayLength());

        foreach (var element in array.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String && element.GetString() is { } value && value.Trim() is { Length: > 0 } model)
            {
                models.Add(model.ToLowerInvariant());
            }
        }

        return models;
    }

    /// <param name="PermissionFactory">Null when the kind is read but not yet wired to permissions.</param>
    private sealed record KindDescriptor(
        SupplementalModelKind Kind,
        string JsonProperty,
        Func<string, string>? PermissionFactory);
}
