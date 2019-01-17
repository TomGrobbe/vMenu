using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;
using static CitizenFX.Core.BaseScript;
using MenuAPI;

namespace vMenuClient
{
    public static class IplManager
    {
        // Exports dictionary used to call exports. This is impossible to do directly from BaseScript itself for some stupid reason.
        private static readonly ExportDictionary Exports = new ExportDictionary();

        private static string resourceName = "bob74_ipl";

        internal static List<Interior> interiors = new List<Interior>();

        /// <summary>
        /// Checks if the dependency for the ipl menu is present and enabled via the config option in the __resource.lua of the ipl resource.
        /// </summary>
        /// <returns></returns>
        internal static bool IsDependencyPresentAndEnabled()
        {
            var resCount = GetNumResources();
            bool found = false;
            for (int i = 0; i < resCount; i++)
            {
                // Get the resource name.
                string name = GetResourceByFindIndex(i);

                // resource name is invalid or missing, skip this resource.
                if (string.IsNullOrEmpty(name))
                    continue;

                // resource is not started, skip this resource.
                if (GetResourceState(name) != "started")
                    continue;

                // we don't want to check for vMenu itself.
                if (GetCurrentResourceName() == name)
                    continue;

                // Get the metadata to see if this is bob's IPL resource and if it's integration is enabled or disabled.
                string metadata = GetResourceMetadata(name, "enable_vmenu_bob74_ipl_integration", 0);

                // invalid or no meta data present.
                if (string.IsNullOrEmpty(metadata))
                    continue;

                // found and enabled.
                if (metadata.ToLower() == "true")
                {
                    found = true;
                    resourceName = name;
                    break;
                }
                // found, but not enabled.
                else if (metadata.ToLower() == "false")
                {
                    break;
                }
            }

            // If it's not found or not enabled by the IPL's config, then return false.
            if (!found)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the ipl integration with bob74's ipl resource is enabled.
        /// </summary>
        /// <returns></returns>
        internal static bool IsIplIntegrationEnabled()
        {
            // Check if the dependency is present and enabled.
            if (IsDependencyPresentAndEnabled())
            {
                // Check if the setting in vMenu is enabled.
                if (GetSettingsBool(Setting.vmenu_enable_bob74_ipl_integration))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool AreInteriorsLoaded { get; private set; } = false;

        /// <summary>
        /// Loads interiors from default setup.
        /// </summary>
        internal static async Task LoadAllInteriors()
        {
            if (IsIplIntegrationEnabled())
            {
                #region apartments, houses, penthouses
                // High End & High Life Apartments
                #region High End & High Life Apartments

                // 4 Integrity Way
                await AddInterior(new Apartment("4 Integrity Way, Apt 30", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOApartmentHi1Object(),
                    posTpInt = new Vector3(-18.61f, -581.87f, 90.11f),
                    posTpExt = new List<Vector3>() { new Vector3(-48.95f, -588.96f, 37.95f) },
                    teleportHeading = 73.58f,
                    interiorLocation = new Vector3(-29.49f, -581.29f, 88.71f),
                    TvPosition = new Vector3(-40.24f, -571.05f, 88.92f)
                });

                await AddInterior(new Apartment("4 Integrity Way, Apt 28", MainMenu.IplManagementMenu.apartmentsMenu) // 35, 30, 28
                {
                    iplObject = Exports[resourceName].GetHLApartment5Object(),
                    posTpInt = new Vector3(-23.27f, -598.06f, 80.03f),
                    posTpExt = new List<Vector3>() { new Vector3(-48.95f, -588.96f, 37.95f) },
                    teleportHeading = 250.22f,
                    interiorLocation = new Vector3(-23.27f, -598.06f, 80.03f),
                    TvPosition = new Vector3(-21.86f, -578.69f, 78.23f)
                });

                // Del Perro Heights
                await AddInterior(new Apartment("Del Perro Heights, Apt 7", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOApartmentHi2Object(),
                    posTpInt = new Vector3(-1458.32f, -520.78f, 56.93f),
                    posTpExt = new List<Vector3>() { new Vector3(-1441.51f, -544.53f, 34.74f) },
                    teleportHeading = 126.62f,
                    interiorLocation = new Vector3(-1465.84f, -530.33f, 55.53f),
                    TvPosition = new Vector3(-1479.18f, -531.98f, 55.74f),
                });

                await AddInterior(new Apartment("Del Perro Heights, Apt 4", MainMenu.IplManagementMenu.apartmentsMenu) // 4, 20, 7
                {
                    iplObject = Exports[resourceName].GetHLApartment1Object(),
                    posTpInt = new Vector3(-1457.06f, -533.43f, 74.04f),
                    posTpExt = new List<Vector3>() { new Vector3(-1441.51f, -544.53f, 34.74f) },
                    teleportHeading = 33.81f,
                    interiorLocation = new Vector3(-1462.28100000f, -539.62760000f, 72.44434000f),
                    TvPosition = new Vector3(-1469.47f, -548.6f, 72.24f)
                });

                // Richards Majestic
                await AddInterior(new Apartment("Richards Majestic, Apt 2", MainMenu.IplManagementMenu.apartmentsMenu) // 4 & 51
                {
                    iplObject = Exports[resourceName].GetHLApartment2Object(),
                    posTpInt = new Vector3(-920.10f, -368.91f, 114.27f),
                    posTpExt = new List<Vector3>() { new Vector3(-935.01f, -380.46f, 38.96f) },
                    teleportHeading = 115.6f,
                    interiorLocation = new Vector3(-915.39f, -378.57f, 113.67f),
                    TvPosition = new Vector3(-907.03f, -383.24f, 112.47f)
                });

                // Tinsel Towers
                await AddInterior(new Apartment("Tinsel Towers, Apt 42", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetHLApartment3Object(),
                    posTpInt = new Vector3(-611.32f, 58.89f, 98.2f),
                    posTpExt = new List<Vector3>() { new Vector3(-614.56f, 37f, 43.57f) },
                    teleportHeading = 89.65f,
                    interiorLocation = new Vector3(-612.94f, 49.82f, 97.6f),
                    TvPosition = new Vector3(-606.35f, 40.25f, 96.39f)
                });

                // Eclipse Towers
                await AddInterior(new Apartment("Eclipse Towers, Apt 3", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetHLApartment4Object(),
                    posTpInt = new Vector3(-776.57f, 323.57f, 212f),
                    posTpExt = new List<Vector3>() { new Vector3(-777.48f, 312.71f, 85.7f) },
                    teleportHeading = 270.98f,
                    interiorLocation = new Vector3(-776.57f, 323.57f, 212f),
                    TvPosition = new Vector3(-781.74f, 342.33f, 210.19f)
                });

                #endregion

                // Penthouses 
                #region penthouses
                await AddInterior(new Penthouse("Eclipse Towers, Penthouse Suite 1", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetExecApartment1Object(),
                    posTpInt = new Vector3(-781.7f, 318.62f, 217.67f),
                    posTpExt = new List<Vector3>() { new Vector3(-777.48f, 312.71f, 85.7f) },
                    teleportHeading = 40.98f,
                    interiorLocation = new Vector3(-786.9f, 330.32f, 217.04f),
                    TvPosition = new Vector3(-781.74f, 337.91f, 216.84f),
                });

                await AddInterior(new Penthouse("Eclipse Towers, Penthouse Suite 2", MainMenu.IplManagementMenu.apartmentsMenu, 5)
                {
                    iplObject = Exports[resourceName].GetExecApartment2Object(),
                    posTpInt = new Vector3(-779.25f, 338.95f, 196.69f),
                    posTpExt = new List<Vector3>() { new Vector3(-777.48f, 312.71f, 85.7f) },
                    teleportHeading = 40.98f,
                    interiorLocation = new Vector3(-773.2258f, 322.8252f, 194.8862f),
                    TvPosition = new Vector3(-780.61f, 319.28f, 194.88f),
                });

                await AddInterior(new Penthouse("Eclipse Towers, Penthouse Suite 3", MainMenu.IplManagementMenu.apartmentsMenu, 3)
                {
                    iplObject = Exports[resourceName].GetExecApartment3Object(),
                    posTpInt = new Vector3(-781.82f, 318.93f, 187.92f),
                    posTpExt = new List<Vector3>() { new Vector3(-777.48f, 312.71f, 85.7f) },
                    teleportHeading = 40.98f,
                    interiorLocation = new Vector3(-787.7805f, 334.9232f, 186.1134f),
                    TvPosition = new Vector3(-780.36f, 338.47f, 186.11f),
                });
                #endregion

                // High end houses
                #region High end Houses
                await AddInterior(new House("Stilt House, 3655 Wild Oats Drive", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOHouseHi1Object(),
                    posTpInt = new Vector3(-174.26f, 497.18f, 137.67f),
                    posTpExt = new List<Vector3>() { new Vector3(-175.6f, 501.42f, 137.42f) },
                    teleportHeading = 209.6f,
                    interiorLocation = new Vector3(-174.26f, 497.18f, 137.67f),
                    TvPosition = new Vector3(-161.66f, 482.89f, 136.24f),
                });

                await AddInterior(new House("Stilt House, 2044 North Conker Avenue", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOHouseHi2Object(),
                    posTpInt = new Vector3(341.27f, 437.41f, 149.39f),
                    posTpExt = new List<Vector3>() { new Vector3(347.1f, 441.02f, 147.7f) },
                    teleportHeading = 141.24f,
                    interiorLocation = new Vector3(341.27f, 437.41f, 149.39f),
                    TvPosition = new Vector3(331.13f, 421.66f, 147.97f),
                });

                await AddInterior(new House("Stilt House, 2045 North Conker Avenue", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOHouseHi3Object(),
                    posTpInt = new Vector3(373.51f, 423.02f, 145.91f),
                    posTpExt = new List<Vector3>() { new Vector3(373.02f, 428.12f, 145.68f) },
                    teleportHeading = 182.56f,
                    interiorLocation = new Vector3(373.51f, 423.02f, 145.91f),
                    TvPosition = new Vector3(377.44f, 404.72f, 144.51f),
                });

                await AddInterior(new House("Stilt House, 2862 Hillcrest Avenue", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOHouseHi4Object(),
                    posTpInt = new Vector3(-681.95f, 591.94f, 145.39f),
                    posTpExt = new List<Vector3>() { new Vector3(-686.57f, 596.89f, 143.64f) },
                    teleportHeading = 228.75f,
                    interiorLocation = new Vector3(-681.95f, 591.94f, 145.39f),
                    TvPosition = new Vector3(-664.1f, 585.9f, 143.69f),
                });

                await AddInterior(new House("Stilt House, 2868 Hillcrest Avenue", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOHouseHi5Object(),
                    posTpInt = new Vector3(-759.09f, 618.69f, 144.15f),
                    posTpExt = new List<Vector3>() { new Vector3(-751.47f, 621.01f, 142.24f) },
                    teleportHeading = 109.83f,
                    interiorLocation = new Vector3(-759.09f, 618.69f, 144.15f),
                    TvPosition = new Vector3(-771.4f, 604.58f, 142.73f),
                });

                await AddInterior(new House("Stilt House, 2874 Hillcrest Avenue", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOHouseHi6Object(),
                    posTpInt = new Vector3(-859.82f, 690.7f, 152.86f),
                    posTpExt = new List<Vector3>() { new Vector3(-853.14f, 696.42f, 148.78f) },
                    teleportHeading = 199.17f,
                    interiorLocation = new Vector3(-859.82f, 690.7f, 152.86f),
                    TvPosition = new Vector3(-850.26f, 674.47f, 151.46f),
                });

                await AddInterior(new House("Stilt House, 2677 Whispymound Drive", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOHouseHi7Object(),
                    posTpInt = new Vector3(117.34f, 559.17f, 184.3f),
                    posTpExt = new List<Vector3>() { new Vector3(119.38f, 564.83f, 183.96f) },
                    teleportHeading = 187.86f,
                    interiorLocation = new Vector3(117.34f, 559.17f, 184.3f),
                    TvPosition = new Vector3(127.29f, 543.4f, 182.9f),
                });

                await AddInterior(new House("Stilt House, 2113 Mad Wayne Thunder", MainMenu.IplManagementMenu.apartmentsMenu)
                {
                    iplObject = Exports[resourceName].GetGTAOHouseHi8Object(),
                    posTpInt = new Vector3(-1290.16f, 449.12f, 97.9f),
                    posTpExt = new List<Vector3>() { new Vector3(-1294.39f, 455.2f, 97.4f) },
                    teleportHeading = 187.31f,
                    interiorLocation = new Vector3(-1290.16f, 449.12f, 97.9f),
                    TvPosition = new Vector3(-1281.54f, 432.17f, 96.5f),
                });
                #endregion

                var blipLocations = new List<Vector3>();
                foreach (Interior inter in interiors)
                {
                    if (inter is Apartment || inter is Penthouse || inter is House)
                    {
                        foreach (var pos in inter.posTpExt)
                        {
                            if (pos != Vector3.Zero)
                            {
                                if (!blipLocations.Contains(pos))
                                {
                                    var b = AddBlipForCoord(pos.X, pos.Y, pos.Z);
                                    SetBlipSprite(b, 40);
                                    blipLocations.Add(pos);
                                    SetBlipAsShortRange(b, true);
                                }
                            }
                        }

                    }
                }
                #endregion

                #region Hangars
                interiors.Add(new Hanger("Aircraft Hanger", MainMenu.IplManagementMenu.hangarsMenu)
                {
                    iplObject = Exports[resourceName].GetSmugglerHangarObject(),
                    posTpInt = new Vector3(-1279.75f, -3048.1f, -48.49f),
                    posTpExt = new List<Vector3>()
                    {
                        new Vector3(-1150.38f, -3412.7f, 13.95f),
                        new Vector3(-1395.34f, -3266.47f, 13.94f),
                    },
                    teleportHeading = 325.96f,
                    interiorLocation = new Vector3(-1267.0f, -3013.135f, -49.5f),
                    TvPosition = new Vector3(-1235.24f, -3010.3f, -42.88f),
                });
                //interiors.Last().iplObject.LoadDefault();
                #endregion

                #region Night club
                interiors.Add(new Nightclub("Nightclub", MainMenu.IplManagementMenu.nightclubsMenu)
                {
                    iplObject = Exports[resourceName].GetAfterHoursNightclubsObject(),
                    posTpInt = new Vector3(-1569.44f, -3016.43f, -74.41f),
                    posTpExt = new List<Vector3>()
                    {
                        new Vector3(-676.18f, -2458.81f, 13.94f),   // Airport 
                        new Vector3(870.8f, -2100.33f, 30.46f),     // Cypress Flats
                        new Vector3(-1285.78f, -651.23f, 26.58f),   // Del Perro
                        new Vector3(194.89f, -3168.45f, 5.79f),     // Elysian Island
                        new Vector3(757.34f, -1332.71f, 27.28f),    // La Mesa
                        new Vector3(346.11f, -978.37f, 29.37f),     // Mission Row
                        new Vector3(-120.4f, -1259.77f, 29.31f),    // Srawberry
                        new Vector3(-1174.12f, -1152.86f, 5.66f),   // Vespucci
                        new Vector3(372.56f, 253.09f, 103.01f),     // Vinewood
                        new Vector3(5.65f, 220.6f, 107.8f),         // Vinewood West
                    },
                    teleportHeading = 0f,
                    interiorLocation = new Vector3(-1604.664f, -3012.583f, -78.000f),
                    //TvPosition = new Vector3(-1604.664f, -3012.583f, -80.00f)
                    TvPosition = new Vector3(-1601.286f, -3012.74f, -78.3553f)
                });

                foreach (Vector3 v in interiors.Last().posTpExt)
                {
                    if (!v.IsZero)
                    {
                        var b = AddBlipForCoord(v.X, v.Y, v.Z);
                        SetBlipSprite(b, 614);
                        SetBlipAsShortRange(b, true);
                    }
                }
                #endregion

                AreInteriorsLoaded = true;
            }
        }

        /// <summary>
        /// Loads cusomizations that were saved to the server.
        /// </summary>
        internal static void LoadAllCustomizationOptions()
        {
            // todo.
        }

        /// <summary>
        /// Saves the interior customization to the server.
        /// </summary>
        /// <param name="interior"></param>
        internal static void SaveCustomizationOptions(Interior interior)
        {
            // todo.
        }

        /// <summary>
        /// Adds an interior to the list of interiors.
        /// </summary>
        /// <param name="interior"></param>
        /// <returns>Returns the interior that was added.</returns>
        private static async Task<Interior> AddInterior(Interior interior)
        {
            interiors.Add(interior);
            await Delay(0);
            return interior;
        }

        //internal enum InteriorType { house, highLifeApartment, penthouse, office, warehouse, cocaineLockup, methLab, weedFarm, forgery, factory, clubhouse1, clubhouse2, garages, vehicleWarehouse, bunker, hangar, facility, clubhouse, arena, arena_vip, arena_garage1, arena_garage2, arena_garage3 }

        /// <summary>
        /// Interior class used for managing the interior data.
        /// </summary>
        internal class Interior
        {
            internal string name = null; // The interior display name.
            internal dynamic iplObject;

            internal Vector3 posTpInt = Vector3.Zero; // interior teleport location in the world.
            internal List<Vector3> posTpExt = new List<Vector3>() { Vector3.Zero }; // exterior teleport location in the world.
            internal Vector3 outPos = Vector3.Zero;

            internal float teleportHeading = 0f;
            internal Vector3 interiorLocation = Vector3.Zero; // used to get the interior ID, interior ID at the posInt (entrance) is usually NOT the correct interior ID.

            internal Vector3 TvPosition { get; set; } = Vector3.Zero;

            //internal int InteriorId => iplObject != null ? (((System.Dynamic.ExpandoObject)iplObject).Any(k => k.Key == "interiorId")) ? (int)iplObject.interiorId : (int)iplObject.currentInteriorId : 0;
            internal int InteriorId => GetInteriorAtCoords(interiorLocation.X, interiorLocation.Y, interiorLocation.Z);

            internal Menu Menu { get; set; }
            internal MenuItem tpButton = new MenuItem("Teleport To Interior", "Teleport directly to the interior.");

            internal Interior(string name, Menu parentMenu)
            {
                this.name = name;

                Menu = new Menu("Interior Options", name ?? "n/a");
                Menu.AddMenuItem(tpButton);
                Menu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == tpButton)
                    {
                        TeleportToInterior();
                    }
                };
                MenuController.AddSubmenu(parentMenu, Menu);
                var menuBindItem = new MenuItem(name, $"Manage {name} interior options.") { Label = "→→→" };
                parentMenu.AddMenuItem(menuBindItem);
                MenuController.BindMenuItem(parentMenu, Menu, menuBindItem);
            }

            internal async void TeleportToInterior()
            {
                RequestCollisionAtCoord(posTpInt.X, posTpInt.Y, posTpInt.Z);
                DoScreenFadeOut(500);
                while (!IsScreenFadedOut())
                {
                    await Delay(0);
                }
                RequestCollisionAtCoord(posTpInt.X, posTpInt.Y, posTpInt.Z);
                SetEntityCoordsNoOffset(PlayerPedId(), posTpInt.X, posTpInt.Y, posTpInt.Z, false, false, false);
                SetEntityHeading(Game.PlayerPed.Handle, teleportHeading);
                DoScreenFadeIn(500);
            }
        }

        /// <summary>
        /// Apartment class, is a type of interior.
        /// </summary>
        internal class Apartment : Interior
        {
            // Is low end or mid end house, used for smoke state differences.
            internal bool IsMidOrLowEnd = false;

            // Clothing strip states
            internal bool StripA { get; private set; } = false;
            internal bool StripB { get; private set; } = false;
            internal bool StripC { get; private set; } = false;

            // Booze states
            internal bool BoozeA { get; private set; } = false;
            internal bool BoozeB { get; private set; } = false;
            internal bool BoozeC { get; private set; } = false;

            // Smoke states
            internal bool SmokeA { get; private set; } = false;
            internal bool SmokeB { get; private set; } = false;
            internal bool SmokeC { get; private set; } = false;

            /// <summary>
            /// Sets the strip clothes state.
            /// </summary>
            /// <param name="stripA"></param>
            /// <param name="stripB"></param>
            /// <param name="stripC"></param>
            /// <param name="refresh"></param>
            internal void SetStrip(bool stripA, bool stripB, bool stripC, bool refresh)
            {
                StripA = stripA;
                StripB = stripB;
                StripC = stripC;

                iplObject.Strip.Enable(iplObject.Strip.A, stripA, refresh);
                iplObject.Strip.Enable(iplObject.Strip.B, stripB, refresh);
                iplObject.Strip.Enable(iplObject.Strip.C, stripC, refresh);
            }

            /// <summary>
            /// Sets the booze state.
            /// </summary>
            /// <param name="boozeA"></param>
            /// <param name="boozeB"></param>
            /// <param name="boozeC"></param>
            /// <param name="refresh"></param>
            internal void SetBooze(bool boozeA, bool boozeB, bool boozeC, bool refresh)
            {
                BoozeA = boozeA;
                BoozeB = boozeB;
                BoozeC = boozeC;

                iplObject.Booze.Enable(iplObject.Booze.A, boozeA, refresh);
                iplObject.Booze.Enable(iplObject.Booze.B, boozeB, refresh);
                iplObject.Booze.Enable(iplObject.Booze.C, boozeC, refresh);
            }

            /// <summary>
            /// Sets the smoke state.
            /// </summary>
            /// <param name="smokeA"></param>
            /// <param name="smokeB"></param>
            /// <param name="smokeC"></param>
            /// <param name="refresh"></param>
            internal void SetSmoke(bool smokeA, bool smokeB, bool smokeC, bool refresh)
            {
                SmokeA = smokeA;
                SmokeB = smokeB;
                SmokeC = smokeC;

                if (IsMidOrLowEnd || this is Penthouse)
                {
                    // clear smoke
                    iplObject.Smoke.Clear(refresh);

                    // Smoke a, b, c or none.
                    iplObject.Smoke.Set(SmokeA ? iplObject.Smoke.stage1 : SmokeB ? iplObject.Smoke.stage2 : SmokeC ? iplObject.Smoke.stage3 : iplObject.Smoke.none);
                }
                else
                {
                    iplObject.Smoke.Enable(iplObject.Smoke.A, smokeA, refresh);
                    iplObject.Smoke.Enable(iplObject.Smoke.B, smokeB, refresh);
                    iplObject.Smoke.Enable(iplObject.Smoke.C, smokeC, refresh);
                }
            }

            const string toggle_prop_desc = "Toggle this interior prop.";
            public Apartment(string name, Menu parentMenu) : base(name, parentMenu)
            {
                Menu propsSubmenu = new Menu("Interior Props", "enable or disable props");

                MenuCheckboxItem stripABtn = new MenuCheckboxItem("Strip A", toggle_prop_desc, StripA);
                MenuCheckboxItem stripBBtn = new MenuCheckboxItem("Strip B", toggle_prop_desc, StripB);
                MenuCheckboxItem stripCBtn = new MenuCheckboxItem("Strip C", toggle_prop_desc, StripC);

                MenuCheckboxItem boozeABtn = new MenuCheckboxItem("Booze A", toggle_prop_desc, BoozeA);
                MenuCheckboxItem boozeBBtn = new MenuCheckboxItem("Booze B", toggle_prop_desc, BoozeB);
                MenuCheckboxItem boozeCBtn = new MenuCheckboxItem("Booze C", toggle_prop_desc, BoozeC);

                MenuCheckboxItem smokeABtn = new MenuCheckboxItem("Smoke A", toggle_prop_desc, SmokeA);
                MenuCheckboxItem smokeBBtn = new MenuCheckboxItem("Smoke B", toggle_prop_desc, SmokeB);
                MenuCheckboxItem smokeCBtn = new MenuCheckboxItem("Smoke C", toggle_prop_desc, SmokeC);

                int stage = SmokeA ? 1 : SmokeB ? 2 : SmokeC ? 3 : 0;
                MenuListItem smokeList = new MenuListItem("Smoke Stage", new List<string>() { "No Smoke", "Smoke Stage A", "Smoke Stage B", "Smoke Stage C" }, stage, "Set a smoke props stage/style.");

                propsSubmenu.AddMenuItem(stripABtn);
                propsSubmenu.AddMenuItem(stripBBtn);
                propsSubmenu.AddMenuItem(stripCBtn);

                propsSubmenu.AddMenuItem(boozeABtn);
                propsSubmenu.AddMenuItem(boozeBBtn);
                propsSubmenu.AddMenuItem(boozeCBtn);

                if (IsMidOrLowEnd || this is Penthouse)
                {
                    propsSubmenu.AddMenuItem(smokeList);
                }
                else
                {
                    propsSubmenu.AddMenuItem(smokeABtn);
                    propsSubmenu.AddMenuItem(smokeBBtn);
                    propsSubmenu.AddMenuItem(smokeCBtn);
                }


                propsSubmenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
                {
                    if (item == smokeList)
                    {
                        if (newIndex == 0)
                        {
                            SmokeA = false;
                            SmokeB = false;
                            SmokeC = false;
                        }
                        else if (newIndex == 1)
                        {
                            SmokeA = true;
                            SmokeB = false;
                            SmokeC = false;
                        }
                        else if (newIndex == 2)
                        {
                            SmokeA = false;
                            SmokeB = true;
                            SmokeC = false;
                        }
                        else if (newIndex == 3)
                        {
                            SmokeA = false;
                            SmokeB = false;
                            SmokeC = true;
                        }
                        SetSmoke(SmokeA, SmokeB, SmokeC, true);
                    }
                };


                MenuController.AddSubmenu(Menu, propsSubmenu);

                MenuItem propsMenuBtn = new MenuItem("Interior Props", "Enable or disable interior props.") { Label = "→→→" };

                Menu.AddMenuItem(propsMenuBtn);

                MenuController.BindMenuItem(Menu, propsSubmenu, propsMenuBtn);



                propsSubmenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    // smoke
                    if (item == smokeABtn)
                    {
                        SmokeA = _checked;
                        SetSmoke(SmokeA, SmokeB, SmokeC, true);
                    }
                    else if (item == smokeBBtn)
                    {
                        SmokeB = _checked;
                        SetSmoke(SmokeA, SmokeB, SmokeC, true);
                    }
                    else if (item == smokeCBtn)
                    {
                        SmokeC = _checked;
                        SetSmoke(SmokeA, SmokeB, SmokeC, true);
                    }

                    // booze
                    else if (item == boozeABtn)
                    {
                        BoozeA = _checked;
                        SetBooze(BoozeA, BoozeB, BoozeC, true);
                    }
                    else if (item == boozeBBtn)
                    {
                        BoozeB = _checked;
                        SetBooze(BoozeA, BoozeB, BoozeC, true);
                    }
                    else if (item == boozeCBtn)
                    {
                        BoozeC = _checked;
                        SetBooze(BoozeA, BoozeB, BoozeC, true);
                    }

                    // strip
                    else if (item == stripABtn)
                    {
                        StripA = _checked;
                        SetStrip(StripA, StripB, StripC, true);
                    }
                    else if (item == stripBBtn)
                    {
                        StripB = _checked;
                        SetStrip(StripA, StripB, StripC, true);
                    }
                    else if (item == stripCBtn)
                    {
                        StripC = _checked;
                        SetStrip(StripA, StripB, StripC, true);
                    }
                };
            }
        }

