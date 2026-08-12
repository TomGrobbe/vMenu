using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.MenuFramework;

namespace vMenu.Enhanced.Menus.Weapons;

/// <summary>
/// The damage, rate of fire, accuracy and range bars the game draws beside a weapon, and the amount
/// each component moves them.
/// </summary>
internal static class WeaponStatistics
{
    private static readonly Bars Weapons = new(NativeFixer.GetWeaponHudStats);

    private static readonly Bars Components = new(NativeFixer.GetWeaponComponentHudStats);

    /// <inheritdoc cref="Bars.Request" />
    internal static void Request(uint weaponHash) => Weapons.Request(weaponHash);

    /// <inheritdoc cref="Bars.For" />
    internal static WeaponStats? For(uint weaponHash) => Weapons.For(weaponHash);

    /// <inheritdoc cref="Bars.Request" />
    internal static void RequestComponent(uint componentHash) => Components.Request(componentHash);

    /// <summary>
    /// How far this component moves each bar, which the panel draws on top of the weapon's own. Can
    /// be negative: a suppressor costs damage.
    /// </summary>
    /// <inheritdoc cref="Bars.For" />
    internal static WeaponStats? ForComponent(uint componentHash) => Components.For(componentHash);

    /// <summary>One native's answers, kept for as long as the resource runs.</summary>
    // Both natives fill the same struct and both are read the same way, so the only thing that
    // differs between a weapon and a component is which one gets called.
    private sealed class Bars(Func<uint, WeaponHudStatsData, bool> fetch)
    {
        /// <summary>The game counts each bar to 100; the panel wants them as a fraction.</summary>
        private const float Scale = 100f;

        private readonly Dictionary<uint, WeaponStats?> _known = [];

        /// <summary>Asked for, but the game has not handed the buffer back yet.</summary>
        private readonly Dictionary<uint, (WeaponHudStatsData Data, int Frame)> _pending = [];

        /// <summary>
        /// Asks the game about a hash. Call this a frame or more before the bars are needed, because
        /// the answer is not readable in the frame it was asked for.
        /// </summary>
        internal void Request(uint hash)
        {
            if (_known.ContainsKey(hash) || _pending.ContainsKey(hash))
            {
                return;
            }

            var data = new WeaponHudStatsData();

            if (!fetch(hash, data))
            {
                _known[hash] = null;

                return;
            }

            _pending[hash] = (data, Native.GetFrameCount());
        }

        /// <summary>
        /// Null for a hash the game has no bars for, or one whose bars have not arrived yet, which
        /// leaves the panel hidden rather than drawing four empty bars.
        /// </summary>
        // Kept because the panel is re-read every time the highlighted row changes, and neither a
        // weapon's stats nor a component's effect on them ever move.
        internal WeaponStats? For(uint hash)
        {
            if (_known.TryGetValue(hash, out var known))
            {
                return known;
            }

            if (!_pending.TryGetValue(hash, out var pending))
            {
                Request(hash);

                return null;
            }

            // NativeFixer.GetWeaponHudStats explains the delay. Reading during the frame the request
            // was made would see the untouched buffer and keep those zeroes for good.
            if (Native.GetFrameCount() == pending.Frame)
            {
                return null;
            }

            var stats = new WeaponStats(
                pending.Data.Damage / Scale,
                pending.Data.Speed / Scale,
                pending.Data.Accuracy / Scale,
                pending.Data.Range / Scale);

            _known[hash] = stats;
            _pending.Remove(hash);

            return stats;
        }
    }
}
