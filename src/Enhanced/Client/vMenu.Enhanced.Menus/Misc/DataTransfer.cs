using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Developer;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Misc;

public static class DataTransfer
{
    // How much of an unreadable code goes in the log before it is cut off.
    private const int LogSample = 200;

    public static async Task ExportAsync()
    {
        var bundle = KvpTransfer.Export();

        if (bundle.Entries.Count == 0)
        {
            Notifications.Warning(MenuText.Key(Loc.DataTransfer.NothingToExport));

            return;
        }

        // Serialized a frame later, so the hitch a large profile costs on System.Text.Json's reflection path
        // lands while the screen is coming up rather than while the menu is still being drawn.
        await API.Delay(0);

        await DataTransferScreen.ShowAsync(Prompt(exporting: true, replacing: false), ClientJson.Serialize(bundle));
    }

    public static async Task ImportAsync(KvpImportMode mode)
    {
        var replacing = mode == KvpImportMode.Replace;
        var pasted = await DataTransferScreen.AskAsync(Prompt(exporting: false, replacing: replacing));

        if (string.IsNullOrEmpty(pasted) || Read(pasted) is not { } bundle)
        {
            return;
        }

        var made = string.IsNullOrWhiteSpace(bundle.CreatedAt) ? "an unknown date" : bundle.CreatedAt;

        Log.Debug($"[Transfer] Reading a code made on {made}, holding {bundle.Entries.Count} item(s).");

        KvpImportResult result;

        try
        {
            result = KvpTransfer.Import(bundle, mode);
        }
        catch (Exception exception)
        {
            Log.Error($"[Transfer] Reading that code failed part way through: {exception}");
            Notifications.Error(MenuText.Key(Loc.DataTransfer.Failed));

            return;
        }

        Reapply();
        Report(result, replacing);
    }

    // Null when the code is unusable. The player has already been told.
    private static KvpBundle? Read(string pasted)
    {
        if (!ClientJson.TryDeserialize<KvpBundle>(pasted, out var bundle) || bundle is null)
        {
            Log.Error($"[Transfer] A pasted code did not hold readable JSON. It starts: {Sample(pasted)}");
            Notifications.Error(MenuText.Key(Loc.DataTransfer.Unreadable));

            return null;
        }

        if (!string.Equals(bundle.Format, KvpBundle.FormatName, StringComparison.Ordinal))
        {
            Log.Error($"[Transfer] A pasted code calls itself '{bundle.Format}' rather than '{KvpBundle.FormatName}'.");
            Notifications.Error(MenuText.Key(Loc.DataTransfer.NotABundle));

            return null;
        }

        if (bundle.Version > KvpBundle.CurrentVersion)
        {
            Log.Error(
                $"[Transfer] A pasted code is version {bundle.Version}, and this build understands "
                + $"{KvpBundle.CurrentVersion}.");

            Notifications.Error(MenuText.Key(Loc.DataTransfer.TooNew));

            return null;
        }

        if (bundle.Entries.Count == 0)
        {
            Notifications.Warning(MenuText.Key(Loc.DataTransfer.NothingInside));

            return null;
        }

        return bundle;
    }

    // Puts what was imported into effect, so nobody has to reconnect to see it.
    private static void Reapply()
    {
        UserPreferences.Restore();

        // The one preference in vMenu that is copied into a field rather than read where it is used.
        FingerPointing.SetDebug(UserDefaults.PointingDebug.Value);

        DeveloperFeaturesState.Reevaluate();

        // Last, because this is what re-labels the menus, and everything above changes what they say.
        ClientPermissions.Reevaluate();
    }

    private static void Report(KvpImportResult result, bool replacing)
    {
        Notifications.Success(replacing
            ? MenuText.Key(
                Loc.DataTransfer.ImportedReplacing,
                ("count", Number(result.Applied)),
                ("removed", Number(result.Deleted)))
            : MenuText.Key(Loc.DataTransfer.Imported, ("count", Number(result.Applied))));

        // Its own message rather than an aside on the one above: nothing here went wrong, and burying the
        // good news under a warning would read as though something had.
        if (result.SkippedNewer > 0)
        {
            Notifications.Warning(MenuText.Key(Loc.DataTransfer.SkippedNewer, ("count", Number(result.SkippedNewer))));
        }

        var unusable = result.SkippedMalformed + result.SkippedDuplicate;

        if (unusable > 0)
        {
            Notifications.Warning(MenuText.Key(Loc.DataTransfer.Skipped, ("count", Number(unusable))));
        }
    }

    private static TransferPrompt Prompt(bool exporting, bool replacing)
    {
        var localizer = Localizer.Current;

        return new TransferPrompt
        {
            Title = localizer.Get(exporting ? Loc.DataTransfer.ExportTitle : Loc.DataTransfer.ImportTitle),
            Summary = localizer.Get(exporting ? Loc.DataTransfer.ExportSummary : Loc.DataTransfer.ImportSummary),
            Warning = replacing ? localizer.Get(Loc.DataTransfer.ReplaceWarning) : string.Empty,
            Hint = localizer.Get(exporting ? Loc.DataTransfer.ExportHint : Loc.DataTransfer.ImportHint),
            Placeholder = localizer.Get(Loc.DataTransfer.Placeholder),
            Copy = localizer.Get(Loc.DataTransfer.Copy),
            Copied = localizer.Get(Loc.DataTransfer.Copied),
            CopyFailed = localizer.Get(Loc.DataTransfer.CopyFailed),
            Confirm = localizer.Get(Loc.DataTransfer.Confirm),
            Close = localizer.Get(Loc.DataTransfer.Close),
            Working = localizer.Get(Loc.DataTransfer.Working),
            EmptyCode = localizer.Get(Loc.DataTransfer.EmptyCode),
            NotACode = localizer.Get(Loc.DataTransfer.NotACode),
            BadCode = localizer.Get(Loc.DataTransfer.BadCode),
        };
    }

    private static MenuText Number(int value) => MenuText.Literal(value.ToString());

    private static string Sample(string text) => text.Length <= LogSample ? text : text[..LogSample] + "...";
}
