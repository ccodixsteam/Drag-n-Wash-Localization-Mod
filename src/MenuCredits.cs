using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DragNWashLocalization
{
    public static class MenuCredits
    {
        private const string CreditObjectName = "ccodix_LocalizationCredit";
        private static TextMeshProUGUI? _creditTextComponent;
        private static GameObject? _creditInstance;

        public static void SetVisible(bool visible)
        {
            if (_creditInstance != null)
            {
                _creditInstance.SetActive(visible);
            }
        }

        public static void EnsureCreditOnMenu()
        {
            try
            {
                MenuMain menuMain = UnityEngine.Object.FindAnyObjectByType<MenuMain>();
                if (menuMain == null || !menuMain.isShown)
                {
                    if (_creditInstance != null)
                    {
                        _creditInstance.SetActive(false);
                    }
                    return;
                }

                Transform parentTransform = menuMain.transform;
                Transform containerChild = menuMain.transform.Find("Container");
                if (containerChild != null)
                {
                    parentTransform = containerChild;
                }

                Transform existing = parentTransform.Find(CreditObjectName);
                if (existing != null)
                {
                    _creditInstance = existing.gameObject;
                    _creditInstance.SetActive(true);
                    _creditTextComponent = existing.GetComponent<TextMeshProUGUI>();
                    UpdateCredits();
                    return;
                }

                GameObject creditObj = new GameObject(CreditObjectName);
                _creditInstance = creditObj;
                creditObj.transform.SetParent(parentTransform, false);

                RectTransform rect = creditObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(340f, 32f);
                rect.sizeDelta = new Vector2(260f, 26f);

                _creditTextComponent = creditObj.AddComponent<TextMeshProUGUI>();
                _creditTextComponent.fontSize = 18f;
                _creditTextComponent.alignment = TextAlignmentOptions.Center;
                _creditTextComponent.color = Color.white;
                _creditTextComponent.outlineWidth = 0f;
                _creditTextComponent.extraPadding = true;
                _creditTextComponent.raycastTarget = false;
                _creditTextComponent.textWrappingMode = TextWrappingModes.NoWrap;

                TMP_FontAsset? font = FontManager.GetOrCreateFontAsset();
                if (font != null)
                {
                    _creditTextComponent.font = font;
                }

                UpdateCredits();
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error creating menu credit: " + ex.Message);
            }
        }

        public static void UpdateCredits()
        {
            if (_creditTextComponent == null)
            {
                return;
            }

            string text = "Localization by ccodix";
            if (LanguageManager.CurrentLanguage == GameLanguage.Ukrainian)
            {
                text = "Локалізація від ccodix";
            }
            else if (LanguageManager.CurrentLanguage == GameLanguage.Russian)
            {
                text = "Локализация от ccodix";
            }

            _creditTextComponent.text = text;

            TMP_FontAsset? font = FontManager.GetOrCreateFontAsset();
            if (font != null)
            {
                _creditTextComponent.font = font;
            }

            _creditTextComponent.outlineWidth = 0f;
            _creditTextComponent.ForceMeshUpdate(true, true);
        }
    }
}
