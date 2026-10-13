using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using PlayerAppearancePermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerAppearance;

namespace vMenu.Enhanced.Menus.Players.Appearance;

public enum ClothingGlow
{
    Off = 0,

    Solid = 1,

    Fade = 2,

    Flash = 3,
}

public static class PedIlluminatedClothing
{
    // How long one brighten-and-dim takes.
    private const int CycleMs = 2000;

    // Walking every player slot is not something to do on every frame, and somebody arriving a second
    // late to a light show is nobody's problem.
    private const int ScanMs = 1000;

    private static readonly List<Glowing> Lit = [];

    private static TickHandle? _draw;

    private static int? _published;

    public static ClothingGlow Style
    {
        get => (ClothingGlow)UserDefaults.PlayerClothingGlow.Value;

        set
        {
            // The row follows the permission, but a revoke can land between the two.
            if (!IsAllowed)
            {
                return;
            }

            UserDefaults.PlayerClothingGlow.Value = (int)value;

            Publish();
        }
    }

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerAppearancePermissions.IlluminatedClothing);

    public static void Initialize()
    {
        // Always on, but only once a second. It is also what wakes the drawing tick up when somebody with a
        // glow walks past, which nothing else would notice.
        TickRegistry.Register("Player.ClothingGlow.Scan", Scan, TickRate.Every(ScanMs));

        // Per frame, because a fade that only moves once a second is a stutter rather than a fade. Asleep
        // whenever there is nobody in sight to draw one on, which is most of the time.
        _draw = TickRegistry.Register("Player.ClothingGlow", Draw, TickRate.PerFrame, () => Lit.Count > 0);
    }

    public static void Publish()
    {
        var style = (int)Style;

        if (_published == style)
        {
            return;
        }

        if (StateBags.Set(StateBags.LocalPlayerBag, PlayerStateKeys.ClothingGlow, style))
        {
            _published = style;
        }
    }

    private static void Scan()
    {
        // Here rather than in Initialize, which runs before there is a server id to write against.
        Publish();

        PlayerRoster.Refresh();

        Lit.Clear();

        foreach (var player in PlayerRoster.All)
        {
            var style = (ClothingGlow)StateBags.GetPlayer<int>(player.ServerId, PlayerStateKeys.ClothingGlow);

            // Off is the common case and needs no drawing at all, so it does not keep the per frame tick awake.
            if (style != ClothingGlow.Off)
            {
                Lit.Add(new Glowing(player.Ped, style));
            }
        }

        _draw?.Reevaluate();
    }

    private static void Draw()
    {
        var phase = Phase();

        foreach (var glowing in Lit)
        {
            if (!Native.DoesEntityExist(glowing.Ped))
            {
                continue;
            }

            Native.SetPedIlluminatedClothingGlowIntensity(glowing.Ped, Intensity(glowing.Style, phase));
        }
    }

    private static float Intensity(ClothingGlow style, float phase) => style switch
    {
        ClothingGlow.Solid => 1f,
        ClothingGlow.Fade => phase,
        ClothingGlow.Flash => phase >= 0.5f ? 1f : 0f,
        _ => 0f,
    };

    // Off the game clock rather than legacy's per frame counter, which stepped the same amount every
    // frame and therefore ran twice as fast on a machine drawing twice as many of them.
    private static float Phase()
    {
        var position = Native.GetGameTimer() % CycleMs / (float)CycleMs;

        return position < 0.5f ? position * 2f : (1f - position) * 2f;
    }

    // A class rather than a record: generated equality routes through
    // EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class Glowing(int ped, ClothingGlow style)
    {
        public int Ped { get; } = ped;

        public ClothingGlow Style { get; } = style;
    }
}
