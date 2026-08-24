using System.Reflection;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// A VMenuAttribute turned into the types the framework actually uses. Cached per type: one attribute
// lookup on an already instantiated type, not an assembly scan, which would be the wrong cost and
// would not work anyway since CitizenFX only discovers types in the client_script assembly.
internal sealed class VMenuMetadata
{
    private VMenuMetadata(VMenuAttribute? attribute)
    {
        HasAttribute = attribute is not null;

        Title = attribute?.TitleKey is { } title ? MenuText.Key(title) : MenuText.Empty;
        Subtitle = attribute?.SubtitleKey is { } subtitle ? MenuText.Key(subtitle) : MenuText.Empty;
        LinkDescription = attribute?.DescriptionKey is { } description ? MenuText.Key(description) : MenuText.Empty;
        LinkLabel = attribute?.LinkLabel is { } label ? MenuText.Literal(label) : MenuText.Empty;
        Gate = attribute?.Permission is { } permission ? MenuGate.Permission(permission) : MenuGate.Always;
    }

    public bool HasAttribute { get; }

    public MenuText Title { get; }

    public MenuText Subtitle { get; }

    public MenuText LinkDescription { get; }

    public MenuText LinkLabel { get; }

    public MenuGate Gate { get; }

    // Not cached here: MenuDefinition already holds the result per instance, and a static
    // Dictionary<Type, ...> would be one more default comparer for the sandbox to object to. Guarded
    // because the sandbox decides at call time which framework members an assembly may touch, and a
    // refusal here would take the resource down during startup.
    public static VMenuMetadata For(Type type)
    {
        try
        {
            return new VMenuMetadata(type.GetCustomAttribute<VMenuAttribute>());
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] Could not read [VMenu] from {type.Name}, falling back to its own properties: {exception}");

            return new VMenuMetadata(null);
        }
    }
}
