using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragNWashLocalization
{
    public class LanguageSelector : MonoBehaviour
    {
        private const string PlanetButtonName = "ccodix_PlanetButton";
        private const string ModalRootName = "ccodix_LanguageModal";

        private static Sprite? _earthNormal;
        private static Sprite? _earthHover;
        private static Sprite? _ruNormal;
        private static Sprite? _ruHover;
        private static Sprite? _uaNormal;
        private static Sprite? _uaHover;
        private static Sprite? _usaNormal;
        private static Sprite? _usaHover;

        private static Sprite? _qualityNormal;
        private static Sprite? _qualityHover;
        private static HoverSpriteSwitcher? _qualityBtnSwitcher;

        private static GameObject? _planetInstance;
        private static GameObject? _modalInstance;
        private static Image? _modalLogoImage;
        private static TextMeshProUGUI? _modalTitle;
        private static TextMeshProUGUI? _qualityText;
        private static Image? _qualityBtnImage;
        private static MenuMain? _currentMenuMain;
        private static readonly System.Collections.Generic.List<GameObject> _hiddenLogos = new System.Collections.Generic.List<GameObject>();

        public static bool IsOnMainMenu()
        {
            return _currentMenuMain != null && _currentMenuMain.isShown;
        }

        public static void SetVisible(bool visible)
        {
            if (_planetInstance != null)
            {
                _planetInstance.SetActive(visible);
            }
            if (!visible)
            {
                if (_modalInstance != null)
                {
                    _modalInstance.SetActive(false);
                }
                _hiddenLogos.Clear();
            }
        }

        public static void OnEscape()
        {
            if (_modalInstance != null && _modalInstance.activeSelf)
            {
                CloseModal();
            }
        }

        public static void EnsureSelectorOnMenu(MenuMain menuMain)
        {
            if (menuMain == null)
            {
                return;
            }

            _currentMenuMain = menuMain;

            try
            {
                LoadSprites();

                Transform parentTransform = menuMain.transform;
                Transform containerChild = menuMain.transform.Find("Container");
                if (containerChild != null)
                {
                    parentTransform = containerChild;
                }

                Transform existingPlanet = parentTransform.Find(PlanetButtonName);
                if (existingPlanet != null)
                {
                    _planetInstance = existingPlanet.gameObject;
                    _planetInstance.SetActive(true);
                }
                else
                {
                    CreatePlanetButton(parentTransform);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error creating language button: " + ex.Message);
            }
        }

        private static HoverSpriteSwitcher? _closeBtnSwitcher;
        private static Image? _closeBtnImage;

        private static void CreatePlanetButton(Transform parent)
        {
            GameObject btnObj = new GameObject(PlanetButtonName);
            _planetInstance = btnObj;
            btnObj.transform.SetParent(parent, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(340f, 85f);
            rect.sizeDelta = new Vector2(62f, 62f);

            Image img = btnObj.AddComponent<Image>();
            img.sprite = _earthNormal;
            img.preserveAspect = true;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;

            HoverSpriteSwitcher switcher = btnObj.AddComponent<HoverSpriteSwitcher>();
            switcher.NormalSprite = _earthNormal;
            switcher.HoverSprite = _earthHover;
            switcher.EnableScale = true;

            btn.onClick.AddListener(OpenModal);
        }

        public static void OpenModal()
        {
            SetMenuContainerVisible(false);
            SetAnimatedLogoVisible(false);

            if (_modalInstance != null)
            {
                UpdateModalTexts();
                UpdateCloseButtonSprite();
                UpdateModalLogo();
                _modalInstance.transform.SetAsLastSibling();
                _modalInstance.SetActive(true);
                return;
            }

            if (_currentMenuMain == null)
            {
                return;
            }

            Transform rootParent = _currentMenuMain.transform.parent != null ? _currentMenuMain.transform.parent : _currentMenuMain.transform;

            GameObject modalRoot = new GameObject(ModalRootName);
            _modalInstance = modalRoot;
            modalRoot.transform.SetParent(rootParent, false);
            modalRoot.transform.SetAsLastSibling();

            RectTransform modalRect = modalRoot.AddComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;

            Image backdrop = modalRoot.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.85f);
            backdrop.raycastTarget = true;

            Button backdropBtn = modalRoot.AddComponent<Button>();
            backdropBtn.onClick.AddListener(CloseModal);

            CreateModalLogo(modalRoot.transform);

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(modalRoot.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(0f, -145f);
            panelRect.sizeDelta = new Vector2(520f, 380f);

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = Color.clear;
            panelBg.raycastTarget = true;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, 135f);
            titleRect.sizeDelta = new Vector2(460f, 40f);

            _modalTitle = titleObj.AddComponent<TextMeshProUGUI>();
            _modalTitle.fontSize = 30f;
            _modalTitle.fontStyle = FontStyles.Bold;
            _modalTitle.alignment = TextAlignmentOptions.Center;
            _modalTitle.color = Color.white;
            _modalTitle.outlineWidth = 0f;
            _modalTitle.extraPadding = true;

            TMP_FontAsset? font = FontManager.GetOrCreateFontAsset();
            if (font != null)
            {
                _modalTitle.font = font;
            }

            GameObject flagsRow = new GameObject("FlagsRow");
            flagsRow.transform.SetParent(panel.transform, false);
            RectTransform flagsRect = flagsRow.AddComponent<RectTransform>();
            flagsRect.anchorMin = new Vector2(0.5f, 0.5f);
            flagsRect.anchorMax = new Vector2(0.5f, 0.5f);
            flagsRect.pivot = new Vector2(0.5f, 0.5f);
            flagsRect.anchoredPosition = new Vector2(0f, 55f);
            flagsRect.sizeDelta = new Vector2(360f, 80f);

            HorizontalLayoutGroup layout = flagsRow.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 32f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            CreateModalFlag(flagsRow.transform, "Flag_RU", _ruNormal, _ruHover, GameLanguage.Russian);
            CreateModalFlag(flagsRow.transform, "Flag_UA", _uaNormal, _uaHover, GameLanguage.Ukrainian);
            CreateModalFlag(flagsRow.transform, "Flag_USA", _usaNormal, _usaHover, GameLanguage.English);

            CreateQualityButton(panel.transform);
            CreateCloseButton(panel.transform);
            UpdateModalTexts();
        }

        private static void CreateModalLogo(Transform parent)
        {
            GameObject logoObj = new GameObject("ModalLogo");
            logoObj.transform.SetParent(parent, false);

            RectTransform logoRect = logoObj.AddComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(0.5f, 0.5f);
            logoRect.anchoredPosition = new Vector2(0f, 190f);
            logoRect.sizeDelta = new Vector2(280f, 280f);

            _modalLogoImage = logoObj.AddComponent<Image>();
            _modalLogoImage.sprite = TextureManager.GetCustomLogoSprite();
            _modalLogoImage.preserveAspect = true;
            _modalLogoImage.raycastTarget = false;
        }

        private static void UpdateModalLogo()
        {
            if (_modalLogoImage != null)
            {
                _modalLogoImage.sprite = TextureManager.GetCustomLogoSprite();
            }
        }

        private static void CreateQualityButton(Transform parent)
        {
            GameObject qualityObj = new GameObject("QualityButton");
            qualityObj.transform.SetParent(parent, false);

            RectTransform qualityRect = qualityObj.AddComponent<RectTransform>();
            qualityRect.anchorMin = new Vector2(0.5f, 0.5f);
            qualityRect.anchorMax = new Vector2(0.5f, 0.5f);
            qualityRect.pivot = new Vector2(0.5f, 0.5f);
            qualityRect.anchoredPosition = new Vector2(0f, -30f);
            qualityRect.sizeDelta = new Vector2(280f, 70f);

            _qualityBtnImage = qualityObj.AddComponent<Image>();
            _qualityBtnImage.sprite = _qualityNormal;
            _qualityBtnImage.preserveAspect = true;

            Button btn = qualityObj.AddComponent<Button>();
            btn.targetGraphic = _qualityBtnImage;
            btn.transition = Selectable.Transition.None;

            _qualityBtnSwitcher = qualityObj.AddComponent<HoverSpriteSwitcher>();
            _qualityBtnSwitcher.NormalSprite = _qualityNormal;
            _qualityBtnSwitcher.HoverSprite = _qualityHover;
            _qualityBtnSwitcher.EnableScale = true;
            _qualityBtnSwitcher.HoverScale = 1.05f;
            _qualityBtnSwitcher.PressScale = 0.95f;

            btn.onClick.AddListener(() =>
            {
                TextureManager.ToggleX2Textures();
                UpdateQualityButtonText();
            });

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(qualityObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            _qualityText = textObj.AddComponent<TextMeshProUGUI>();
            _qualityText.fontSize = 17f;
            _qualityText.fontSizeMin = 13f;
            _qualityText.fontSizeMax = 18f;
            _qualityText.enableAutoSizing = true;
            _qualityText.alignment = TextAlignmentOptions.Center;
            _qualityText.color = Color.white;
            _qualityText.outlineWidth = 0f;
            _qualityText.extraPadding = true;
            _qualityText.textWrappingMode = TextWrappingModes.NoWrap;
            _qualityText.raycastTarget = false;

            TMP_FontAsset? font = FontManager.GetOrCreateFontAsset();
            if (font != null)
            {
                _qualityText.font = font;
            }

            UpdateQualityButtonText();
        }

        private static void UpdateQualityButtonText()
        {
            if (_qualityBtnSwitcher != null)
            {
                _qualityBtnSwitcher.NormalSprite = _qualityNormal;
                _qualityBtnSwitcher.HoverSprite = _qualityHover;
                _qualityBtnSwitcher.UpdateVisuals();
            }

            if (_qualityText == null)
            {
                return;
            }

            GameLanguage current = LanguageManager.CurrentLanguage;
            bool on = TextureManager.UseX2Textures;
            if (current == GameLanguage.Ukrainian)
            {
                _qualityText.text = on ? "2x текстури: [УВІМК]" : "2x текстури: [ВИМК]";
            }
            else if (current == GameLanguage.Russian)
            {
                _qualityText.text = on ? "2x текстуры: [ВКЛ]" : "2x текстуры: [ВЫКЛ]";
            }
            else
            {
                _qualityText.text = on ? "2x textures: [ON]" : "2x textures: [OFF]";
            }

            TMP_FontAsset? font = FontManager.GetOrCreateFontAsset();
            if (font != null)
            {
                _qualityText.font = font;
            }
            _qualityText.color = Color.white;
            _qualityText.outlineWidth = 0f;
        }

        private static void CreateCloseButton(Transform parent)
        {
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(parent, false);
            RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0.5f);
            closeRect.anchorMax = new Vector2(0.5f, 0.5f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(0f, -120f);
            closeRect.sizeDelta = new Vector2(220f, 82f);

            _closeBtnImage = closeBtnObj.AddComponent<Image>();
            _closeBtnImage.preserveAspect = true;

            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.targetGraphic = _closeBtnImage;
            closeBtn.transition = Selectable.Transition.None;
            closeBtn.onClick.AddListener(CloseModal);

            _closeBtnSwitcher = closeBtnObj.AddComponent<HoverSpriteSwitcher>();
            _closeBtnSwitcher.EnableScale = true;
            _closeBtnSwitcher.HoverScale = 1.05f;
            _closeBtnSwitcher.PressScale = 0.95f;
            UpdateCloseButtonSprite();
        }

        private static void UpdateModalTexts()
        {
            UpdateQualityButtonText();
            UpdateCloseButtonSprite();

            if (_modalTitle != null)
            {
                GameLanguage current = LanguageManager.CurrentLanguage;
                if (current == GameLanguage.Ukrainian)
                {
                    _modalTitle.text = "Вибір мови";
                }
                else if (current == GameLanguage.Russian)
                {
                    _modalTitle.text = "Выбор языка";
                }
                else
                {
                    _modalTitle.text = "Language";
                }

                TMP_FontAsset? font = FontManager.GetOrCreateFontAsset();
                if (font != null)
                {
                    _modalTitle.font = font;
                }
            }
        }

        private static void UpdateCloseButtonSprite()
        {
            if (_closeBtnImage == null)
            {
                return;
            }

            GameLanguage current = LanguageManager.CurrentLanguage;
            Sprite? normal = TextureManager.GetSprite("MenuButtons0011", current);
            Sprite? hover = TextureManager.GetSprite("MenuButtons0012", current);

            _closeBtnImage.sprite = normal;
            if (_closeBtnSwitcher != null)
            {
                _closeBtnSwitcher.NormalSprite = normal;
                _closeBtnSwitcher.HoverSprite = hover;
                _closeBtnSwitcher.UpdateVisuals();
            }
        }

        public static void CloseModal()
        {
            if (_modalInstance != null)
            {
                _modalInstance.SetActive(false);
            }
            SetAnimatedLogoVisible(true);
            SetMenuContainerVisible(true);
        }

        private static void SetAnimatedLogoVisible(bool visible)
        {
            if (!visible)
            {
                _hiddenLogos.Clear();
                Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (Transform t in transforms)
                {
                    if (t != null && (t.name == "AnimatedLogo" || t.name == "TitleImage"))
                    {
                        if (t.gameObject.activeSelf && !_hiddenLogos.Contains(t.gameObject))
                        {
                            _hiddenLogos.Add(t.gameObject);
                            t.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                foreach (GameObject go in _hiddenLogos)
                {
                    if (go != null)
                    {
                        go.SetActive(true);
                    }
                }
                _hiddenLogos.Clear();
                TextureManager.RestoreGameLogo();
            }
        }

        private static void SetMenuContainerVisible(bool visible)
        {
            if (!visible)
            {
                if (_currentMenuMain != null)
                {
                    Transform container = _currentMenuMain.transform.Find("Container");
                    if (container != null)
                    {
                        container.gameObject.SetActive(false);
                    }
                }
                else
                {
                    GameObject menuMain = GameObject.Find("Menu_Main");
                    if (menuMain != null)
                    {
                        Transform container = menuMain.transform.Find("Container");
                        if (container != null)
                        {
                            container.gameObject.SetActive(false);
                        }
                    }
                }

                if (_planetInstance != null)
                {
                    _planetInstance.SetActive(false);
                }
                MenuCredits.SetVisible(false);
            }
            else
            {
                if (_currentMenuMain != null && _currentMenuMain.gameObject.activeInHierarchy)
                {
                    Transform container = _currentMenuMain.transform.Find("Container");
                    if (container != null)
                    {
                        container.gameObject.SetActive(true);
                    }
                }
                else
                {
                    GameObject menuMain = GameObject.Find("Menu_Main");
                    if (menuMain != null && menuMain.activeInHierarchy)
                    {
                        Transform container = menuMain.transform.Find("Container");
                        if (container != null)
                        {
                            container.gameObject.SetActive(true);
                        }
                    }
                }

                if (_planetInstance != null)
                {
                    _planetInstance.SetActive(true);
                }
                MenuCredits.SetVisible(true);
            }
        }

        private static void CreateModalFlag(Transform parent, string name, Sprite? normal, Sprite? hover, GameLanguage language)
        {
            GameObject flagObj = new GameObject(name);
            flagObj.transform.SetParent(parent, false);

            RectTransform rect = flagObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(78f, 78f);

            Image img = flagObj.AddComponent<Image>();
            img.sprite = normal;
            img.preserveAspect = true;

            Button btn = flagObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;

            HoverSpriteSwitcher switcher = flagObj.AddComponent<HoverSpriteSwitcher>();
            switcher.NormalSprite = normal;
            switcher.HoverSprite = hover;
            switcher.TargetLanguage = language;
            switcher.EnableScale = true;

            btn.onClick.AddListener(() =>
            {
                LanguageManager.SetLanguage(language);
                UpdateModalTexts();
                UpdateCloseButtonSprite();
                CloseModal();
            });
        }

        private static void LoadSprites()
        {
            string icons = Path.Combine(LanguageManager.DataDirectory, "icons");
            if (_earthNormal == null)
            {
                _earthNormal = LoadSpriteFromFile(Path.Combine(icons, "earth.png"));
            }
            if (_earthHover == null)
            {
                _earthHover = LoadSpriteFromFile(Path.Combine(icons, "earth_hover.png"));
            }
            if (_ruNormal == null)
            {
                _ruNormal = LoadSpriteFromFile(Path.Combine(icons, "russian.png"));
            }
            if (_ruHover == null)
            {
                _ruHover = LoadSpriteFromFile(Path.Combine(icons, "russian_hover.png"));
            }
            if (_uaNormal == null)
            {
                _uaNormal = LoadSpriteFromFile(Path.Combine(icons, "ukrainian.png"));
            }
            if (_uaHover == null)
            {
                _uaHover = LoadSpriteFromFile(Path.Combine(icons, "ukrainian_hover.png"));
            }
            if (_usaNormal == null)
            {
                _usaNormal = LoadSpriteFromFile(Path.Combine(icons, "usa.png"));
            }
            if (_usaHover == null)
            {
                _usaHover = LoadSpriteFromFile(Path.Combine(icons, "usa_hover.png"));
            }
            if (_qualityNormal == null)
            {
                _qualityNormal = LoadSpriteFromFile(Path.Combine(icons, "quality_btn_normal.png"));
            }
            if (_qualityHover == null)
            {
                _qualityHover = LoadSpriteFromFile(Path.Combine(icons, "quality_btn_hover.png"));
            }
        }

        private static Sprite? LoadSpriteFromFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                byte[] data = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (ImageConversion.LoadImage(tex, data))
                {
                    tex.name = Path.GetFileNameWithoutExtension(path);
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error loading sprite " + path + ": " + ex.Message);
            }
            return null;
        }
    }

    public class HoverSpriteSwitcher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Sprite? NormalSprite;
        public Sprite? HoverSprite;
        public GameLanguage? TargetLanguage;
        public bool EnableScale = false;
        public float HoverScale = 1.08f;
        public float PressScale = 0.94f;

        private Image? _image;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private Vector3 _targetScale = Vector3.one;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnDisable()
        {
            _isHovered = false;
            _isPressed = false;
            transform.localScale = Vector3.one;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (_image == null)
            {
                return;
            }

            bool isSelected = TargetLanguage.HasValue && LanguageManager.CurrentLanguage == TargetLanguage.Value;

            if ((_isHovered || isSelected) && HoverSprite != null)
            {
                if (_image.sprite != HoverSprite)
                {
                    _image.sprite = HoverSprite;
                }
            }
            else if (NormalSprite != null)
            {
                if (_image.sprite != NormalSprite)
                {
                    _image.sprite = NormalSprite;
                }
            }
        }

        private void Update()
        {
            UpdateVisuals();

            if (!EnableScale)
            {
                if (transform.localScale != Vector3.one)
                {
                    transform.localScale = Vector3.one;
                }
                return;
            }

            bool isSelected = TargetLanguage.HasValue && LanguageManager.CurrentLanguage == TargetLanguage.Value;

            float target = 1.0f;
            if (_isPressed)
            {
                target = PressScale;
            }
            else if (_isHovered)
            {
                target = HoverScale;
            }
            else if (isSelected)
            {
                target = 1.04f;
            }

            _targetScale = new Vector3(target, target, 1f);
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * 14f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateVisuals();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateVisuals();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
        }
    }
}