        /// <summary>
        /// Penthouse class, inherits from Apartment.
        /// </summary>
        internal class Penthouse : Apartment
        {
            internal int Style { get; private set; } = 0;
            List<dynamic> styles = new List<dynamic>();

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name"></param>
            /// <param name="parentMenu"></param>
            internal Penthouse(string name, Menu parentMenu) : this(name, parentMenu, 0) { }
            /// <summary>
            /// Constructor overload with style option.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="parentMenu"></param>
            /// <param name="style"></param>
            internal Penthouse(string name, Menu parentMenu, int style) : base(name, parentMenu)
            {
                Style = style;

                MenuListItem stylesList = new MenuListItem("Penthouse Style", new List<string>() { "Modern", "Moody", "Vibrant", "Sharp", "Monochrome", "Seductive", "Regal", "Aqua" }, Style, "Select a penthouse style.");
                Menu.AddMenuItem(stylesList);

                Menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
                {
                    if (item == stylesList)
                    {
                        SetStyle(newIndex);
                    }
                };
            }


            /// <summary>
            /// Set the interior penthouse style.
            /// </summary>
            /// <param name="styleIndex"></param>
            internal void SetStyle(int styleIndex)
            {
                if (styles.Count == 0)
                {
                    styles.Add(iplObject.Style.Theme.modern);
                    styles.Add(iplObject.Style.Theme.moody);
                    styles.Add(iplObject.Style.Theme.vibrant);
                    styles.Add(iplObject.Style.Theme.sharp);
                    styles.Add(iplObject.Style.Theme.monochrome);
                    styles.Add(iplObject.Style.Theme.seductive);
                    styles.Add(iplObject.Style.Theme.regal);
                    styles.Add(iplObject.Style.Theme.aqua);
                }
                Style = styleIndex;

                iplObject.Style.Set(styles[styleIndex], true);

                SetStrip(StripA, StripB, StripC, true);
                SetBooze(BoozeA, BoozeB, BoozeC, true);
                SetSmoke(SmokeA, SmokeB, SmokeC, true);
            }

        }

