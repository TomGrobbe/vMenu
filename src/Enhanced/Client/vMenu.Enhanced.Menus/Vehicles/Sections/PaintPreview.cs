using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class PaintPreview
{
    private static PaintSnapshot? _committed;

    public static PaintSnapshot? Committed => _committed;

    public static void Show(Action<int> apply)
    {
        if (Session() is not { } session)
        {
            return;
        }

        session.Restore();
        apply(session.Vehicle);
    }

    public static void Commit(Action<int> apply)
    {
        Show(apply);

        if (_committed is { } session)
        {
            _committed = PaintSnapshot.Take(session.Vehicle);
        }
    }

    public static void Begin() => Session();

    public static void End()
    {
        _committed?.Restore();
        _committed = null;
    }

    public static MenuItem.Icon CurrentIcon(Func<PaintSnapshot, bool> isCurrent)
    {
        if (_committed is not { } session || !isCurrent(session))
        {
            return MenuItem.Icon.NONE;
        }

        return Native.IsThisModelABike(Native.GetEntityModel(session.Vehicle))
            ? MenuItem.Icon.BIKE
            : MenuItem.Icon.CAR;
    }

    private static PaintSnapshot? Session()
    {
        if (SectionRows.DrivenWithModKit() is not { } vehicle)
        {
            return null;
        }

        if (_committed is { } session && session.Vehicle == vehicle)
        {
            return session;
        }

        End();

        return _committed = PaintSnapshot.Take(vehicle);
    }
}
