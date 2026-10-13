using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Misc;

public static class OverheadNames
{
    private const int ComponentName = 0;
    private const int ComponentHealthArmour = 2;

    private const int ComponentWantedStars = 7;

    private const int ComponentArrow = 12;

    // Rockstar swaps to the arrow at 30m, too close to pick somebody out for a menu resource.
    private const float FullTagRange = 50f;

    private const float ArrowRange = 75f;

    // Global to the game rather than per tag, so it is put back on the way out.
    private const float EngineDefaultDistance = 75f;

    private const int FullAlpha = 255;

    // HUD palette: 15 is the orange that marks staff, 0 the usual freemode white.
    private const int StaffNameColour = 15;

    private const int PlainNameColour = 0;

    // A tag is not up the moment it is made, and judging one on the pass that created it removes
    // and remakes it forever, rendering nothing at all.
    private const int GraceMs = 1000;

    // A legibility limit rather than the game's; the pool these come from holds 128.
    private const int MaxTags = 100;

    // The game stops reading at sixteen, the length of a Social Club name, which is also why
    // there is no server id on a tag: there is nowhere left to put one.
    private const int MaxLabelLength = 16;

    // Keyed by server id, because a player index only holds until somebody leaves range and comes back.
    private static readonly Dictionary<int, TrackedTag> Tags = [];

    private static readonly List<int> Doomed = [];

    private static TickHandle? _refresh;

    private static bool _engineDistanceSet;

    public static void Initialize() =>
        // The game clears a tag's component visibility itself, so it has to be asked for every frame.
        _refresh = TickRegistry.Register("Player.OverheadNames", Refresh, TickRate.PerFrame, () => Tags.Count > 0);

    public static void Apply(IReadOnlyList<PresenceView> slice, bool wanted)
    {
        if (!wanted)
        {
            if (Tags.Count > 0 || _engineDistanceSet)
            {
                RemoveAll();
            }

            return;
        }

        if (!_engineDistanceSet)
        {
            _engineDistanceSet = true;

            Native.SetMpGamerTagsVisibleDistance(ArrowRange);
        }

        var self = Native.GetEntityCoords(Native.PlayerPedId(), true);

        foreach (var player in slice)
        {
            Update(player, Vector3.Distance(self, player.Position));
        }

        Reap();

        _refresh?.Reevaluate();
    }

    public static void RemoveAll()
    {
        foreach (var serverId in Tags.Keys.ToList())
        {
            Remove(serverId);
        }

        Tags.Clear();

        if (_engineDistanceSet)
        {
            _engineDistanceSet = false;

            Native.SetMpGamerTagsVisibleDistance(EngineDefaultDistance);
        }

        _refresh?.Reevaluate();
    }

    private static void Update(PresenceView player, float distance)
    {
        // A tag needs a character to float above, so only streamed players get one.
        var wanted =
            player.IsStreamed
            && !player.IsHidden
            && distance <= ArrowRange
            && (player.NoClip || Native.IsEntityVisible(player.Ped))
            && !Native.IsPlayerDead(player.Slot);

        if (!wanted)
        {
            Remove(player.ServerId);

            return;
        }

        var tag = Ensure(player);

        if (tag is null)
        {
            return;
        }

        // Worked out on the slow pass and replayed by the per frame one, which cannot afford
        // distance maths and a wanted lookup per player.
        tag.ShowFull = distance <= FullTagRange;
        tag.WantedLevel = tag.ShowFull ? Native.GetPlayerWantedLevel(player.Slot) : 0;
        tag.IsStaff = player.IsStaff;
    }

