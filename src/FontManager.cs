using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace DragNWashLocalization
{
    public class FontManager
    {
        private static TMP_FontAsset? _uiFontAsset;
        private static TMP_FontAsset? _cartoonFontAsset;
        private const string PreloadCharacters = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюяІіЇїЄєҐґABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,!?:;\"'()-+=/\\%*#@<>[]{}~–—«»… №";

        public static TMP_FontAsset? GetOrCreateUIFontAsset()
        {
            if (_uiFontAsset != null)
            {
                return _uiFontAsset;
            }

            try
            {
                string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                string[] candidates = new string[]
                {
                    Path.Combine(fontsFolder, "segoeuib.ttf"),
                    Path.Combine(fontsFolder, "arialbd.ttf"),
                    Path.Combine(fontsFolder, "segoeui.ttf"),
                    Path.Combine(fontsFolder, "arial.ttf")
                };

                foreach (string path in candidates)
                {
                    if (File.Exists(path))
                    {
                        Font osFont = new Font(path);
                        _uiFontAsset = TMP_FontAsset.CreateFontAsset(osFont, 48, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
                        if (_uiFontAsset != null)
                        {
                            _uiFontAsset.name = "Game_UI_Font";
                            _uiFontAsset.boldStyle = 0f;
                            _uiFontAsset.boldSpacing = 0f;
                            if (_uiFontAsset.material != null)
                            {
                                _uiFontAsset.material.SetFloat(ShaderUtilities.ID_WeightBold, 0f);
                                _uiFontAsset.material.SetFloat(ShaderUtilities.ID_WeightNormal, 0f);
                                _uiFontAsset.material.SetFloat(ShaderUtilities.ID_FaceDilate, 0f);
                            }
                            _uiFontAsset.TryAddCharacters(PreloadCharacters, true);
                            UnityEngine.Object.DontDestroyOnLoad(_uiFontAsset);
                            return _uiFontAsset;
                        }
                    }
                }

                Font dynamicFont = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI", "Arial" }, 48);
                if (dynamicFont != null)
                {
                    _uiFontAsset = TMP_FontAsset.CreateFontAsset(dynamicFont, 48, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
                    if (_uiFontAsset != null)
                    {
                        _uiFontAsset.name = "Game_UI_Dynamic";
                        _uiFontAsset.boldStyle = 0f;
                        _uiFontAsset.boldSpacing = 0f;
                        if (_uiFontAsset.material != null)
                        {
                            _uiFontAsset.material.SetFloat(ShaderUtilities.ID_WeightBold, 0f);
                            _uiFontAsset.material.SetFloat(ShaderUtilities.ID_WeightNormal, 0f);
                            _uiFontAsset.material.SetFloat(ShaderUtilities.ID_FaceDilate, 0f);
                        }
                        _uiFontAsset.TryAddCharacters(PreloadCharacters, true);
                        UnityEngine.Object.DontDestroyOnLoad(_uiFontAsset);
                        return _uiFontAsset;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Failed to initialize UI font: " + ex.Message);
            }

            return GetOrCreateFontAsset();
        }

        public static TMP_FontAsset? GetOrCreateFontAsset()
        {
            if (_cartoonFontAsset != null)
            {
                return _cartoonFontAsset;
            }

            try
            {
                string bundledPath = Path.Combine(LanguageManager.DataDirectory, "fonts", "font.ttf");
                if (File.Exists(bundledPath))
                {
                    Font loadedFont = new Font(bundledPath);
                    _cartoonFontAsset = TMP_FontAsset.CreateFontAsset(loadedFont, 56, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
                    if (_cartoonFontAsset != null)
                    {
                        _cartoonFontAsset.name = "Game_Cartoon_Font";
                        _cartoonFontAsset.boldStyle = 0f;
                        _cartoonFontAsset.boldSpacing = 0f;
                        if (_cartoonFontAsset.material != null)
                        {
                            _cartoonFontAsset.material.SetFloat(ShaderUtilities.ID_WeightBold, 0f);
                            _cartoonFontAsset.material.SetFloat(ShaderUtilities.ID_WeightNormal, 0f);
                            _cartoonFontAsset.material.SetFloat(ShaderUtilities.ID_FaceDilate, 0f);
                        }
                        _cartoonFontAsset.TryAddCharacters(PreloadCharacters, true);
                        UnityEngine.Object.DontDestroyOnLoad(_cartoonFontAsset);
                        return _cartoonFontAsset;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Failed to initialize cartoon font: " + ex.Message);
            }

            return _uiFontAsset;
        }

        public static void ApplyToAllFonts()
        {
            TMP_FontAsset? cartoonFont = GetOrCreateFontAsset();
            TMP_FontAsset? uiFont = GetOrCreateUIFontAsset();

            if (cartoonFont == null && uiFont == null)
            {
                return;
            }

            TMP_FontAsset primaryFallback = cartoonFont ?? uiFont!;
            TMP_FontAsset? secondaryFallback = (primaryFallback == cartoonFont) ? uiFont : null;

            TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (TMP_FontAsset font in allFonts)
            {
                if (font == uiFont || font == cartoonFont)
                {
                    continue;
                }

                if (font.fallbackFontAssetTable == null)
                {
                    font.fallbackFontAssetTable = new List<TMP_FontAsset>();
                }

                if (secondaryFallback != null && !font.fallbackFontAssetTable.Contains(secondaryFallback))
                {
                    font.fallbackFontAssetTable.Add(secondaryFallback);
                }

                if (font.fallbackFontAssetTable.Contains(primaryFallback))
                {
                    font.fallbackFontAssetTable.Remove(primaryFallback);
                }
                font.fallbackFontAssetTable.Insert(0, primaryFallback);
            }

            TMP_Text[] allTexts = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text text in allTexts)
            {
                if (text == null)
                {
                    continue;
                }

                if (text.font != null && text.font != uiFont && text.font != cartoonFont)
                {
                    if (text.font.fallbackFontAssetTable == null)
                    {
                        text.font.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    }

                    if (secondaryFallback != null && !text.font.fallbackFontAssetTable.Contains(secondaryFallback))
                    {
                        text.font.fallbackFontAssetTable.Add(secondaryFallback);
                    }

                    if (text.font.fallbackFontAssetTable.Contains(primaryFallback))
                    {
                        text.font.fallbackFontAssetTable.Remove(primaryFallback);
                    }
                    text.font.fallbackFontAssetTable.Insert(0, primaryFallback);
                }

                text.SetAllDirty();
            }
        }
    }
}
