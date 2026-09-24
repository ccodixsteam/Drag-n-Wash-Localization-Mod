using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace DragNWashLocalization
{
    public static class TextureManager
    {
        private static readonly HashSet<string> _trackedTextures = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Dragon_Rig_Bake_Low_Ryan_Dirty_Writing_Color",
            "PressWhenDone",
            "Reception",
            "Request",
            "ScrubAndRinse",
            "SingleCarryOnly",
            "Touch",
            "TryCrouch",
            "Uppies",
            "LookinCute",
            "MenuPlaques",
            "MenuPlaques0002",
            "MenuButtons0001",
            "MenuButtons0002",
            "MenuButtons0003",
            "MenuButtons0004",
            "MenuButtons0005",
            "MenuButtons0006",
            "MenuButtons0007",
            "MenuButtons0008",
            "MenuButtons0009",
            "MenuButtons0010",
            "MenuButtons0011",
            "MenuButtons0012",
            "MenuButtons0013",
            "MenuButtons0014",
            "MenuButtons0015",
            "MenuButtons0016",
            "MenuButtons0017",
            "MenuButtons0018",
            "MenuButtons0019",
            "MenuButtons0020",
            "MenuButtons0021",
            "MenuButtons0022",
            "MenuButtons0023",
            "MenuButtons0024",
            "MenuButtons0025",
            "MenuButtons0026",
            "MenuButtons0027",
            "MenuButtons0028",
            "MenuButtons0029",
            "MenuButtons0030",
            "MenuButtons0031",
            "MenuButtons0032",
            "Hi",
            "LoadingExport0001",
            "LoadingExport0002",
            "LoadingExport0003",
            "LoadingExport0004"
        };

        private static readonly Dictionary<GameLanguage, Dictionary<string, Texture2D>> _languageTextures =
            new Dictionary<GameLanguage, Dictionary<string, Texture2D>>();

        private static readonly Dictionary<GameLanguage, Dictionary<string, Sprite>> _languageSprites =
            new Dictionary<GameLanguage, Dictionary<string, Sprite>>();

        private static readonly Dictionary<string, byte[]> _rawFileBytes = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        private static readonly string[] _materialTextureProperties = new string[]
        {
            "_BaseMap",
            "Base_Map",
            "_MainTex",
            "_DecalColorMap",
            "_BaseColorMap",
            "_DirtyMap",
            "_Tex",
            "_DetailAlbedoMap",
            "_Label"
        };

        private static GameLanguage _currentLanguage = GameLanguage.English;
        private static bool _useX2Textures = PlayerPrefs.GetInt("ccodix_texture_x2", 0) == 1;

        public static bool UseX2Textures
        {
            get => _useX2Textures;
            set
            {
                if (_useX2Textures != value)
                {
                    _useX2Textures = value;
                    PlayerPrefs.SetInt("ccodix_texture_x2", _useX2Textures ? 1 : 0);
                    PlayerPrefs.Save();
                    ReloadAllTextures();
                }
            }
        }

        public static void ToggleX2Textures()
        {
            UseX2Textures = !UseX2Textures;
        }

        public static void ReloadAllTextures()
        {
            _rawFileBytes.Clear();
            _languageTextures.Clear();
            _languageSprites.Clear();
            _customLogoSprite = null;
            ApplyLanguage(_currentLanguage);
        }

        public static void ApplyLanguage(GameLanguage language)
        {
            _currentLanguage = language;
            EnsureLanguageLoaded(language);

            UpdateUIImages(language);
            UpdateRawImages(language);
            UpdateRenderers(language);
            UpdateAllDecalProjectors(language);
            UpdateDirtyDecals(language);
            UpdateSpriteRenderers(language);
            UpdateDragonMaterials(language);
            RestoreGameLogo();
        }

        public static void UpdateCurrentScene()
        {
            if (_currentLanguage != GameLanguage.English)
            {
                ApplyLanguage(_currentLanguage);
            }
        }

        public static Sprite? GetSprite(string name, GameLanguage language)
        {
            EnsureLanguageLoaded(language);
            if (_languageSprites.TryGetValue(language, out var dict) && dict.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }
            return null;
        }

        private static string GetSubFolder(GameLanguage language)
        {
            if (language == GameLanguage.Russian) return "ru";
            if (language == GameLanguage.Ukrainian) return "ua";
            return "en";
        }

        private static byte[]? GetTextureBytes(string subFolder, string textureName)
        {
            string key = subFolder + "/" + textureName + (_useX2Textures ? "_x2" : "");
            if (_rawFileBytes.TryGetValue(key, out byte[] cached))
            {
                return cached;
            }

            string texturesDir = Path.Combine(LanguageManager.DataDirectory, "textures", subFolder);
            string filePath = Path.Combine(texturesDir, textureName + ".png");

            if (_useX2Textures)
            {
                string x2Path = Path.Combine(texturesDir, textureName + "_x2.png");
                if (File.Exists(x2Path))
                {
                    filePath = x2Path;
                }
            }

            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                _rawFileBytes[key] = data;
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error reading texture bytes for " + textureName + ": " + ex.Message);
                return null;
            }
        }

        private static void EnsureLanguageLoaded(GameLanguage language)
        {
            if (_languageTextures.ContainsKey(language))
            {
                return;
            }

            string subFolder = GetSubFolder(language);
            var texDict = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            var spriteDict = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

            _languageTextures[language] = texDict;
            _languageSprites[language] = spriteDict;

            foreach (string name in _trackedTextures)
            {
                byte[]? bytes = GetTextureBytes(subFolder, name);
                if (bytes == null || bytes.Length == 0)
                {
                    continue;
                }

                try
                {
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (ImageConversion.LoadImage(tex, bytes))
                    {
                        tex.name = name;
                        tex.filterMode = FilterMode.Bilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;
                        UnityEngine.Object.DontDestroyOnLoad(tex);
                        texDict[name] = tex;

                        Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                        sprite.name = name;
                        UnityEngine.Object.DontDestroyOnLoad(sprite);
                        spriteDict[name] = sprite;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("[LocalizationMod] Error loading texture " + name + ": " + ex.Message);
                }
            }
        }

        private static void UpdateUIImages(GameLanguage language)
        {
            Image[] images = UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (!_languageSprites.TryGetValue(language, out var targetSprites))
            {
                return;
            }

            foreach (Image img in images)
            {
                if (img == null || img.sprite == null) continue;
                string sName = img.sprite.name;
                if (targetSprites.TryGetValue(sName, out Sprite rep))
                {
                    if (img.sprite != rep)
                    {
                        img.sprite = rep;
                        img.SetAllDirty();
                    }
                }
            }
        }

        private static void UpdateRawImages(GameLanguage language)
        {
            RawImage[] rawImages = UnityEngine.Object.FindObjectsByType<RawImage>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (!_languageTextures.TryGetValue(language, out var targetTextures))
            {
                return;
            }

            foreach (RawImage raw in rawImages)
            {
                if (raw == null || raw.texture == null) continue;
                string tName = raw.texture.name;
                if (targetTextures.TryGetValue(tName, out Texture2D rep))
                {
                    if (raw.texture != rep)
                    {
                        raw.texture = rep;
                        raw.SetAllDirty();
                    }
                }
            }
        }

        private static void UpdateRenderers(GameLanguage language)
        {
            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (!_languageTextures.TryGetValue(language, out var targetTextures))
            {
                return;
            }

            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                if (r.GetType().Name == "VFXRenderer") continue;

                Material[] sharedMats = r.sharedMaterials;
                if (sharedMats != null)
                {
                    foreach (Material mat in sharedMats)
                    {
                        if (mat == null) continue;
                        ApplyTexturesToMaterial(mat, targetTextures);
                    }
                }
            }
        }

        public static void UpdateDecalProjector(MonoBehaviour projector, GameLanguage language)
        {
            if (projector == null) return;
            if (!_languageTextures.TryGetValue(language, out var targetTextures))
            {
                return;
            }

            try
            {
                PropertyInfo? matProp = projector.GetType().GetProperty("material", BindingFlags.Instance | BindingFlags.Public);
                if (matProp != null)
                {
                    Material? mat = matProp.GetValue(projector) as Material;
                    if (mat != null)
                    {
                        ApplyTexturesToMaterial(mat, targetTextures);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error updating DecalProjector: " + ex.Message);
            }
        }

        private static void UpdateAllDecalProjectors(GameLanguage language)
        {
            try
            {
                MonoBehaviour[] scripts = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (MonoBehaviour mb in scripts)
                {
                    if (mb == null) continue;
                    if (mb.GetType().Name == "DecalProjector")
                    {
                        UpdateDecalProjector(mb, language);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error searching DecalProjector components: " + ex.Message);
            }
        }

        private static void ApplyTexturesToMaterial(Material mat, Dictionary<string, Texture2D> targetTextures)
        {
            foreach (string prop in _materialTextureProperties)
            {
                if (!mat.HasProperty(prop)) continue;
                Texture cur = mat.GetTexture(prop);
                if (cur != null && targetTextures.TryGetValue(cur.name, out Texture2D rep))
                {
                    if (cur != rep)
                    {
                        mat.SetTexture(prop, rep);
                    }
                }
            }
        }

        private static Type? GetDirtyDecalType()
        {
            Type? t = Type.GetType("WalkNWashDirtyDecal, Assembly-CSharp");
            if (t != null) return t;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType("WalkNWashDirtyDecal");
                if (t != null) return t;
            }
            return null;
        }

        public static void UpdateSingleDirtyDecal(MonoBehaviour decal, GameLanguage language)
        {
            if (decal == null) return;
            EnsureLanguageLoaded(language);
            if (!_languageTextures.TryGetValue(language, out var targetTextures)) return;

            try
            {
                FieldInfo? field = decal.GetType().GetField("dirtyTexture", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    Texture2D? current = field.GetValue(decal) as Texture2D;
                    string targetKey = "Dragon_Rig_Bake_Low_Ryan_Dirty_Writing_Color";
                    Texture2D? rep = null;
                    if (current != null && targetTextures.TryGetValue(current.name, out Texture2D matchRep))
                    {
                        rep = matchRep;
                    }
                    else if (targetTextures.TryGetValue(targetKey, out Texture2D defRep))
                    {
                        rep = defRep;
                    }

                    if (rep != null && current != rep)
                    {
                        field.SetValue(decal, rep);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error updating single dirty decal: " + ex.Message);
            }
        }

        private static void UpdateDirtyDecals(GameLanguage language)
        {
            try
            {
                Type? decalType = GetDirtyDecalType();
                if (decalType == null) return;

                FieldInfo? field = decalType.GetField("dirtyTexture", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null) return;

                UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsByType(decalType, FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (objects == null || !_languageTextures.TryGetValue(language, out var targetTextures)) return;

                string targetKey = "Dragon_Rig_Bake_Low_Ryan_Dirty_Writing_Color";
                foreach (UnityEngine.Object obj in objects)
                {
                    if (obj == null) continue;
                    Texture2D? current = field.GetValue(obj) as Texture2D;
                    Texture2D? rep = null;
                    if (current != null && targetTextures.TryGetValue(current.name, out Texture2D matchRep))
                    {
                        rep = matchRep;
                    }
                    else if (targetTextures.TryGetValue(targetKey, out Texture2D defRep))
                    {
                        rep = defRep;
                    }

                    if (rep != null && current != rep)
                    {
                        field.SetValue(obj, rep);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error updating dirty decals: " + ex.Message);
            }
        }

        private static void UpdateSpriteRenderers(GameLanguage language)
        {
            SpriteRenderer[] spriteRenderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (!_languageSprites.TryGetValue(language, out var targetSprites))
            {
                return;
            }

            foreach (SpriteRenderer sr in spriteRenderers)
            {
                if (sr == null || sr.sprite == null) continue;
                string spriteName = sr.sprite.name;

                if (targetSprites.TryGetValue(spriteName, out Sprite rep))
                {
                    if (sr.sprite != rep)
                    {
                        sr.sprite = rep;
                    }
                }
            }
        }

        private static readonly FieldInfo? _auxDirtyMapField =
            typeof(WalkNWashDragonDescriptor).GetField("auxDirtyMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public static void PrepareDragonAuxDirtyMap(WalkNWashDragonDescriptor dragon, GameLanguage language)
        {
            if (dragon == null) return;
            EnsureLanguageLoaded(language);
            if (!_languageTextures.TryGetValue(language, out var targetTextures)) return;

            if (_auxDirtyMapField != null)
            {
                Texture2D? aux = _auxDirtyMapField.GetValue(dragon) as Texture2D;
                string targetKey = "Dragon_Rig_Bake_Low_Ryan_Dirty_Writing_Color";
                Texture2D? rep = null;
                if (aux != null && targetTextures.TryGetValue(aux.name, out Texture2D matchRep))
                {
                    rep = matchRep;
                }
                else if (targetTextures.TryGetValue(targetKey, out Texture2D defRep))
                {
                    rep = defRep;
                }

                if (rep != null && aux != rep)
                {
                    _auxDirtyMapField.SetValue(dragon, rep);
                }
            }
        }

        public static void UpdateDragonMaterials(GameLanguage language)
        {
            if (!_languageTextures.TryGetValue(language, out var targetTextures))
            {
                return;
            }

            try
            {
                WalkNWashDragonDescriptor[] dragons = UnityEngine.Object.FindObjectsByType<WalkNWashDragonDescriptor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (WalkNWashDragonDescriptor dragon in dragons)
                {
                    if (dragon == null) continue;

                    PrepareDragonAuxDirtyMap(dragon, language);

                    SkinnedMeshRenderer[] smrs = dragon.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (SkinnedMeshRenderer smr in smrs)
                    {
                        if (smr == null) continue;

                        Material[] sharedMats = smr.sharedMaterials;
                        if (sharedMats != null)
                        {
                            foreach (Material mat in sharedMats)
                            {
                                if (mat != null) ApplyTexturesToMaterial(mat, targetTextures);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error updating dragon materials: " + ex.Message);
            }
        }

        private static Sprite? _customLogoSprite;

        public static Sprite? GetCustomLogoSprite()
        {
            if (_customLogoSprite != null)
            {
                return _customLogoSprite;
            }

            string logoPath = Path.Combine(LanguageManager.DataDirectory, "textures", "logo_ccodix.png");
            if (!File.Exists(logoPath))
            {
                return null;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(logoPath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (ImageConversion.LoadImage(tex, bytes))
                {
                    tex.name = "logo_ccodix";
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    UnityEngine.Object.DontDestroyOnLoad(tex);

                    _customLogoSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    _customLogoSprite.name = "logo_ccodix";
                    UnityEngine.Object.DontDestroyOnLoad(_customLogoSprite);
                    return _customLogoSprite;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error loading custom logo: " + ex.Message);
            }
            return null;
        }

        public static void RestoreGameLogo()
        {
            Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Transform t in transforms)
            {
                if (t == null) continue;
                if (t.name == "AnimatedLogo")
                {
                    Animator anim = t.GetComponent<Animator>();
                    if (anim != null && !anim.enabled)
                    {
                        anim.enabled = true;
                    }
                    Image[] images = t.GetComponentsInChildren<Image>(true);
                    foreach (Image img in images)
                    {
                        if (img != null)
                        {
                            if (img.overrideSprite != null)
                            {
                                img.overrideSprite = null;
                            }
                            img.preserveAspect = true;
                        }
                    }
                }
                else if (t.name == "TitleImage")
                {
                    Image img = t.GetComponent<Image>();
                    if (img != null)
                    {
                        if (img.overrideSprite != null)
                        {
                            img.overrideSprite = null;
                        }
                        img.preserveAspect = true;
                    }
                }
            }
        }
    }
}
