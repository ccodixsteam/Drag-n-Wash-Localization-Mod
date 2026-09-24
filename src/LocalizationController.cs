using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DragNWashLocalization
{
    public class LocalizationController : MonoBehaviour
    {
        private static LocalizationController? _instance;
        private float _periodicTimer = 0f;

        public static void Install()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject host = new GameObject("[DragNWash_LocalizationController]");
            UnityEngine.Object.DontDestroyOnLoad(host);
            _instance = host.AddComponent<LocalizationController>();
            host.AddComponent<GrandOpeningSignLocalization>();
        }

        private void Awake()
        {
            LanguageManager.Initialize();
            DialogueManager.Load(LanguageManager.DataDirectory);
            UIManager.Load(LanguageManager.DataDirectory);

            if (LanguageManager.CurrentLanguage != GameLanguage.English)
            {
                FontManager.GetOrCreateFontAsset();
                FontManager.ApplyToAllFonts();
            }

            CleanupFloatingStatusLabels();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            ApplyCurrentLocalization();
        }

        private float _optionsCheckTimer = 0f;

        private void LateUpdate()
        {
            _periodicTimer += Time.unscaledDeltaTime;
            if (_periodicTimer >= 1.5f)
            {
                _periodicTimer = 0f;
                if (LanguageManager.CurrentLanguage != GameLanguage.English)
                {
                    TextureManager.UpdateCurrentScene();
                }
            }

            _optionsCheckTimer += Time.unscaledDeltaTime;
            if (_optionsCheckTimer >= 0.4f)
            {
                _optionsCheckTimer = 0f;
                if (LanguageManager.CurrentLanguage != GameLanguage.English)
                {
                    GameObject optionsMenu = GameObject.Find("Menu_Options");
                    if (optionsMenu != null && optionsMenu.activeInHierarchy)
                    {
                        UIManager.ApplyLanguage(LanguageManager.CurrentLanguage);
                        TextureManager.ApplyLanguage(LanguageManager.CurrentLanguage);
                    }
                    else if (GameObject.Find("CreditScroll") != null || GameObject.Find("ThanksForPlaying") != null)
                    {
                        UIManager.ApplyLanguage(LanguageManager.CurrentLanguage);
                    }
                }
            }

            UpdateLoadingScreen();
            DisablePauseMenuPanelBackground();
            CleanupFloatingStatusLabels();
        }

        private void UpdateLoadingScreen()
        {
            GameObject loading = GameObject.Find("Loading");
            if (loading == null || !loading.activeInHierarchy)
            {
                return;
            }

            UnityEngine.UI.Image img = loading.GetComponent<UnityEngine.UI.Image>();
            if (img == null || img.sprite == null)
            {
                return;
            }

            GameLanguage lang = LanguageManager.CurrentLanguage;
            if (lang == GameLanguage.English)
            {
                if (img.overrideSprite != null)
                {
                    img.overrideSprite = null;
                }
            }
            else
            {
                Sprite? rep = TextureManager.GetSprite(img.sprite.name, lang);
                if (rep != null && img.overrideSprite != rep)
                {
                    img.overrideSprite = rep;
                }
            }
        }

        private void DisablePauseMenuPanelBackground()
        {
            GameObject menuMain = GameObject.Find("Menu_Main");
            if (menuMain != null)
            {
                Transform panel = menuMain.transform.Find("Container/Panel");
                if (panel != null)
                {
                    UnityEngine.UI.Image img = panel.GetComponent<UnityEngine.UI.Image>();
                    if (img != null && img.enabled)
                    {
                        img.enabled = false;
                    }
                }
            }
        }

        private static void CleanupFloatingStatusLabels()
        {
            GameObject floating = GameObject.Find("BoothStatusLabel");
            if (floating != null)
            {
                UnityEngine.Object.Destroy(floating);
            }
            GameObject world = GameObject.Find("WorldBoothStatusLabel");
            if (world != null)
            {
                UnityEngine.Object.Destroy(world);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyCurrentLocalization();
            CleanupFloatingStatusLabels();
            DisablePauseMenuPanelBackground();
        }

        public static void ApplyCurrentLocalization()
        {
            GameLanguage lang = LanguageManager.CurrentLanguage;

            if (lang != GameLanguage.English)
            {
                FontManager.ApplyToAllFonts();
            }

            DialogueManager.ApplyLanguage(lang);
            UIManager.ApplyLanguage(lang);
            TextureManager.ApplyLanguage(lang);
            GrandOpeningSignLocalization.UpdateLanguage(lang);

            if (LanguageSelector.IsOnMainMenu())
            {
                MenuCredits.EnsureCreditOnMenu();
            }
            else
            {
                MenuCredits.SetVisible(false);
            }
        }
    }
}
