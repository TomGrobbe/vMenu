namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class DeveloperFeatures
    {
        [English("Developer Features")]
        public const string Title = "devfeatures.title";

        [English("Developer Features")]
        public const string Subtitle = "devfeatures.subtitle";

        [English("Development and debugging tools.")]
        public const string LinkDescription = "devfeatures.link.desc";

        [English("Show Vehicle Dimensions")]
        public const string VehicleDimensions = "devfeatures.vehicledimensions";

        [English("Draws the model outlines for every vehicle that's currently close to you.")]
        public const string VehicleDimensionsDescription = "devfeatures.vehicledimensions.desc";

        [English("Show Prop Dimensions")]
        public const string PropDimensions = "devfeatures.propdimensions";

        [English("Draws the model outlines for every prop that's currently close to you.")]
        public const string PropDimensionsDescription = "devfeatures.propdimensions.desc";

        [English("Show Ped Dimensions")]
        public const string PedDimensions = "devfeatures.peddimensions";

        [English("Draws the model outlines for every ped that's currently close to you.")]
        public const string PedDimensionsDescription = "devfeatures.peddimensions.desc";

        [English("Show Entity Handles")]
        public const string EntityHandles = "devfeatures.entityhandles";

        [English(
            "Draws the entity handle of every nearby entity. Enable one of the outline options above for this to show up.")]
        public const string EntityHandlesDescription = "devfeatures.entityhandles.desc";

        [English("Show Entity Models")]
        public const string EntityModels = "devfeatures.entitymodels";

        [English(
            "Draws the model hash of every nearby entity. Enable one of the outline options above for this to show up.")]
        public const string EntityModelsDescription = "devfeatures.entitymodels.desc";

        [English("Show Network Owners")]
        public const string NetworkOwners = "devfeatures.networkowners";

        [English(
            "Draws the network owner of every nearby entity. Enable one of the outline options above for this to show up.")]
        public const string NetworkOwnersDescription = "devfeatures.networkowners.desc";

        [English("Show Dimensions Radius")]
        public const string DrawRadius = "devfeatures.drawradius";

        [English("How far away entities are still outlined and labelled. Currently {radius}.")]
        public const string DrawRadiusDescription = "devfeatures.drawradius.desc";

        [English("Dimensions Box Opacity")]
        public const string BoxOpacity = "devfeatures.boxopacity";

        [English(
            "How solidly the coloured boxes are filled in. The outlines and labels around them are not affected. Currently {opacity}.")]
        public const string BoxOpacityDescription = "devfeatures.boxopacity.desc";

        [English("Ticks Overlay")]
        public const string TicksOverlay = "devfeatures.ticksoverlay";

        [English(
            "Shows a panel listing every loop vMenu is running and whether it's currently on. You can also use the ~d~/vmenu_ticks_overlay~s~ command to toggle this overlay.")]
        public const string TicksOverlayDescription = "devfeatures.ticksoverlay.desc";

        [English("Finger Pointing Probe")]
        public const string PointingProbe = "devfeatures.pointingprobe";

        [English(
            "While you are pointing, draws the sphere the game checks for walls at, red when something is in the way and purple when nothing is, plus a readout of what pointing is currently doing. You can also use the ~d~/vmenu_pointing_debug~s~ command to toggle this overlay.")]
        public const string PointingProbeDescription = "devfeatures.pointingprobe.desc";
    }
}
