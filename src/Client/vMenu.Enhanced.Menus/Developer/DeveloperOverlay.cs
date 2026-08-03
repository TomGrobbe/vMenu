using System.Numerics;
using System.Text;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Ticks;

using DeveloperFeaturesSetting = vMenu.Enhanced.Data.Configuration.Settings.DeveloperFeatures;

namespace vMenu.Enhanced.Menus.Developer;

/// <summary>
/// Draws the outlines and labels the developer features menu switches on.
/// </summary>
/// <remarks>
/// Split across two ticks by cost. Finding nearby entities means walking a whole game pool, which is
/// far too expensive per frame but barely matters four times a second; drawing has to happen every
/// frame or it flickers. Anything that can be worked out at scan rate is, so the frame loop is left
/// with little more than the draw calls themselves.
/// <para>
/// Both ticks are gated on the same condition, so with the feature off there is no loop running at
/// all rather than one that checks a flag and returns.
/// </para>
/// </remarks>
public static class DeveloperOverlay
{
    /// <summary>
    /// Four refreshes a second. Fast enough that walking does not outrun the list, slow enough that
    /// the pool walk stays off the frame budget.
    /// </summary>
    private const long ScanIntervalMs = 250;

    private const float LabelSize = 0.3f;

    private const int LabelFont = 0;

    private static readonly OutlinePool[] Pools =
    [
        new()
        {
            PoolName = "CVehicle",
            LabelPrefix = "Veh",
            Red = 250,
            Green = 150,
            Blue = 0,
            IsEnabled = () => DeveloperFeaturesState.ShowVehicleDimensions,
        },
        new()
        {
            PoolName = "CObject",
            LabelPrefix = "Prop",
            Red = 255,
            Green = 0,
            Blue = 0,
            IsEnabled = () => DeveloperFeaturesState.ShowPropDimensions,
        },
        new()
        {
            PoolName = "CPed",
            LabelPrefix = "Ped",
            Red = 50,
            Green = 255,
            Blue = 50,
            IsEnabled = () => DeveloperFeaturesState.ShowPedDimensions,
        },
    ];

    /// <summary>Constant per model, and otherwise rebuilt for every entity on every frame.</summary>
    private static readonly Dictionary<uint, string> ModelLabels = [];

    private static readonly StringBuilder LabelBuilder = new();

    private static readonly MenuGate Condition =
        MenuGate.Setting(DeveloperFeaturesSetting.Enabled)
        & MenuGate.When(() => DeveloperFeaturesState.AnyOutlineEnabled && DeveloperFeaturesState.DrawRadius > 0);

    private static TickHandle? _scan;

    private static TickHandle? _draw;

    /// <summary>Call after <see cref="Configuration.ClientConfig.Initialize"/>.</summary>
    public static void Initialize()
    {
        _scan = TickRegistry.Register(
            "DevFeatures.Scan",
            ScanAsync,
            TickRate.Every(ScanIntervalMs),
            Condition.Evaluate);

        _draw = TickRegistry.Register(
            "DevFeatures.Draw",
            DrawAsync,
            TickRate.PerFrame,
            Condition.Evaluate,
            onStopped: Reset);

        DeveloperFeaturesState.Changed += Reevaluate;
    }

    private static void Reevaluate()
    {
        _scan?.Reevaluate();
        _draw?.Reevaluate();
    }

    /// <summary>
    /// Dropped when the overlay stops rather than kept for next time: a session long cache of every
    /// model the player has ever driven past is not worth holding for a debugging aid.
    /// </summary>
    private static void Reset()
    {
        foreach (var pool in Pools)
        {
            pool.Clear();
        }

        ModelLabels.Clear();

        EntityBox.ClearCache();
    }

    private static Task ScanAsync()
    {
        var origin = Native.GetEntityCoords(Native.PlayerPedId(), true);
        var radius = (float)DeveloperFeaturesState.DrawRadiusMetres;
        var radiusSquared = radius * radius;
        var withOwners = DeveloperFeaturesState.ShowNetworkOwners;

        foreach (var pool in Pools)
        {
            if (pool.IsEnabled())
            {
                Refresh(pool, origin, radiusSquared, withOwners);
            }
            else
            {
                pool.Clear();
            }
        }

        return Task.CompletedTask;
    }

    private static void Refresh(OutlinePool pool, Vector3 origin, float radiusSquared, bool withOwners)
    {
        var staging = pool.Staging;

        staging.Clear();

        foreach (var handle in NativeFixer.GetGamePool(pool.PoolName))
        {
            var position = Native.GetEntityCoords(handle, false);

            if (Vector3.DistanceSquared(position, origin) > radiusSquared)
            {
                continue;
            }

            staging.Add(new TrackedEntity(
                handle,
                Native.GetEntityModel(handle),
                withOwners ? OwnerLabel(handle) : null));
        }

        // Nothing is awaited between filling the staging list and publishing it, so the draw tick
        // can only ever see a complete one. Legacy needed a flag per pool to get the same guarantee.
        pool.Publish();
    }

