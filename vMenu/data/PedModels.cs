using System.Collections.Generic;

using static CitizenFX.Core.Native.API;

namespace vMenuClient.data
{
    public static class PedModels
    {
        public static List<uint> AnimalHashes = new()
        {
            (uint)GetHashKey("a_c_boar"),
            (uint)GetHashKey("a_c_boar_02"), // mp2023_01
            (uint)GetHashKey("a_c_cat_01"),
            (uint)GetHashKey("a_c_chickenhawk"),
            (uint)GetHashKey("a_c_chimp"),
            (uint)GetHashKey("a_c_chimp_02"), // mpchristmas3
            (uint)GetHashKey("a_c_chop"),
            (uint)GetHashKey("a_c_chop_02"), // mpsecurity
            (uint)GetHashKey("a_c_cormorant"),
            (uint)GetHashKey("a_c_cow"),
            (uint)GetHashKey("a_c_coyote"),
            (uint)GetHashKey("a_c_coyote_02"), // mp2023_01
            (uint)GetHashKey("a_c_crow"),
            (uint)GetHashKey("a_c_deer"),
            (uint)GetHashKey("a_c_deer_02"), // mp2023_01
            (uint)GetHashKey("a_c_dolphin"),
            (uint)GetHashKey("a_c_fish"),
            (uint)GetHashKey("a_c_hen"),
            (uint)GetHashKey("a_c_humpback"),
            (uint)GetHashKey("a_c_husky"),
            (uint)GetHashKey("a_c_killerwhale"),
            (uint)GetHashKey("a_c_mtlion"),
            (uint)GetHashKey("a_c_mtlion_02"), // mp2023_01
            (uint)GetHashKey("a_c_panther"), // mpheist4
            (uint)GetHashKey("a_c_pig"),
            (uint)GetHashKey("a_c_pigeon"),
            (uint)GetHashKey("a_c_poodle"),
            (uint)GetHashKey("a_c_pug"),
            (uint)GetHashKey("a_c_pug_02"), // mp2023_01
            (uint)GetHashKey("a_c_rabbit_01"),
            (uint)GetHashKey("a_c_rabbit_02"), // mpchristmas3
            (uint)GetHashKey("a_c_rat"),
            (uint)GetHashKey("a_c_retriever"),
            (uint)GetHashKey("a_c_rhesus"),
            (uint)GetHashKey("a_c_rottweiler"),
            (uint)GetHashKey("a_c_seagull"),
            (uint)GetHashKey("a_c_sharkhammer"),
            (uint)GetHashKey("a_c_sharktiger"),
            (uint)GetHashKey("a_c_shepherd"),
            (uint)GetHashKey("a_c_stingray"),
            (uint)GetHashKey("a_c_westy")
        };
    }
}