        /// <summary>
        /// House class, inherits from Apartment.
        /// </summary>
        internal class House : Apartment
        {
            internal House(string name, Menu parentMenu) : base(name, parentMenu)
            {

            }
        }

        /// <summary>
        /// Hanger class, (aircraft hangers) inherits from Interior
        /// </summary>
        internal class Hanger : Interior
        {
            //internal List<Vector3> outTpLocations = new List<Vector3>();

            internal bool ModArea { get; private set; } = true;

            internal bool Cranes { get; private set; } = true;

            internal int WallsColor { get; private set; } = 0;

            internal int FloorStyle { get; private set; } = 1;
            internal int FloorDecals { get; private set; } = 0;
            internal int FloorDecalsColor { get; private set; } = 0;

            internal int LightingCeiling { get; private set; } = 0;
            internal int LightingWalls { get; private set; } = 0;
            internal int LightingFakeLights { get; private set; } = 0;

            internal int OfficeStyle { get; private set; } = 0;

            internal int BedroomStyle { get; private set; } = 1;
            internal int BedroomStyleColor { get; private set; } = 0;
            internal bool BedroomBlindsOpen { get; private set; } = false;
            internal bool DetailsBedroomClutter { get; private set; } = false;

            internal Hanger(string name, Menu parentMenu) : base(name, parentMenu)
            {
                var wallStyles = new List<string>()
                {
                    "Sable, red, gray.", // 1
                    "White, blue, orange.", // 2
                    "Gray, orange, blue.", // 3
                    "Gray, blue, orange.", // 4
                    "Gray, light gray, red.", // 5
                    "Yellow, gray, light gray.", // 6
                    "(Light) Black and white.", // 7
                    "(Dark) Black and white.", // 8
                    "Sable and gray." // 9
                };
                var floorStyle = new List<string>()
                {
                    "Raw concrete ground.",
                    "Plain concrete ground"
                };
                var officeStyles = new List<string>()
                {
                    "Basic office",
                    "Modern office",
                    "Traditional office"
                };


                MenuListItem walls = new MenuListItem("Walls Color", wallStyles, WallsColor, "Set the color scheme of the hangar walls.");
                MenuListItem floors = new MenuListItem("Floor Style", floorStyle, FloorStyle, "Set the floor style.");
                MenuListItem office = new MenuListItem("Office Style", officeStyles, OfficeStyle, "Set a office style.");

                Menu.AddMenuItem(walls);
                Menu.AddMenuItem(floors);
                Menu.AddMenuItem(office);

                Menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
                {
                    if (item == walls)
                    {
                        SetWallsColor(newIndex);
                    }
                    else if (item == floors)
                    {
                        SetFloorStyle(newIndex);
                    }
                    else if (item == office)
                    {
                        SetOfficeStyle(newIndex);
                    }
                };
            }

