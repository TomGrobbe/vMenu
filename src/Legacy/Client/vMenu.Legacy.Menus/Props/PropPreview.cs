
using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Developer;
using vMenu.Enhanced.Menus.Props.Saved;
using vMenu.Enhanced.Ticks;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

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

        var hash = Hashing.Joaat(model);

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

            var hash = Hashing.Joaat(prop.Model);
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

        if (entity == 0 || !API.DoesEntityExist(entity))
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

        API.RequestModel(hash);

        var deadline = API.GetGameTimer() + ModelTimeoutMs;

        while (!API.HasModelLoaded(hash))
        {
            if (API.GetGameTimer() > deadline || mine != _generation)
            {
                API.SetModelAsNoLongerNeeded(hash);

                return false;
            }

            await BaseScript.Delay(0);
        }

        if (mine != _generation)
        {
            API.SetModelAsNoLongerNeeded(hash);

            return false;
        }

        var handle = NativeFixer.CreateObjectNoOffset(hash, position.X, position.Y, position.Z, false, true, true, false);

        API.SetModelAsNoLongerNeeded(hash);

        if (handle <= 0)
        {
            return false;
        }

        var prop = new Prop(handle);

        var entity = prop.Handle;

        API.SetEntityHeading(entity, heading);
        API.FreezeEntityPosition(entity, true);
        API.SetEntityCollision(entity, false, false);
        NativeFixer.SetEntityInvincible(entity, true, false);
        API.SetEntityHasGravity(entity, false);
        NativeFixer.SetEntityAlpha(entity, SteadyAlpha, false);

        Ghosts.Add(new Ghost(entity, hash));

        return true;
    }

    private static void Frame()
    {
        var alpha = _breathe ? Breath() : SteadyAlpha;

        for (var index = Ghosts.Count - 1; index >= 0; index--)
        {
            var ghost = Ghosts[index];

            if (!API.DoesEntityExist(ghost.Entity))
            {
                Ghosts.RemoveAt(index);

                continue;
            }

            NativeFixer.SetEntityAlpha(ghost.Entity, alpha, false);

            EntityBox.DrawEdges(ghost.Entity, ghost.Model, OutlineRed, OutlineGreen, OutlineBlue, OutlineAlpha);
        }

        if (_highlighted == 0)
        {
            return;
        }

        if (!API.DoesEntityExist(_highlighted))
        {
            _highlighted = 0;

            _tick?.Reevaluate();

            return;
        }

        EntityBox.DrawEdges(
            _highlighted,
            NativeFixer.GetEntityModel(_highlighted),
            OutlineRed,
            OutlineGreen,
            OutlineBlue,
            OutlineAlpha);
    }

    private static int Breath()
    {
        var through = API.GetGameTimer() % BreathMs / (float)BreathMs;
        var swing = through < 0.5f ? through * 2f : (1f - through) * 2f;

        return MinAlpha + (int)((MaxAlpha - MinAlpha) * swing);
    }

    private static void Destroy(int entity)
    {
        if (entity == 0 || !API.DoesEntityExist(entity))
        {
            return;
        }

        API.SetEntityAsMissionEntity(entity, true, true);

        // Ref<T> cannot cross an await, and the out overload pushes a literal 0.
        var doomed = entity;

        NativeFixer.DeleteObject(ref doomed);
    }

    private readonly record struct Ghost(int Entity, uint Model);
}
