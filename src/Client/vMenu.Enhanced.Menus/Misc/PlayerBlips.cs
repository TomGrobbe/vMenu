using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Players;

namespace vMenu.Enhanced.Menus.Misc;

/// <summary>
/// A blip on the map for every other player on the server.
/// </summary>
/// <remarks>
/// Two kinds of blip, because there are two kinds of player. Somebody the game has streamed in has
/// a character we can pin a blip to, and the engine then carries it along every frame for free.
/// Somebody it has not is only a set of coordinates the server sent us, so their blip sits on those
/// coordinates and moves when the next update arrives.
///
/// <para>
/// Every blip is remembered here against the owner's <em>server id</em>, and that is the whole
/// safety design. Legacy looked its blips up with <c>GetBlipFromEntity</c>, which means a blip left
/// behind on a character that no longer exists can never be found again, and therefore never
/// removed. Holding our own handle means we can always delete it, and keying on a server id means a
/// recycled entity handle can never be mistaken for somebody we are tracking.
/// </para>
/// </remarks>
public static class PlayerBlips
{
    /// <summary>The plain dot: anybody on foot, and anybody in an ordinary car.</summary>
    private const int OnFootSprite = VehicleBlipSprites.StandardSprite;

    // White
    private const int DefaultColour = 0;
    // Green
    private const int NoClipColour = 2;

    // Orange
    private const int OutlineColour = 15;


    private const int PlayerCategory = 7;

    /// <summary>Shown on the minimap and the pause map, and selectable there.</summary>
    private const int DisplayBoth = 2;

    /// <summary>Where blips stop shrinking, and where they stop again.</summary>
    private const float NearScaleRange = 400f;

    private const float FarScaleRange = 1400f;

    private const float MinimumScale = 0.78f;

    /// <summary>How faint a distant player's blip is allowed to get on the minimap.</summary>
    private const int MinimumAlpha = 100;

    private const int FullAlpha = 255;

    /// <summary>Beyond this, the engine is asked to keep the blip off the minimap.</summary>
    private const float MinimapRange = 1000f;

    /// <summary>
    /// The most blips that will ever exist at once, nearest kept.
    /// </summary>
    // A guess, not a measurement. We can change this later if we want to.
    private const int MaxBlips = 512;

    private static readonly Dictionary<int, TrackedBlip> Blips = [];

    private static readonly List<int> Doomed = [];

    private static bool _budgetReported;

    /// <summary>Whether the game is currently drawing names beside blips on the expanded minimap.</summary>
    private static bool _labelsShown;

    /// <summary>Brings every blip into line with what this pass knows.</summary>
    public static void Apply(IReadOnlyList<PresenceView> slice, bool wanted)
    {
        if (!wanted)
        {
            if (Blips.Count > 0 || _labelsShown)
            {
                RemoveAll();
            }

            return;
        }

        if (!_labelsShown)
        {
            _labelsShown = true;

            Native.DisplayPlayerNameTagsOnBlips(true);
        }

        var self = Native.GetEntityCoords(Native.PlayerPedId(), true);

        // Read once for the whole pass rather than per player, since it cannot change halfway
        // through one.
        var paused = Native.IsPauseMenuActive();

        foreach (var player in slice)
        {
            if (player.IsHidden)
            {
                Remove(player.ServerId);

                continue;
            }

            Update(player, Vector3.Distance(self, player.Position), paused);
        }

        Reap();
    }

    /// <summary>Takes every blip off the map. Safe to call when there are none.</summary>
    public static void RemoveAll()
    {
        foreach (var blip in Blips.Values)
        {
            Destroy(blip.Handle);
        }

        Blips.Clear();
        Doomed.Clear();

        if (_labelsShown)
        {
            _labelsShown = false;

            Native.DisplayPlayerNameTagsOnBlips(false);
        }
    }

