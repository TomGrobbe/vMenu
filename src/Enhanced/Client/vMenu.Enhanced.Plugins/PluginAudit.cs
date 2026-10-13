using System.Globalization;

using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.Plugins;

internal static class PluginAudit
{
    private const int MaxValueLength = 64;

    internal static void Report(PluginState state, PluginCallback callback)
    {
        if (callback.ItemId is not { } itemId
            || !state.ItemsById.TryGetValue(itemId, out var node)
            || node.Log != true)
        {
            return;
        }

        if (Kind(callback.Type) is not { } kind)
        {
            return;
        }

        MenuAudit.ReportPluginItem(state.Resource, itemId, kind, Value(state, node, callback));
    }

    private static string? Kind(string type) => type switch
    {
        CallbackTypes.ItemSelected
            or CallbackTypes.Confirmed
            or CallbackTypes.PlayerActionSelected
            or CallbackTypes.PlayerActionConfirmed => MenuActionKinds.Button,
        CallbackTypes.CheckboxChanged => MenuActionKinds.Checkbox,
        CallbackTypes.ListSelected or CallbackTypes.PlayerActionListSelected => MenuActionKinds.List,
        CallbackTypes.SliderSelected => MenuActionKinds.Slider,
        CallbackTypes.DynamicSelected => MenuActionKinds.DynamicList,
        _ => null,
    };

    private static string Value(PluginState state, ItemNode node, PluginCallback callback) => callback.Type switch
    {
        CallbackTypes.CheckboxChanged => callback.Checked == true ? "1" : "0",
        CallbackTypes.ListSelected or CallbackTypes.PlayerActionListSelected =>
            Option(state, node, callback.SelectedIndex),
        CallbackTypes.SliderSelected =>
            (callback.Position ?? 0).ToString(CultureInfo.InvariantCulture),
        CallbackTypes.DynamicSelected => Trim(callback.Value),
        _ => string.Empty,
    };

    private static string Option(PluginState state, ItemNode node, int? index)
    {
        if (node.Options is not { } options || index is not { } at || at < 0 || at >= options.Count)
        {
            return string.Empty;
        }

        return Trim(state.Resolve(options[at]));
    }

    private static string Trim(string? value) =>
        value is null || value.Length <= MaxValueLength ? value ?? string.Empty : value[..MaxValueLength];
}
