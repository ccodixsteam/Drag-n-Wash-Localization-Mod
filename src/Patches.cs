using System;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace DragNWashLocalization
{
    [HarmonyPatch(typeof(GameStateManager), "Awake")]
    public static class GameStateManagerPatch
    {
        public static void Postfix()
        {
            LocalizationController.Install();
        }
    }

    [HarmonyPatch(typeof(MenuManager), "RegisterMenu")]
    public static class MenuManagerPatch
    {
        public static void Postfix()
        {
            LocalizationController.Install();
        }
    }

    [HarmonyPatch(typeof(Menu), "OnShow")]
    public static class MenuOnShowPatch
    {
        public static void Postfix(Menu __instance)
        {
            LocalizationController.Install();
            bool isMain = __instance is MenuMain;
            LanguageSelector.SetVisible(isMain);
            MenuCredits.SetVisible(isMain);
            if (isMain)
            {
                LanguageSelector.EnsureSelectorOnMenu((MenuMain)__instance);
                MenuCredits.EnsureCreditOnMenu();
            }
            LocalizationController.ApplyCurrentLocalization();
        }
    }

    [HarmonyPatch]
    public static class DecalProjectorPatch
    {
        public static MethodBase? TargetMethod()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? t = asm.GetType("UnityEngine.Rendering.Universal.DecalProjector");
                if (t != null)
                {
                    return t.GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?? t.GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }
            return null;
        }

        public static void Postfix(MonoBehaviour __instance)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English)
            {
                TextureManager.UpdateDecalProjector(__instance, LanguageManager.CurrentLanguage);
            }
        }
    }

    [HarmonyPatch]
    public static class WalkNWashDirtyDecalPatch
    {
        public static MethodBase? TargetMethod()
        {
            Type? t = AccessTools.TypeByName("WalkNWashDirtyDecal") ?? Type.GetType("WalkNWashDirtyDecal, Assembly-CSharp");
            if (t != null)
            {
                return AccessTools.Method(t, "Start") ?? AccessTools.Method(t, "Awake");
            }
            return null;
        }

        public static void Prefix(MonoBehaviour __instance)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English)
            {
                TextureManager.UpdateSingleDirtyDecal(__instance, LanguageManager.CurrentLanguage);
            }
        }
    }

    [HarmonyPatch(typeof(TMP_Text), "text", MethodType.Setter)]
    public static class TMPTextSetterPatch
    {
        public static void Prefix(TMP_Text __instance, ref string value)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English && !string.IsNullOrEmpty(value))
            {
                if (UIManager.IsVersionText(__instance))
                {
                    return;
                }
                if (UIManager.TryTranslate(value, LanguageManager.CurrentLanguage, out string translated))
                {
                    value = translated;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TMP_Text), "SetText", new Type[] { typeof(string) })]
    public static class TMPTextSetTextSinglePatch
    {
        public static void Prefix(TMP_Text __instance, ref string sourceText)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English && !string.IsNullOrEmpty(sourceText))
            {
                if (UIManager.IsVersionText(__instance))
                {
                    return;
                }
                if (UIManager.TryTranslate(sourceText, LanguageManager.CurrentLanguage, out string translated))
                {
                    sourceText = translated;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TMP_Text), "SetText", new Type[] { typeof(string), typeof(bool) })]
    public static class TMPTextSetTextSyncPatch
    {
        public static void Prefix(TMP_Text __instance, ref string sourceText)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English && !string.IsNullOrEmpty(sourceText))
            {
                if (UIManager.IsVersionText(__instance))
                {
                    return;
                }
                if (UIManager.TryTranslate(sourceText, LanguageManager.CurrentLanguage, out string translated))
                {
                    sourceText = translated;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TMP_Text), "OnEnable")]
    public static class TMPTextOnEnablePatch
    {
        public static void Postfix(TMP_Text __instance)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English)
            {
                UIManager.TranslateTmpText(__instance, LanguageManager.CurrentLanguage);
            }
        }
    }

    [HarmonyPatch]
    public static class SettingGetLabelPatch
    {
        public static MethodBase? TargetMethod()
        {
            Type? t = Type.GetType("UnityScriptableSettings.Setting, Naelstrof.UnityScriptableSettings");
            return t?.GetMethod("GetLabel", BindingFlags.Public | BindingFlags.Instance);
        }

        public static void Postfix(ref string __result)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English && !string.IsNullOrEmpty(__result))
            {
                if (UIManager.TryTranslate(__result, LanguageManager.CurrentLanguage, out string translated))
                {
                    __result = translated;
                }
            }
        }
    }

    [HarmonyPatch]
    public static class SettingGroupGetLabelPatch
    {
        public static MethodBase? TargetMethod()
        {
            Type? t = Type.GetType("UnityScriptableSettings.SettingGroup, Naelstrof.UnityScriptableSettings");
            return t?.GetMethod("GetLabel", BindingFlags.Public | BindingFlags.Instance);
        }

        public static void Postfix(ref string __result)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English && !string.IsNullOrEmpty(__result))
            {
                if (UIManager.TryTranslate(__result, LanguageManager.CurrentLanguage, out string translated))
                {
                    __result = translated;
                }
            }
        }
    }

    [HarmonyPatch(typeof(WalkNWashDragonDescriptor), "Start")]
    public static class DragonDescriptorStartPatch
    {
        public static void Prefix(WalkNWashDragonDescriptor __instance)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English)
            {
                TextureManager.PrepareDragonAuxDirtyMap(__instance, LanguageManager.CurrentLanguage);
            }
        }

        public static void Postfix(WalkNWashDragonDescriptor __instance)
        {
            if (LanguageManager.CurrentLanguage != GameLanguage.English)
            {
                TextureManager.UpdateDragonMaterials(LanguageManager.CurrentLanguage);
            }
        }
    }
}