            internal void SetOfficeStyle(int newStyle)
            {
                OfficeStyle = newStyle;
                if (newStyle == 0)
                {
                    iplObject.Office.Set(iplObject.Office.basic, true);
                }
                else if (newStyle == 1)
                {
                    iplObject.Office.Set(iplObject.Office.modern, true);
                }
                else if (newStyle == 2)
                {
                    iplObject.Office.Set(iplObject.Office.traditional, true);
                }
            }

            internal void SetFloorStyle(int newStyle)
            {
                FloorStyle = newStyle;
                if (newStyle == 0)
                {
                    iplObject.Floor.Style.Set(iplObject.Floor.Style.raw, true);
                }
                else if (newStyle == 1)
                {
                    iplObject.Floor.Style.Set(iplObject.Floor.Style.plain, true);
                }
            }

            internal void SetWallsColor(int newStyle)
            {
                WallsColor = newStyle;
                dynamic style = null;
                switch (newStyle)
                {
                    case 0:
                        style = iplObject.Colors.colorSet1;
                        break;
                    case 1:
                        style = iplObject.Colors.colorSet2;
                        break;
                    case 2:
                        style = iplObject.Colors.colorSet3;
                        break;
                    case 3:
                        style = iplObject.Colors.colorSet4;
                        break;
                    case 4:
                        style = iplObject.Colors.colorSet5;
                        break;
                    case 5:
                        style = iplObject.Colors.colorSet6;
                        break;
                    case 6:
                        style = iplObject.Colors.colorSet7;
                        break;
                    case 7:
                        style = iplObject.Colors.colorSet8;
                        break;
                    case 8:
                        style = iplObject.Colors.colorSet9;
                        break;
                }
                iplObject.Walls.SetColor(style, true);
            }

        }

