using System.Globalization;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

/// <summary>What a colour picker reads from and writes to.</summary>
internal sealed class RgbTarget
{
    /// <summary>The colour now, or null when this target has no colour of its own set.</summary>
    public required Func<RgbValue?> Read { get; init; }

    public required Action<int, int, int> Write { get; init; }

    /// <summary>Puts the target back to a colour from the game's lists. Null when it has no such state.</summary>
    public Action? Clear { get; init; }
}

/// <summary>Three channels, so the picker can hand one value around instead of three.</summary>
internal sealed class RgbValue(int red, int green, int blue)
{
    public int Red { get; set; } = red;

    public int Green { get; set; } = green;

    public int Blue { get; set; } = blue;
}

/// <summary>
/// A colour mixed by hand: a hex code and three sliders.
/// </summary>
/// <remarks>
/// The same shape appears for the primary paint, the secondary paint, the neon tubes, the xenon
/// headlights and the tyre smoke, so it is declared once and pointed at whichever of those the
/// caller means.
/// </remarks>
internal static class RgbPicker
{
    private const int MaxChannel = 255;

    private const int HexLength = 6;

    public static void Build(MenuBuilder menu, RgbTarget target)
    {
        // Seeded from the target and then carried in this closure, because a slider only ever hears
        // about its own channel and the write needs all three.
        var current = target.Read() ?? new RgbValue(MaxChannel, MaxChannel, MaxChannel);

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.HexColor),
            Description = MenuText.Key(Loc.VehicleOptions.HexColorDescription),
            OnSelectedAsync = _ => ApplyHexAsync(current, target),
        });

        menu.Entries.Add(Channel(Loc.VehicleOptions.ChannelRed, current, target, value => current.Red = value, () => current.Red));
        menu.Entries.Add(Channel(Loc.VehicleOptions.ChannelGreen, current, target, value => current.Green = value, () => current.Green));
        menu.Entries.Add(Channel(Loc.VehicleOptions.ChannelBlue, current, target, value => current.Blue = value, () => current.Blue));

        if (target.Clear is { } clear)
        {
            menu.Entries.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.ResetCustomColor),
                Description = MenuText.Key(Loc.VehicleOptions.ResetCustomColorDescription),
                OnSelected = _ => clear(),
            });
        }
    }

    private static MenuEntry Channel(
        string textKey,
        RgbValue current,
        RgbTarget target,
        Action<int> set,
        Func<int> get) => new SliderEntry
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(Loc.VehicleOptions.ChannelDescription),
            Min = 0,
            Max = MaxChannel,
            ReadPosition = get,
            OnMoved = moved =>
            {
                set(moved.NewPosition);

                target.Write(current.Red, current.Green, current.Blue);
            },
        };

    private static async Task ApplyHexAsync(RgbValue current, RgbTarget target)
    {
        var typed = await UserInput.GetTextAsync(MenuText.Key(Loc.VehicleOptions.HexColorPrompt), HexLength + 1);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var code = typed.Trim().TrimStart('#');

        if (!TryParseHex(code, out var red, out var green, out var blue))
        {
            Notifications.Error(MenuText.Key(
                Loc.VehicleOptions.HexColorInvalid,
                ("code", MenuText.Literal(typed))));

            return;
        }

        current.Red = red;
        current.Green = green;
        current.Blue = blue;

        target.Write(red, green, blue);
    }

    private static bool TryParseHex(string code, out int red, out int green, out int blue)
    {
        red = 0;
        green = 0;
        blue = 0;

        if (code.Length != HexLength)
        {
            return false;
        }

        return TryParseChannel(code, 0, out red)
            && TryParseChannel(code, 2, out green)
            && TryParseChannel(code, 4, out blue);
    }

    private static bool TryParseChannel(string code, int offset, out int value) =>
        int.TryParse(
            code.Substring(offset, 2),
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture,
            out value);
}
