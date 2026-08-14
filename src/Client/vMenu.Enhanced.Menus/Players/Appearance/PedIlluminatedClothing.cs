using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using PlayerAppearancePermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerAppearance;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>How a player's glowing clothes behave.</summary>
public enum ClothingGlow
{
    Off = 0,

    /// <summary>Lit, and staying lit.</summary>
    Solid = 1,

    /// <summary>Brightening and dimming.</summary>
    Fade = 2,

    /// <summary>Full on, full off, repeating.</summary>
    Flash = 3,
}

/// <summary>
/// The glow on clothes that light up, and the animation the player chose for it.
/// </summary>
/// <remarks>
/// Glow intensity is drawn by each machine for itself, so a player choosing one has to tell everyone
/// else what they picked. That goes on an entity decorator, which the game syncs for us, and every
/// client reads it off the peds around them and does the drawing.
///
/// <para>
/// Only clothes that were made to light up are affected, which on most outfits is nothing at all.
/// That is the game's doing and not something vMenu can widen.
/// </para>
/// </remarks>
public static class PedIlluminatedClothing
{
    /// <summary>The decorator every vMenu client reads other players' choices from.</summary>
    public const string Decorator = "vmenu_enhanced_clothing_glow";

    /// <summary>The game's decorator type number for a whole number.</summary>
    private const int IntegerDecorator = 3;

    /// <summary>How long one brighten-and-dim takes.</summary>
    private const int CycleMs = 2000;

    /// <summary>How often the list of people worth drawing a glow on is rebuilt.</summary>
    // Walking every player slot is not something to do on every frame, and somebody arriving a
    // second late to a light show is nobody's problem.
    private const int ScanMs = 1000;

    /// <summary>The highest player slot the game will hand out.</summary>
    private const int PlayerSlots = 256;

    private static readonly List<int> Glowing = [];

    private static TickHandle? _draw;

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

    /// <summary>Call once at startup, before permissions have arrived.</summary>
    public static void Initialize()
    {
        // Registering can fail when another resource has already used up the game's decorator budget,
        // which is that resource's bug rather than something vMenu can work around. Said once and
        // then left alone, rather than retried forever.
        if (!Native.DecorIsRegisteredAsType(Decorator, IntegerDecorator))
        {
            Native.DecorRegister(Decorator, IntegerDecorator);
        }

        if (!Native.DecorIsRegisteredAsType(Decorator, IntegerDecorator))
        {
            Log.Error(
                $"[Appearance] The '{Decorator}' decorator could not be registered, most likely because "
                + "another resource has used up the game's supply of them. Glowing clothes will not be "
                + "shared between players until that is sorted out.");

            return;
        }

        // Always on, but only once a second. It is also what wakes the drawing tick up when somebody
        // with a glow walks past, which nothing else would notice.
        TickRegistry.Register("Player.ClothingGlow.Scan", Scan, TickRate.Every(ScanMs));

        // Per frame, because a fade that only moves once a second is a stutter rather than a fade.
        // Asleep whenever there is nobody in sight to draw one on, which is most of the time.
        _draw = TickRegistry.Register("Player.ClothingGlow", Draw, TickRate.PerFrame, () => Glowing.Count > 0);
    }

    /// <summary>Tells everyone else what this player picked.</summary>
    // Written again after every model change as well, because a new ped is a new entity and carries
    // none of the old one's decorators.
    public static void Publish()
    {
        if (!Native.DecorIsRegisteredAsType(Decorator, IntegerDecorator))
        {
            return;
        }

        Native.DecorSetInt(Native.PlayerPedId(), Decorator, (int)Style);
    }

    private static void Scan()
    {
        Publish();

        Glowing.Clear();

        for (var slot = 0; slot < PlayerSlots; slot++)
        {
            if (!Native.NetworkIsPlayerActive(slot))
            {
                continue;
            }

            var ped = Native.GetPlayerPed(slot);

            if (ped == 0 || !Native.DoesEntityExist(ped) || !Native.DecorExistOn(ped, Decorator))
            {
                continue;
            }

            // Off is the common case and needs no drawing at all, so it does not keep the per frame
            // tick awake.
            if (Native.DecorGetInt(ped, Decorator) != (int)ClothingGlow.Off)
            {
                Glowing.Add(ped);
            }
        }

        _draw?.Reevaluate();
    }

    private static void Draw()
    {
        var phase = Phase();

        foreach (var ped in Glowing)
        {
            if (!Native.DoesEntityExist(ped))
            {
                continue;
            }

            Native.SetPedIlluminatedClothingGlowIntensity(ped, Intensity((ClothingGlow)Native.DecorGetInt(ped, Decorator), phase));
        }
    }

    private static float Intensity(ClothingGlow style, float phase) => style switch
    {
        ClothingGlow.Solid => 1f,
        ClothingGlow.Fade => phase,
        ClothingGlow.Flash => phase >= 0.5f ? 1f : 0f,
        _ => 0f,
    };

    /// <summary>Where in the brighten-and-dim we are, from 0 dark to 1 bright and back.</summary>
    // Off the game clock rather than legacy's per frame counter, which stepped the same amount every
    // frame and therefore ran twice as fast on a machine drawing twice as many of them.
    private static float Phase()
    {
        var position = Native.GetGameTimer() % CycleMs / (float)CycleMs;

        return position < 0.5f ? position * 2f : (1f - position) * 2f;
    }
}
