using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Il2CppSystem;
using HarmonyLib;
using UnhollowerBaseLib;
using Assets.CoreScripts;

namespace TheOtherRoles {
    public class CustomColors {
        protected static Dictionary<int, string> ColorStrings = new Dictionary<int, string>();
        private static List<int> lighterColors = new List<int>();
        public static void Load() {
            lighterColors = (List<int>) typeof(Helpers).GetField("lighterColors", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null); // Get shared Reference

            List<StringNames> longlist = Enumerable.ToList<StringNames>(BLMBFIODBKL.ALHCFFDHINL); // Palette.ColorNames
            List<StringNames> shortlist = Enumerable.ToList<StringNames>(BLMBFIODBKL.MBDDHJCCLBP); // Palette.ShortColorNames
            List<Color32> colorlist = Enumerable.ToList<Color32>(BLMBFIODBKL.AEDCMKGJKAG); // Palette.PlayerColors
            List<Color32> shadowlist = Enumerable.ToList<Color32>(BLMBFIODBKL.PHFOPNDOEMD); // Palette.ShadowColors

            List<CustomColor> colors = new List<CustomColor>();

            colors.Add(new CustomColor { longname = "Salmon", shortname = "SALMN", 
                                        color = new Color32(239, 191, 192, byte.MaxValue), 
                                        shadow = new Color32(182, 119, 114, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Bordeaux", shortname = "BRDX", 
                                        color = new Color32(109, 7, 26, byte.MaxValue), 
                                        shadow = new Color32(54, 2, 11, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Olive", shortname = "OLIVE", 
                                        color = new Color32(154, 140, 61, byte.MaxValue), 
                                        shadow = new Color32(104, 95, 40, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Turqoise", shortname = "TURQ", 
                                        color = new Color32(22, 132, 176, byte.MaxValue), 
                                        shadow = new Color32(15, 89, 117, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Mint", shortname = "MINT", 
                                        color = new Color32(111, 192, 156, byte.MaxValue), 
                                        shadow = new Color32(65, 148, 111, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Lavender", shortname = "LVNDR", 
                                        color = new Color32(173, 126, 201, byte.MaxValue), 
                                        shadow = new Color32(131, 58, 203, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Nougat", shortname = "NOUGT", 
                                        color = new Color32(160, 101, 56, byte.MaxValue), 
                                        shadow = new Color32(115, 15, 78, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Peach", shortname = "PEACH", 
                                        color = new Color32(255, 164, 119, byte.MaxValue), 
                                        shadow = new Color32(238, 128, 100, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Neon Green", shortname = "NEON", // CHANGE THIS
                                        color = new Color32(51, 255, 119, byte.MaxValue), 
                                        shadow = new Color32(0, 234, 77, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Hot Pink", shortname = "HTPNK", 
                                        color = new Color32(255, 51, 102, byte.MaxValue), 
                                        shadow = new Color32(232, 0, 58, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Gray", shortname = "GRAY", 
                                        color = new Color32(147, 147, 147, byte.MaxValue), 
                                        shadow = new Color32(120, 120, 120, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Petrol", shortname = "PTRL", 
                                        color = new Color32(0, 99, 105, byte.MaxValue), 
                                        shadow = new Color32(0, 61, 54, byte.MaxValue),
                                        isLighterColor = false });
                                        
            int id = 50000;
            foreach (CustomColor cc in colors) {
                longlist.Add((StringNames)id);
                CustomColors.ColorStrings[id++] = cc.longname;
                shortlist.Add((StringNames)id);
                CustomColors.ColorStrings[id++] = cc.shortname;
                colorlist.Add(cc.color);
                shadowlist.Add(cc.shadow);
                if (cc.isLighterColor)
                    lighterColors.Add(colorlist.Count - 1);
            }

            BLMBFIODBKL.MBDDHJCCLBP = shortlist.ToArray();
            BLMBFIODBKL.ALHCFFDHINL = longlist.ToArray();
            BLMBFIODBKL.AEDCMKGJKAG = colorlist.ToArray();
            BLMBFIODBKL.PHFOPNDOEMD = shadowlist.ToArray();
            MedScanMinigame.ALHCFFDHINL = BLMBFIODBKL.ALHCFFDHINL; // MedScanMinigame.ColorNames = Palette.ColorNames
            Telemetry.ALHCFFDHINL = BLMBFIODBKL.ALHCFFDHINL; // Telemetry.ColorNames = Palette.ColorNames
        }

        protected internal struct CustomColor {
            public string longname;
            public string shortname;
            public Color32 color;
            public Color32 shadow;
            public bool isLighterColor;
        }

        [HarmonyPatch]
        public static class CustomColorPatches {
            [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]
            private class ColorStringPatch {
                public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name) {
                    if ((int)name >= 50000) {
                        string text = CustomColors.ColorStrings[(int)name];
                        if (text != null) {
                            __result = text;
                            return false;
                        }
                    }
                    return true;
                }
            }
            [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
            private static class PlayerTabEnablePatch {
                public static void Postfix(PlayerTab __instance) { // Replace instead
                    Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();
                    int cols = 4; // TODO: Design an algorithm to dynamically position chips to optimally fill space
                    for (int i = 0; i < chips.Length; i++) {
                        ColorChip chip = chips[i];
                        int row = i / cols, col = i % cols; // Dynamically do the positioning
                        chip.transform.localPosition = new Vector3(1.46f + (col * 0.6f), -0.43f - (row * 0.55f), chip.transform.localPosition.z);
                        chip.transform.localScale *= 0.9f;
                    }
                }
            }
        }
    }
}