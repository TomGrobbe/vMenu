using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Misc;

/// <summary>
/// Names, health bars and wanted stars floating above the players around you.
/// </summary>
/// <remarks>
/// Only ever for players the game has streamed in, because a tag needs a character to float above.
/// Somebody the server merely told us about has no head to put a name over, so they get a blip and
/// nothing else.
/// </remarks>
public static class OverheadNames
{
    /// <summary>The components of a tag, as the game numbers them.</summary>
    private const int ComponentName = 0;
    private const int ComponentHealthArmour = 2;

    private const int ComponentWantedStars = 7;

    private const int ComponentArrow = 12;

    /// <summary>Within this, the full tag. Past it, just an arrow.</summary>
    // Rockstar swaps to the arrow at 30m in freemode, which is very close indeed. 100m suits a menu
    // resource better: far enough to pick somebody out across a street, close enough that a crowd
    // does not turn into a wall of text.
    private const float FullTagRange = 50f;

    /// <summary>Past this the tag is taken down altogether, freeing the slot.</summary>
    private const float ArrowRange = 75f;

    /// <summary>What the engine is told, so it stops drawing tags before we stop tracking them.</summary>
    // Global to the game rather than per tag, so other resources share it. Put back on the way out.
    private const float EngineDefaultDistance = 75f;

    private const int FullAlpha = 255;

    /// <summary>The colours a name is drawn in, as the game numbers its HUD palette.</summary>
    // Orange says the player under this name is staff, so anybody can pick them out of a crowd.
    // Everybody else gets the plain white a freemode name is normally drawn in.
    private const int StaffNameColour = 15;

    private const int PlainNameColour = 0;

    /// <summary>
    /// How long a new tag is left alone before the sweep is allowed to judge it.
    /// </summary>
    // Creating a tag is not instant. Without this, the sweep that runs at the end of the same pass
    // sees a tag that has not come up yet, decides it is dead, and removes it. Then the next pass
    // makes it again. That loop renders nothing at all, forever.
    private const int GraceMs = 1000;

    /// <summary>
    /// The most tags on screen at once, nearest kept.
    /// </summary>
    // A legibility limit rather than the game's. A hundred names on screen is already far past
    // useful, and the pool these come from holds 128.
    private const int MaxTags = 100;

    /// <summary>How much of a tag's name the game will actually draw before it stops reading.</summary>
    // Sixteen is the length of a Rockstar Social Club name, which is what the buffer was sized for.
    // Anything longer is silently cut off rather than refused.
    private const int MaxLabelLength = 16;

    // The player's server id is deliberately not shown here, though legacy vMenu put it after the
    // name. There is nowhere for it to go: the name is capped at sixteen characters, so on anybody
    // with a long name the id either fell off the end or ate into the name to fit. The game's two
    // other text slots are no better. Big text is drawn at headline size floating well above the
    // player, for announcements rather than details, and the crew tag only renders real crew data,
    // which a tag made from a ped has none of. The online players menu is where to look up an id.

    /// <summary>Which player currently has a tag, keyed by server id.</summary>
    // Keyed by server id like everything else here, because a player index is only meaningful until
    // somebody walks out of range and comes back with a different one.
    private static readonly Dictionary<int, TrackedTag> Tags = [];

    private static readonly List<int> Doomed = [];

    private static TickHandle? _refresh;

    private static bool _engineDistanceSet;

    public static void Initialize() =>
        // Per frame, and asleep unless there is at least one tag up. The game clears a tag's
        // component visibility for itself, so it has to be asked for again continually rather than
        // set once.
        _refresh = TickRegistry.Register("Player.OverheadNames", Refresh, TickRate.PerFrame, () => Tags.Count > 0);

    /// <summary>Decides who should have a tag. Called from the slow pass, on a slice of players.</summary>
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

    /// <summary>Takes every tag down and hands the game its own distance back.</summary>
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
        // A tag needs a character, and somebody hidden should not have one whether they have a
        // character or not.
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

