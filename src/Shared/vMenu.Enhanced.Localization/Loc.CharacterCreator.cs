namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class CharacterCreator
    {
        [English("Custom Character Creator")]
        public const string Title = "charactercreator.title";

        [English("Custom Character Creator")]
        public const string Subtitle = "charactercreator.subtitle";

        [English("Build a freemode online character, and give each one its own outfits and hairstyles.")]
        public const string LinkDescription = "charactercreator.link.desc";

        #region Root

        [English("Create Male Character", "Create a new male character.")]
        public const string CreateMale = "charactercreator.create.male";

        [English("Create Female Character", "Create a new female character.")]
        public const string CreateFemale = "charactercreator.create.female";

        [English(
            "Save Current Character",
            "Save the freemode character you are using right now, even if something other than vMenu made it.")]
        public const string SaveCurrent = "charactercreator.savecurrent";

        [English("~r~You need to get out of your vehicle before you can use this.")]
        public const string InVehicle = "charactercreator.invehicle";

        [English("You are not wearing a freemode character, so there is nothing to save. Create one first.")]
        public const string NotFreemode = "charactercreator.notfreemode";

        #endregion

        #region Building

        [English("Create A New Character")]
        public const string CreateTitle = "charactercreator.build.title";

        [English("Create A New Character")]
        public const string CreateSubtitle = "charactercreator.build.subtitle";

        [English("Edit Character")]
        public const string EditSubtitle = "charactercreator.build.subtitle.edit";

        [English("Randomize Character", "Give this character a completely random face, hair and set of clothes.")]
        public const string Randomize = "charactercreator.build.randomize";

        [English(
            "Character Inheritance",
            "Pick the two parents this character takes after, and how much of each one shows.")]
        public const string Inheritance = "charactercreator.build.inheritance";

        [English("Character Inheritance Options")]
        public const string InheritanceSubtitle = "charactercreator.build.inheritance.subtitle";

        [English("Character Appearance", "Hair, facial hair, makeup and everything else drawn onto the skin.")]
        public const string Appearance = "charactercreator.build.appearance";

        [English("Character Appearance Options")]
        public const string AppearanceSubtitle = "charactercreator.build.appearance.subtitle";

        [English("Character Face Shape Options", "Twenty sliders for the shape of the face itself.")]
        public const string FaceShape = "charactercreator.build.faceshape";

        [English("Character Face Shape Options")]
        public const string FaceShapeSubtitle = "charactercreator.build.faceshape.subtitle";

        [English("Character Tattoo Options", "Tattoos and badges, sorted by where they go on the body.")]
        public const string Tattoos = "charactercreator.build.tattoos";

        [English("Character Tattoo Options")]
        public const string TattoosSubtitle = "charactercreator.build.tattoos.subtitle";

        [English("Character Clothes", "What this character is wearing.")]
        public const string Clothes = "charactercreator.build.clothes";

        [English("Character Clothing Options")]
        public const string ClothesSubtitle = "charactercreator.build.clothes.subtitle";

        [English(
            "Match Arms To Clothes",
            "Pick the arms to match a top, undershirt or trousers you choose, the way GTA Online does. Turn this off to choose the arms yourself. ~r~Note:~s~ this is not perfect, and some tops/jackets require specific undershirts before it can pick a correct torso/arm combination.")]
        public const string FitTorso = "charactercreator.build.clothes.fittorso";

        [English("Character Props", "Hats, glasses, earrings, watches and bracelets.")]
        public const string Props = "charactercreator.build.props";

        [English("Character Props Options")]
        public const string PropsSubtitle = "charactercreator.build.props.subtitle";

        [English(
            "Clothing Collections",
            "The same clothes, grouped by the update or addon pack that added them, which is easier when you know roughly where something came from.")]
        public const string Collections = "charactercreator.build.collections";

        [English("Clothing Collections")]
        public const string CollectionsSubtitle = "charactercreator.build.collections.subtitle";

        [English("Browse the clothes and props that came with ~b~{name}~s~.")]
        public const string CollectionRowDescription = "charactercreator.build.collections.row.desc";

        [English("Base Game")]
        public const string BaseCollection = "charactercreator.build.collections.base";

        [English("Facial Expression", "The mood this character's face rests in.")]
        public const string Expression = "charactercreator.build.expression";

        [English("Character Category", "Which group this character is saved into.")]
        public const string Category = "charactercreator.build.category";

        [English("Uncategorised")]
        public const string Uncategorised = "charactercreator.build.category.none";

        [English("Save Character", "Save this character and everything you have given it.")]
        public const string Save = "charactercreator.build.save";

        [English("Exit Without Saving", "Leave the creator and throw away everything you have changed.")]
        public const string Exit = "charactercreator.build.exit";

        [English("Are you sure? You will lose every change you have made to this character.")]
        public const string ExitConfirm = "charactercreator.build.exit.confirm";

        [English("Save this character first, or pick ~r~Exit Without Saving~s~ to throw it away.")]
        public const string BackBlocked = "charactercreator.build.backblocked";

        [English("Character creator: explain why back is blocked")]
        public const string BackBlockedBinding = "charactercreator.backblocked.binding";

        [English("Randomized this character.")]
        public const string RandomizeDone = "charactercreator.build.randomize.done";

        #endregion

        #region Facial expressions

        [English("Normal")]
        public const string ExpressionNormal = "charactercreator.expression.normal";

        [English("Happy")]
        public const string ExpressionHappy = "charactercreator.expression.happy";

        [English("Angry")]
        public const string ExpressionAngry = "charactercreator.expression.angry";

        [English("Aiming")]
        public const string ExpressionAiming = "charactercreator.expression.aiming";

        [English("Injured")]
        public const string ExpressionInjured = "charactercreator.expression.injured";

        [English("Stressed")]
        public const string ExpressionStressed = "charactercreator.expression.stressed";

        [English("Smug")]
        public const string ExpressionSmug = "charactercreator.expression.smug";

        [English("Sulk")]
        public const string ExpressionSulk = "charactercreator.expression.sulk";

        #endregion

        #region Inheritance

        [English("Parent #1")]
        public const string ParentOne = "charactercreator.inheritance.parentone";

        [English("Parent #1 Skin")]
        public const string ParentOneSkin = "charactercreator.inheritance.parentone.skin";

        [English("Parent #2")]
        public const string ParentTwo = "charactercreator.inheritance.parenttwo";

        [English("Parent #2 Skin")]
        public const string ParentTwoSkin = "charactercreator.inheritance.parenttwo.skin";

        [English("Pick the face this character takes after. Currently ~b~{name}~s~.")]
        public const string ParentRowDescription = "charactercreator.inheritance.parent.desc";

        [English("Pick the skin tone this character takes after. Currently ~b~{name}~s~.")]
        public const string SkinRowDescription = "charactercreator.inheritance.skin.desc";

        [English(
            "Head Shape Mix",
            "Slide towards ~b~Parent #1~s~ or ~b~Parent #2~s~ to decide whose face shape shows more.")]
        public const string ShapeMix = "charactercreator.inheritance.shapemix";

        [English(
            "Body Skin And Color Mix",
            "Slide towards ~b~Parent #1~s~ or ~b~Parent #2~s~ to decide whose skin texture and tone shows more.")]
        public const string SkinMix = "charactercreator.inheritance.skinmix";

        [English("Parent #{number}")]
        public const string ParentUnnamed = "charactercreator.inheritance.parent.unnamed";

        [English("{name} (Male)")]
        public const string ParentMale = "charactercreator.inheritance.parent.male";

        [English("{name} (Female)")]
        public const string ParentFemale = "charactercreator.inheritance.parent.female";

        #endregion

        #region Appearance rows

        [English("Hair Style", "Pick this character's haircut.")]
        public const string HairStyle = "charactercreator.appearance.hair";

        [English("Hair Color")]
        public const string HairColour = "charactercreator.appearance.hair.colour";

        [English("Hair Highlight Color")]
        public const string HairHighlight = "charactercreator.appearance.hair.highlight";

        [English("Blemishes")]
        public const string OverlayBlemishes = "charactercreator.appearance.blemishes";

        [English("Beard")]
        public const string OverlayBeard = "charactercreator.appearance.beard";

        [English("Eyebrows")]
        public const string OverlayEyebrows = "charactercreator.appearance.eyebrows";

        [English("Ageing")]
        public const string OverlayAgeing = "charactercreator.appearance.ageing";

        [English("Makeup")]
        public const string OverlayMakeup = "charactercreator.appearance.makeup";

        [English("Blush")]
        public const string OverlayBlush = "charactercreator.appearance.blush";

        [English("Complexion")]
        public const string OverlayComplexion = "charactercreator.appearance.complexion";

        [English("Sun Damage")]
        public const string OverlaySunDamage = "charactercreator.appearance.sundamage";

        [English("Lipstick")]
        public const string OverlayLipstick = "charactercreator.appearance.lipstick";

        [English("Moles and Freckles")]
        public const string OverlayMolesFreckles = "charactercreator.appearance.moles";

        [English("Chest Hair")]
        public const string OverlayChestHair = "charactercreator.appearance.chesthair";

        [English("Body Blemishes")]
        public const string OverlayBodyBlemishes = "charactercreator.appearance.bodyblemishes";

        [English("Eye Colors", "Pick the colour of this character's eyes.")]
        public const string EyeColour = "charactercreator.appearance.eyes";

        [English("{name} Opacity")]
        public const string OpacityRow = "charactercreator.appearance.opacity.row";

        [English("{name} Color")]
        public const string ColourRow = "charactercreator.appearance.colour.row";

        [English("Pick which ~b~{name}~s~ is drawn on this character.")]
        public const string StyleRowDescription = "charactercreator.appearance.style.desc";

        [English("How strongly ~b~{name}~s~ is drawn. Zero percent hides it completely.")]
        public const string OpacityRowDescription = "charactercreator.appearance.opacity.desc";

        [English("What colour ~b~{name}~s~ is drawn in.")]
        public const string ColourRowDescription = "charactercreator.appearance.colour.desc";

        [English("This is not available for female characters.")]
        public const string MaleOnly = "charactercreator.appearance.maleonly";

        [English("Style #{number}")]
        public const string StyleOption = "charactercreator.appearance.style.option";

        [English("None")]
        public const string StyleNone = "charactercreator.appearance.style.none";

        [English("Color #{number}")]
        public const string ColourOption = "charactercreator.appearance.colour.option";

        [English("{percent}%")]
        public const string OpacityOption = "charactercreator.appearance.opacity.option";

        [English("Eye Color #{number}")]
        public const string EyeColourOption = "charactercreator.appearance.eyes.option";

        #endregion

        #region Face shape

        [English("Nose Width")]
        public const string FaceNoseWidth = "charactercreator.face.nose.width";

        [English("Nose Peak Height")]
        public const string FaceNoseHeight = "charactercreator.face.nose.height";

        [English("Nose Peak Length")]
        public const string FaceNoseLength = "charactercreator.face.nose.length";

        [English("Nose Bone Height")]
        public const string FaceNoseBridge = "charactercreator.face.nose.bridge";

        [English("Nose Peak Lowering")]
        public const string FaceNoseTip = "charactercreator.face.nose.tip";

        [English("Nose Bone Twist")]
        public const string FaceNoseTwist = "charactercreator.face.nose.twist";

        [English("Eyebrows Height")]
        public const string FaceBrowHeight = "charactercreator.face.brow.height";

        [English("Eyebrows Depth")]
        public const string FaceBrowDepth = "charactercreator.face.brow.depth";

        [English("Cheekbones Height")]
        public const string FaceCheekboneHeight = "charactercreator.face.cheekbone.height";

        [English("Cheekbones Width")]
        public const string FaceCheekboneWidth = "charactercreator.face.cheekbone.width";

        [English("Cheeks Width")]
        public const string FaceCheekWidth = "charactercreator.face.cheek.width";

        [English("Eyes Opening")]
        public const string FaceEyeOpening = "charactercreator.face.eyes";

        [English("Lips Thickness")]
        public const string FaceLipThickness = "charactercreator.face.lips";

        [English("Jaw Bone Width")]
        public const string FaceJawWidth = "charactercreator.face.jaw.width";

        [English("Jaw Bone Depth/Length")]
        public const string FaceJawLength = "charactercreator.face.jaw.length";

        [English("Chin Height")]
        public const string FaceChinHeight = "charactercreator.face.chin.height";

        [English("Chin Depth/Length")]
        public const string FaceChinLength = "charactercreator.face.chin.length";

        [English("Chin Width")]
        public const string FaceChinWidth = "charactercreator.face.chin.width";

        [English("Chin Hole Size")]
        public const string FaceChinDimple = "charactercreator.face.chin.dimple";

        [English("Neck Thickness")]
        public const string FaceNeckThickness = "charactercreator.face.neck";

        [English("Slide to change ~b~{name}~s~. The middle is the face as it came out of the two parents.")]
        public const string FaceRowDescription = "charactercreator.face.row.desc";

        #endregion

        #region Tattoos

        [English("Hair Tattoos")]
        public const string TattooHair = "charactercreator.tattoos.hair";

        [English("Head Tattoos")]
        public const string TattooHead = "charactercreator.tattoos.head";

        [English("Torso Tattoos")]
        public const string TattooTorso = "charactercreator.tattoos.torso";

        [English("Left Arm Tattoos")]
        public const string TattooLeftArm = "charactercreator.tattoos.leftarm";

        [English("Right Arm Tattoos")]
        public const string TattooRightArm = "charactercreator.tattoos.rightarm";

        [English("Left Leg Tattoos")]
        public const string TattooLeftLeg = "charactercreator.tattoos.leftleg";

        [English("Right Leg Tattoos")]
        public const string TattooRightLeg = "charactercreator.tattoos.rightleg";

        [English("Badge Overlays")]
        public const string TattooBadges = "charactercreator.tattoos.badges";

        [English("Addon Tattoos")]
        public const string TattooAddons = "charactercreator.tattoos.addons";

        [English("Cycle through to preview tattoos, press enter to toggle specific tattoo.")]
        public const string TattooListDescription = "charactercreator.tattoos.list.desc";

        [English(
            "Cycle through to preview badges, press enter to toggle specific badge. NOTE: Badges require a shirt or they may not appear!")]
        public const string BadgeListDescription = "charactercreator.tattoos.badges.desc";

        [English("Tattoo #{number} (of {total})")]
        public const string TattooOption = "charactercreator.tattoos.option";

        [English("Badge #{number} (of {total})")]
        public const string BadgeOption = "charactercreator.tattoos.badge.option";

        [English("Addon Tattoo #{number} (of {total})")]
        public const string AddonOption = "charactercreator.tattoos.addon.option";

        [English("{name} (#{number} of {total})")]
        public const string TattooOptionNamed = "charactercreator.tattoos.option.named";

        [English("~g~Worn~s~")]
        public const string TattooWorn = "charactercreator.tattoos.worn";

        [English("{name} has been ~g~added~s~.")]
        public const string TattooAdded = "charactercreator.tattoos.added";

        [English("{name} has been ~r~removed~s~.")]
        public const string TattooRemoved = "charactercreator.tattoos.removed";

        [English("Jump To Number")]
        public const string TattooExactIdButton = "charactercreator.tattoos.exactid.button";

        [English("Which one do you want, from 1 to {max}?")]
        public const string TattooExactIdPrompt = "charactercreator.tattoos.exactid.prompt";

        [English("That is not a number between 1 and {max}.")]
        public const string TattooExactIdInvalid = "charactercreator.tattoos.exactid.invalid";

        [English("Remove All Tattoos & Badges", "Take every tattoo and badge off this character.")]
        public const string TattooRemoveAll = "charactercreator.tattoos.removeall";

        [English("Are you sure? Every tattoo and badge on this character will be removed.")]
        public const string TattooRemoveAllConfirm = "charactercreator.tattoos.removeall.confirm";

        [English("Every tattoo and badge has been removed.")]
        public const string TattooRemoveAllDone = "charactercreator.tattoos.removeall.done";

        [English(
            "Nothing Here",
            "The server owner's tattoo file has nothing for this part of the body, at least not for this character.")]
        public const string TattooZoneEmpty = "charactercreator.tattoos.empty";

        [English(
            "No Tattoos Available",
            "The server owner has no tattoos configured, so there is nothing to put on this character. They can add them in ~b~config/tattoos.json~s~.")]
        public const string TattooFileEmpty = "charactercreator.tattoos.file.empty";

        #endregion

        #region Camera

        [English("Move Camera")]
        public const string MoveCamera = "charactercreator.camera.move";

        [English("Zoom")]
        public const string ZoomCamera = "charactercreator.camera.zoom";

        [English("Camera Height")]
        public const string CameraHeight = "charactercreator.camera.height";

        [English("Reset Camera")]
        public const string ResetCamera = "charactercreator.camera.reset";

        [English("Turn Character")]
        public const string TurnCharacter = "charactercreator.camera.turncharacter";

        [English(
            "Disable Auto Camera",
            "Stop the camera moving itself to whatever you are editing. Your own angle then stays where you left it.")]
        public const string DisableAutoCamera = "charactercreator.camera.auto";

        [English("Auto Camera: On")]
        public const string AutoCameraButtonOn = "charactercreator.camera.auto.button.on";

        [English("Auto Camera: Off")]
        public const string AutoCameraButtonOff = "charactercreator.camera.auto.button.off";

        [English("The camera follows what you are editing again.")]
        public const string AutoCameraOn = "charactercreator.camera.auto.on";

        [English("The camera stays where you put it.")]
        public const string AutoCameraOff = "charactercreator.camera.auto.off";

        #endregion

        #region Saving

        [English("Saved Characters", "Spawn, edit or delete your existing saved characters.")]
        public const string SavedCharacters = "charactercreator.saved";

        [English("Manage Saved Characters")]
        public const string SavedCharactersSubtitle = "charactercreator.saved.subtitle";

        [English("Enter a name for this character")]
        public const string NamePrompt = "charactercreator.save.name";

        [English("Enter a description (optional)")]
        public const string DescriptionPrompt = "charactercreator.save.description";

        [English("~g~{name}~s~ has been saved.")]
        public const string Saved = "charactercreator.save.success";

        [English("{name} could not be saved.")]
        public const string SaveFailed = "charactercreator.save.failed";

        [English("You already have a character called ~y~{name}~s~.")]
        public const string NameTaken = "charactercreator.save.nametaken";

        [English("You are not allowed to save characters on this server.")]
        public const string SaveDenied = "charactercreator.save.denied";

        [English("Spawn, edit, clone, rename or delete ~b~{name}~s~.")]
        public const string CharacterRowDescription = "charactercreator.saved.row.desc";

        [English("The characters you sorted into ~b~{name}~s~.")]
        public const string CategoryRowDescription = "charactercreator.saved.category.row.desc";

        [English("No Saved Characters", "You have not saved a character yet. Create one and it will show up here.")]
        public const string NoCharacters = "charactercreator.saved.empty";

        [English(
            "~y~Saved by a newer version of vMenu.~s~ You can still wear it, but editing it here would throw away whatever that version added.")]
        public const string FromNewerBuild = "charactercreator.saved.newer";



        #endregion

        #region Managing a saved character

        [English("{name}")]
        public const string ManageSubtitle = "charactercreator.manage.subtitle";

        [English("Spawn Saved Character", "Turn into this character.")]
        public const string Spawn = "charactercreator.manage.spawn";

        [English("You are now ~g~{name}~s~.")]
        public const string Spawned = "charactercreator.manage.spawn.done";

        [English("You are now ~g~{name}~s~, but {count} thing(s) could not be applied. Press F8 to see which.")]
        public const string SpawnedWithDifferences = "charactercreator.manage.spawn.differences";

        [English("Edit Saved Character", "Open this character back up in the creator.")]
        public const string EditCharacter = "charactercreator.manage.edit";

        [English(
            "Edit What I Am Wearing",
            "Keep the clothes and hair you have on right now and edit those. Saving keeps them.")]
        public const string EditAsWorn = "charactercreator.manage.edit.asworn";

        [English(
            "Put The Saved Look Back On",
            "Throw away what you are wearing, put this character back on exactly as it was saved, and edit that.")]
        public const string EditRestored = "charactercreator.manage.edit.restored";

        [English(
            "Character Outfits",
            "The sets of clothes saved against this character. Saving additional outfits allows you to easily swap between them without having to edit your character each time.")]
        public const string Outfits = "charactercreator.manage.outfits";

        [English("Character Outfits")]
        public const string OutfitsSubtitle = "charactercreator.manage.outfits.subtitle";

        [English(
            "Character Hair & Makeup Styles",
            "The different haircuts and grooming styles saved against this character. Saving additional hair and makeup styles allows you to easily swap between them without having to edit your character each time.")]
        public const string Styles = "charactercreator.manage.styles";

        [English("Hair & Makeup Styles")]
        public const string StylesSubtitle = "charactercreator.manage.styles.subtitle";

        [English("Clone Saved Character", "Save a second copy of this character under another name.")]
        public const string Clone = "charactercreator.manage.clone";

        [English("Cloned to ~g~{name}~s~.")]
        public const string Cloned = "charactercreator.manage.clone.done";

        [English("Set Character Category", "Move this character into another group. Select to save.")]
        public const string SetCategory = "charactercreator.manage.setcategory";

        [English("Moved to ~b~{name}~s~.")]
        public const string CategoryChanged = "charactercreator.manage.setcategory.done";

        [English(
            "Set As Default Character",
            "Come back as this character whenever you respawn. Select again to clear it.")]
        public const string SetDefault = "charactercreator.manage.setdefault";

        [English("~g~{name}~s~ is now your default character.")]
        public const string DefaultSet = "charactercreator.manage.setdefault.done";

        [English("You no longer have a default character.")]
        public const string DefaultCleared = "charactercreator.manage.setdefault.cleared";

        [English("~g~This character is currently set as your default character and will be used whenever you respawn.")]
        public const string IsDefault = "charactercreator.manage.isdefault";

        [English("Rename Saved Character", "Change this character's name and description.")]
        public const string Rename = "charactercreator.manage.rename";

        [English("Renamed to ~g~{name}~s~.")]
        public const string Renamed = "charactercreator.manage.rename.done";

        [English("Delete Saved Character", "Forget this character and everything saved against it.")]
        public const string Delete = "charactercreator.manage.delete";

        [English("Are you sure? ~y~{name}~s~ and all of its outfits and styles will be gone.")]
        public const string DeleteConfirm = "charactercreator.manage.delete.confirm";

        [English("~y~{name}~s~ has been deleted.")]
        public const string Deleted = "charactercreator.manage.delete.done";

        #endregion

        #region Outfit and style variants

        [English("Save Current Clothes As New")]
        public const string SaveOutfit = "charactercreator.variants.outfit.save";

        [English("Save what this character is wearing right now as another outfit you can switch back to.")]
        public const string SaveOutfitDescription = "charactercreator.variants.outfit.save.desc";

        [English("Save Current Style As New")]
        public const string SaveStyle = "charactercreator.variants.style.save";

        [English("Save this character's haircut and grooming as another style you can switch back to.")]
        public const string SaveStyleDescription = "charactercreator.variants.style.save.desc";

        [English("Enter a name")]
        public const string VariantNamePrompt = "charactercreator.variants.name";

        [English("Enter a description (optional)")]
        public const string VariantDescriptionPrompt = "charactercreator.variants.description";

        [English("Wear, replace, rename or delete ~b~{name}~s~.")]
        public const string VariantRowDescription = "charactercreator.variants.row.desc";


        [English("~g~{name}~s~ has been saved.")]
        public const string VariantSaved = "charactercreator.variants.saved";

        [English("There is already one called ~y~{name}~s~.")]
        public const string VariantNameTaken = "charactercreator.variants.nametaken";

        [English("Renamed to ~g~{name}~s~.")]
        public const string VariantRenamed = "charactercreator.variants.renamed";

        [English("~y~{name}~s~ has been deleted.")]
        public const string VariantDeleted = "charactercreator.variants.deleted";

        [English("Are you sure? ~y~{name}~s~ will be gone.")]
        public const string VariantDeleteConfirm = "charactercreator.variants.delete.confirm";

        [English("That could not be saved.")]
        public const string VariantFailed = "charactercreator.variants.failed";

        [English("Wear This")]
        public const string VariantApply = "charactercreator.variants.apply";

        [English("Replace With What I Am Wearing")]
        public const string VariantReplace = "charactercreator.variants.replace";

        [English("Rename")]
        public const string VariantRename = "charactercreator.variants.rename";

        [English("Delete")]
        public const string VariantDelete = "charactercreator.variants.delete";

        [English(
            "Nothing Saved Yet",
            "Nothing has been saved against this character yet. Wear something you like and save it here.")]
        public const string NoVariants = "charactercreator.variants.empty";

        #endregion

        #region Categories

        [English("Create Category", "Make a new group to sort your characters into.")]
        public const string CreateCategory = "charactercreator.categories.create";

        [English("Enter a category name")]
        public const string CategoryName = "charactercreator.categories.name";

        [English("Enter a category description (optional)")]
        public const string CategoryDescriptionPrompt = "charactercreator.categories.description";

        [English("~g~{name}~s~ has been created.")]
        public const string CategoryCreated = "charactercreator.categories.created";

        [English("You already have a category called ~y~{name}~s~.")]
        public const string CategoryNameTaken = "charactercreator.categories.nametaken";

        [English("Edit Category", "Rename a category, or change what it says.")]
        public const string EditCategory = "charactercreator.categories.edit";

        [English("~g~{name}~s~ has been updated.")]
        public const string CategoryEdited = "charactercreator.categories.edited";

        [English("Delete Category", "Forget a category. The characters in it are kept and become uncategorised.")]
        public const string DeleteCategory = "charactercreator.categories.delete";

        [English("Are you sure? ~y~{name}~s~ will be gone and everything in it becomes uncategorised.")]
        public const string DeleteCategoryConfirm = "charactercreator.categories.delete.confirm";

        [English("~y~{name}~s~ has been deleted.")]
        public const string CategoryDeleted = "charactercreator.categories.deleted";

        #endregion

        #region Outfit presets

        [English(
            "Outfit Presets",
            "Ready made outfits, both the ones this server published and the ones that came with the game.")]
        public const string Presets = "charactercreator.presets";

        [English("Outfit Presets")]
        public const string PresetsSubtitle = "charactercreator.presets.subtitle";

        [English("Server Presets", "The outfits the server owner published for everybody to use.")]
        public const string ServerPresets = "charactercreator.presets.server";

        [English("Server Presets")]
        public const string ServerPresetsSubtitle = "charactercreator.presets.server.subtitle";

        [English("{name}")]
        public const string PresetCategorySubtitle = "charactercreator.presets.category.subtitle";

        [English("The outfits in ~b~{name}~s~.")]
        public const string PresetCategoryRowDescription = "charactercreator.presets.category.row.desc";

        [English("Put on ~b~{name}~s~.")]
        public const string PresetRowDescription = "charactercreator.presets.row.desc";


        [English("~y~{name}~s~ was saved on the other freemode model, so it would not look right on this one.")]
        public const string PresetWrongSex = "charactercreator.presets.wrongsex";

        [English("Nothing Here Yet", "The server owner has not published any outfits yet.")]
        public const string NoPresets = "charactercreator.presets.empty";

        [English(
            "Save Clothes To Server Presets",
            "Publish what you are wearing so everybody on this server can put it on.")]
        public const string SavePreset = "charactercreator.presets.save";

        [English("Enter a name for this preset")]
        public const string PresetNamePrompt = "charactercreator.presets.save.name";

        [English("Enter a description (optional)")]
        public const string PresetDescriptionPrompt = "charactercreator.presets.save.description";

        [English("~g~{name}~s~ has been published.")]
        public const string PresetSaved = "charactercreator.presets.save.done";

        [English("That could not be saved to the server.")]
        public const string PresetSaveFailed = "charactercreator.presets.save.failed";

        [English("There is already a preset called ~y~{name}~s~.")]
        public const string PresetNameTaken = "charactercreator.presets.save.nametaken";

        [English("You are not allowed to publish outfits on this server.")]
        public const string PresetSaveDenied = "charactercreator.presets.save.denied";

        [English("That category is not there any more.")]
        public const string PresetCategoryGone = "charactercreator.presets.save.categorygone";

        [English(
            "Create Preset Category",
            "Make a new group for the server's outfits, such as one for police uniforms.")]
        public const string CreatePresetCategory = "charactercreator.presets.categories.create";

        [English("Enter a category name")]
        public const string PresetCategoryName = "charactercreator.presets.categories.name";

        [English("Enter a category description (optional)")]
        public const string PresetCategoryDescriptionPrompt = "charactercreator.presets.categories.description";

        [English("~g~{name}~s~ has been created.")]
        public const string PresetCategoryCreated = "charactercreator.presets.categories.created";

        [English("Delete Preset Category", "Forget a category and every outfit in it, for everybody on this server.")]
        public const string DeletePresetCategory = "charactercreator.presets.categories.delete";

        [English("Are you sure? ~y~{name}~s~ and every outfit in it will be gone for everybody.")]
        public const string DeletePresetCategoryConfirm = "charactercreator.presets.categories.delete.confirm";

        [English("~y~{name}~s~ has been deleted.")]
        public const string PresetCategoryDeleted = "charactercreator.presets.categories.deleted";

        [English("Delete Preset", "Remove one of the server's outfits, for everybody.")]
        public const string DeletePreset = "charactercreator.presets.delete";

        [English("Are you sure? ~y~{name}~s~ will be gone for everybody on this server.")]
        public const string DeletePresetConfirm = "charactercreator.presets.delete.confirm";

        [English("~y~{name}~s~ has been deleted.")]
        public const string PresetDeleted = "charactercreator.presets.delete.done";

        #endregion

        #region GTA Online outfits

        [English(
            "Default Game Outfits",
            "The ready made outfits that shipped with the game's own content packs. Or any addon packs the server owner installed.")]
        public const string OnlineOutfits = "charactercreator.online";

        [English("Default Game Outfits")]
        public const string OnlineOutfitsSubtitle = "charactercreator.online.subtitle";

        [English("{name}")]
        public const string OnlinePackSubtitle = "charactercreator.online.pack.subtitle";

        [English("The outfits that came with ~b~{name}~s~.")]
        public const string OnlinePackRowDescription = "charactercreator.online.pack.row.desc";

        [English("Put on ~b~{name}~s~.")]
        public const string OnlineOutfitRowDescription = "charactercreator.online.row.desc";


        [English("Reading Outfits", "The game is being asked what outfits it has. Please wait.")]
        public const string OnlineLoading = "charactercreator.online.loading";

        [English("Nothing Here", "The game reported no preset outfits for this character.")]
        public const string OnlineEmpty = "charactercreator.online.empty";


        [English("Outfit #{number}")]
        public const string OnlineUnnamedOutfit = "charactercreator.online.outfit.unnamed";

        #endregion
    }
}
