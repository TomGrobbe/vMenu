namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class DataTransfer
    {
        [English("Import & Export")]
        public const string Title = "datatransfer.title";

        [English("Move Your Saved Data")]
        public const string Subtitle = "datatransfer.subtitle";

        [English(
            "Turn everything vMenu has saved on this computer into one code you can copy, and paste it back in anywhere.")]
        public const string LinkDescription = "datatransfer.link.desc";

        [English("Currently Saved")]
        public const string Summary = "datatransfer.summary";

        [English(
            "~y~{total}~s~ item(s) in total: ~y~{vehicles}~s~ vehicle(s), ~y~{peds}~s~ ped(s), ~y~{characters}~s~ character(s), ~y~{loadouts}~s~ weapon loadout(s) and ~y~{settings}~s~ setting(s).")]
        public const string SummaryDescription = "datatransfer.summary.desc";

        [English("Nothing is saved on this computer yet.")]
        public const string SummaryEmpty = "datatransfer.summary.empty";

        [English(
            "Show My Transfer Code",
            "Put everything saved here into one code. Copy it somewhere safe, and you can bring all of it back later, or on another computer.")]
        public const string Export = "datatransfer.export";

        [English("Bring Data Back")]
        public const string ImportGroup = "datatransfer.import.group";

        [English(
            "Paste A Code",
            "Paste a transfer code and add everything in it to what is already here. Nothing saved on this computer is deleted.")]
        public const string Merge = "datatransfer.import.merge";

        [English(
            "Paste A Code And Replace Everything",
            "Paste a transfer code and start over from it. Everything vMenu has saved here is deleted first, so you end up with exactly what was in the code.")]
        public const string Replace = "datatransfer.import.replace";

        [English(
            "~r~This deletes every vehicle, ped, loadout and setting saved on this computer~s~ before it reads the code. Press enter again if you are sure.")]
        public const string ReplaceConfirm = "datatransfer.import.replace.confirm";

        [English("Your Transfer Code")]
        public const string ExportTitle = "datatransfer.screen.export.title";

        [English("Copy the whole code and keep it somewhere safe. Paste it back in to bring everything here again.")]
        public const string ExportSummary = "datatransfer.screen.export.summary";

        [English("Ctrl+C copies. Escape closes.")]
        public const string ExportHint = "datatransfer.screen.export.hint";

        [English("Paste A Transfer Code")]
        public const string ImportTitle = "datatransfer.screen.import.title";

        [English("Paste the code you saved earlier, then press Import.")]
        public const string ImportSummary = "datatransfer.screen.import.summary";

        [English("Ctrl+V pastes. Enter imports, Escape cancels.")]
        public const string ImportHint = "datatransfer.screen.import.hint";

        [English("Everything saved on this computer is deleted first, and replaced with whatever is in this code.")]
        public const string ReplaceWarning = "datatransfer.screen.replace.warning";

        [English("Paste your transfer code here")]
        public const string Placeholder = "datatransfer.screen.placeholder";

        [English("Copy")]
        public const string Copy = "datatransfer.screen.copy";

        [English("Copied.")]
        public const string Copied = "datatransfer.screen.copied";

        [English("The clipboard could not be reached. Select the code and press Ctrl+C.")]
        public const string CopyFailed = "datatransfer.screen.copy.failed";

        [English("Import")]
        public const string Confirm = "datatransfer.screen.confirm";

        [English("Close")]
        public const string Close = "datatransfer.screen.close";

        [English("Working on it...")]
        public const string Working = "datatransfer.screen.working";

        [English("Paste your code first.")]
        public const string EmptyCode = "datatransfer.screen.code.empty";

        [English("That does not look like a vMenu transfer code.")]
        public const string NotACode = "datatransfer.screen.code.wrong";

        [English("That code is damaged or incomplete. Copy the whole thing and try again.")]
        public const string BadCode = "datatransfer.screen.code.damaged";

        [English("There is nothing saved here yet, so there is nothing to make a code from.")]
        public const string NothingToExport = "datatransfer.export.nothing";

        [English("Brought back ~g~{count}~s~ item(s).")]
        public const string Imported = "datatransfer.imported";

        [English("Brought back ~g~{count}~s~ item(s), replacing the ~r~{removed}~s~ that were here.")]
        public const string ImportedReplacing = "datatransfer.imported.replacing";

        [English(
            "~y~{count}~s~ item(s) here were saved by a newer vMenu than the one that made that code, so they were left alone.")]
        public const string SkippedNewer = "datatransfer.skipped.newer";

        [English("~y~{count}~s~ item(s) in that code were skipped. Press F8 to see which ones.")]
        public const string Skipped = "datatransfer.skipped";

        [English("That code could not be read.")]
        public const string Unreadable = "datatransfer.failed.unreadable";

        [English("That is not a vMenu transfer code.")]
        public const string NotABundle = "datatransfer.failed.notabundle";

        [English(
            "That code was made by a ~y~newer version~s~ of vMenu. Update vMenu Enhanced, or ask for a code made with this version.")]
        public const string TooNew = "datatransfer.failed.toonew";

        [English("There is nothing in that code to bring back.")]
        public const string NothingInside = "datatransfer.failed.empty";

        [English("Something went wrong part way through. Press F8 for the details.")]
        public const string Failed = "datatransfer.failed";
    }
}
