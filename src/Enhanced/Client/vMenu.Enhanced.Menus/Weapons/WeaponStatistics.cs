using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.MenuFramework;

namespace vMenu.Enhanced.Menus.Weapons;

internal static class WeaponStatistics
{
    private static readonly Bars Weapons = new(Native.GetWeaponHudStats);

    private static readonly Bars Components = new(Native.GetWeaponComponentHudStats);

    internal static void Request(uint weaponHash) => Weapons.Request(weaponHash);

    internal static WeaponStats? For(uint weaponHash) => Weapons.For(weaponHash);

    internal static void RequestComponent(uint componentHash) => Components.Request(componentHash);

    // How far this component moves each bar, which the panel draws on top of the weapon's own. Can be
    // negative: a suppressor costs damage.
    internal static WeaponStats? ForComponent(uint componentHash) => Components.For(componentHash);

    // Both natives fill the same struct and both are read the same way, so the only thing that differs
    // between a weapon and a component is which one gets called.
    private sealed class Bars(Func<uint, WeaponHudStatsData, bool> fetch)
    {
        // The game counts each bar to 100; the panel wants them as a fraction.
        private const float Scale = 100f;

        private readonly Dictionary<uint, WeaponStats?> _known = [];

        // Asked for, but the game has not handed the buffer back yet.
        private readonly Dictionary<uint, (WeaponHudStatsData Data, int Frame)> _pending = [];

        // Call a frame or more before the bars are needed, because the answer is not readable in the frame
        // it was asked for.
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

        // Null for a hash the game has no bars for, or one whose bars have not arrived yet, which leaves the
        // panel hidden rather than drawing four empty bars. Kept because the panel is re-read every time the
        // highlighted row changes, and neither a weapon's stats nor a component's effect on them ever move.
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

            // WeaponHudStatsData explains the delay. Reading during the frame the request was made would see the
            // untouched buffer and keep those zeroes for good.
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
