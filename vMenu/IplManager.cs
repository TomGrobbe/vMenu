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
            internal Vector3 outPos = Vector3.Zero;
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
            internal Nightclub(string name, Menu parentMenu) : base(name, parentMenu)
            {

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