        /// <summary>
        /// Bunker class, inherits from Interior
        /// </summary>
        internal class Bunker : Interior
        {
            internal Bunker(string name, Menu parentMenu) : base(name, parentMenu)
            {

            }
        }

        internal class Nightclub : Interior
        {
            #region interior options

            #region interior variables
            // Styles etc.
            internal int ClubName { get; private set; } = 0;

            internal int ClubStyle { get; private set; } = 1;

            internal int ClubPodiumStyle { get; private set; } = 1;

            internal int ClubSpeakers { get; private set; } = 2;

            internal bool ClubSecurity { get; private set; } = true;

            internal int ClubTurntables { get; private set; } = 1;

            internal bool ClubBar { get; private set; } = true;

            // Booze
            internal bool BoozeA { get; private set; } = true;
            internal bool BoozeB { get; private set; } = true;
            internal bool BoozeC { get; private set; } = true;

            // Trophies
            internal int TrophyNumber1 { get; private set; } = 3; // 0 = disabled, 1 = bronze, 2 = silver, 3 = gold
            internal int TrophyBattler { get; private set; } = 0;
            internal int TrophyDancer { get; private set; } = 0;

            // Details (props)
            internal bool Clutter { get; private set; } = false;
            internal bool Worklamps { get; private set; } = false;
            internal bool Truck { get; private set; } = false;
            internal bool DryIce { get; private set; } = false;
            internal bool LightRigsOff { get; private set; } = false;
            internal bool RoofLightsOff { get; private set; } = false;
            internal bool FloorTradLights { get; private set; } = false;
            internal bool Chest { get; private set; } = false;
            internal bool VaultAmmo { get; private set; } = false;
            internal bool VaultMeth { get; private set; } = false;
            internal bool VaultFakeID { get; private set; } = false;
            internal bool VaultWeed { get; private set; } = false;
            internal bool VaultCoke { get; private set; } = false;
            internal bool VaultCash { get; private set; } = false;

            // Lights
            internal int DropletLights { get; private set; } = 0;
            internal int NeonLights { get; private set; } = 0;
            internal int BandLights { get; private set; } = 3;
            internal int LaserLights { get; private set; } = 0;
            #endregion

            #region interior options menu setup
            private Menu interiorOptions = new Menu("Nightclub", "Nightclub Interior Options");

