using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Props;

internal static class PropPlacementButtons
{
    private const string Scaleform = "INSTRUCTIONAL_BUTTONS";

    private const int ControlGroup = 0;

    // A pair of icons draws as nothing without this. A single one still works, which hides it.
    private const string IconSeparator = "%b_998%";

    private static int _scaleform = -1;

    private static bool _stale = true;

    internal static void Invalidate() => _stale = true;

    internal static async Task DrawAsync()
    {
        if (Native.IsHudHidden())
        {
            return;
        }

        if (!Native.HasScaleformMovieLoaded(_scaleform))
        {
            await PrepareAsync();
        }

        if (_stale)
        {
            Build();

            _stale = false;
        }

        Native.DrawScaleformMovieFullscreen(_scaleform, 255, 255, 255, 255, 0);
    }

    internal static void Release()
    {
        if (_scaleform == -1)
        {
            return;
        }

        Native.SetScaleformMovieAsNoLongerNeeded(ref _scaleform);

        _scaleform = -1;
        _stale = true;
    }

    private static async Task PrepareAsync()
    {
        if (_scaleform != -1 && Native.HasScaleformMovieLoaded(_scaleform))
        {
            return;
        }

        _scaleform = Native.RequestScaleformMovie(Scaleform);

        while (!Native.HasScaleformMovieLoaded(_scaleform))
        {
            await API.Delay(0);
        }

        _stale = true;
    }

    private static void Build()
    {
        var localizer = Localizer.Current;

        Native.CallScaleformMovieMethod(_scaleform, "CLEAR_ALL");

        Native.BeginScaleformMovieMethod(_scaleform, "TOGGLE_MOUSE_BUTTONS");
        Native.PushScaleformMovieFunctionParameterInt(0);
        Native.EndScaleformMovieMethod();

        var slot = 0;

        Slot(slot++, Icon(PropPlacement.ConfirmControl), localizer.Get(Loc.PropSpawner.ButtonPlace));
        Slot(slot++, Icon(PropPlacement.CancelControl), localizer.Get(Loc.PropSpawner.ButtonCancel));

        Slot(
            slot++,
            Icon(PropPlacement.RotateLeftControl) + IconSeparator + Icon(PropPlacement.RotateRightControl),
            localizer.Get(Loc.PropSpawner.ButtonRotate));

        Slot(
            slot++,
            Icon(PropPlacement.NearerControl) + IconSeparator + Icon(PropPlacement.FurtherControl),
            localizer.Get(Loc.PropSpawner.ButtonDistance));

        Slot(
            slot,
            Icon(PropPlacement.SnapControl),
            localizer.Get(PropSpawnOptions.SnapToGround
                ? Loc.PropSpawner.ButtonSnapOn
                : Loc.PropSpawner.ButtonSnapOff));

        Native.CallScaleformMovieMethod(_scaleform, "DRAW_INSTRUCTIONAL_BUTTONS");
    }

    private static void Slot(int slot, string icon, string text)
    {
        Native.BeginScaleformMovieMethod(_scaleform, "SET_DATA_SLOT");
        Native.ScaleformMovieMethodAddParamInt(slot);
        Native.ScaleformMovieMethodAddParamTextureNameString(icon);
        Native.ScaleformMovieMethodAddParamTextureNameString(text);
        Native.EndScaleformMovieMethod();
    }

    private static string Icon(Control control) =>
        Native.GetControlInstructionalButton(ControlGroup, (int)control, true);
}
