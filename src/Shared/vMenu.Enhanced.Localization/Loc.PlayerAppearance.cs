namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class PlayerAppearance
    {
        [English("Player Appearance")]
        public const string Title = "playerappearance.title";

        [English("Player Appearance Menu")]
        public const string Subtitle = "playerappearance.subtitle";

        [English("Change the clothes and props on the ped you are wearing.")]
        public const string LinkDescription = "playerappearance.link.desc";

        #region Pages

        [English(
            "Ped Customization",
            "Everything this ped can wear, one row per body slot. The numbers on each row are the ids, so you can tell somebody else exactly what you picked.")]
        public const string Customize = "playerappearance.customize";

        [English("Everything This Ped Can Wear")]
        public const string CustomizeSubtitle = "playerappearance.customize.subtitle";

        [English(
            "Ped Collections",
            "The same clothes, grouped by the game update they came with. Useful when you know roughly when something was added.")]
        public const string Collections = "playerappearance.collections";

        [English("Browse By Game Update")]
        public const string CollectionsSubtitle = "playerappearance.collections.subtitle";

        [English(
            "This ped does not have any collections other than the base collection. Ped collections will be available for models that have DLC updates, or streamed collections only (typically the multiplayer peds).")]
        public const string CollectionsLocked = "playerappearance.collections.locked";

        [English(
            "Random Outfit",
            "Dresses this ped in a combination the game itself considers a sensible one for it. Quicker than working through every slot when you just want a different look.")]
        public const string Random = "playerappearance.random";

        [English("~g~Gave this ped a random outfit~s~.")]
        public const string RandomDone = "playerappearance.random.done";

        [English("Base Game")]
        public const string BaseCollection = "playerappearance.collection.base";

        [English(
            "Everything ~y~{name}~s~ added for this ped. Anything you pick in here also shows up under Ped Customization.")]
        public const string CollectionRowDescription = "playerappearance.collection.row.desc";

        [English("Clothes From One Update")]
        public const string CollectionSubtitle = "playerappearance.collection.subtitle";

        #endregion

        #region Slot names

        [English("[0] Head")]
        public const string ComponentHead = "playerappearance.component.head";

        [English("[1] Mask / Facial Hair")]
        public const string ComponentMask = "playerappearance.component.mask";

        [English("[2] Hair")]
        public const string ComponentHair = "playerappearance.component.hair";

        [English("[3] Hands / Upper Body")]
        public const string ComponentTorso = "playerappearance.component.torso";

        [English("[4] Legs / Pants")]
        public const string ComponentLegs = "playerappearance.component.legs";

        [English("[5] Bags / Parachutes")]
        public const string ComponentBags = "playerappearance.component.bags";

        [English("[6] Shoes")]
        public const string ComponentShoes = "playerappearance.component.shoes";

        [English("[7] Neck / Scarves")]
        public const string ComponentNeck = "playerappearance.component.neck";

        [English("[8] Undershirt")]
        public const string ComponentUndershirt = "playerappearance.component.undershirt";

        [English("[9] Body Armour")]
        public const string ComponentArmour = "playerappearance.component.armour";

        [English("[10] Badges / Logos")]
        public const string ComponentDecals = "playerappearance.component.decals";

        [English("[11] Tops / Jackets")]
        public const string ComponentTops = "playerappearance.component.tops";

        [English("[0] Hats / Helmets")]
        public const string PropHats = "playerappearance.prop.hats";

        [English("[1] Glasses")]
        public const string PropGlasses = "playerappearance.prop.glasses";

        [English("[2] Ears")]
        public const string PropEars = "playerappearance.prop.ears";

        [English("[6] Watches")]
        public const string PropWatches = "playerappearance.prop.watches";

        [English("[7] Bracelets")]
        public const string PropBracelets = "playerappearance.prop.bracelets";

        [English("~b~Props", "Everything below here goes on top of the clothes, such as a hat or a pair of glasses.")]
        public const string PropsHeading = "playerappearance.props.heading";

        #endregion

        #region Row values and descriptions

        [English("[{position}/{drawables}] texture {texturePosition}")]
        public const string Value = "playerappearance.value";

        [English("none")]
        public const string ValueNone = "playerappearance.value.none";

        [English("[{position}/{drawables}] missing")]
        public const string ValueUnavailable = "playerappearance.value.unavailable";

        [English(
            "~h~{slot}~h~\nVariation ~y~{position}~s~ / ~y~{drawables}~s~\nTexture ~y~{texturePosition}~s~ / ~y~{textures}~s~\nPress ~b~enter~s~ for the next texture.")]
        public const string RowDescription = "playerappearance.slot.desc";

        [English(
            "~h~{slot}~h~\nVariation ~y~{position}~s~ / ~y~{drawables}~s~ in ~y~{collection}~s~\nTexture ~y~{texturePosition}~s~ / ~y~{textures}~s~\nPress ~b~enter~s~ for the next texture.")]
        public const string RowDescriptionInCollection = "playerappearance.slot.desc.collection";

        [English(
            "~h~{slot}~h~\nWhat this ped has on is not from ~y~{collection}~s~\nScroll to put something from this update on instead.")]
        public const string RowDescriptionElsewhere = "playerappearance.slot.desc.notmine";

        [English("~h~{slot}~h~\nNothing worn, ~y~{drawables}~s~ to choose from\nScroll to pick one.")]
        public const string PropRowDescriptionEmpty = "playerappearance.prop.desc.none";

        #endregion

        #region Typing an exact id

        [English("Jump To A Number")]
        public const string ExactIdButton = "playerappearance.exactid.button";

        [English("Jump to an exact number")]
        public const string ExactIdBinding = "playerappearance.exactid.binding";

        [English("Which item for {slot}? Between 1 and {max}")]
        public const string ExactIdPrompt = "playerappearance.exactid.prompt";

        [English("That has to be a whole number between ~y~1~s~ and ~y~{max}~s~.")]
        public const string ExactIdInvalid = "playerappearance.exactid.invalid";

        #endregion

        #region Everything else

        [English(
            "Walking Style",
            "How you walk and run. Press enter to try the one you picked. Some of these were animated for one kind of body and will not suit every ped.")]
        public const string WalkingStyle = "playerappearance.walk";

        [English("Normal")]
        public const string WalkingStyleNormal = "playerappearance.walk.normal";

        [English("You now walk like ~g~{style}~s~.")]
        public const string WalkingStyleApplied = "playerappearance.walk.applied";

        [English("The game has no ~y~{style}~s~ animations for this ped, so you are still walking the way you were.")]
        public const string WalkingStyleUnavailable = "playerappearance.walk.unavailable";

        [English(
            "Illuminated Clothing",
            "Makes clothes that light up flash or fade, and everybody around you sees it. Most outfits have nothing that lights up, so on those this does nothing.")]
        public const string Glow = "playerappearance.glow";

        [English("Off")]
        public const string GlowOff = "playerappearance.glow.off";

        [English("Always On")]
        public const string GlowSolid = "playerappearance.glow.solid";

        [English("Fade")]
        public const string GlowFade = "playerappearance.glow.fade";

        [English("Flash")]
        public const string GlowFlash = "playerappearance.glow.flash";

        [English(
            "~r~Reset To Default",
            "Puts this ped back in the clothes it spawns with and takes off everything it is wearing on top.")]
        public const string Reset = "playerappearance.reset";

        [English("~r~Put this ped back to its default look? Press enter again to confirm.")]
        public const string ResetConfirm = "playerappearance.reset.confirm";

        [English("~g~Back to the default look~s~.")]
        public const string ResetDone = "playerappearance.reset.done";

        [English(
            "Nothing To Change",
            "This ped has no clothes or props the game will let you swap. Animals are usually like this.")]
        public const string Empty = "playerappearance.empty";



        [English(
            "That helmet has a visor. ~y~Hold the visor key~s~ to flip it up or down. You can see and change that key under Settings, Key Bindings, FiveM.")]
        public const string VisorHint = "playerappearance.visorhint";

        [English("Goggles can only be pushed up ~y~on foot~s~. The game has no animation for doing it while riding.")]
        public const string VisorGogglesInVehicle = "playerappearance.visor.goggles.invehicle";

        #endregion
    }
}
