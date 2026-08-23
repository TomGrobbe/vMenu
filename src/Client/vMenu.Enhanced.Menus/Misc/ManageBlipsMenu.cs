using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

namespace vMenu.Enhanced.Menus.Misc;

internal static class ManageBlipsMenu
{
    private const string AlwaysOnList = "alwayson";

    private const string ToggleableList = "toggleable";

    private const int NameMaxLength = 40;

    private const int NumberMaxLength = 6;

    internal static void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.BlipAddAlwaysOn),
            Description = MenuText.Key(Loc.DisplaySettings.BlipAddAlwaysOnDescription),
            OnSelectedAsync = _ => AddHereAsync(AlwaysOnList),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.BlipAddToggleable),
            Description = MenuText.Key(Loc.DisplaySettings.BlipAddToggleableDescription),
            OnSelectedAsync = _ => AddHereAsync(ToggleableList),
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.BlipRemoveNearest),
            Description = MenuText.Key(Loc.DisplaySettings.BlipRemoveNearestDescription),
            ConfirmationDescription = MenuText.Key(Loc.DisplaySettings.BlipRemoveNearestConfirm),
            OnConfirmedAsync = _ => RemoveNearestAsync(),
        });
    }

    private static async Task AddHereAsync(string list)
    {
        var answers = await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.DisplaySettings.BlipName), NameMaxLength),
            new InputPrompt(MenuText.Key(Loc.DisplaySettings.BlipSprite), NumberMaxLength, "1"),
            new InputPrompt(MenuText.Key(Loc.DisplaySettings.BlipColour), NumberMaxLength, "0"),
            new InputPrompt(MenuText.Key(Loc.DisplaySettings.BlipScale), NumberMaxLength, "0"));

        if (answers is null)
        {
            return;
        }

        if (!int.TryParse(answers[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var sprite)
            || !int.TryParse(answers[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var colour)
            || !float.TryParse(answers[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var scale))
        {
            Notifications.Error(MenuText.Key(Loc.DisplaySettings.BlipInvalidNumber));

            return;
        }

        var position = API.Players.Local.Ped is { } ped
            ? Native.GetEntityCoords(ped.Handle, true)
            : default;

        Report(await ServerActions.InvokeAsync(
            ActionIds.DisplaySettings.AddBlip,
            list,
            answers[0],
            Number(sprite),
            Number(colour),
            Coord(scale),
            Coord(position.X),
            Coord(position.Y),
            Coord(position.Z)));
    }

    private static async Task RemoveNearestAsync()
    {
        var position = API.Players.Local.Ped is { } ped
            ? Native.GetEntityCoords(ped.Handle, true)
            : default;

        if (LocationBlips.Nearest(position, out var alwaysOn) is not { } blip)
        {
            Notifications.Warning(MenuText.Key(Loc.DisplaySettings.BlipNoneNearby));

            return;
        }

        ReportDeleted(await ServerActions.InvokeAsync(
            ActionIds.DisplaySettings.RemoveBlip,
            alwaysOn ? AlwaysOnList : ToggleableList,
            blip.Name));
    }

    private static void Report(ActionResult result)
    {
        if (result.IsOk)
        {
            Notifications.Success(MenuText.Key(Loc.DisplaySettings.BlipSaved));

            return;
        }

        Notifications.Error(MenuText.Key(result.Status switch
        {
            ActionStatus.Denied => Loc.DisplaySettings.BlipSaveDenied,
            ActionStatus.Refused => Loc.DisplaySettings.BlipNameTaken,
            _ => Loc.DisplaySettings.BlipSaveFailed,
        }));
    }

    private static void ReportDeleted(ActionResult result)
    {
        if (result.IsOk)
        {
            Notifications.Success(MenuText.Key(Loc.DisplaySettings.BlipDeleted));

            return;
        }

        Notifications.Error(MenuText.Key(result.Status switch
        {
            ActionStatus.Denied => Loc.DisplaySettings.BlipDeleteDenied,
            ActionStatus.NotFound => Loc.DisplaySettings.BlipDeleteGone,
            _ => Loc.DisplaySettings.BlipDeleteFailed,
        }));
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Coord(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
