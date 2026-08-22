using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

internal static class TorsoGloveTable
{
    private const char Separator = '|';

    private const string Male = "M";

    private static readonly string[] Rows =
    [
        "F|0|2|DLC_MP_HEIST_F_TORSO_0_0",
        "F|0|3|DLC_MP_HEIST_F_TORSO_13_0",
        "F|0|4|DLC_MP_HEIST_F_TORSO_26_0",
        "F|0|5|DLC_MP_HEIST_F_TORSO_39_0",
        "F|0|6|DLC_MP_HEIST_F_TORSO_52_0",
        "F|0|7|DLC_MP_HEIST_F_TORSO_65_0",
        "F|0|8|DLC_MP_HEIST_F_TORSO_78_0",
        "F|0|15|DLC_MP_GR_F_TORSO_2_0",
        "F|0|16|DLC_MP_GR_F_TORSO_18_0",
        "F|0|17|DLC_MP_SUM_F_TORSO_2_0",
        "F|1|2|DLC_MP_HEIST_F_TORSO_1_0",
        "F|1|3|DLC_MP_HEIST_F_TORSO_14_0",
        "F|1|4|DLC_MP_HEIST_F_TORSO_27_0",
        "F|1|5|DLC_MP_HEIST_F_TORSO_40_0",
        "F|1|6|DLC_MP_HEIST_F_TORSO_53_0",
        "F|1|7|DLC_MP_HEIST_F_TORSO_66_0",
        "F|1|8|DLC_MP_HEIST_F_TORSO_79_0",
        "F|1|15|DLC_MP_GR_F_TORSO_3_0",
        "F|1|16|DLC_MP_GR_F_TORSO_19_0",
        "F|1|17|DLC_MP_SUM_F_TORSO_3_0",
        "F|2|2|DLC_MP_HEIST_F_TORSO_2_0",
        "F|2|3|DLC_MP_HEIST_F_TORSO_15_0",
        "F|2|4|DLC_MP_HEIST_F_TORSO_28_0",
        "F|2|5|DLC_MP_HEIST_F_TORSO_41_0",
        "F|2|6|DLC_MP_HEIST_F_TORSO_54_0",
        "F|2|7|DLC_MP_HEIST_F_TORSO_67_0",
        "F|2|8|DLC_MP_HEIST_F_TORSO_80_0",
        "F|2|15|DLC_MP_GR_F_TORSO_4_0",
        "F|2|16|DLC_MP_GR_F_TORSO_20_0",
        "F|2|17|DLC_MP_SUM_F_TORSO_4_0",
        "F|3|0|DLC_MP_LTS_F_UPPR_0_0",
        "F|3|1|DLC_MP_LTS_F_UPPR_1_0",
        "F|3|2|DLC_MP_HEIST_F_TORSO_3_0",
        "F|3|3|DLC_MP_HEIST_F_TORSO_16_0",
        "F|3|4|DLC_MP_HEIST_F_TORSO_29_0",
        "F|3|5|DLC_MP_HEIST_F_TORSO_42_0",
        "F|3|6|DLC_MP_HEIST_F_TORSO_55_0",
        "F|3|7|DLC_MP_HEIST_F_TORSO_68_0",
        "F|3|8|DLC_MP_HEIST_F_TORSO_81_0",
        "F|3|15|DLC_MP_GR_F_TORSO_5_0",
        "F|3|16|DLC_MP_GR_F_TORSO_21_0",
        "F|3|17|DLC_MP_SUM_F_TORSO_5_0",
        "F|4|2|DLC_MP_HEIST_F_TORSO_4_0",
        "F|4|3|DLC_MP_HEIST_F_TORSO_17_0",
        "F|4|4|DLC_MP_HEIST_F_TORSO_30_0",
        "F|4|5|DLC_MP_HEIST_F_TORSO_43_0",
        "F|4|6|DLC_MP_HEIST_F_TORSO_56_0",
        "F|4|7|DLC_MP_HEIST_F_TORSO_69_0",
        "F|4|8|DLC_MP_HEIST_F_TORSO_82_0",
        "F|4|15|DLC_MP_GR_F_TORSO_6_0",
        "F|4|16|DLC_MP_GR_F_TORSO_22_0",
        "F|4|17|DLC_MP_SUM_F_TORSO_6_0",
        "F|5|2|DLC_MP_HEIST_F_TORSO_5_0",
        "F|5|3|DLC_MP_HEIST_F_TORSO_18_0",
        "F|5|4|DLC_MP_HEIST_F_TORSO_31_0",
        "F|5|5|DLC_MP_HEIST_F_TORSO_44_0",
        "F|5|6|DLC_MP_HEIST_F_TORSO_57_0",
        "F|5|7|DLC_MP_HEIST_F_TORSO_70_0",
        "F|5|8|DLC_MP_HEIST_F_TORSO_83_0",
        "F|5|15|DLC_MP_GR_F_TORSO_7_0",
        "F|5|16|DLC_MP_GR_F_TORSO_23_0",
        "F|5|17|DLC_MP_SUM_F_TORSO_7_0",
        "F|6|2|DLC_MP_HEIST_F_TORSO_6_0",
        "F|6|3|DLC_MP_HEIST_F_TORSO_19_0",
        "F|6|4|DLC_MP_HEIST_F_TORSO_32_0",
        "F|6|5|DLC_MP_HEIST_F_TORSO_45_0",
        "F|6|6|DLC_MP_HEIST_F_TORSO_58_0",
        "F|6|7|DLC_MP_HEIST_F_TORSO_71_0",
        "F|6|8|DLC_MP_HEIST_F_TORSO_84_0",
        "F|6|15|DLC_MP_GR_F_TORSO_8_0",
        "F|6|16|DLC_MP_GR_F_TORSO_24_0",
        "F|6|17|DLC_MP_SUM_F_TORSO_8_0",
        "F|7|2|DLC_MP_HEIST_F_TORSO_7_0",
        "F|7|3|DLC_MP_HEIST_F_TORSO_20_0",
        "F|7|4|DLC_MP_HEIST_F_TORSO_33_0",
        "F|7|5|DLC_MP_HEIST_F_TORSO_46_0",
        "F|7|6|DLC_MP_HEIST_F_TORSO_59_0",
        "F|7|7|DLC_MP_HEIST_F_TORSO_72_0",
        "F|7|8|DLC_MP_HEIST_F_TORSO_85_0",
        "F|7|15|DLC_MP_GR_F_TORSO_9_0",
        "F|7|16|DLC_MP_GR_F_TORSO_25_0",
        "F|7|17|DLC_MP_SUM_F_TORSO_9_0",
        "F|9|2|DLC_MP_HEIST_F_TORSO_8_0",
        "F|9|3|DLC_MP_HEIST_F_TORSO_21_0",
        "F|9|4|DLC_MP_HEIST_F_TORSO_34_0",
        "F|9|5|DLC_MP_HEIST_F_TORSO_47_0",
        "F|9|6|DLC_MP_HEIST_F_TORSO_60_0",
        "F|9|7|DLC_MP_HEIST_F_TORSO_73_0",
        "F|9|8|DLC_MP_HEIST_F_TORSO_86_0",
        "F|9|15|DLC_MP_GR_F_TORSO_10_0",
        "F|9|16|DLC_MP_GR_F_TORSO_26_0",
        "F|9|17|DLC_MP_SUM_F_TORSO_10_0",
        "F|12|2|DLC_MP_HEIST_F_TORSO_10_0",
        "F|12|3|DLC_MP_HEIST_F_TORSO_23_0",
        "F|12|4|DLC_MP_HEIST_F_TORSO_36_0",
        "F|12|5|DLC_MP_HEIST_F_TORSO_49_0",
        "F|12|6|DLC_MP_HEIST_F_TORSO_62_0",
        "F|12|7|DLC_MP_HEIST_F_TORSO_75_0",
        "F|12|8|DLC_MP_HEIST_F_TORSO_88_0",
        "F|12|15|DLC_MP_GR_F_TORSO_12_0",
        "F|12|16|DLC_MP_GR_F_TORSO_28_0",
        "F|12|17|DLC_MP_SUM_F_TORSO_12_0",
        "F|14|2|DLC_MP_HEIST_F_TORSO_11_0",
        "F|14|3|DLC_MP_HEIST_F_TORSO_24_0",
        "F|14|4|DLC_MP_HEIST_F_TORSO_37_0",
        "F|14|5|DLC_MP_HEIST_F_TORSO_50_0",
        "F|14|6|DLC_MP_HEIST_F_TORSO_63_0",
        "F|14|7|DLC_MP_HEIST_F_TORSO_76_0",
        "F|14|8|DLC_MP_HEIST_F_TORSO_89_0",
        "F|14|15|DLC_MP_GR_F_TORSO_13_0",
        "F|14|16|DLC_MP_GR_F_TORSO_29_0",
        "F|14|17|DLC_MP_SUM_F_TORSO_13_0",
        "F|15|2|DLC_MP_HEIST_F_TORSO_12_0",
        "F|15|3|DLC_MP_HEIST_F_TORSO_25_0",
        "F|15|4|DLC_MP_HEIST_F_TORSO_38_0",
        "F|15|5|DLC_MP_HEIST_F_TORSO_51_0",
        "F|15|6|DLC_MP_HEIST_F_TORSO_64_0",
        "F|15|7|DLC_MP_HEIST_F_TORSO_77_0",
        "F|15|8|DLC_MP_HEIST_F_TORSO_90_0",
        "F|15|15|DLC_MP_GR_F_TORSO_0_0",
        "F|15|16|DLC_MP_GR_F_TORSO_1_0",
        "F|15|17|DLC_MP_SUM_F_TORSO_1_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|2|DLC_MP_BIKER_F_TORSO_3_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|3|DLC_MP_BIKER_F_TORSO_4_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|4|DLC_MP_BIKER_F_TORSO_5_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|5|DLC_MP_BIKER_F_TORSO_6_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|6|DLC_MP_BIKER_F_TORSO_7_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|7|DLC_MP_BIKER_F_TORSO_8_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|8|DLC_MP_BIKER_F_TORSO_9_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|15|DLC_MP_GR_F_TORSO_14_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|16|DLC_MP_GR_F_TORSO_30_0",
        "F|DLC_MP_BIKER_F_TORSO_0_0|17|DLC_MP_SUM_F_TORSO_14_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|2|DLC_MP_BIKER_F_TORSO_10_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|3|DLC_MP_BIKER_F_TORSO_11_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|4|DLC_MP_BIKER_F_TORSO_12_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|5|DLC_MP_BIKER_F_TORSO_13_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|6|DLC_MP_BIKER_F_TORSO_14_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|7|DLC_MP_BIKER_F_TORSO_15_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|8|DLC_MP_BIKER_F_TORSO_16_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|15|DLC_MP_GR_F_TORSO_15_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|16|DLC_MP_GR_F_TORSO_31_0",
        "F|DLC_MP_BIKER_F_TORSO_1_0|17|DLC_MP_SUM_F_TORSO_15_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|2|DLC_MP_BIKER_F_TORSO_17_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|3|DLC_MP_BIKER_F_TORSO_18_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|4|DLC_MP_BIKER_F_TORSO_19_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|5|DLC_MP_BIKER_F_TORSO_20_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|6|DLC_MP_BIKER_F_TORSO_21_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|7|DLC_MP_BIKER_F_TORSO_22_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|8|DLC_MP_BIKER_F_TORSO_23_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|15|DLC_MP_GR_F_TORSO_16_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|16|DLC_MP_GR_F_TORSO_32_0",
        "F|DLC_MP_BIKER_F_TORSO_2_0|17|DLC_MP_SUM_F_TORSO_16_0",
        "F|DLC_MP_IE_F_TORSO_0_0|2|DLC_MP_IE_F_TORSO_1_0",
        "F|DLC_MP_IE_F_TORSO_0_0|3|DLC_MP_IE_F_TORSO_2_0",
        "F|DLC_MP_IE_F_TORSO_0_0|4|DLC_MP_IE_F_TORSO_3_0",
        "F|DLC_MP_IE_F_TORSO_0_0|5|DLC_MP_IE_F_TORSO_4_0",
        "F|DLC_MP_IE_F_TORSO_0_0|6|DLC_MP_IE_F_TORSO_5_0",
        "F|DLC_MP_IE_F_TORSO_0_0|7|DLC_MP_IE_F_TORSO_6_0",
        "F|DLC_MP_IE_F_TORSO_0_0|8|DLC_MP_IE_F_TORSO_7_0",
        "F|DLC_MP_IE_F_TORSO_0_0|15|DLC_MP_GR_F_TORSO_17_0",
        "F|DLC_MP_IE_F_TORSO_0_0|16|DLC_MP_GR_F_TORSO_33_0",
        "F|DLC_MP_IE_F_TORSO_0_0|17|DLC_MP_SUM_F_TORSO_17_0",
        "F|DLC_MP_IE_F_TORSO_8_0|2|DLC_MP_IE_F_TORSO_9_0",
        "F|DLC_MP_IE_F_TORSO_8_0|3|DLC_MP_IE_F_TORSO_10_0",
        "F|DLC_MP_IE_F_TORSO_8_0|4|DLC_MP_IE_F_TORSO_11_0",
        "F|DLC_MP_IE_F_TORSO_8_0|5|DLC_MP_IE_F_TORSO_12_0",
        "F|DLC_MP_IE_F_TORSO_8_0|6|DLC_MP_IE_F_TORSO_13_0",
        "F|DLC_MP_IE_F_TORSO_8_0|7|DLC_MP_IE_F_TORSO_14_0",
        "F|DLC_MP_IE_F_TORSO_8_0|8|DLC_MP_IE_F_TORSO_15_0",
        "F|DLC_MP_IE_F_TORSO_8_0|15|DLC_MP_GR_F_TORSO_34_0",
        "F|DLC_MP_IE_F_TORSO_8_0|16|DLC_MP_GR_F_TORSO_35_0",
        "F|DLC_MP_IE_F_TORSO_8_0|17|DLC_MP_SUM_F_TORSO_18_0",
        "F|DLC_MP_H4_F_TORSO_0_0|2|DLC_MP_H4_F_TORSO_1_0",
        "F|DLC_MP_H4_F_TORSO_0_0|3|DLC_MP_H4_F_TORSO_2_0",
        "F|DLC_MP_H4_F_TORSO_0_0|4|DLC_MP_H4_F_TORSO_3_0",
        "F|DLC_MP_H4_F_TORSO_0_0|5|DLC_MP_H4_F_TORSO_4_0",
        "F|DLC_MP_H4_F_TORSO_0_0|6|DLC_MP_H4_F_TORSO_5_0",
        "F|DLC_MP_H4_F_TORSO_0_0|7|DLC_MP_H4_F_TORSO_6_0",
        "F|DLC_MP_H4_F_TORSO_0_0|8|DLC_MP_H4_F_TORSO_7_0",
        "F|DLC_MP_H4_F_TORSO_0_0|15|DLC_MP_H4_F_TORSO_8_0",
        "F|DLC_MP_H4_F_TORSO_0_0|16|DLC_MP_H4_F_TORSO_9_0",
        "F|DLC_MP_H4_F_TORSO_0_0|17|DLC_MP_H4_F_TORSO_10_0",
        "M|0|2|DLC_MP_HEIST_M_TORSO_0_0",
        "M|0|3|DLC_MP_HEIST_M_TORSO_11_0",
        "M|0|4|DLC_MP_HEIST_M_TORSO_22_0",
        "M|0|5|DLC_MP_HEIST_M_TORSO_33_0",
        "M|0|6|DLC_MP_HEIST_M_TORSO_44_0",
        "M|0|7|DLC_MP_HEIST_M_TORSO_55_0",
        "M|0|8|DLC_MP_HEIST_M_TORSO_66_0",
        "M|0|9|DLC_MP_GR_M_TORSO_2_0",
        "M|0|10|DLC_MP_GR_M_TORSO_15_0",
        "M|0|11|DLC_MP_SUM_M_TORSO_2_0",
        "M|1|2|DLC_MP_HEIST_M_TORSO_1_0",
        "M|1|3|DLC_MP_HEIST_M_TORSO_12_0",
        "M|1|4|DLC_MP_HEIST_M_TORSO_23_0",
        "M|1|5|DLC_MP_HEIST_M_TORSO_34_0",
        "M|1|6|DLC_MP_HEIST_M_TORSO_45_0",
        "M|1|7|DLC_MP_HEIST_M_TORSO_56_0",
        "M|1|8|DLC_MP_HEIST_M_TORSO_67_0",
        "M|1|9|DLC_MP_GR_M_TORSO_3_0",
        "M|1|10|DLC_MP_GR_M_TORSO_16_0",
        "M|1|11|DLC_MP_SUM_M_TORSO_3_0",
        "M|2|2|DLC_MP_HEIST_M_TORSO_2_0",
        "M|2|3|DLC_MP_HEIST_M_TORSO_13_0",
        "M|2|4|DLC_MP_HEIST_M_TORSO_24_0",
        "M|2|5|DLC_MP_HEIST_M_TORSO_35_0",
        "M|2|6|DLC_MP_HEIST_M_TORSO_46_0",
        "M|2|7|DLC_MP_HEIST_M_TORSO_57_0",
        "M|2|8|DLC_MP_HEIST_M_TORSO_68_0",
        "M|2|9|DLC_MP_GR_M_TORSO_4_0",
        "M|2|10|DLC_MP_GR_M_TORSO_17_0",
        "M|2|11|DLC_MP_SUM_M_TORSO_4_0",
        "M|4|0|DLC_MP_LTS_M_UPPR_0_0",
        "M|4|1|DLC_MP_LTS_M_UPPR_1_0",
        "M|4|2|DLC_MP_HEIST_M_TORSO_3_0",
        "M|4|3|DLC_MP_HEIST_M_TORSO_14_0",
        "M|4|4|DLC_MP_HEIST_M_TORSO_25_0",
        "M|4|5|DLC_MP_HEIST_M_TORSO_36_0",
        "M|4|6|DLC_MP_HEIST_M_TORSO_47_0",
        "M|4|7|DLC_MP_HEIST_M_TORSO_58_0",
        "M|4|8|DLC_MP_HEIST_M_TORSO_69_0",
        "M|4|9|DLC_MP_GR_M_TORSO_5_0",
        "M|4|10|DLC_MP_GR_M_TORSO_18_0",
        "M|4|11|DLC_MP_SUM_M_TORSO_5_0",
        "M|5|2|DLC_MP_HEIST_M_TORSO_4_0",
        "M|5|3|DLC_MP_HEIST_M_TORSO_15_0",
        "M|5|4|DLC_MP_HEIST_M_TORSO_26_0",
        "M|5|5|DLC_MP_HEIST_M_TORSO_37_0",
        "M|5|6|DLC_MP_HEIST_M_TORSO_48_0",
        "M|5|7|DLC_MP_HEIST_M_TORSO_59_0",
        "M|5|8|DLC_MP_HEIST_M_TORSO_70_0",
        "M|5|9|DLC_MP_GR_M_TORSO_6_0",
        "M|5|10|DLC_MP_GR_M_TORSO_19_0",
        "M|5|11|DLC_MP_SUM_M_TORSO_6_0",
        "M|6|2|DLC_MP_HEIST_M_TORSO_5_0",
        "M|6|3|DLC_MP_HEIST_M_TORSO_16_0",
        "M|6|4|DLC_MP_HEIST_M_TORSO_27_0",
        "M|6|5|DLC_MP_HEIST_M_TORSO_38_0",
        "M|6|6|DLC_MP_HEIST_M_TORSO_49_0",
        "M|6|7|DLC_MP_HEIST_M_TORSO_60_0",
        "M|6|8|DLC_MP_HEIST_M_TORSO_71_0",
        "M|6|9|DLC_MP_GR_M_TORSO_7_0",
        "M|6|10|DLC_MP_GR_M_TORSO_20_0",
        "M|6|11|DLC_MP_SUM_M_TORSO_7_0",
        "M|8|2|DLC_MP_HEIST_M_TORSO_6_0",
        "M|8|3|DLC_MP_HEIST_M_TORSO_17_0",
        "M|8|4|DLC_MP_HEIST_M_TORSO_28_0",
        "M|8|5|DLC_MP_HEIST_M_TORSO_39_0",
        "M|8|6|DLC_MP_HEIST_M_TORSO_50_0",
        "M|8|7|DLC_MP_HEIST_M_TORSO_61_0",
        "M|8|8|DLC_MP_HEIST_M_TORSO_72_0",
        "M|8|9|DLC_MP_GR_M_TORSO_8_0",
        "M|8|10|DLC_MP_GR_M_TORSO_21_0",
        "M|8|11|DLC_MP_SUM_M_TORSO_8_0",
        "M|11|2|DLC_MP_HEIST_M_TORSO_7_0",
        "M|11|3|DLC_MP_HEIST_M_TORSO_18_0",
        "M|11|4|DLC_MP_HEIST_M_TORSO_29_0",
        "M|11|5|DLC_MP_HEIST_M_TORSO_40_0",
        "M|11|6|DLC_MP_HEIST_M_TORSO_51_0",
        "M|11|7|DLC_MP_HEIST_M_TORSO_62_0",
        "M|11|8|DLC_MP_HEIST_M_TORSO_73_0",
        "M|11|9|DLC_MP_GR_M_TORSO_9_0",
        "M|11|10|DLC_MP_GR_M_TORSO_22_0",
        "M|11|11|DLC_MP_SUM_M_TORSO_9_0",
        "M|12|2|DLC_MP_HEIST_M_TORSO_8_0",
        "M|12|3|DLC_MP_HEIST_M_TORSO_19_0",
        "M|12|4|DLC_MP_HEIST_M_TORSO_30_0",
        "M|12|5|DLC_MP_HEIST_M_TORSO_41_0",
        "M|12|6|DLC_MP_HEIST_M_TORSO_52_0",
        "M|12|7|DLC_MP_HEIST_M_TORSO_63_0",
        "M|12|8|DLC_MP_HEIST_M_TORSO_74_0",
        "M|12|9|DLC_MP_GR_M_TORSO_10_0",
        "M|12|10|DLC_MP_GR_M_TORSO_23_0",
        "M|12|11|DLC_MP_SUM_M_TORSO_10_0",
        "M|14|2|DLC_MP_HEIST_M_TORSO_9_0",
        "M|14|3|DLC_MP_HEIST_M_TORSO_20_0",
        "M|14|4|DLC_MP_HEIST_M_TORSO_31_0",
        "M|14|5|DLC_MP_HEIST_M_TORSO_42_0",
        "M|14|6|DLC_MP_HEIST_M_TORSO_53_0",
        "M|14|7|DLC_MP_HEIST_M_TORSO_64_0",
        "M|14|8|DLC_MP_HEIST_M_TORSO_75_0",
        "M|14|9|DLC_MP_GR_M_TORSO_11_0",
        "M|14|10|DLC_MP_GR_M_TORSO_24_0",
        "M|14|11|DLC_MP_SUM_M_TORSO_11_0",
        "M|15|2|DLC_MP_HEIST_M_TORSO_10_0",
        "M|15|3|DLC_MP_HEIST_M_TORSO_21_0",
        "M|15|4|DLC_MP_HEIST_M_TORSO_32_0",
        "M|15|5|DLC_MP_HEIST_M_TORSO_43_0",
        "M|15|6|DLC_MP_HEIST_M_TORSO_54_0",
        "M|15|7|DLC_MP_HEIST_M_TORSO_65_0",
        "M|15|8|DLC_MP_HEIST_M_TORSO_76_0",
        "M|15|9|DLC_MP_GR_M_TORSO_0_0",
        "M|15|10|DLC_MP_GR_M_TORSO_1_0",
        "M|15|11|DLC_MP_SUM_M_TORSO_1_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|2|DLC_MP_BIKER_M_TORSO_3_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|3|DLC_MP_BIKER_M_TORSO_4_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|4|DLC_MP_BIKER_M_TORSO_5_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|5|DLC_MP_BIKER_M_TORSO_6_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|6|DLC_MP_BIKER_M_TORSO_7_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|7|DLC_MP_BIKER_M_TORSO_8_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|8|DLC_MP_BIKER_M_TORSO_9_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|9|DLC_MP_GR_M_TORSO_12_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|10|DLC_MP_GR_M_TORSO_25_0",
        "M|DLC_MP_BIKER_M_TORSO_0_0|11|DLC_MP_SUM_M_TORSO_12_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|2|DLC_MP_BIKER_M_TORSO_10_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|3|DLC_MP_BIKER_M_TORSO_11_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|4|DLC_MP_BIKER_M_TORSO_12_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|5|DLC_MP_BIKER_M_TORSO_13_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|6|DLC_MP_BIKER_M_TORSO_14_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|7|DLC_MP_BIKER_M_TORSO_15_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|8|DLC_MP_BIKER_M_TORSO_16_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|9|DLC_MP_GR_M_TORSO_13_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|10|DLC_MP_GR_M_TORSO_26_0",
        "M|DLC_MP_BIKER_M_TORSO_1_0|11|DLC_MP_SUM_M_TORSO_13_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|2|DLC_MP_BIKER_M_TORSO_17_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|3|DLC_MP_BIKER_M_TORSO_18_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|4|DLC_MP_BIKER_M_TORSO_19_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|5|DLC_MP_BIKER_M_TORSO_20_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|6|DLC_MP_BIKER_M_TORSO_21_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|7|DLC_MP_BIKER_M_TORSO_22_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|8|DLC_MP_BIKER_M_TORSO_23_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|9|DLC_MP_GR_M_TORSO_14_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|10|DLC_MP_GR_M_TORSO_14_0",
        "M|DLC_MP_BIKER_M_TORSO_2_0|11|DLC_MP_SUM_M_TORSO_14_0",
        "M|DLC_MP_H4_M_TORSO_0_0|2|DLC_MP_H4_M_TORSO_1_0",
        "M|DLC_MP_H4_M_TORSO_0_0|3|DLC_MP_H4_M_TORSO_2_0",
        "M|DLC_MP_H4_M_TORSO_0_0|4|DLC_MP_H4_M_TORSO_3_0",
        "M|DLC_MP_H4_M_TORSO_0_0|5|DLC_MP_H4_M_TORSO_4_0",
        "M|DLC_MP_H4_M_TORSO_0_0|6|DLC_MP_H4_M_TORSO_5_0",
        "M|DLC_MP_H4_M_TORSO_0_0|7|DLC_MP_H4_M_TORSO_6_0",
        "M|DLC_MP_H4_M_TORSO_0_0|8|DLC_MP_H4_M_TORSO_7_0",
        "M|DLC_MP_H4_M_TORSO_0_0|9|DLC_MP_H4_M_TORSO_8_0",
        "M|DLC_MP_H4_M_TORSO_0_0|10|DLC_MP_H4_M_TORSO_9_0",
        "M|DLC_MP_H4_M_TORSO_0_0|11|DLC_MP_H4_M_TORSO_10_0",
    ];

