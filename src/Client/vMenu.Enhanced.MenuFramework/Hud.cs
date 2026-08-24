using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.MenuFramework;

// Text drawn straight onto the screen by the game, for overlays that have to sit in the world or
// track the minimap rather than live in the NUI page like Notifications.
public static class Hud
{
    public enum TextAlignment
    {
        Center = 0,
        Left = 1,
        Right = 2,
    }

    // The line break the game's text renderer understands. A \n is drawn literally.
    public const string NewLine = "~n~";

    private const int DefaultFont = 6;

    private const float DefaultSize = 0.48f;

    // Whether the game is in a state where drawing anything would be wrong: mid player switch, in the
    // pause menu, faded out, or with the HUD turned off by the player or by a script.
    public static bool CanDraw =>
        Native.IsHudPreferenceSwitchedOn()
        && !Native.IsHudHidden()
        && !Native.IsPlayerSwitchInProgress()
        && Native.IsScreenFadedIn()
        && !Native.IsPauseMenuActive()
        && !Native.IsFrontendFading()
        && !Native.IsPauseMenuRestarting();

    // Draws at screen coordinates, or relative to the current draw origin when one is set by
    // SetDrawOrigin. The outline is what keeps text readable over a bright sky.
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

    // Draws text at a point in the world. Multi-line text runs downward from z, so the caller decides
    // what sits on top.
    public static void DrawText3D(
        string text,
        float x,
        float y,
        float z,
        float size = DefaultSize,
        TextAlignment alignment = TextAlignment.Center,
        int font = 0)
    {
        // Checked before the origin is set rather than leaving it to DrawText, so a hidden HUD costs nothing
        // for callers drawing one of these per entity per frame.
        if (!CanDraw)
        {
            return;
        }

        Native.SetDrawOrigin(x, y, z, false);

        DrawText(text, 0f, 0f, size, alignment, font);

        Native.ClearDrawOrigin();
    }
}
