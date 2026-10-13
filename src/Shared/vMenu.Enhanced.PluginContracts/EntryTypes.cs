namespace vMenu.Enhanced.PluginContracts;

/// <summary>The menu item kinds a plugin can declare, mirroring the vMenu MenuFramework entry types.</summary>
public static class EntryTypes
{
    public const string Button = "button";
    public const string Checkbox = "checkbox";
    public const string List = "list";
    public const string Slider = "slider";
    public const string DynamicList = "dynamicList";
    public const string Submenu = "submenu";
    public const string Separator = "separator";
    public const string ConfirmButton = "confirmButton";
    public const string ConfirmList = "confirmList";
}
