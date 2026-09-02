using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Players;

namespace vMenu.Enhanced.Menus.Misc;

// Blips are held against the owner's server id rather than looked up with GetBlipFromEntity, so
// one left behind on a character that no longer exists can still be found and removed.
public static class PlayerBlips
{
    private const int OnFootSprite = VehicleBlipSprites.StandardSprite;

    // White
    private const int DefaultColour = 0;
    // Green
    private const int NoClipColour = 2;

    // Orange
    private const int OutlineColour = 15;

    private const int PlayerCategory = 7;

    // Minimap and pause map, and selectable on it.
    private const int DisplayBoth = 2;

    private const float NearScaleRange = 400f;

    private const float FarScaleRange = 1400f;

    private const float MinimumScale = 0.78f;

    private const int MinimumAlpha = 100;

    private const int FullAlpha = 255;

    private const float MinimapRange = 1000f;

    // A guess, not a measurement.
    private const int MaxBlips = 512;

    private static readonly Dictionary<int, TrackedBlip> Blips = [];

    private static readonly List<int> Doomed = [];

    private static bool _budgetReported;

    private static bool _labelsShown;

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

        var rotates = BlipRotation.WantedForModel(player.VehicleModel);

        var blip = Ensure(player, sprite, rotates);

        if (blip is null)
        {
            return;
        }

        if (blip.Sprite != sprite)
        {
            Native.SetBlipSprite(blip.Handle, sprite);

            blip.Sprite = sprite;
            blip.Rotates = rotates;

            // The game resets these when the sprite changes, so they are put back rather than assumed.
            Reapply(blip, player);
        }
        else
        {
            // A car and a bicycle are both the plain dot, so this can change without the sprite changing.
            if (blip.Rotates != rotates)
            {
                blip.Rotates = rotates;

                Native.ShowHeadingIndicatorOnBlip(blip.Handle, ConeWanted(blip));
            }

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

        // Only a coordinate blip has to be moved. One pinned to a character is carried by the engine.
        if (!blip.IsOnPed)
        {
            Native.SetBlipCoords(blip.Handle, player.Position.X, player.Position.Y, player.Position.Z);
        }

        Rotate(blip, player.Heading);

        Native.SetBlipScale(blip.Handle, ScaleFor(distance));
        Native.SetBlipAlpha(blip.Handle, paused ? FullAlpha : AlphaFor(distance));

        // Expanding the minimap is how you ask to see the wider world, so distant players come back then.
        var shortRange = distance > MinimapRange && !MinimapControls.IsBigmapExpanded;

        if (blip.ShortRange != shortRange)
        {
            Native.SetBlipAsShortRange(blip.Handle, shortRange);

            blip.ShortRange = shortRange;
        }
    }

    private static void Rotate(TrackedBlip blip, int heading)
    {
        if (blip.Rotates)
        {
            if (!blip.IsOnPed)
            {
                Native.SetBlipRotation(blip.Handle, heading);
            }

            return;
        }

        Native.SetBlipRotation(blip.Handle, 0);
    }

    private static bool ConeWanted(TrackedBlip blip) => blip.Sprite == OnFootSprite && blip.Rotates;

    private static TrackedBlip? Ensure(PresenceView player, int sprite, bool rotates)
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

            // The old blip is on something that is no longer them, which is what used to leave blips wandering.
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

        var blip = new TrackedBlip(handle, player.IsStreamed ? player.Ped : 0, sprite, rotates);

        Native.SetBlipSprite(handle, sprite);

        Reapply(blip, player);

        Blips[player.ServerId] = blip;

        return blip;
    }

    // A change of sprite wipes these, and creation has to set them anyway.
    private static void Reapply(TrackedBlip blip, PresenceView player)
    {
        Recolour(blip, ColourFor(player));

        Native.SetBlipCategory(blip.Handle, PlayerCategory);
        Native.SetBlipDisplay(blip.Handle, DisplayBoth);
        Native.SetBlipHighDetail(blip.Handle, true);

        Outline(blip, player.IsStaff);

        Native.ShowHeadingIndicatorOnBlip(blip.Handle, ConeWanted(blip));

        // Only worth knowing while flying, which is the one case Rockstar turns it on for as well.
        Native.ShowHeightOnBlip(blip.Handle, IsLocalPlayerFlying());

        Name(blip, player);

        // This also runs after a change of sprite, so a distant player must not quietly reappear on the minimap.
        Native.SetBlipAsShortRange(blip.Handle, false);

        blip.ShortRange = false;
    }

    // A noclipping player only has a blip in front of somebody allowed to see through noclip,
    // so this needs no permission check of its own.
    private static int ColourFor(PresenceView player) => player.NoClip ? NoClipColour : DefaultColour;

    // Remembered rather than read back, because no native asks a blip its colour.
    private static void Recolour(TrackedBlip blip, int colour)
    {
        blip.Colour = colour;

        Native.SetBlipColour(blip.Handle, colour);
    }

    // The ring is GTA Online's friend and crew marking, the only outline a blip has. Both are asked for
    // together because they draw on top of each other, and the secondary colour is what the game reads for it.
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

    // Streamed in, the game knows who owns the slot. Otherwise the name came from the server as plain text.
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

    // Bounded rather than a full sweep: the pass this runs from only looked at a slice of the players.
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

        // Dimming continues over the same distance again past the point where shrinking stopped.
        var travelled = Math.Min((distance - FarScaleRange) / FarScaleRange, 1f);

        return (int)(FullAlpha - ((FullAlpha - MinimumAlpha) * travelled));
    }

    // A class rather than a record: generated equality routes through
    // EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class TrackedBlip(int handle, int ped, int sprite, bool rotates)
    {
        public int Handle { get; } = handle;

        public int Ped { get; } = ped;

        public bool IsOnPed => Ped != 0;

        public int Sprite { get; set; } = sprite;

        public bool Rotates { get; set; } = rotates;

        public int Colour { get; set; } = DefaultColour;

        public bool ShortRange { get; set; }

        public bool Outlined { get; set; }
    }
}