        // Worked out here, on the slow pass, and simply replayed by the per frame one. Distance
        // maths and a wanted level lookup are not things to do sixty times a second per player.
        tag.ShowFull = distance <= FullTagRange;
        tag.WantedLevel = tag.ShowFull ? Native.GetPlayerWantedLevel(player.Slot) : 0;
        tag.IsStaff = player.IsStaff;
    }

    /// <summary>Re-asks for everything the game forgets between frames.</summary>
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

            // Set either way rather than only for staff, so somebody who loses the permission while
            // playing goes back to white on the next frame instead of keeping the colour their tag
            // was last given.
            Native.SetMpGamerTagColour(tag.TagId, ComponentName, tag.IsStaff ? StaffNameColour : PlainNameColour);
        }
    }

    /// <summary>Finds this player's tag, making one if they have not got a usable one.</summary>
    private static TrackedTag? Ensure(PresenceView player)
    {
        if (Tags.TryGetValue(player.ServerId, out var existing))
        {
            // A tag is tied to the character it was made for and to the slot it was made in, and
            // both can change underneath us: a new ped model, or the player leaving range and
            // coming back in a different slot.
            //
            // The slot is compared against the slot it was made with, and not against the tag id.
            // Those are unrelated numbers, so comparing them never matches, and the tag gets thrown
            // away and rebuilt on every pass. That reads on screen as a name flashing several times
            // a second.
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

        // Made from the ped rather than from the player index. The game has two natives for this and
        // they are not interchangeable: CreateMpGamerTagWithCrewColor takes a player index and makes
        // that index the tag id, which is what Rockstar's own scripts use, but on FiveM Enhanced it
        // does nothing whatsoever. The tag never reports itself active and never draws. This one works.
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

    /// <summary>Shows or hides one part of a tag.</summary>
    /// <remarks>
    /// The binding carries a fourth argument that the game only grew in a later build. The generator
    /// has guessed at a name for it and there is no documentation saying what it does, so it is left
    /// at false, which is the value the ordinary three argument calls end up passing.
    /// </remarks>
    private static void SetComponent(int tagId, int component, bool visible) =>
        Native.SetMpGamerTagVisibility(tagId, component, visible, false);

    /// <summary>
    /// Takes down tags belonging to players who are no longer there.
    /// </summary>
    /// <remarks>
    /// This has to cover every tag rather than only the ones this pass looked at. Somebody who
    /// disconnects drops out of the list the passes work through, so their turn never comes round
    /// again and their tag would sit above a character that no longer exists for good.
    /// </remarks>
    private static void Reap()
    {
        Doomed.Clear();

        foreach (var pair in Tags)
        {
            // A tag too young to have come up yet is not a dead tag. Judging one on the pass that
            // created it is what stops any of them ever appearing.
            if (pair.Value.IsNew)
            {
                continue;
            }

            // The character is asked about directly as well as the roster, because the roster is
            // rebuilt on its own schedule and the character can go at any moment in between.
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

        // Asked for unconditionally, and everything switched off first, which Rockstar's own code is
        // explicit about: anything turned on has to be turned off before the tag goes. Skipping this
        // when the tag does not report itself active drops our record of it while leaving the tag
        // itself in place, with nothing left that knows how to take it down.
        Native.SetAllMpGamerTagsVisibility(tag.TagId, false);
        Native.RemoveMpGamerTag(tag.TagId);
    }

    /// <summary>The player's name, trimmed to what the game will actually draw.</summary>
    private static string Label(string? rawName)
    {
        var name = Sanitise(rawName);

        return name.Length <= MaxLabelLength ? name : name[..MaxLabelLength];
    }

    /// <summary>
    /// Strips the characters that would otherwise be read as formatting.
    /// </summary>
    // A name is somebody else's text drawn on your screen. Left alone, a tilde turns the rest of the
    // tag a different colour and a caret does the same, so a player could disguise their own name or
    // scribble over the layout. Legacy passed names through untouched.
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

    // A plain class rather than a record, matching the rest of this codebase: the generated equality
    // routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class TrackedTag(int tagId, int slot, int ped, int createdAt)
    {
        /// <summary>What the game calls this tag. Not the player index, and not interchangeable with it.</summary>
        public int TagId { get; } = tagId;

        /// <summary>The player index this tag was made for, kept so a change of slot is noticed.</summary>
        public int Slot { get; } = slot;

        public int Ped { get; } = ped;

        /// <summary>Whether this tag is still inside its settling-in period.</summary>
        public bool IsNew => Native.GetGameTimer() - createdAt < GraceMs;

        public bool ShowFull { get; set; }

        public int WantedLevel { get; set; }

        /// <summary>Whether the player this tag belongs to is staff.</summary>
        public bool IsStaff { get; set; }
    }
}