            internal void CreateInteriorOptionsMenu()
            {
                // Other details submenu
                Menu otherDetails = new Menu("Nightclub Details", "Nightclub Interior Details Options");
                MenuController.AddSubmenu(interiorOptions, otherDetails);
                var otherDetailsBtn = new MenuItem("Other interior details", "Mangage other interior details, mostly props.") { Label = "→→→" };
                MenuController.BindMenuItem(interiorOptions, otherDetails, otherDetailsBtn);

                // Other details submenu menu items
                MenuCheckboxItem clutter = new MenuCheckboxItem("Clutter and graffiti", Clutter);
                MenuCheckboxItem worklamps = new MenuCheckboxItem("Work lamps and trash", Worklamps);
                MenuCheckboxItem truck = new MenuCheckboxItem("Boxtruck in garage", Truck);
                MenuCheckboxItem dryIce = new MenuCheckboxItem("Dry ice machines", DryIce);
                MenuCheckboxItem lightRigsOff = new MenuCheckboxItem("Disabled Light Racks", "This adds all light rigs but in the disabled state, might glitch out if you have this turned on while you also have other lights setup in the previous menu.", LightRigsOff);
                MenuCheckboxItem roofLigthsOff = new MenuCheckboxItem("Fake lights", RoofLightsOff);
                MenuCheckboxItem floorTradLights = new MenuCheckboxItem("Floor and wall lights", "These lights are supposed to be only added on the traditional club style, they might not work correctly for other club styles.", FloorTradLights);
                MenuCheckboxItem chest = new MenuCheckboxItem("Chest", "A chest on the desk in the VIP room upstairs.", Chest);
                MenuCheckboxItem vaultAmmunations = new MenuCheckboxItem("Vault Ammo", "Style of the inside of the vault in the VIP room upstairs (inside is not visible).", VaultAmmo);
                MenuCheckboxItem vaultMeth = new MenuCheckboxItem("Vault Meth", "Style of the inside of the vault in the VIP room upstairs (inside is not visible).", VaultMeth);
                MenuCheckboxItem vaultFakeID = new MenuCheckboxItem("Vault Fake ID", "Style of the inside of the vault in the VIP room upstairs (inside is not visible).", VaultFakeID);
                MenuCheckboxItem vaultWeed = new MenuCheckboxItem("Vault Weed", "Style of the inside of the vault in the VIP room upstairs (inside is not visible).", VaultWeed);
                MenuCheckboxItem vaultCoke = new MenuCheckboxItem("Vault Coke", "Style of the inside of the vault in the VIP room upstairs (inside is not visible).", VaultCoke);
                MenuCheckboxItem vaultCash = new MenuCheckboxItem("Vault Cash", "Style of the inside of the vault in the VIP room upstairs (inside is not visible).", VaultCash);

                otherDetails.AddMenuItem(clutter);
                otherDetails.AddMenuItem(worklamps);
                otherDetails.AddMenuItem(truck);
                otherDetails.AddMenuItem(dryIce);
                otherDetails.AddMenuItem(lightRigsOff);
                otherDetails.AddMenuItem(roofLigthsOff);
                otherDetails.AddMenuItem(floorTradLights);
                otherDetails.AddMenuItem(chest);
                otherDetails.AddMenuItem(vaultAmmunations);
                otherDetails.AddMenuItem(vaultMeth);
                otherDetails.AddMenuItem(vaultFakeID);
                otherDetails.AddMenuItem(vaultWeed);
                otherDetails.AddMenuItem(vaultCoke);
                otherDetails.AddMenuItem(vaultCash);

                otherDetails.OnCheckboxChange += (sender, item, index, enabled) =>
                {
                    SetProp(index, enabled);
                };


                // Create the items.
                MenuListItem clubName = new MenuListItem("Club Name", new List<string>() { "Galaxy", "Studio", "Omega", "Technologie", "Gefangnis", "Maisonette", "Tony", "Palace", "Paradise" }, ClubName, "Select a club name for the Nightclub interior.");

                MenuListItem clubStyle = new MenuListItem("Club Style", new List<string>() { "Traditional", "Edgy", "Glamorous" }, ClubStyle, "Select a club style for the Nightclub interior.");

                MenuListItem clubPodiumStyle = new MenuListItem("Podium Style", new List<string>() { "Traditional", "Edgy", "Glamorous" }, ClubPodiumStyle, "Select a podium style for the Nightclub interior.");

                MenuListItem clubSpeakers = new MenuListItem("Speakers", new List<string>() { "No Speakers", "Basic Speakers", "Upgraded Speakers" }, ClubSpeakers, "Select the speakers type for the Nightclub interior.");

                MenuCheckboxItem clubSecurity = new MenuCheckboxItem("Security Cameras", "Enable or disable club security cameras in the Nightclub interior.", ClubSecurity);

                MenuListItem turntables = new MenuListItem("Turntables", new List<string>() { "No turntables", "Style 1", "Style 2", "Style 3", "Style 4" }, ClubTurntables, "Select a turntables variant for the Nightclub interior.");

                MenuCheckboxItem bar = new MenuCheckboxItem("Bar", "Enable or disable the bar.", ClubBar);

                MenuCheckboxItem boozeA = new MenuCheckboxItem("Booze A", "Enable or disable Booze style A.", BoozeA);
                MenuCheckboxItem boozeB = new MenuCheckboxItem("Booze B", "Enable or disable Booze style C.", BoozeB);
                MenuCheckboxItem boozeC = new MenuCheckboxItem("Booze C", "Enable or disable Booze style B.", BoozeC);

                MenuListItem trophyNumberOne = new MenuListItem("Trophy - Number 1", new List<string>() { "No trophy", "Bronze", "Silver", "Gold" }, TrophyNumber1, "Select a 'Number One' trophy variation. Trophies are on the desk in the VIP room upstairs.");
                MenuListItem trophyBattler = new MenuListItem("Trophy - Battler", new List<string>() { "No trophy", "Bronze", "Silver", "Gold" }, TrophyBattler, "Select a 'Battler' trophy variation. Trophies are on the desk in the VIP room upstairs.");
                MenuListItem trophyDancer = new MenuListItem("Trophy - Dancer", new List<string>() { "No trophy", "Bronze", "Silver", "Gold" }, TrophyDancer, "Select a 'Dancer' trophy variation. Trophies are on the desk in the VIP room upstairs.");

                MenuListItem dropletsLights = new MenuListItem("Droplet Lights", new List<string>() { "No droplet lights", "Yellow", "Green", "White", "Purple" }, DropletLights, "Droplet lights variation.");
                MenuListItem neonsLights = new MenuListItem("Neon Lights", new List<string>() { "No neon lights", "Yellow", "White", "Purple", "Cyan" }, NeonLights, "Neon lights variation.");
                MenuListItem bandsLights = new MenuListItem("Band Lights", new List<string>() { "No band lights", "Yellow", "Green", "white", "Cyan" }, BandLights, "Band lights variation.");
                MenuListItem lasersLights = new MenuListItem("Laser Lights", new List<string>() { "No laser lights", "Yellow", "Green", "White", "Purple" }, LaserLights, "Laser lights variation.");


                // Add items to the menu.
                // misc options
                interiorOptions.AddMenuItem(otherDetailsBtn);
                interiorOptions.AddMenuItem(clubName);
                interiorOptions.AddMenuItem(clubStyle);
                interiorOptions.AddMenuItem(clubPodiumStyle);
                interiorOptions.AddMenuItem(clubSpeakers);
                interiorOptions.AddMenuItem(clubSecurity);
                interiorOptions.AddMenuItem(turntables);
                interiorOptions.AddMenuItem(bar);
                // booze
                interiorOptions.AddMenuItem(boozeA);
                interiorOptions.AddMenuItem(boozeB);
                interiorOptions.AddMenuItem(boozeC);
                // trophies
                interiorOptions.AddMenuItem(trophyNumberOne);
                interiorOptions.AddMenuItem(trophyBattler);
                interiorOptions.AddMenuItem(trophyDancer);
                // lights
                interiorOptions.AddMenuItem(dropletsLights);
                interiorOptions.AddMenuItem(neonsLights);
                interiorOptions.AddMenuItem(bandsLights);
                interiorOptions.AddMenuItem(lasersLights);

                // Checkbox changes.
                interiorOptions.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    if (item == clubSecurity)
                    {
                        ClubSecurity = _checked;
                        SetClubSecurity(ClubSecurity);
                    }
                    else if (item == bar)
                    {
                        ClubBar = _checked;
                        SetClubBar(ClubBar);
                    }
                    else if (item == boozeA)
                    {
                        BoozeA = _checked;
                        SetBooze(BoozeA, BoozeB, BoozeC);
                    }
                    else if (item == boozeB)
                    {
                        BoozeB = _checked;
                        SetBooze(BoozeA, BoozeB, BoozeC);
                    }
                    else if (item == boozeC)
                    {
                        BoozeC = _checked;
                        SetBooze(BoozeA, BoozeB, BoozeC);
                    }
                };

