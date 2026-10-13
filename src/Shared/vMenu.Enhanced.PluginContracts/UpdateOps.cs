namespace vMenu.Enhanced.PluginContracts;

/// <summary>The operation names an <see cref="UpdateOp"/> can carry.</summary>
public static class UpdateOps
{
    // Presentation.
    public const string SetText = "setText";
    public const string SetDescription = "setDescription";
    public const string SetLabel = "setLabel";
    public const string SetLockedDescription = "setLockedDescription";
    public const string SetConfirmationDescription = "setConfirmationDescription";
    public const string SetIcons = "setIcons";

    // State.
    public const string SetChecked = "setChecked";
    public const string SetOptions = "setOptions";
    public const string SetSelectedIndex = "setSelectedIndex";
    public const string SetSliderPosition = "setSliderPosition";
    public const string SetValue = "setValue";

    // Gating.
    public const string SetVisible = "setVisible";
    public const string SetEnabled = "setEnabled";
    public const string SetGate = "setGate";
    public const string SetLog = "setLog";
    public const string SetBehaviour = "setBehaviour";
    public const string SetItemEvents = "setItemEvents";

    // Structure.
    public const string AddItems = "addItems";
    public const string RemoveItems = "removeItems";
    public const string ClearMenu = "clearMenu";
    public const string MoveItem = "moveItem";
    public const string AddPlayerActions = "addPlayerActions";

    // Menus.
    public const string SetMenuTitle = "setMenuTitle";
    public const string SetMenuSubtitle = "setMenuSubtitle";
    public const string OpenMenu = "openMenu";
    public const string CloseMenu = "closeMenu";
    public const string SelectItem = "selectItem";
    public const string SetMenuEvents = "setMenuEvents";
    public const string SetFilter = "setFilter";
    public const string ClearFilter = "clearFilter";

    // Keys.
    public const string AddKeys = "addKeys";
    public const string SetKeyText = "setKeyText";
    public const string SetKeyEnabled = "setKeyEnabled";
    public const string SetKeyGate = "setKeyGate";

    // Translations.
    public const string MergeTranslations = "mergeTranslations";
}
