using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// A row the host can put back to asking, whatever kind of item it happens to be.
internal interface IConfirmable
{
    void ResetConfirmation();
}

// A row that does nothing on its first press. It swaps its description for a warning and only runs
// its handler if the next press comes without the player leaving the row in between.
public abstract class ConfirmEntry<TItem> : MenuEntry<TItem>, IConfirmable
    where TItem : MenuItem
{
    private bool _armed;

    // What the description says while the row waits. Colour it yourself with the game's markup, the same
    // way the rest of the tables do.
    public MenuText ConfirmationDescription { get; init; } = MenuText.Key(Loc.Framework.ConfirmDescription);

    // Drops a second confirmation while the asynchronous handler is still running.
    public bool SingleFlight { get; init; } = true;

    // Whether the row is waiting for its confirming press right now.
    public bool IsArmed => _armed;

    public void ResetConfirmation()
    {
        if (!_armed)
        {
            return;
        }

        _armed = false;

        Paint(Localizer.Current);
    }

    // Answers whether this was the confirming press, arming the row when it was not.
    internal bool Press()
    {
        if (_armed)
        {
            ResetConfirmation();

            return true;
        }

        _armed = true;

        Paint(Localizer.Current);

        return false;
    }

    internal override void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        // A row that just lost its permission must not keep a warning about something it will no longer do.
        if (!IsAllowed)
        {
            _armed = false;
        }

        base.ApplyPresentation(localizer, behaviour);

        // After the base pass rather than instead of it, so a locked row still says it is restricted and a
        // language change reaches the warning too.
        if (_armed && Item is { } item)
        {
            item.Description = ConfirmationDescription.Resolve(localizer);
        }
    }

    private void Paint(ILocalizer localizer)
    {
        if (Item is { } item)
        {
            item.Description = (_armed ? ConfirmationDescription : Description).Resolve(localizer);
        }
    }
}
