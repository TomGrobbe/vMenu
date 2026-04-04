/*
    Data generated using a custom made by Tom Grobbe.
    Data source: GTA V <update>_overlays.xml files.
*/

using System.Collections.Generic;

namespace vMenuClient.data
{
    public enum TattooZone
    {
        ZONE_TORSO = 0,
        ZONE_HEAD = 1,
        ZONE_LEFT_ARM = 2,
        ZONE_RIGHT_ARM = 3,
        ZONE_LEFT_LEG = 4,
        ZONE_RIGHT_LEG = 5,
        ZONE_UNKNOWN = 6,
        ZONE_NONE = 7
    }

    public struct Tattoo
    {
        public int gender;
        public string name;
        public string collectionName;
        public TattooZone zoneId;
        public string type;

        /// <summary>
        /// Creates a new Tattoo object.
        /// </summary>
        /// <param name="gender">0 = male, 1 = female, 2 = both</param>
        /// <param name="name">Tattoo name</param>
        /// <param name="collectionName">Tattoo collection name</param>
        /// <param name="zoneId">Tattoo zone <see cref="TattooZone"/></param>
        /// <param name="type">Tattoo type</param>
        public Tattoo(int gender, string name, string collectionName, TattooZone zoneId, string type)
        {
            this.gender = gender;
            this.name = name;
            this.collectionName = collectionName;
            this.zoneId = zoneId;
            this.type = type;
        }
    }

    internal static class MaleTattoosCollection
    {
        internal static List<Tattoo> HAIR = new();
        internal static List<Tattoo> TORSO = new();
        internal static List<Tattoo> HEAD = new();
        internal static List<Tattoo> LEFT_ARM = new();
        internal static List<Tattoo> RIGHT_ARM = new();
        internal static List<Tattoo> LEFT_LEG = new();
        internal static List<Tattoo> RIGHT_LEG = new();
        internal static List<Tattoo> BADGES = new();
    }

    internal struct FemaleTattoosCollection
    {
        internal static List<Tattoo> HAIR = new();
        internal static List<Tattoo> TORSO = new();
        internal static List<Tattoo> HEAD = new();
        internal static List<Tattoo> LEFT_ARM = new();
        internal static List<Tattoo> RIGHT_ARM = new();
        internal static List<Tattoo> LEFT_LEG = new();
        internal static List<Tattoo> RIGHT_LEG = new();
        internal static List<Tattoo> BADGES = new();
    }

    internal static class TattoosData
    {
        private static bool isDataSetup = false;
        internal static void GenerateTattoosData()
        {
            if (isDataSetup)
            {
                return;
            }

            isDataSetup = true;

            foreach (var tattoo in Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tattoo>>(Properties.Resources.overlays))
            {
                if (!string.IsNullOrEmpty(tattoo.name))
                {
                    if (tattoo.name.ToLower().Contains("hair_"))
                    {
                        if (tattoo.gender is 0 or 2)
                        {
                            MaleTattoosCollection.HAIR.Add(tattoo);
                        }
                        if (tattoo.gender is 1 or 2)
                        {
                            FemaleTattoosCollection.HAIR.Add(tattoo);
                        }
                    }
                    else if (tattoo.type == "TYPE_TATTOO")
                    {
                        switch (tattoo.zoneId)
                        {
                            case TattooZone.ZONE_TORSO:
                                if (tattoo.gender is 0 or 2)
                                {
                                    MaleTattoosCollection.TORSO.Add(tattoo);
                                }
                                if (tattoo.gender is 1 or 2)
                                {
                                    FemaleTattoosCollection.TORSO.Add(tattoo);
                                }
                                break;
                            case TattooZone.ZONE_HEAD:
                                if (tattoo.gender is 0 or 2)
                                {
                                    MaleTattoosCollection.HEAD.Add(tattoo);
                                }
                                if (tattoo.gender is 1 or 2)
                                {
                                    FemaleTattoosCollection.HEAD.Add(tattoo);
                                }
                                break;
                            case TattooZone.ZONE_LEFT_ARM:
                                if (tattoo.gender is 0 or 2)
                                {
                                    MaleTattoosCollection.LEFT_ARM.Add(tattoo);
                                }
                                if (tattoo.gender is 1 or 2)
                                {
                                    FemaleTattoosCollection.LEFT_ARM.Add(tattoo);
                                }
                                break;
                            case TattooZone.ZONE_RIGHT_ARM:
                                if (tattoo.gender is 0 or 2)
                                {
                                    MaleTattoosCollection.RIGHT_ARM.Add(tattoo);
                                }
                                if (tattoo.gender is 1 or 2)
                                {
                                    FemaleTattoosCollection.RIGHT_ARM.Add(tattoo);
                                }
                                break;
                            case TattooZone.ZONE_LEFT_LEG:
                                if (tattoo.gender is 0 or 2)
                                {
                                    MaleTattoosCollection.LEFT_LEG.Add(tattoo);
                                }
                                if (tattoo.gender is 1 or 2)
                                {
                                    FemaleTattoosCollection.LEFT_LEG.Add(tattoo);
                                }
                                break;
                            case TattooZone.ZONE_RIGHT_LEG:
                                if (tattoo.gender is 0 or 2)
                                {
                                    MaleTattoosCollection.RIGHT_LEG.Add(tattoo);
                                }
                                if (tattoo.gender is 1 or 2)
                                {
                                    FemaleTattoosCollection.RIGHT_LEG.Add(tattoo);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else if (tattoo.type == "TYPE_BADGE")
                    {
                        if (tattoo.gender is 0 or 2)
                        {
                            MaleTattoosCollection.BADGES.Add(tattoo);
                        }
                        if (tattoo.gender is 1 or 2)
                        {
                            FemaleTattoosCollection.BADGES.Add(tattoo);
                        }
                    } 
                }
            }

        }
    }
}