    private static void Update(PresenceView player, float distance, bool paused)
    {
        var sprite = player.VehicleModel == 0
            ? OnFootSprite
            : BlipSprites.ForVehicleModel(player.VehicleModel);

        var blip = Ensure(player, sprite);

        if (blip is null)
        {
            return;
        }

        if (blip.Sprite != sprite)
        {
            Native.SetBlipSprite(blip.Handle, sprite);

            blip.Sprite = sprite;

            // Every one of these is reset by the game when the sprite changes, so they have to be
            // put back rather than assumed to have stuck. Rockstar's own code does the same.
            Reapply(blip, player);
        }
        else
        {
            // Only when they have actually changed, which after the first pass means only when this
            // player was granted or lost the permission, or stepped in or out of noclip. A blip is
            // otherwise left alone here.
            if (blip.Outlined != player.IsStaff)
            {
                Outline(blip, player.IsStaff);
            }

            var colour = ColourFor(player);

            if (blip.Colour != colour)
            {
                Recolour(blip, colour);
            }
        }

        // A coordinate blip is the only kind that has to be moved. One pinned to a character is
        // carried by the engine every frame, which no tick could match.
        if (!blip.IsOnPed)
        {
            Native.SetBlipCoords(blip.Handle, player.Position.X, player.Position.Y, player.Position.Z);
        }

        Rotate(blip, sprite, player.Heading);

        Native.SetBlipScale(blip.Handle, ScaleFor(distance));
        Native.SetBlipAlpha(blip.Handle, paused ? FullAlpha : AlphaFor(distance));

        // Expanding the minimap is how you ask to see the wider world, so that is when distant
        // players come back onto it. The rest of the time the engine keeps them to the pause map.
        var shortRange = distance > MinimapRange && !MinimapControls.IsBigmapExpanded;

        if (blip.ShortRange != shortRange)
        {
            Native.SetBlipAsShortRange(blip.Handle, shortRange);

            blip.ShortRange = shortRange;
        }
    }

    /// <summary>
    /// Turns a blip to face where its owner is heading, but only when nothing else will.
    /// </summary>
    private static void Rotate(TrackedBlip blip, int sprite, int heading)
    {
        if (blip.IsOnPed || sprite != OnFootSprite)
        {
            return;
        }

        Native.SetBlipRotation(blip.Handle, heading);
    }

    /// <summary>
    /// Finds this player's blip, rebuilding it if it is the wrong kind or is on the wrong character.
    /// </summary>
    private static TrackedBlip? Ensure(PresenceView player, int sprite)
    {
        if (Blips.TryGetValue(player.ServerId, out var existing))
        {
            var stillRight =
                Native.DoesBlipExist(existing.Handle)
                && existing.IsOnPed == player.IsStreamed
                && existing.Ped == player.Ped;

            if (stillRight)
            {
                return existing;
            }

            // Either the player streamed in or out, or they changed character. The old blip is on
            // something that is no longer them, which is precisely the case that used to leave blips
            // wandering the city attached to whatever inherited the entity handle.
            Destroy(existing.Handle);
            Blips.Remove(player.ServerId);
        }

        if (Blips.Count >= MaxBlips)
        {
            if (!_budgetReported)
            {
                _budgetReported = true;

                Log.Warning(
                    $"[Blips] There are already {MaxBlips} player blips on the map, so no more are being "
                    + "made. Everything past this point is invisible until somebody moves away.");
            }

            return null;
        }

        var handle = player.IsStreamed
            ? Native.AddBlipForEntity(player.Ped)
            : Native.AddBlipForCoord(player.Position.X, player.Position.Y, player.Position.Z);

        if (handle == 0 || !Native.DoesBlipExist(handle))
        {
            return null;
        }

        var blip = new TrackedBlip(handle, player.IsStreamed ? player.Ped : 0, sprite);

        Native.SetBlipSprite(handle, sprite);

        Reapply(blip, player);

        Blips[player.ServerId] = blip;

        return blip;
    }

    /// <summary>The settings that a change of sprite wipes, and that creation has to set anyway.</summary>
    private static void Reapply(TrackedBlip blip, PresenceView player)
    {
        Recolour(blip, ColourFor(player));

        Native.SetBlipCategory(blip.Handle, PlayerCategory);
        Native.SetBlipDisplay(blip.Handle, DisplayBoth);
        Native.SetBlipHighDetail(blip.Handle, true);

        Outline(blip, player.IsStaff);

        // Only the plain dot is allowed one, which is Rockstar's rule and not an arbitrary one: the
        // cone is drawn around the dot and looks wrong on anything shaped like a vehicle.
        Native.ShowHeadingIndicatorOnBlip(blip.Handle, blip.Sprite == OnFootSprite);

        // Worth knowing whether somebody is above or below you only when you are flying, which is
        // the one case Rockstar turns it on for as well.
        Native.ShowHeightOnBlip(blip.Handle, IsLocalPlayerFlying());

        Name(blip, player);

        // Set rather than assumed. A blip is created long range, but this also runs after a change
        // of sprite, and a distant player whose blip was hidden from the minimap must not quietly
        // reappear on it just because they got into a helicopter.
        Native.SetBlipAsShortRange(blip.Handle, false);

        blip.ShortRange = false;
    }

    /// <summary>What colour this player's dot should be.</summary>
    // Noclip is the only thing that changes it, and a noclipping player can only have a blip at all
    // in front of somebody allowed to see through noclip, so this needs no permission check of its own.
    private static int ColourFor(PresenceView player) => player.NoClip ? NoClipColour : DefaultColour;