    private static readonly Dictionary<int, uint> GloveItemForBaseTorso = [];

    private static readonly Dictionary<uint, int> BaseTorsoForGloveItem = [];

    private static uint _resolvedModel;

    private static bool _resolved;

    internal static uint GloveItemFor(int ped, bool male, int baseTorso, int gloveType)
    {
        Resolve(ped);

        return GloveItemForBaseTorso.TryGetValue(Key(male, baseTorso, gloveType), out var item) ? item : 0;
    }

    internal static bool Worn(int ped, uint torsoItem, out int baseTorso, out int gloveType)
    {
        baseTorso = -1;
        gloveType = -1;

        if (!TorsoItems.IsRealItem(torsoItem))
        {
            return false;
        }

        Resolve(ped);

        if (!BaseTorsoForGloveItem.TryGetValue(torsoItem, out var key))
        {
            return false;
        }

        baseTorso = (key >> 8) & 0xFFFF;
        gloveType = key & 0xFF;

        return true;
    }

    internal static void Forget()
    {
        GloveItemForBaseTorso.Clear();
        BaseTorsoForGloveItem.Clear();
        _resolvedModel = 0;
        _resolved = false;
    }

    private static int Key(bool male, int baseTorso, int gloveType) =>
        ((male ? 1 : 0) << 24) | ((baseTorso & 0xFFFF) << 8) | (gloveType & 0xFF);

    private static void Resolve(int ped)
    {
        var model = (uint)Native.GetEntityModel(ped);

        if (_resolved && model == _resolvedModel)
        {
            return;
        }

        GloveItemForBaseTorso.Clear();
        BaseTorsoForGloveItem.Clear();

        foreach (var row in Rows)
        {
            var parts = row.Split(Separator);
            var male = parts[0] == Male;

            if (BaseTorsoDrawable(ped, parts[1]) is not { } baseTorso)
            {
                continue;
            }

            if (TorsoItems.DrawableOfNamed(ped, PedComponentSlots.Torso, parts[3]) is null)
            {
                continue;
            }

            var gloveType = int.Parse(parts[2]);
            var gloveItem = TorsoItems.HashOfName(parts[3]);
            var key = Key(male, baseTorso, gloveType);

            GloveItemForBaseTorso[key] = gloveItem;
            BaseTorsoForGloveItem[gloveItem] = key;
        }

        _resolvedModel = model;
        _resolved = true;
    }

    private static int? BaseTorsoDrawable(int ped, string outerKey) =>
        int.TryParse(outerKey, out var drawable)
            ? drawable
            : TorsoItems.DrawableOfNamed(ped, PedComponentSlots.Torso, outerKey);
}
