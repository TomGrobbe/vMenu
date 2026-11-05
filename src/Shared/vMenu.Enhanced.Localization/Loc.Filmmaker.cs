namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class Filmmaker
    {
        #region Menu chrome

        [English("Filmmaker")]
        public const string Title = "filmmaker.title";

        [English("Filmmaker Tools")]
        public const string Subtitle = "filmmaker.subtitle";

        [English("Set up cinematic shots with the freecam: camera settings, effects, target lock, saved shots and camera paths.")]
        public const string LinkDescription = "filmmaker.link.desc";

        [English("Freecam", "Starts or stops the freecam. Your freecam key does the same, F3 unless you changed it.")]
        public const string FreecamToggle = "filmmaker.freecam";

        #endregion

        #region Shared

        [English("Rename")]
        public const string Rename = "filmmaker.rename";

        [English("Delete")]
        public const string Delete = "filmmaker.delete";

        [English("Name")]
        public const string NamePrompt = "filmmaker.name.prompt";

        [English("~b~{name}~s~ was renamed.")]
        public const string Renamed = "filmmaker.renamed";

        [English("~b~{name}~s~ was deleted.")]
        public const string Deleted = "filmmaker.deleted";

        [English("Something called ~b~{name}~s~ already exists.")]
        public const string NameTaken = "filmmaker.nametaken";

        [English("That was saved by a newer vMenu, so it was not changed.")]
        public const string SaveRefused = "filmmaker.saverefused";

        #endregion

        #region Camera

        [English("Camera")]
        public const string CameraTitle = "filmmaker.camera.title";

        [English("Camera Settings")]
        public const string CameraSubtitle = "filmmaker.camera.subtitle";

        [English("How the freecam moves, turns and zooms.")]
        public const string CameraLinkDescription = "filmmaker.camera.link.desc";

        [English("Speed", "How fast the freecam flies. The mouse wheel changes it too while the freecam is on.")]
        public const string Speed = "filmmaker.camera.speed";

        [English("Look Sensitivity", "How fast the camera turns with the mouse or the right stick.")]
        public const string LookSensitivity = "filmmaker.camera.sensitivity";

        [English("Smoothing", "How long the camera takes to speed up and slow down. Zero stops dead.")]
        public const string Smoothing = "filmmaker.camera.smoothing";

        [English("Invert Look", "Flip looking up and down.")]
        public const string InvertLook = "filmmaker.camera.invert";

        [English("Zoom", "The field of view of the camera. A smaller number zooms in further.")]
        public const string Zoom = "filmmaker.camera.zoom";

        [English("Roll", "Tilts the camera sideways.")]
        public const string Roll = "filmmaker.camera.roll";

        [English("Reset Roll And Zoom")]
        public const string ResetLens = "filmmaker.camera.reset";

        [English("Hide Button Hints", "Hides the buttons along the bottom of the screen while the freecam is on.")]
        public const string HideHints = "filmmaker.camera.hidehints";

        #endregion

        #region Effects

        [English("Effects")]
        public const string EffectsTitle = "filmmaker.effects.title";

        [English("Camera Effects")]
        public const string EffectsSubtitle = "filmmaker.effects.subtitle";

        [English("Hide the HUD, add black bars, motion blur and camera shake.")]
        public const string EffectsLinkDescription = "filmmaker.effects.link.desc";

        [English("Hide HUD", "Hides the HUD, the minimap and notifications while the freecam is on.")]
        public const string HideHud = "filmmaker.effects.hidehud";

        [English("Letterbox", "Adds black cinema bars to the top and bottom of the screen.")]
        public const string Letterbox = "filmmaker.effects.letterbox";

        [English("Motion Blur")]
        public const string MotionBlur = "filmmaker.effects.motionblur";

        [English("Camera Shake")]
        public const string Shake = "filmmaker.effects.shake";

        [English("None")]
        public const string ShakeNone = "filmmaker.effects.shake.none";

        [English("Handheld")]
        public const string ShakeHandheld = "filmmaker.effects.shake.handheld";

        [English("Drunk")]
        public const string ShakeDrunk = "filmmaker.effects.shake.drunk";

        [English("Road Vibration")]
        public const string ShakeRoad = "filmmaker.effects.shake.road";

        [English("Air Turbulence")]
        public const string ShakeAir = "filmmaker.effects.shake.air";

        [English("Vibrate")]
        public const string ShakeVibrate = "filmmaker.effects.shake.vibrate";

        [English("Shake Intensity")]
        public const string ShakeIntensity = "filmmaker.effects.shake.intensity";

        #endregion

        #region Depth of field

        [English("Depth Of Field")]
        public const string DofTitle = "filmmaker.dof.title";

        [English("Depth Of Field")]
        public const string DofSubtitle = "filmmaker.dof.subtitle";

        [English("Blur what is closer or further away than the point you focus on.")]
        public const string DofLinkDescription = "filmmaker.dof.link.desc";

        [English("Depth Of Field")]
        public const string DofEnabled = "filmmaker.dof.enabled";

        [English("Focus", "Manual focuses at a set distance. Target keeps the locked target sharp.")]
        public const string DofFocusMode = "filmmaker.dof.focus";

        [English("Manual")]
        public const string DofFocusManual = "filmmaker.dof.focus.manual";

        [English("Target")]
        public const string DofFocusTarget = "filmmaker.dof.focus.target";

        [English("Focus Distance", "How far away from the camera things are sharp. Saved with each shot and keyframe.")]
        public const string DofDistance = "filmmaker.dof.distance";

        [English(
            "Aperture",
            "How thin the sharp zone around the focus point is. Higher makes it thinner, so more of what is in front and behind gets blurred, like a wide open camera lens.")]
        public const string DofAperture = "filmmaker.dof.aperture";

        #endregion

        #region Target lock

        [English("Target Lock")]
        public const string TargetTitle = "filmmaker.target.title";

        [English("Target Lock")]
        public const string TargetSubtitle = "filmmaker.target.subtitle";

        [English("Keep the camera aimed at a player, ped, vehicle or object, or make it ride along with one.")]
        public const string TargetLinkDescription = "filmmaker.target.link.desc";

        [English("Locked onto ~b~{name}~s~")]
        public const string TargetStatusLocked = "filmmaker.target.status.locked";

        [English("Nothing locked")]
        public const string TargetStatusNone = "filmmaker.target.status.none";

        [English("Lock Onto What You Aim At", "Locks onto whatever is in the middle of the screen.")]
        public const string TargetAim = "filmmaker.target.aim";

        [English("There is nothing in the middle of the screen to lock onto.")]
        public const string TargetAimNothing = "filmmaker.target.aim.nothing";

        [English("Nearby Players")]
        public const string TargetPlayers = "filmmaker.target.players";

        [English("Nearby Vehicles")]
        public const string TargetVehicles = "filmmaker.target.vehicles";

        [English("Nearby Peds")]
        public const string TargetPeds = "filmmaker.target.peds";

        [English("Nothing nearby")]
        public const string TargetNoneNearby = "filmmaker.target.nonenearby";

        [English("Ped")]
        public const string TargetPedName = "filmmaker.target.ped.name";

        [English("Object")]
        public const string TargetObjectName = "filmmaker.target.object.name";

        [English("Locked onto ~b~{name}~s~.")]
        public const string TargetLocked = "filmmaker.target.locked";

        [English("Behaviour")]
        public const string TargetGroupBehaviour = "filmmaker.target.group.behaviour";

        [English("Look At Target", "Keeps the camera pointed at the target while you fly around it.")]
        public const string TargetLookAt = "filmmaker.target.lookat";

        [English("Follow Target", "Makes the camera ride along with the target, keeping the distance it had when you turned this on.")]
        public const string TargetFollow = "filmmaker.target.follow";

        [English("Follow Mode", "Full turns with the target in every direction. Heading only turns left and right with it. Position only never turns.")]
        public const string TargetAttachMode = "filmmaker.target.attachmode";

        [English("Full")]
        public const string AttachFull = "filmmaker.target.attach.full";

        [English("Heading Only")]
        public const string AttachHeading = "filmmaker.target.attach.heading";

        [English("Position Only")]
        public const string AttachPosition = "filmmaker.target.attach.position";

        [English("Aim Offset Sideways", "Moves the point the camera looks at, measured from the target.")]
        public const string TargetOffsetX = "filmmaker.target.offset.x";

        [English("Aim Offset Forward", "Moves the point the camera looks at, measured from the target.")]
        public const string TargetOffsetY = "filmmaker.target.offset.y";

        [English("Aim Offset Up", "Moves the point the camera looks at, measured from the target.")]
        public const string TargetOffsetZ = "filmmaker.target.offset.z";

        [English("Release Target")]
        public const string TargetRelease = "filmmaker.target.release";

        #endregion

        #region Shots

        [English("Saved Shots")]
        public const string ShotsTitle = "filmmaker.shots.title";

        [English("Saved Shots")]
        public const string ShotsSubtitle = "filmmaker.shots.subtitle";

        [English("Save where the camera is, and jump back to it later.")]
        public const string ShotsLinkDescription = "filmmaker.shots.link.desc";

        [English("Save Current Shot", "Saves the position, angle, zoom and focus of the camera.")]
        public const string ShotSave = "filmmaker.shots.save";

        [English("Saved")]
        public const string ShotsGroupSaved = "filmmaker.shots.group.saved";

        [English("No saved shots yet")]
        public const string ShotsEmpty = "filmmaker.shots.empty";

        [English("Go To Shot", "Moves the freecam to this shot.")]
        public const string ShotGoTo = "filmmaker.shots.goto";

        [English("Overwrite With Current Camera")]
        public const string ShotOverwrite = "filmmaker.shots.overwrite";

        [English("Shot ~b~{name}~s~ was saved.")]
        public const string ShotSaved = "filmmaker.shots.saved";

        #endregion

        #region Path

        [English("Camera Path")]
        public const string PathTitle = "filmmaker.path.title";

        [English("Camera Path")]
        public const string PathSubtitle = "filmmaker.path.subtitle";

        [English("Place keyframes and let the camera glide from one to the next.")]
        public const string PathLinkDescription = "filmmaker.path.link.desc";

        [English("{name} · {count} keyframes · {seconds} s")]
        public const string PathStatus = "filmmaker.path.status";

        [English("{name} · {count} keyframes · {seconds} s · follows target")]
        public const string PathStatusTarget = "filmmaker.path.status.target";

        [English("Unsaved path")]
        public const string PathUnsaved = "filmmaker.path.unsaved";

        [English("Play Path")]
        public const string PathPlay = "filmmaker.path.play";

        [English("Stop Path")]
        public const string PathStop = "filmmaker.path.stop";

        [English("Plays the path from the first keyframe. The freecam has to be on.")]
        public const string PathPlayDescription = "filmmaker.path.play.desc";

        [English("Pause")]
        public const string PathPause = "filmmaker.path.pause";

        [English("Resume")]
        public const string PathResume = "filmmaker.path.resume";

        [English("Preview", "Shows any moment of the path. Move it left and right to scrub through.")]
        public const string PathScrub = "filmmaker.path.scrub";

        [English("Add Keyframe Here", "Adds the current camera to the end of the path.")]
        public const string PathAddKeyframe = "filmmaker.path.addkeyframe";

        [English("Go To Keyframe", "Moves the freecam to a keyframe so you can fly on from there. Press left and right to step through them.")]
        public const string PathGoTo = "filmmaker.path.goto";

        [English("Go To First Keyframe")]
        public const string PathFirst = "filmmaker.path.first";

        [English("Go To Last Keyframe")]
        public const string PathLast = "filmmaker.path.last";

        [English("Keyframes", "Change, reorder or remove the keyframes of this path.")]
        public const string PathKeyframes = "filmmaker.path.keyframes";

        [English("No keyframes yet")]
        public const string PathEmptyRow = "filmmaker.path.emptyrow";

        [English("Playback Settings", "Looping, speed and what the screen shows while a path plays.")]
        public const string PathPlayback = "filmmaker.path.playback";

        [English("Manage")]
        public const string PathGroupManage = "filmmaker.path.group.manage";

        [English("Save As", "Saves this path under a name. A saved path keeps itself up to date after every change.")]
        public const string PathSaveAs = "filmmaker.path.saveas";

        [English("Path saved as ~b~{name}~s~.")]
        public const string PathSavedAs = "filmmaker.path.savedas";

        [English("New Path", "Starts an empty path. A saved path stays saved.")]
        public const string PathNew = "filmmaker.path.new";

        [English("Saved Paths")]
        public const string PathSaved = "filmmaker.path.saved";

        [English("No saved paths yet")]
        public const string PathsEmpty = "filmmaker.path.savedempty";

        [English("Load Path")]
        public const string PathLoad = "filmmaker.path.load";

        [English("Loaded ~b~{name}~s~.")]
        public const string PathLoaded = "filmmaker.path.loaded";

        [English("Keyframe {index}")]
        public const string KeyframeRow = "filmmaker.keyframe.row";

        [English("Go To Keyframe", "Moves the freecam to this keyframe so you can fly on from there.")]
        public const string KeyframeJump = "filmmaker.keyframe.jump";

        [English("Replace With Current Camera")]
        public const string KeyframeReplace = "filmmaker.keyframe.replace";

        [English("Insert Current Camera Before This")]
        public const string KeyframeInsert = "filmmaker.keyframe.insert";

        [English("Time To Next Keyframe", "How long the camera takes to get to the next keyframe.")]
        public const string KeyframeDuration = "filmmaker.keyframe.duration";

        [English("Blend To Next", "Cut holds this keyframe and then jumps. Linear goes in a straight line. Smooth curves through the keyframes around it.")]
        public const string KeyframeBlend = "filmmaker.keyframe.blend";

        [English("Cut")]
        public const string BlendCut = "filmmaker.keyframe.blend.cut";

        [English("Linear")]
        public const string BlendLinear = "filmmaker.keyframe.blend.linear";

        [English("Smooth")]
        public const string BlendSmooth = "filmmaker.keyframe.blend.smooth";

        [English("Easing", "Ease in starts slow, ease out ends slow.")]
        public const string KeyframeEase = "filmmaker.keyframe.ease";

        [English("None")]
        public const string EaseNone = "filmmaker.keyframe.ease.none";

        [English("Ease In")]
        public const string EaseIn = "filmmaker.keyframe.ease.in";

        [English("Ease Out")]
        public const string EaseOut = "filmmaker.keyframe.ease.out";

        [English("Ease In And Out")]
        public const string EaseInOut = "filmmaker.keyframe.ease.inout";

        [English("Move Up")]
        public const string KeyframeMoveUp = "filmmaker.keyframe.moveup";

        [English("Move Down")]
        public const string KeyframeMoveDown = "filmmaker.keyframe.movedown";

        [English("Loop", "Once stops at the end. Loop starts over. Ping Pong plays back and forth. Circuit flies on from the last keyframe back to the first, using the last keyframe's settings.")]
        public const string PlaybackLoop = "filmmaker.playback.loop";

        [English("Once")]
        public const string LoopOnce = "filmmaker.playback.loop.once";

        [English("Loop")]
        public const string LoopRepeat = "filmmaker.playback.loop.repeat";

        [English("Ping Pong")]
        public const string LoopPingPong = "filmmaker.playback.loop.pingpong";

        [English("Circuit")]
        public const string LoopCircuit = "filmmaker.playback.loop.circuit";

        [English("Playback Speed")]
        public const string PlaybackSpeed = "filmmaker.playback.speed";

        [English("Start Delay", "Waits on the first keyframe for this long before moving.")]
        public const string PlaybackDelay = "filmmaker.playback.delay";

        [English("Stay At The End", "When a path finishes, the freecam stays where the path ended and you fly on from there. Turned off, it goes back to where you were before playing.")]
        public const string PlaybackStay = "filmmaker.playback.stay";

        [English("Hide HUD While Playing")]
        public const string PlaybackHideHud = "filmmaker.playback.hidehud";

        [English("Letterbox While Playing")]
        public const string PlaybackLetterbox = "filmmaker.playback.letterbox";

        #endregion
    }
}