                // List changes
                interiorOptions.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
                {
                    if (item == clubName)
                    {
                        ClubName = newIndex;
                        SetClubName(ClubName);
                    }
                    else if (item == clubStyle)
                    {
                        ClubStyle = newIndex;
                        SetClubStyle(ClubStyle);
                    }
                    else if (item == clubPodiumStyle)
                    {
                        ClubPodiumStyle = newIndex;
                        SetClubPodiumStyle(ClubPodiumStyle);
                    }
                    else if (item == clubSpeakers)
                    {
                        ClubSpeakers = newIndex;
                        SetClubSpeakers(ClubSpeakers);
                    }
                    else if (item == turntables)
                    {
                        ClubTurntables = newIndex;
                        SetClubTurntables(ClubTurntables);
                    }
                    else if (item == trophyNumberOne)
                    {
                        TrophyNumber1 = newIndex;
                        SetTrophy(0, TrophyNumber1);
                    }
                    else if (item == trophyBattler)
                    {
                        TrophyBattler = newIndex;
                        SetTrophy(1, TrophyBattler);
                    }
                    else if (item == trophyDancer)
                    {
                        TrophyDancer = newIndex;
                        SetTrophy(2, TrophyDancer);
                    }
                    else if (item == dropletsLights)
                    {
                        DropletLights = newIndex;
                        SetLights(0, DropletLights);
                    }
                    else if (item == neonsLights)
                    {
                        NeonLights = newIndex;
                        SetLights(1, NeonLights);
                    }
                    else if (item == bandsLights)
                    {
                        BandLights = newIndex;
                        SetLights(2, BandLights);
                    }
                    else if (item == lasersLights)
                    {
                        LaserLights = newIndex;
                        SetLights(3, LaserLights);
                    }
                };

                // manage binding and setup for the submenu.
                MenuController.AddSubmenu(Menu, interiorOptions);
                var btn = new MenuItem("Nightclub Interior Options", "Configure interior options for the nightclub.") { Label = "→→→" };
                Menu.AddMenuItem(btn);
                MenuController.BindMenuItem(Menu, interiorOptions, btn);
            }
            #endregion

            #region interior options set functions
            /// <summary>
            /// Sets the club name.
            /// </summary>
            /// <param name="index"></param>
            internal async void SetClubName(int index)
            {
                ClubName = index;
                dynamic name = null;
                switch (index)
                {
                    case 0:
                        name = iplObject.Interior.Name.galaxy;
                        break;
                    case 1:
                        name = iplObject.Interior.Name.studio;
                        break;
                    case 2:
                        name = iplObject.Interior.Name.omega;
                        break;
                    case 3:
                        name = iplObject.Interior.Name.technologie;
                        break;
                    case 4:
                        name = iplObject.Interior.Name.gefangnis;
                        break;
                    case 5:
                        name = iplObject.Interior.Name.maisonette;
                        break;
                    case 6:
                        name = iplObject.Interior.Name.tony;
                        break;
                    case 7:
                        name = iplObject.Interior.Name.palace;
                        break;
                    case 8:
                        name = iplObject.Interior.Name.paradise;
                        break;
                }
                iplObject.Interior.Name.Set(name ?? "", false);
                await Delay(100);
                RefreshInterior(InteriorId);
            }

            /// <summary>
            /// Sets the club style.
            /// </summary>
            /// <param name="index"></param>
            internal void SetClubStyle(int index)
            {
                ClubStyle = index;
                dynamic style = null;
                switch (index)
                {
                    case 0:
                        style = iplObject.Interior.Style.trad;
                        break;
                    case 1:
                        style = iplObject.Interior.Style.edgy;
                        break;
                    case 2:
                        style = iplObject.Interior.Style.glam;
                        break;
                }

                iplObject.Interior.Style.Set(style, true);
            }

            /// <summary>
            /// Sets the club podium style.
            /// </summary>
            /// <param name="index"></param>
            internal void SetClubPodiumStyle(int index)
            {
                ClubPodiumStyle = index;
                dynamic podium = null;
                switch (index)
                {
                    case 0:
                        podium = iplObject.Interior.Podium.trad;
                        break;
                    case 1:
                        podium = iplObject.Interior.Podium.edgy;
                        break;
                    case 2:
                        podium = iplObject.Interior.Podium.glam;
                        break;
                }
                iplObject.Interior.Podium.Set(podium, true);
            }

            /// <summary>
            /// Sets the club speakers style, not using API here because that one is bugged somehow.
            /// </summary>
            /// <param name="index"></param>
            internal void SetClubSpeakers(int index)
            {
                ClubSpeakers = index;
                // Clubspeakers are buggy as fuck when using the api, i'll just manually enable/disable them for a better result.
                string basic = "Int01_ba_equipment_setup";
                string upgraded = "Int01_ba_equipment_upgrade";

                DisableInteriorProp(InteriorId, upgraded);
                DisableInteriorProp(InteriorId, basic);
                if (index == 1)
                {
                    EnableInteriorProp(InteriorId, basic);
                }
                else if (index == 2)
                {
                    EnableInteriorProp(InteriorId, basic);
                    EnableInteriorProp(InteriorId, upgraded);
                }
                RefreshInterior(InteriorId);
            }

            /// <summary>
            /// Enables or disables the club security cameras.
            /// </summary>
            /// <param name="enabled"></param>
            internal void SetClubSecurity(bool enabled)
            {
                ClubSecurity = enabled;
                if (enabled)
                    iplObject.Interior.Security.Set(iplObject.Interior.Security.on, true);
                else
                    iplObject.Interior.Security.Set(iplObject.Interior.Security.off, true);
            }

            /// <summary>
            /// Sets the club turntables style.
            /// </summary>
            /// <param name="index"></param>
            internal void SetClubTurntables(int index)
            {
                ClubTurntables = index;
                dynamic turntable = null;
                switch (index)
                {
                    case 0:
                        turntable = iplObject.Interior.Turntables.none;
                        break;
                    case 1:
                        turntable = iplObject.Interior.Turntables.style01;
                        break;
                    case 2:
                        turntable = iplObject.Interior.Turntables.style02;
                        break;
                    case 3:
                        turntable = iplObject.Interior.Turntables.style03;
                        break;
                    case 4:
                        turntable = iplObject.Interior.Turntables.style04;
                        break;
                }
                iplObject.Interior.Turntables.Set(turntable, true);
            }

            /// <summary>
            /// Enables or disables the club bar.
            /// </summary>
            /// <param name="enabled"></param>
            internal void SetClubBar(bool enabled)
            {
                ClubBar = enabled;
                if (enabled)
                    iplObject.Interior.Bar.Enable(true, true);
                else
                    iplObject.Interior.Bar.Enable(false, true);
            }

            /// <summary>
            /// Sets the booze states for a, b and c.
            /// </summary>
            /// <param name="bA"></param>
            /// <param name="bB"></param>
            /// <param name="bC"></param>
            internal void SetBooze(bool bA, bool bB, bool bC)
            {
                BoozeA = bA;
                BoozeB = bB;
                BoozeC = bC;

                iplObject.Interior.Booze.Enable(iplObject.Interior.Booze.A, bA, false);
                iplObject.Interior.Booze.Enable(iplObject.Interior.Booze.B, bB, false);
                iplObject.Interior.Booze.Enable(iplObject.Interior.Booze.C, bC, false);

                RefreshInterior(InteriorId);
            }

