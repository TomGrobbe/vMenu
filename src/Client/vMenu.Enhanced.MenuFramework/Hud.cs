using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Text drawn straight onto the screen by the game, for overlays that have to sit in the world or
/// track the minimap rather than live in the NUI page like <see cref="Notifications"/>.
/// </summary>
public static class Hud
{
    /// <summary>How <see cref="DrawText"/> places text around the coordinates it is given.</summary>
    public enum TextAlignment
    {
        Center = 0,
        Left = 1,
        Right = 2,
    }

    /// <summary>The line break the game's text renderer understands. A <c>\n</c> is drawn literally.</summary>
    public const string NewLine = "~n~";

    private const int DefaultFont = 6;

    private const float DefaultSize = 0.48f;

    /// <summary>
    /// Whether the game is in a state where drawing anything would be wrong: mid player switch, in
    /// the pause menu, faded out, or with the HUD turned off by the player or by a script.
    /// </summary>
    public static bool CanDraw =>
        Native.IsHudPreferenceSwitchedOn()
        && !Native.IsHudHidden()
        && !Native.IsPlayerSwitchInProgress()
        && Native.IsScreenFadedIn()
        && !Native.IsPauseMenuActive()
        && !Native.IsFrontendFading()
        && !Native.IsPauseMenuRestarting();

    /// <summary>
    /// Draws at screen coordinates, or relative to the current draw origin when one is set by
    /// <see cref="Native.SetDrawOrigin"/>.
    /// </summary>
    /// <param name="disableOutline">The outline is what keeps text readable over a bright sky.</param>
    public static void DrawText(
        string text,
        float x,
        float y,
        float size = DefaultSize,
        TextAlignment alignment = TextAlignment.Left,
        int font = DefaultFont,
        bool disableOutline = false)
    {
        if (!CanDraw)
        {
            return;
        }

        Native.SetTextFont(font);
        Native.SetTextScale(1.0f, size);

        if (alignment is TextAlignment.Right)
        {
            Native.SetTextWrap(0f, x);
        }

        Native.SetTextJustification((int)alignment);

        if (!disableOutline)
        {
            Native.SetTextOutline();
        }

        Native.BeginTextCommandDisplayText("STRING");
        Native.AddTextComponentSubstringPlayerName(text);

        // The third argument is new in Enhanced and has no legacy counterpart.
        Native.EndTextCommandDisplayText(x, y, 0);
    }

    /// <summary>
    /// Draws text at a point in the world. Multi-line text runs downward from
    /// <paramref name="z"/>, so the caller decides what sits on top.
    /// </summary>
    public static void DrawText3D(
        string text,
        float x,
        float y,
        float z,
        float size = DefaultSize,
        TextAlignment alignment = TextAlignment.Center,
        int font = 0)
    {
        // Checked before the origin is set rather than leaving it to DrawText, so a hidden HUD costs
        // nothing for callers drawing one of these per entity per frame.
        if (!CanDraw)
        {
            return;
        }

        Native.SetDrawOrigin(x, y, z, false);

        DrawText(text, 0f, 0f, size, alignment, font);

        Native.ClearDrawOrigin();
    }
}
