using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

namespace vMenu.Enhanced.Menus.Misc;

public static class TimecycleState
{
    public const int MinIntensity = 0;

    public const int MaxIntensity = 20;

    // What the game has room for: a primary and one extra. Not a choice.
    public const int Slots = 2;

    private const int DefaultIntensity = 10;

    private const string StoreKey = "vmenu_timecycles";

    private const int SchemaVersion = 2;

    private static readonly string?[] Active = new string?[Slots];

    private static int _intensity = DefaultIntensity;

    private static readonly int[] FilledAt = new int[Slots];

    private static bool _watching;

    private static bool _restoring;

    public static event Action? Changed;

    public static bool IsAllowed => ClientPermissions.IsAllowed(DisplaySettingsPermissions.Timecycles);

    public static bool AnyActive => NameOf(0) is not null || NameOf(1) is not null;

    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += OnPermissionsChanged;

        Restore();
    }

    public static string? NameOf(int slot) => (uint)slot < Slots ? Active[slot] : null;

    public static int Intensity => _intensity;

    public static bool IsActive(string name)
    {
        foreach (var held in Active)
        {
            if (string.Equals(held, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static void Toggle(string name)
    {
        if (IsActive(name))
        {
            Disable(name);

            return;
        }

        Enable(name);
    }

    public static void Enable(string name)
    {
        if (!IsAllowed || IsActive(name))
        {
            return;
        }

        var slot = FreeSlot() ?? LongestHeldSlot();

        Active[slot] = name;
        FilledAt[slot] = Native.GetGameTimer();

        Write(slot);
        Watch(true);
        Save();

        Changed?.Invoke();
    }

    public static void Disable(string name)
    {
        for (var slot = 0; slot < Slots; slot++)
        {
            if (!string.Equals(Active[slot], name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Active[slot] = null;

            Clear(slot);
            Watch(AnyActive);
            Save();

            Changed?.Invoke();

            return;
        }
    }

    public static void ClearAll()
    {
        if (!AnyActive)
        {
            return;
        }

        for (var slot = 0; slot < Slots; slot++)
        {
            Active[slot] = null;

            Clear(slot);
        }

        Watch(false);
        Save();

        Changed?.Invoke();
    }

    public static void SetIntensity(int intensity)
    {
        var wanted = Math.Clamp(intensity, MinIntensity, MaxIntensity);

        if (wanted == _intensity)
        {
            return;
        }

        _intensity = wanted;

        Reapply();
        Save();

        Changed?.Invoke();
    }

    public static void Restore()
    {
        if (!KvpStore.TryRead<Stored>(StoreKey, KvpValueType.Json, SchemaVersion, out var stored, out _)
            || stored?.Names is not { } slots)
        {
            return;
        }

        _restoring = true;

        try
        {
            _intensity = Math.Clamp(stored.Intensity, MinIntensity, MaxIntensity);

            foreach (var name in slots)
            {
                if (name is { Length: > 0 } && Known(name) is { } known)
                {
                    Enable(known);
                }
            }
        }
        finally
        {
            _restoring = false;
        }

        Save();
    }

    private static void OnPermissionsChanged()
    {
        if (IsAllowed)
        {
            return;
        }

        ClearAll();
    }

    private static string? Known(string name)
    {
        foreach (var known in TimecycleModifiers.Names)
        {
            if (string.Equals(known, name, StringComparison.OrdinalIgnoreCase))
            {
                return known;
            }
        }

        return null;
    }

    private static int? FreeSlot()
    {
        for (var slot = 0; slot < Slots; slot++)
        {
            if (Active[slot] is null)
            {
                return slot;
            }
        }

        return null;
    }

    private static int LongestHeldSlot()
    {
        var oldest = 0;

        for (var slot = 1; slot < Slots; slot++)
        {
            if (FilledAt[slot] < FilledAt[oldest])
            {
                oldest = slot;
            }
        }

        return oldest;
    }

    private static void Write(int slot)
    {
        if (Active[slot] is not { } name)
        {
            return;
        }

        var strength = _intensity / (float)MaxIntensity;

        if (slot == 0)
        {
            Native.SetTimecycleModifier(name);
            Native.SetTimecycleModifierStrength(strength);

            return;
        }

        Native.SetExtraTimecycleModifier(name);
        Native.SetExtraTimecycleModifierStrength(strength);
    }

    private static void Clear(int slot)
    {
        if (slot == 0)
        {
            Native.ClearTimecycleModifier();

            return;
        }

        Native.ClearExtraTimecycleModifier();
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
            LocalPlayerTicks.PlayerPedRevived += OnRevived;

            return;
        }

        LocalPlayerTicks.PlayerPedIdChanged -= OnPedChanged;
        LocalPlayerTicks.PlayerPedRevived -= OnRevived;
    }

    private static void OnPedChanged(PlayerPedIdChanged _) => Reapply();

    private static void OnRevived(PlayerPedRevived _) => Reapply();

    private static void Reapply()
    {
        for (var slot = 0; slot < Slots; slot++)
        {
            Write(slot);
        }
    }

    private static void Save()
    {
        if (_restoring)
        {
            return;
        }

        var stored = new Stored { Intensity = _intensity, Names = [] };

        foreach (var name in Active)
        {
            if (name is { Length: > 0 })
            {
                stored.Names.Add(name);
            }
        }

        KvpStore.TryWrite(StoreKey, KvpValueType.Json, SchemaVersion, stored);
    }

    // A class, not a record: the sandbox refuses the generated EqualityComparer<T>.Default.
    private sealed class Stored
    {
        public List<string> Names { get; set; } = [];

        public int Intensity { get; set; }
    }
}