    private static void Refresh()
    {
        foreach (var tag in Tags.Values)
        {
            if (!Native.DoesEntityExist(tag.Ped) || !Native.IsEntityOnScreen(tag.Ped))
            {
                Native.SetAllMpGamerTagsVisibility(tag.TagId, false);

                continue;
            }

            SetComponent(tag.TagId, ComponentName, tag.ShowFull);
            SetComponent(tag.TagId, ComponentHealthArmour, tag.ShowFull);
            SetComponent(tag.TagId, ComponentArrow, !tag.ShowFull);
            SetComponent(tag.TagId, ComponentWantedStars, tag.WantedLevel > 0);

            if (tag.WantedLevel > 0)
            {
                Native.SetMpGamerTagWantedLevel(tag.TagId, tag.WantedLevel);
            }

            Native.SetMpGamerTagAlpha(tag.TagId, ComponentName, FullAlpha);
            Native.SetMpGamerTagAlpha(tag.TagId, ComponentHealthArmour, FullAlpha);
            Native.SetMpGamerTagAlpha(tag.TagId, ComponentArrow, FullAlpha);
            Native.SetMpGamerTagAlpha(tag.TagId, ComponentWantedStars, FullAlpha);

            // Set either way, so somebody who loses the permission goes back to white on the next frame.
            Native.SetMpGamerTagColour(tag.TagId, ComponentName, tag.IsStaff ? StaffNameColour : PlainNameColour);
        }
    }

    private static TrackedTag? Ensure(PresenceView player)
    {
        if (Tags.TryGetValue(player.ServerId, out var existing))
        {
            // The slot is compared against the slot the tag was made with, not against the tag id.
            // Those are unrelated numbers, and comparing them rebuilds the tag on every pass.
            var stillRight =
                existing.Ped == player.Ped
                && existing.Slot == player.Slot
                && (existing.IsNew || Native.IsMpGamerTagActive(existing.TagId));

            if (stillRight)
            {
                return existing;
            }

            Remove(player.ServerId);
        }

        if (Tags.Count >= MaxTags)
        {
            return null;
        }

        // CreateMpGamerTagWithCrewColor takes a player index and is what Rockstar's own scripts use,
        // but on Enhanced it never draws. This one, made from the ped, works.
        var tagId = Native.CreateFakeMpGamerTag(
            player.Ped,
            Label(Native.GetPlayerName(player.Slot)),
            true,
            false,
            "",
            0);

        if (tagId == 0)
        {
            return null;
        }

        var tag = new TrackedTag(tagId, player.Slot, player.Ped, Native.GetGameTimer());

        Tags[player.ServerId] = tag;

        return tag;
    }

    // The fourth argument arrived in a later build undocumented, so it stays at what the three
    // argument calls pass.
    private static void SetComponent(int tagId, int component, bool visible) =>
        Native.SetMpGamerTagVisibility(tagId, component, visible, false);

    // Covers every tag rather than this pass's, because somebody who disconnects drops out of
    // the list and their turn never comes round again.
    private static void Reap()
    {
        Doomed.Clear();

        foreach (var pair in Tags)
        {
            if (pair.Value.IsNew)
            {
                continue;
            }

            // The character is asked about directly too, because the roster is rebuilt on its own schedule.
            var gone =
                !PlayerRoster.IsStreamed(pair.Key)
                || !Native.DoesEntityExist(pair.Value.Ped)
                || !Native.IsMpGamerTagActive(pair.Value.TagId);

            if (gone)
            {
                Doomed.Add(pair.Key);
            }
        }

        foreach (var serverId in Doomed)
        {
            Remove(serverId);
        }
    }

    private static void Remove(int serverId)
    {
        if (!Tags.Remove(serverId, out var tag))
        {
            return;
        }

        // Rockstar is explicit that anything turned on has to be turned off before the tag goes.
        Native.SetAllMpGamerTagsVisibility(tag.TagId, false);
        Native.RemoveMpGamerTag(tag.TagId);
    }

    private static string Label(string? rawName)
    {
        var name = Sanitise(rawName);

        return name.Length <= MaxLabelLength ? name : name[..MaxLabelLength];
    }

    // A name is somebody else's text on your screen: a tilde or caret would recolour the rest of it.
    private static string Sanitise(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var clean = new System.Text.StringBuilder(name.Length);

        foreach (var character in name)
        {
            if (character is not ('~' or '^' or '<' or '>'))
            {
                clean.Append(character);
            }
        }

        return clean.Length == 0 ? "?" : clean.ToString();
    }

    // A class rather than a record: generated equality routes through
    // EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class TrackedTag(int tagId, int slot, int ped, int createdAt)
    {
        public int TagId { get; } = tagId;

        public int Slot { get; } = slot;

        public int Ped { get; } = ped;

        public bool IsNew => Native.GetGameTimer() - createdAt < GraceMs;

        public bool ShowFull { get; set; }

        public int WantedLevel { get; set; }

        public bool IsStaff { get; set; }
    }
}