    private static string? OwnerLabel(int entity)
    {
        var owner = Native.NetworkGetEntityOwner(entity);

        // Nobody owns it: every map prop answers this way. Handing that index to the player natives
        // below makes the game log "Player ID -1 is invalid!" for each one, every scan.
        if (owner < 0)
        {
            return null;
        }

        var serverId = Native.GetPlayerServerId(owner);

        // Covers a stale owner index, which the legacy check for a non-zero local index did not:
        // index zero is a real player.
        return serverId == 0 ? null : $"Owner ID {serverId} ({Native.GetPlayerName(owner)})";
    }

    private static Task DrawAsync()
    {
        var showHandles = DeveloperFeaturesState.ShowEntityHandles;
        var showModels = DeveloperFeaturesState.ShowEntityModels;
        var showOwners = DeveloperFeaturesState.ShowNetworkOwners;
        var fillAlpha = DeveloperFeaturesState.BoxFillAlpha;

        // The outlines still draw over a hidden HUD, as they did in legacy. Only the labels go, so
        // the string building behind them is skipped rather than thrown away by the text call.
        var anyLabel = (showHandles || showModels || showOwners) && Hud.CanDraw;

        foreach (var pool in Pools)
        {
            var entities = pool.Visible;

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                // The list is up to one scan interval old, so an entity may already be gone.
                if (!Native.DoesEntityExist(entity.Handle) || !Native.IsEntityOnScreen(entity.Handle))
                {
                    continue;
                }

                EntityBox.Draw(entity.Handle, entity.Model, pool.Red, pool.Green, pool.Blue, fillAlpha);

                if (anyLabel)
                {
                    DrawLabel(pool, entity, showHandles, showModels, showOwners);
                }
            }
        }

        return Task.CompletedTask;
    }

    private static void DrawLabel(OutlinePool pool, TrackedEntity entity, bool handles, bool models, bool owners)
    {
        LabelBuilder.Clear();

        // Ordered to match legacy's stacking, which put the owner above the handle and the model
        // below it. A block drawn from one origin runs downward, so this is top to bottom.
        if (owners && entity.OwnerLabel is not null)
        {
            LabelBuilder.Append(entity.OwnerLabel);
        }

        if (handles)
        {
            Separate();

            LabelBuilder.Append(pool.LabelPrefix).Append(' ').Append(entity.Handle);
        }

        if (models)
        {
            Separate();

            LabelBuilder.Append(ModelLabel(entity.Model));
        }

        if (LabelBuilder.Length == 0)
        {
            return;
        }

        var position = Native.GetEntityCoords(entity.Handle, false);

        Hud.DrawText3D(
            LabelBuilder.ToString(),
            position.X,
            position.Y,
            position.Z,
            LabelSize,
            Hud.TextAlignment.Center,
            LabelFont);
    }

    private static void Separate()
    {
        if (LabelBuilder.Length > 0)
        {
            LabelBuilder.Append(Hud.NewLine);
        }
    }

    private static string ModelLabel(uint model)
    {
        if (ModelLabels.TryGetValue(model, out var label))
        {
            return label;
        }

        label = $"Hash {unchecked((int)model)} / {model} / 0x{model:X8}";
        ModelLabels[model] = label;

        return label;
    }

    private readonly struct TrackedEntity(int handle, uint model, string? ownerLabel)
    {
        public int Handle { get; } = handle;

        public uint Model { get; } = model;

        public string? OwnerLabel { get; } = ownerLabel;
    }

    /// <summary>
    /// One entity type's tracked entities, double buffered so the scan can rebuild without the draw
    /// tick ever reading a partly filled list.
    /// </summary>
    private sealed class OutlinePool
    {
        private List<TrackedEntity> _visible = [];

        private List<TrackedEntity> _staging = [];

        public required string PoolName { get; init; }

        public required string LabelPrefix { get; init; }

        public required int Red { get; init; }

        public required int Green { get; init; }

        public required int Blue { get; init; }

        public required Func<bool> IsEnabled { get; init; }

        public List<TrackedEntity> Visible => _visible;

        public List<TrackedEntity> Staging => _staging;

        public void Publish() => (_visible, _staging) = (_staging, _visible);

        public void Clear()
        {
            _visible.Clear();
            _staging.Clear();
        }
    }
}
