using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Menus.Misc;

internal static class TimecycleKeyBindings
{
    private const string IntensityUpCommand = "vmenu:timecycle:intensityup";

    private const string IntensityDownCommand = "vmenu:timecycle:intensitydown";

    private const string ClearCommand = "vmenu:timecycle:clear";

    private const string SearchCommand = "vmenu:timecycle:search";

    private const string TopCommand = "vmenu:timecycle:top";

    private const string FavoriteCommand = "vmenu:timecycle:favorite";

    private static bool _registered;

    internal static int IntensityUpControl { get; } = BindingControl(IntensityUpCommand);

    internal static int IntensityDownControl { get; } = BindingControl(IntensityDownCommand);

    internal static int ClearControl { get; } = BindingControl(ClearCommand);

    internal static int SearchControl { get; } = BindingControl(SearchCommand);

    internal static int TopControl { get; } = BindingControl(TopCommand);

    internal static int FavoriteControl { get; } = BindingControl(FavoriteCommand);

    internal static void Register(
        Action onIntensityUp,
        Action onIntensityDown,
        Action onClear,
        Action onSearch,
        Action onTop,
        Action onFavorite)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        SharedAPI.Commands.RegisterCommand(IntensityUpCommand, false, onIntensityUp);
        SharedAPI.Commands.RegisterCommand(IntensityDownCommand, false, onIntensityDown);
        SharedAPI.Commands.RegisterCommand(ClearCommand, false, onClear);
        SharedAPI.Commands.RegisterCommand(SearchCommand, false, onSearch);
        SharedAPI.Commands.RegisterCommand(TopCommand, false, onTop);
        SharedAPI.Commands.RegisterCommand(FavoriteCommand, false, onFavorite);

        Native.RegisterKeyMapping(IntensityUpCommand, "vMenu: Timecycle intensity up", "keyboard", "PAGEUP");
        Native.RegisterKeyMapping(IntensityDownCommand, "vMenu: Timecycle intensity down", "keyboard", "PAGEDOWN");
        Native.RegisterKeyMapping(ClearCommand, "vMenu: Clear all timecycle modifiers", "keyboard", "DELETE");
        Native.RegisterKeyMapping(SearchCommand, "vMenu: Search timecycle modifiers", "keyboard", "0");
        Native.RegisterKeyMapping(TopCommand, "vMenu: Back to the top of the timecycle list", "keyboard", "MINUS");
        Native.RegisterKeyMapping(FavoriteCommand, "vMenu: Favorite a timecycle modifier", "keyboard", "END");
    }

    private static int BindingControl(string command) => API.HashSigned(command) | int.MinValue;
}
