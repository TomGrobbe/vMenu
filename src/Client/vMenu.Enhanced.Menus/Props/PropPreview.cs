using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Developer;
using vMenu.Enhanced.Menus.Props.Saved;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Props;

public static class PropPreview
{
    private const float SamePlace = 1.5f;

    private const int OutlineRed = 90;

    private const int OutlineGreen = 200;

    private const int OutlineBlue = 255;

    private const int OutlineAlpha = 220;

    private const int MinAlpha = 60;

    private const int MaxAlpha = 220;

    private const int SteadyAlpha = 128;

    private const int BreathMs = 1600;

    private const int ModelTimeoutMs = 5000;

    private const int MaxGhosts = 100;

    private static readonly List<Ghost> Ghosts = [];

    private static TickHandle? _tick;

    private static int _highlighted;

    private static bool _breathe;

    private static string? _showing;

    private static int _generation;

    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "PropSpawner.Preview",
            Frame,
            TickRate.PerFrame,
            () => Ghosts.Count > 0 || _highlighted != 0,
            autoStart: false);

        ResourceShutdown.Stopping += Hide;
    }

    public static async void ShowAt(string model, Vector3 position, float heading)
    {
        var key = $"one:{model}:{position.X:0.##},{position.Y:0.##},{position.Z:0.##}";

        if (string.Equals(_showing, key, StringComparison.Ordinal))
        {
            return;
        }

        var hash = API.Hash(model);

        if (SpawnedProps.FindNear(hash, position, SamePlace) is { } standing)
        {
            Hide();

            _highlighted = standing;
            _showing = key;

            _tick?.Reevaluate();

            return;
        }

        Hide();

        // After Hide, which bumps the generation itself.
        var mine = _generation;

        _breathe = true;
        _showing = key;

        await AddGhostAsync(hash, position, heading, mine);

        _tick?.Reevaluate();
    }

    public static async void ShowSet(string name, IReadOnlyList<SavedProp> props)
    {
        var key = $"set:{name}:{props.Count}";

        if (string.Equals(_showing, key, StringComparison.Ordinal))
        {
            return;
        }

        Hide();

        var mine = _generation;

        _breathe = false;
        _showing = key;

        var placed = 0;

        foreach (var prop in props)
        {
            if (placed >= MaxGhosts || mine != _generation)
            {
                break;
            }

            var hash = API.Hash(prop.Model);
            var position = new Vector3(prop.X, prop.Y, prop.Z);

            if (SpawnedProps.FindNear(hash, position, SamePlace) is not null)
            {
                continue;
            }

            if (await AddGhostAsync(hash, position, prop.Heading, mine))
            {
                placed++;
            }
        }

        _tick?.Reevaluate();
    }

    public static void Highlight(int entity)
    {
        var key = $"entity:{entity}";

        if (string.Equals(_showing, key, StringComparison.Ordinal))
        {
            return;
        }

        Hide();

        if (entity == 0 || !Native.DoesEntityExist(entity))
        {
            return;
        }

        _highlighted = entity;
        _showing = key;

        _tick?.Reevaluate();
    }

    public static void Hide()
    {
        _generation++;
        _showing = null;
        _highlighted = 0;

        foreach (var ghost in Ghosts)
        {
            Destroy(ghost.Entity);
        }

        Ghosts.Clear();

        _tick?.Reevaluate();
    }

    private static async Task<bool> AddGhostAsync(uint hash, Vector3 position, float heading, int mine)
    {
        if (!PropSpawning.IsSpawnable(hash))
        {
            return false;
        }

        Native.RequestModel(hash);

        var deadline = Native.GetGameTimer() + ModelTimeoutMs;

        while (!Native.HasModelLoaded(hash))
        {
            if (Native.GetGameTimer() > deadline || mine != _generation)
            {
                Native.SetModelAsNoLongerNeeded(hash);

                return false;
            }

            await API.Delay(0);
        }

        if (mine != _generation)
        {
            Native.SetModelAsNoLongerNeeded(hash);

            return false;
        }

        var prop = API.Props.Create(hash, position, false, false, true, true, false);

        Native.SetModelAsNoLongerNeeded(hash);

        if (prop is null)
        {
            return false;
        }

        var entity = prop.Handle;

        Native.SetEntityHeading(entity, heading);
        Native.FreezeEntityPosition(entity, true);
        Native.SetEntityCollision(entity, false, false);
        Native.SetEntityInvincible(entity, true, false);
        Native.SetEntityHasGravity(entity, false);
        Native.SetEntityAlpha(entity, SteadyAlpha, false);

        Ghosts.Add(new Ghost(entity, hash));

        return true;
    }

    private static void Frame()
    {
        var alpha = _breathe ? Breath() : SteadyAlpha;

        for (var index = Ghosts.Count - 1; index >= 0; index--)
        {
            var ghost = Ghosts[index];

            if (!Native.DoesEntityExist(ghost.Entity))
            {
                Ghosts.RemoveAt(index);

                continue;
            }

            Native.SetEntityAlpha(ghost.Entity, alpha, false);

            EntityBox.DrawEdges(ghost.Entity, ghost.Model, OutlineRed, OutlineGreen, OutlineBlue, OutlineAlpha);
        }

        if (_highlighted == 0)
        {
            return;
        }

        if (!Native.DoesEntityExist(_highlighted))
        {
            _highlighted = 0;

            _tick?.Reevaluate();

            return;
        }

        EntityBox.DrawEdges(
            _highlighted,
            (uint)Native.GetEntityModel(_highlighted),
            OutlineRed,
            OutlineGreen,
            OutlineBlue,
            OutlineAlpha);
    }

    private static int Breath()
    {
        var through = Native.GetGameTimer() % BreathMs / (float)BreathMs;
        var swing = through < 0.5f ? through * 2f : (1f - through) * 2f;

        return MinAlpha + (int)((MaxAlpha - MinAlpha) * swing);
    }

    private static void Destroy(int entity)
    {
        if (entity == 0 || !Native.DoesEntityExist(entity))
        {
            return;
        }

        Native.SetEntityAsMissionEntity(entity, true, true);

        // Ref<T> cannot cross an await, and the out overload pushes a literal 0.
        var doomed = entity;

        Native.DeleteObject(ref doomed);
    }

    private readonly record struct Ghost(int Entity, uint Model);
}
