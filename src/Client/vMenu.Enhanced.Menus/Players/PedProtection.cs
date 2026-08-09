using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>Which of the three "leave me alone in a vehicle" flags a feature wants held down.</summary>
[Flags]
public enum PedProtections
{
    None = 0,
    NotDraggedOut = 1,
    NotShotInVehicle = 2,
    NotKnockedOffBike = 4,
}

/// <summary>
/// The three ped flags more than one feature wants at once, resolved in one place.
/// </summary>
/// <remarks>
/// Each native behind these takes one answer rather than a list of reasons, so two features writing
/// them directly would undo each other. A feature registers a claim and says what it wants instead,
/// and the union of every claim is what reaches the ped.
/// </remarks>
public static class PedProtection
{
    /// <summary>The game's answer for being knocked off: 0 the default, 1 never, 2 always.</summary>
    private const int KnockOffDefault = 0;

    private const int KnockOffNever = 1;

    private static readonly List<Claim> Claims = [];

    private static PedProtections _applied = PedProtections.None;

    /// <summary>The ped <see cref="_applied"/> was written to. A new one starts on the game's defaults.</summary>
    private static int _appliedTo;

    private static bool _watching;

    /// <summary>One feature's say in the three flags. Hold it in a static field and update it.</summary>
    public static Claim Register()
    {
        var claim = new Claim();

        Claims.Add(claim);

        return claim;
    }

    /// <summary>Writes the flags out again, for anything that has reset them behind vMenu's back.</summary>
    public static void Reapply()
    {
        _appliedTo = 0;

        Apply();
    }

    internal static void Apply()
    {
        var wanted = PedProtections.None;

        foreach (var claim in Claims)
        {
            wanted |= claim.Wanted;
        }

        // Only worth watching for a new ped while somebody actually wants a flag held down.
        Watch(wanted != PedProtections.None);

        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        if (ped == _appliedTo && wanted == _applied)
        {
            return;
        }

        _appliedTo = ped;
        _applied = wanted;

        Native.SetPedCanBeDraggedOut(ped, (wanted & PedProtections.NotDraggedOut) == 0);
        Native.SetPedCanBeShotInVehicle(ped, (wanted & PedProtections.NotShotInVehicle) == 0);

        // An answer out of three rather than a yes or no, unlike the two above.
        Native.SetPedCanBeKnockedOffVehicle(
            ped,
            (wanted & PedProtections.NotKnockedOffBike) == 0 ? KnockOffDefault : KnockOffNever);
    }

    private static void Watch(bool watching)
    {
        if (watching == _watching)
        {
            return;
        }

        _watching = watching;

        if (watching)
        {
            LocalPlayerTicks.PlayerPedIdChanged += OnPedChanged;

            return;
        }

        LocalPlayerTicks.PlayerPedIdChanged -= OnPedChanged;
    }

    private static void OnPedChanged(PlayerPedIdChanged _) => Apply();

    public sealed class Claim
    {
        internal Claim()
        {
        }

        internal PedProtections Wanted { get; private set; }

        public void Set(PedProtections wanted)
        {
            if (wanted == Wanted)
            {
                return;
            }

            Wanted = wanted;

            Apply();
        }

        /// <summary>The common shape: a fixed set while the feature is on, and nothing while it is off.</summary>
        public void Set(bool on, PedProtections wanted) => Set(on ? wanted : PedProtections.None);
    }
}