    /// <summary>Sets the blip's own colour, remembering it so the next pass knows what it is.</summary>
    // Remembered rather than read back, because there is no native that asks a blip its colour.
    private static void Recolour(TrackedBlip blip, int colour)
    {
        blip.Colour = colour;

        Native.SetBlipColour(blip.Handle, colour);
    }

    /// <summary>
    /// Draws a ring around a staff member's blip, so anybody can pick them out of a busy map.
    /// </summary>
    /// <remarks>
    /// The ring is really the friend and crew marking the game draws in GTA Online, borrowed here
    /// because it is the only outline a blip has. Both are asked for together: they are drawn on top
    /// of each other, so turning on only one leaves a thinner ring than intended. The colour is the
    /// blip's secondary colour, which is what the game reads for that ring and nothing else, so it
    /// is only worth setting while the ring is actually being drawn.
    /// </remarks>
    private static void Outline(TrackedBlip blip, bool staff)
    {
        blip.Outlined = staff;

        Native.ShowFriendIndicatorOnBlip(blip.Handle, staff);
        Native.ShowCrewIndicatorOnBlip(blip.Handle, staff);

        if (!staff)
        {
            return;
        }

        Native.GetHudColour(OutlineColour, out var red, out var green, out var blue, out var _);
        Native.SetBlipSecondaryColour(blip.Handle, red, green, blue);
    }

    /// <summary>
    /// Puts the player's name on the blip, so the pause map legend reads as names rather than dots.
    /// </summary>
    /// <remarks>
    /// Two ways of doing it, because there are two kinds of player. For somebody streamed in the
    /// game already knows who owns the slot and can be asked. For somebody it has never heard of,
    /// the name came from the server alongside their position and has to be fed in as plain text.
    /// </remarks>
    private static void Name(TrackedBlip blip, PresenceView player)
    {
        if (player.Slot >= 0)
        {
            Native.SetBlipNameToPlayerName(blip.Handle, player.Slot);

            return;
        }

        if (string.IsNullOrEmpty(player.Name))
        {
            return;
        }

        Native.BeginTextCommandSetBlipName("STRING");
        Native.AddTextComponentSubstringPlayerName(player.Name);
        Native.EndTextCommandSetBlipName(blip.Handle);
    }

    /// <summary>Removes blips for anybody who has gone quiet or is no longer worth one.</summary>
    // Bounded rather than a full sweep: the pass this runs from only looked at a slice of the
    // players, so anything it did not touch is only stale, not missing.
    private static void Reap()
    {
        Doomed.Clear();

        foreach (var pair in Blips)
        {
            var gone =
                !Native.DoesBlipExist(pair.Value.Handle)
                || (!PlayerRoster.IsStreamed(pair.Key) && !PlayerPresence.TryGetRemote(pair.Key, out _));

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
        if (!Blips.Remove(serverId, out var blip))
        {
            return;
        }

        Destroy(blip.Handle);
    }

    private static void Destroy(int handle)
    {
        if (Native.DoesBlipExist(handle))
        {
            Native.RemoveBlip(handle);
        }
    }

    private static bool IsLocalPlayerFlying()
    {
        var ped = Native.PlayerPedId();

        return Native.IsPedInAnyHeli(ped) || Native.IsPedInAnyPlane(ped);
    }

    private static float ScaleFor(float distance)
    {
        if (distance <= NearScaleRange)
        {
            return 1f;
        }

        if (distance >= FarScaleRange)
        {
            return MinimumScale;
        }

        var travelled = (distance - NearScaleRange) / (FarScaleRange - NearScaleRange);

        return 1f - ((1f - MinimumScale) * travelled);
    }

    private static int AlphaFor(float distance)
    {
        if (distance <= FarScaleRange)
        {
            return FullAlpha;
        }

        // Dimming continues over the same distance again past the point where shrinking stopped, so
        // there is a visible difference between "far" and "the other end of the map".
        var travelled = Math.Min((distance - FarScaleRange) / FarScaleRange, 1f);

        return (int)(FullAlpha - ((FullAlpha - MinimumAlpha) * travelled));
    }

    // A plain class rather than a record, matching the rest of this codebase: the generated equality
    // routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class TrackedBlip(int handle, int ped, int sprite)
    {
        public int Handle { get; } = handle;

        /// <summary>The character this blip is pinned to, or zero when it sits on coordinates.</summary>
        public int Ped { get; } = ped;

        public bool IsOnPed => Ped != 0;

        public int Sprite { get; set; } = sprite;

        /// <summary>The colour this blip was last given, so a change of colour can be spotted.</summary>
        public int Colour { get; set; } = DefaultColour;

        public bool ShortRange { get; set; }

        /// <summary>Whether this blip currently has the staff ring around it.</summary>
        public bool Outlined { get; set; }
    }
}
