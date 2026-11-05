using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Freecam;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Players;

using FilmmakerPermissions = vMenu.Enhanced.Data.Permissions.Menus.Filmmaker;
using FreecamCamera = vMenu.Enhanced.Freecam.Freecam;

namespace vMenu.Enhanced.Menus.Filmmaking;

[VMenu(
    TitleKey = Loc.Filmmaker.TargetTitle,
    SubtitleKey = Loc.Filmmaker.TargetSubtitle,
    DescriptionKey = Loc.Filmmaker.TargetLinkDescription,
    Permission = FilmmakerPermissions.TargetLock)]
public sealed class FilmmakerTargetMenu : MenuDefinition
{
    private const float AimDistance = 1000f;

    private const float NearbyRadius = 200f;

    private const int NearbyLimit = 30;

    private const float OffsetStep = 0.1f;

    private const int VehiclesPedsObjectsAndMap = 1 | 2 | 4 | 8 | 16;

    private const int ShapeTestOptions = 7;

    private const int ShapeTestReady = 2;

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new SeparatorEntry
        {
            Text = MenuText.From(Status),
            ShowArrows = false,
        });

        menu.Entries.Add(new ButtonEntry(Loc.Filmmaker.TargetAim)
        {
            ReadEnabled = () => FreecamCamera.IsActive,
            OnSelected = _ => LockAimed(),
        });

        menu.Entries.Add(NearbyMenu(Loc.Filmmaker.TargetPlayers, PlayerRows, FilmmakerPermissions.TargetPlayers));
        menu.Entries.Add(NearbyMenu(Loc.Filmmaker.TargetVehicles, () => PoolRows("CVehicle"), MenuGate.Always));
        menu.Entries.Add(NearbyMenu(Loc.Filmmaker.TargetPeds, () => PoolRows("CPed"), MenuGate.Always));

        menu.Entries.Add(new SeparatorEntry(Loc.Filmmaker.TargetGroupBehaviour));

        menu.Entries.Add(new CheckboxEntry(Loc.Filmmaker.TargetLookAt)
        {
            ReadEnabled = () => TargetLock.IsLocked,
            ReadState = () => TargetLock.LookAt,
            OnChanged = changed => TargetLock.SetLookAt(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.Filmmaker.TargetFollow)
        {
            ReadEnabled = () => TargetLock.IsLocked,
            ReadState = () => TargetLock.Follow,
            OnChanged = changed => TargetLock.SetFollow(changed.Checked),
        });

        menu.Entries.Add(new ListEntry(
            Loc.Filmmaker.TargetAttachMode,
            FilmmakerText.Keys(Loc.Filmmaker.AttachFull, Loc.Filmmaker.AttachHeading, Loc.Filmmaker.AttachPosition))
        {
            ReadSelectedIndex = () => (int)TargetLock.Mode,
            OnIndexChanged = changed => TargetLock.SetMode((AttachMode)changed.NewIndex),
        });

        menu.Entries.Add(OffsetEntry(Loc.Filmmaker.TargetOffsetX, static offset => offset.X, static (offset, value) => offset with { X = value }));
        menu.Entries.Add(OffsetEntry(Loc.Filmmaker.TargetOffsetY, static offset => offset.Y, static (offset, value) => offset with { Y = value }));
        menu.Entries.Add(OffsetEntry(Loc.Filmmaker.TargetOffsetZ, static offset => offset.Z, static (offset, value) => offset with { Z = value }));

        menu.Entries.Add(new ButtonEntry(Loc.Filmmaker.TargetRelease)
        {
            ReadEnabled = () => TargetLock.IsLocked,
            OnSelected = _ => TargetLock.Release(),
        });

        TargetLock.Changed += menu.Refresh;
        FreecamCamera.StateChanged += menu.Refresh;
    }

    private static SubmenuEntry NearbyMenu(string textKey, Func<List<MenuEntry>> rows, MenuGate gate) => new(textKey)
    {
        Gate = gate,
        Build = child =>
        {
            child.OnOpened = _ => child.ReplaceEntries(rows(), keepSelection: false);
        },
    };

    private static DynamicListEntry OffsetEntry(string textKey, Func<Vector3, float> read, Func<Vector3, float, Vector3> write) =>
        new(
            textKey,
            () => FilmmakerText.Format(read(TargetLock.LookOffset), "0.0") + " m",
            change =>
            {
                var value = MathF.Round((read(TargetLock.LookOffset) + (change.Left ? -OffsetStep : OffsetStep)) * 10f) / 10f;

                TargetLock.LookOffset = write(TargetLock.LookOffset, value);

                return FilmmakerText.Format(value, "0.0") + " m";
            })
        {
            ReadEnabled = () => TargetLock.IsLocked && TargetLock.LookAt,
        };

    private static string Status() =>
        TargetLock.IsLocked
            ? MenuText.Key(Loc.Filmmaker.TargetStatusLocked, ("name", MenuText.Literal(Describe(TargetLock.Entity)))).Resolve(Localizer.Current)
            : Localizer.Current.Get(Loc.Filmmaker.TargetStatusNone);

    private static void Lock(int entity)
    {
        if (FilmmakerText.NeedsFreecam())
        {
            return;
        }

        TargetLock.Lock(entity, lookAt: true, follow: false);

        Notifications.Success(MenuText.Key(Loc.Filmmaker.TargetLocked, ("name", MenuText.Literal(Describe(entity)))));
    }

    private static void LockAimed()
    {
        if (FilmmakerText.NeedsFreecam())
        {
            return;
        }

        var pose = FreecamCamera.Pose;
        var end = pose.Position + (FreecamCamera.Direction * AimDistance);

        var test = Native.StartExpensiveSynchronousShapeTestLosProbe(
            pose.Position.X, pose.Position.Y, pose.Position.Z,
            end.X, end.Y, end.Z,
            VehiclesPedsObjectsAndMap,
            Native.PlayerPedId(),
            ShapeTestOptions);

        var status = Native.GetShapeTestResult(test, out var hit, out _, out _, out var entity);

        if (status != ShapeTestReady || hit == 0 || entity == 0 || !IsLockable(entity))
        {
            Notifications.Error(MenuText.Key(Loc.Filmmaker.TargetAimNothing));

            return;
        }

        Lock(entity);
    }

    private static bool IsLockable(int entity) =>
        Native.DoesEntityExist(entity)
        && (Native.IsEntityAPed(entity) || Native.IsEntityAVehicle(entity) || Native.IsEntityAnObject(entity));

    private static List<MenuEntry> PlayerRows()
    {
        PlayerRoster.Refresh();

        var self = Native.PlayerId();
        var from = FreecamCamera.Pose.Position;

        var rows = PlayerRoster.All
            .Where(player => player.Slot != self)
            .Select(player => (Player: player, Distance: Vector3.Distance(from, player.Position)))
            .OrderBy(candidate => candidate.Distance)
            .Select(candidate => Row(candidate.Player.Ped, Native.GetPlayerName(candidate.Player.Slot), candidate.Distance))
            .ToList();

        return rows.Count > 0 ? rows : [FilmmakerText.Placeholder(Loc.Filmmaker.TargetNoneNearby)];
    }

    private static List<MenuEntry> PoolRows(string pool)
    {
        var from = FreecamCamera.Pose.Position;
        var self = Native.PlayerPedId();

        var rows = NativeFixer.GetGamePool(pool)
            .Where(entity => entity != self && Native.DoesEntityExist(entity))
            .Where(entity => !Native.IsEntityAPed(entity) || !Native.IsPedAPlayer(entity))
            .Select(entity => (Entity: entity, Distance: Vector3.Distance(from, Native.GetEntityCoords(entity, false))))
            .Where(candidate => candidate.Distance <= NearbyRadius)
            .OrderBy(candidate => candidate.Distance)
            .Take(NearbyLimit)
            .Select(candidate => Row(candidate.Entity, Describe(candidate.Entity), candidate.Distance))
            .ToList();

        return rows.Count > 0 ? rows : [FilmmakerText.Placeholder(Loc.Filmmaker.TargetNoneNearby)];
    }

    private static MenuEntry Row(int entity, string name, float distance) => new ButtonEntry
    {
        Text = MenuText.Literal(name),
        Label = MenuText.Literal(FilmmakerText.Format(distance, "0") + " m"),
        OnSelected = _ => Lock(entity),
    };

    private static string Describe(int entity)
    {
        if (!Native.DoesEntityExist(entity))
        {
            return "?";
        }

        if (Native.IsEntityAVehicle(entity))
        {
            var label = Native.GetDisplayNameFromVehicleModel(Native.GetEntityModel(entity));
            var text = Native.GetLabelText(label);

            return string.IsNullOrEmpty(text) || text == "NULL" ? label : text;
        }

        if (Native.IsEntityAPed(entity))
        {
            return Native.IsPedAPlayer(entity)
                ? Native.GetPlayerName(Native.NetworkGetPlayerIndexFromPed(entity))
                : Localizer.Current.Get(Loc.Filmmaker.TargetPedName);
        }

        return Localizer.Current.Get(Loc.Filmmaker.TargetObjectName);
    }
}
