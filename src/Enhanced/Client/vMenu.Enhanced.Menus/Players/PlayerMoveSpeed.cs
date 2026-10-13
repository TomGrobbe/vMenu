using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public sealed class PlayerMoveSpeed(
    string name,
    string permission,
    string stateKey,
    BoolDefault enabled,
    FloatDefault speed,
    float max,
    Action<int, float> apply)
{
    private const int NormalIndex = 9;

    private const int ScanMs = 1000;

    private static readonly float[] AllSpeeds =
    [
        0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f,
        1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f,
        2.5f, 3f, 3.5f, 4f, 4.5f, 5f, 5.5f, 6f, 6.5f, 7f,
        7.5f, 8f, 8.5f, 9f, 9.5f, 10f,
    ];

    private readonly List<Overridden> _remote = [];

    private TickHandle? _tick;

    private float? _published;

    private bool _on;

    private int _index = NormalIndex;

    public IReadOnlyList<float> Speeds { get; } = SpeedsUpTo(max);

    public static PlayerMoveSpeed Walk { get; } = new(
        "Player.MoveSpeed",
        PlayerOptionsPermissions.MoveSpeed,
        PlayerStateKeys.MoveSpeed,
        UserDefaults.PlayerMoveSpeedEnabled,
        UserDefaults.PlayerMoveSpeed,
        10f,
        Native.SetPedMoveRateOverride);

    public static PlayerMoveSpeed Swim { get; } = new(
        "Player.SwimSpeed",
        PlayerOptionsPermissions.SwimSpeed,
        PlayerStateKeys.SwimSpeed,
        UserDefaults.PlayerSwimSpeedEnabled,
        UserDefaults.PlayerSwimSpeed,
        3f,
        Native.SetPedMoveRateInWaterOverride);

    public bool Enabled => _on && IsAllowed;

    public int SpeedIndex => _index;

    private float Rate => Speeds[_index];

    private bool IsAllowed => ClientPermissions.IsAllowed(permission);

    private static float[] SpeedsUpTo(float max) => [.. AllSpeeds.Where(rate => rate <= max)];

    public static void Initialize()
    {
        Walk.Register();
        Swim.Register();

        TickRegistry.Register("Player.MoveSpeed.Scan", Scan, TickRate.Every(ScanMs));
    }

    public void SetEnabled(bool on)
    {
        if (on && !IsAllowed)
        {
            return;
        }

        enabled.Value = on;
    }

    public void SetSpeedIndex(int index)
    {
        if ((uint)index < Speeds.Count)
        {
            speed.Value = Speeds[index];
        }
    }

    private void Register()
    {
        Load();

        enabled.Changed += Reload;
        speed.Changed += Reload;

        _tick = TickRegistry.Register(name, Apply, TickRate.PerFrame, () => Enabled || _remote.Count > 0);
    }

    private void Load()
    {
        _on = enabled.Value;
        _index = NormalIndex;

        var stored = speed.Value;

        for (var index = 0; index < Speeds.Count; index++)
        {
            if (Speeds[index].Equals(stored))
            {
                _index = index;

                break;
            }
        }
    }

    private void Reload()
    {
        Load();
        Publish();

        _tick?.Reevaluate();
    }

    private void Publish()
    {
        var rate = Enabled ? Rate : 0f;

        if (_published is { } published && published.Equals(rate))
        {
            return;
        }

        if (StateBags.Set(StateBags.LocalPlayerBag, stateKey, rate))
        {
            _published = rate;
        }
    }

    private static void Scan()
    {
        Walk.BeginScan();
        Swim.BeginScan();

        PlayerRoster.Refresh();

        var local = Native.PlayerId();

        foreach (var player in PlayerRoster.All)
        {
            if (player.Slot == local)
            {
                continue;
            }

            Walk.Read(player);
            Swim.Read(player);
        }

        Walk._tick?.Reevaluate();
        Swim._tick?.Reevaluate();
    }

    private void BeginScan()
    {
        Publish();

        _remote.Clear();
    }

    private void Read(RosteredPlayer player)
    {
        var rate = StateBags.GetPlayer<float>(player.ServerId, stateKey);

        if (rate > 0f)
        {
            _remote.Add(new Overridden(player.Ped, Math.Clamp(rate, Speeds[0], max)));
        }
    }

    private void Apply()
    {
        if (Enabled)
        {
            apply(Native.PlayerPedId(), Rate);
        }

        foreach (var overridden in _remote)
        {
            if (Native.DoesEntityExist(overridden.Ped))
            {
                apply(overridden.Ped, overridden.Rate);
            }
        }
    }

    private sealed class Overridden(int ped, float rate)
    {
        public int Ped { get; } = ped;

        public float Rate { get; } = rate;
    }
}