            /// <summary>
            /// Sets the state and style for each trophy.
            /// </summary>
            /// <param name="trophyIndex"></param>
            /// <param name="trophyStyle"></param>
            internal void SetTrophy(int trophyIndex, int trophyStyle)
            {
                // Select the style object.
                dynamic style = null;
                switch (trophyStyle)
                {
                    case 0:
                        break;
                    case 1:
                        style = iplObject.Interior.Trophy.Color.bronze;
                        break;
                    case 2:
                        style = iplObject.Interior.Trophy.Color.silver;
                        break;
                    case 3:
                        style = iplObject.Interior.Trophy.Color.gold;
                        break;
                }

                // Select the trophy object.
                dynamic trophy = null;
                switch (trophyIndex)
                {
                    case 0:
                        TrophyNumber1 = trophyStyle;
                        trophy = iplObject.Interior.Trophy.number1;
                        break;
                    case 1:
                        TrophyBattler = trophyStyle;
                        trophy = iplObject.Interior.Trophy.battler;
                        break;
                    case 2:
                        TrophyDancer = trophyStyle;
                        trophy = iplObject.Interior.Trophy.dancer;
                        break;
                }

                // Disable the trophy.
                if (style == null)
                {
                    iplObject.Interior.Trophy.Enable(trophy, false, style, true);
                }
                // Enable it.
                else
                {
                    iplObject.Interior.Trophy.Enable(trophy, true, style, true);
                }
            }

            /// <summary>
            /// Sets the prop enabled or disabled.
            /// </summary>
            /// <param name="index"></param>
            /// <param name="enabled"></param>
            internal void SetProp(int index, bool enabled)
            {
                dynamic prop = null;
                switch (index)
                {
                    case 0:
                        prop = iplObject.Interior.Details.clutter;
                        Clutter = enabled;
                        break;
                    case 1:
                        prop = iplObject.Interior.Details.worklamps;
                        Worklamps = enabled;
                        break;
                    case 2:
                        prop = iplObject.Interior.Details.truck;
                        Truck = enabled;
                        break;
                    case 3:
                        prop = iplObject.Interior.Details.dryIce;
                        DryIce = enabled;
                        break;
                    case 4:
                        prop = iplObject.Interior.Details.lightRigsOff;
                        LightRigsOff = enabled;
                        break;
                    case 5:
                        prop = iplObject.Interior.Details.roofLigthsOff;
                        RoofLightsOff = enabled;
                        break;
                    case 6:
                        prop = iplObject.Interior.Details.floorTradLights;
                        FloorTradLights = enabled;
                        break;
                    case 7:
                        prop = iplObject.Interior.Details.chest;
                        Chest = enabled;
                        break;
                    case 8:
                        prop = iplObject.Interior.Details.vaultAmmunations;
                        VaultAmmo = enabled;
                        break;
                    case 9:
                        prop = iplObject.Interior.Details.vaultMeth;
                        VaultMeth = enabled;
                        break;
                    case 10:
                        prop = iplObject.Interior.Details.vaultFakeID;
                        VaultFakeID = enabled;
                        break;
                    case 11:
                        prop = iplObject.Interior.Details.vaultWeed;
                        VaultWeed = enabled;
                        break;
                    case 12:
                        prop = iplObject.Interior.Details.vaultCoke;
                        VaultCoke = enabled;
                        break;
                    case 13:
                        prop = iplObject.Interior.Details.vaultCash;
                        VaultCash = enabled;
                        break;
                }
                iplObject.Interior.Details.Enable(prop, enabled, true);
            }

            /// <summary>
            /// Sets the lights state.
            /// </summary>
            /// <param name="lightType"></param>
            /// <param name="lightColor"></param>
            internal void SetLights(int lightType, int lightColor)
            {
                dynamic light = null;
                dynamic style = null;

                // Determine the light object and style object based on the index.
                switch (lightType)
                {
                    case 0:
                        DropletLights = lightColor;
                        light = iplObject.Interior.Lights.Droplets;
                        switch (lightColor)
                        {
                            case 1:
                                style = light.yellow;
                                break;
                            case 2:
                                style = light.green;
                                break;
                            case 3:
                                style = light.white;
                                break;
                            case 4:
                                style = light.purple;
                                break;
                        }
                        break;
                    case 1:
                        NeonLights = lightColor;
                        light = iplObject.Interior.Lights.Neons;
                        switch (lightColor)
                        {
                            case 1:
                                style = light.yellow;
                                break;
                            case 2:
                                style = light.white;
                                break;
                            case 3:
                                style = light.purple;
                                break;
                            case 4:
                                style = light.cyan;
                                break;
                        }
                        break;
                    case 2:
                        BandLights = lightColor;
                        light = iplObject.Interior.Lights.Bands;
                        switch (lightColor)
                        {
                            case 1:
                                style = light.yellow;
                                break;
                            case 2:
                                style = light.green;
                                break;
                            case 3:
                                style = light.white;
                                break;
                            case 4:
                                style = light.cyan;
                                break;
                        }
                        break;
                    case 3:
                        LaserLights = lightColor;
                        light = iplObject.Interior.Lights.Lasers;
                        switch (lightColor)
                        {
                            case 1:
                                style = light.yellow;
                                break;
                            case 2:
                                style = light.green;
                                break;
                            case 3:
                                style = light.white;
                                break;
                            case 4:
                                style = light.purple;
                                break;
                        }
                        break;
                }

                // set the style for that specific light, or disable it all together.
                if (light != null)
                {
                    if (style == null)
                    {
                        light.Clear(true);
                    }
                    else
                    {
                        light.Set(style, true);
                    }
                }

            }
            #endregion
            #endregion

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name"></param>
            /// <param name="parentMenu"></param>
            internal Nightclub(string name, Menu parentMenu) : base(name, parentMenu)
            {
                CreateInteriorOptionsMenu();
            }


        }

        internal class Facility : Interior
        {
            internal Facility(string name, Menu parentMenu) : base(name, parentMenu)
            {

            }
        }

        #region Biker stuff (clubhouse, cocaine, forgery, etc) DLC_BIKERS
        internal class BikerGang
        {

        }

        internal class Clubhouse : Interior
        {
            internal Clubhouse(string name, Menu parentMenu) : base(name, parentMenu)
            {
            }
        }

        internal class BikerBusiness : Interior
        {
            internal BikerBusiness(string name, Menu parentMenu) : base(name, parentMenu)
            {
            }
        }

        internal class Cocaine : BikerBusiness
        {
            internal Cocaine(string name, Menu parentMenu) : base(name, parentMenu)
            {
            }
        }

        internal class Meth : BikerBusiness
        {
            internal Meth(string name, Menu parentMenu) : base(name, parentMenu)
            {
            }
        }

        internal class Weed : BikerBusiness
        {
            internal Weed(string name, Menu parentMenu) : base(name, parentMenu)
            {
            }
        }

        internal class CounterfeitCash : BikerBusiness
        {
            internal CounterfeitCash(string name, Menu parentMenu) : base(name, parentMenu)
            {
            }
        }

        internal class DocumentForgery : BikerBusiness
        {
            internal DocumentForgery(string name, Menu parentMenu) : base(name, parentMenu)
            {
            }
        }
        #endregion


    }
}
