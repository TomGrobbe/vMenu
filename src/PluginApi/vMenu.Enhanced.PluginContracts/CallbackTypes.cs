namespace vMenu.Enhanced.PluginContracts;

/// <summary>The discriminator values a <see cref="PluginCallback"/> can carry.</summary>
public static class CallbackTypes
{
    public const string ItemSelected = "itemSelected";
    public const string CheckboxChanged = "checkboxChanged";
    public const string ListIndexChanged = "listIndexChanged";
    public const string ListSelected = "listSelected";
    public const string SliderMoved = "sliderMoved";
    public const string SliderSelected = "sliderSelected";
    public const string DynamicSelected = "dynamicSelected";
    public const string DynamicChanging = "dynamicChanging";
    public const string Confirmed = "confirmed";
    public const string ItemHighlighted = "itemHighlighted";
    public const string MenuOpened = "menuOpened";
    public const string MenuClosed = "menuClosed";
    public const string MenuIndexChanged = "menuIndexChanged";
    public const string PlayerActionSelected = "playerActionSelected";
    public const string PlayerActionConfirmed = "playerActionConfirmed";
    public const string PlayerActionListSelected = "playerActionListSelected";
}
