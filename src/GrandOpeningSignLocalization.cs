using System;
using System.IO;
using TMPro;
using UnityEngine;

namespace DragNWashLocalization
{
    public class GrandOpeningSignLocalization : MonoBehaviour
    {
        private static GrandOpeningSignLocalization? _instance;
        private GameObject? _signObj;
        private MeshFilter? _meshFilter;
        private Mesh? _originalMesh;
        private Mesh? _ribbonOnlyMesh;
        private GameObject? _textObj;
        private TextMeshPro? _textComponent;

        private string? _curvedText;

        public static void EnsureInitialized(GameObject host)
        {
            if (host.GetComponent<GrandOpeningSignLocalization>() == null)
            {
                host.AddComponent<GrandOpeningSignLocalization>();
            }
        }

        public static void UpdateLanguage(GameLanguage lang)
        {
            if (_instance != null)
            {
                _instance.ApplyLanguage(lang);
            }
        }

        private void Awake()
        {
            _instance = this;
            LanguageManager.OnLanguageChanged += OnLanguageChanged;
            LoadRibbonMesh();
        }

        private void OnDestroy()
        {
            LanguageManager.OnLanguageChanged -= OnLanguageChanged;
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnLanguageChanged(GameLanguage lang)
        {
            ApplyLanguage(lang);
        }

        private void LateUpdate()
        {
            if (_signObj == null || !_signObj)
            {
                _signObj = null;
                _meshFilter = null;
                _textObj = null;
                _textComponent = null;
                _curvedText = null;
                FindAndSetupSign();
                return;
            }

            if (_textComponent != null && _textObj != null && _textObj.activeSelf)
            {
                if (_curvedText != _textComponent.text)
                {
                    CurveTextAlongRibbon();
                    _curvedText = _textComponent.text;
                }
            }
        }

        private void FindAndSetupSign()
        {
            GameObject found = GameObject.Find("grand_opening_sign");
            if (found == null)
            {
                return;
            }

            _signObj = found;
            _meshFilter = _signObj.GetComponent<MeshFilter>();
            if (_meshFilter == null)
            {
                return;
            }

            if (_originalMesh == null)
            {
                _originalMesh = _meshFilter.sharedMesh;
            }

            if (_ribbonOnlyMesh == null)
            {
                LoadRibbonMesh();
            }

            EnsureTextComponent();
            ApplyLanguage(LanguageManager.CurrentLanguage);
        }

        private void LoadRibbonMesh()
        {
            try
            {
                string meshPath = Path.Combine(LanguageManager.DataDirectory, "ribbon_mesh.bytes");
                if (!File.Exists(meshPath))
                {
                    return;
                }

                using (BinaryReader reader = new BinaryReader(File.OpenRead(meshPath)))
                {
                    int vertexCount = reader.ReadInt32();
                    int indexCount = reader.ReadInt32();

                    Vector3[] vertices = new Vector3[vertexCount];
                    for (int i = 0; i < vertexCount; i++)
                    {
                        vertices[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    Vector3[] normals = new Vector3[vertexCount];
                    for (int i = 0; i < vertexCount; i++)
                    {
                        normals[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    Vector2[] uvs = new Vector2[vertexCount];
                    for (int i = 0; i < vertexCount; i++)
                    {
                        uvs[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    Color32[] colors = new Color32[vertexCount];
                    for (int i = 0; i < vertexCount; i++)
                    {
                        colors[i] = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    }

                    int[] triangles = new int[indexCount];
                    for (int i = 0; i < indexCount; i++)
                    {
                        triangles[i] = reader.ReadInt32();
                    }

                    Mesh mesh = new Mesh();
                    mesh.name = "grand_opening_ribbon";
                    mesh.vertices = vertices;
                    mesh.normals = normals;
                    mesh.uv = uvs;
                    mesh.colors32 = colors;
                    mesh.triangles = triangles;
                    mesh.RecalculateBounds();
                    _ribbonOnlyMesh = mesh;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error loading ribbon mesh: " + ex.Message);
            }
        }

        private void EnsureTextComponent()
        {
            if (_textComponent != null || _signObj == null)
            {
                return;
            }

            Transform existing = _signObj.transform.Find("ccodix_GrandOpeningText");
            if (existing != null)
            {
                _textObj = existing.gameObject;
                _textComponent = _textObj.GetComponent<TextMeshPro>();
                return;
            }

            _textObj = new GameObject("ccodix_GrandOpeningText");
            _textObj.transform.SetParent(_signObj.transform, false);
            _textObj.transform.localPosition = Vector3.zero;
            _textObj.transform.localRotation = Quaternion.identity;
            _textObj.transform.localScale = Vector3.one;

            RectTransform rt = _textObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(8.5f, 2.2f);

            _textComponent = _textObj.AddComponent<TextMeshPro>();
            _textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            _textComponent.enableAutoSizing = true;
            _textComponent.fontSizeMin = 2.4f;
            _textComponent.fontSizeMax = 3.6f;
            _textComponent.fontSize = 2.8f;
            _textComponent.fontStyle = FontStyles.Bold;
            _textComponent.alignment = TextAlignmentOptions.Center;
            _textComponent.color = new Color(0.96f, 0.94f, 0.82f, 1f);
            _textComponent.outlineColor = new Color32(80, 15, 20, 255);
            _textComponent.outlineWidth = 0.30f;
            _textComponent.extraPadding = true;

            TMP_FontAsset? font = FontManager.GetOrCreateFontAsset();
            if (font != null)
            {
                _textComponent.font = font;
            }
        }

        public void ApplyLanguage(GameLanguage lang)
        {
            if (_signObj == null || _meshFilter == null)
            {
                return;
            }

            if (lang == GameLanguage.English)
            {
                if (_originalMesh != null && _meshFilter.sharedMesh != _originalMesh)
                {
                    _meshFilter.sharedMesh = _originalMesh;
                }
                if (_textObj != null)
                {
                    _textObj.SetActive(false);
                }
            }
            else
            {
                if (_ribbonOnlyMesh != null && _meshFilter.sharedMesh != _ribbonOnlyMesh)
                {
                    _meshFilter.sharedMesh = _ribbonOnlyMesh;
                }

                if (_textObj != null && _textComponent != null)
                {
                    _textObj.SetActive(true);
                    RectTransform? rt = _textObj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = new Vector2(8.5f, 2.2f);
                    }
                    _textComponent.fontSizeMin = 2.4f;
                    _textComponent.fontSizeMax = 3.6f;
                    _textComponent.fontSize = 2.8f;

                    string text = (lang == GameLanguage.Ukrainian) ? "УРОЧИСТЕ ВІДКРИТТЯ" : "ТОРЖЕСТВЕННОЕ ОТКРЫТИЕ";
                    _textComponent.text = text;

                    TMP_FontAsset? font = FontManager.GetOrCreateFontAsset();
                    if (font != null)
                    {
                        _textComponent.font = font;
                    }

                    CurveTextAlongRibbon();
                    _curvedText = _textComponent.text;
                }
            }
        }

        private void CurveTextAlongRibbon()
        {
            if (_textComponent == null)
            {
                return;
            }

            _textComponent.ForceMeshUpdate(true, true);
            TMP_TextInfo textInfo = _textComponent.textInfo;
            if (textInfo == null || textInfo.meshInfo == null || textInfo.meshInfo.Length == 0)
            {
                return;
            }

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                {
                    continue;
                }

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;
                Vector3[] verts = textInfo.meshInfo[materialIndex].vertices;
                Vector3[] origVerts = new Vector3[4];
                for (int v = 0; v < 4; v++)
                {
                    origVerts[v] = verts[vertexIndex + v];
                }

                float charMidX = (origVerts[0].x + origVerts[2].x) * 0.5f;
                float charMidY = (origVerts[0].y + origVerts[2].y) * 0.5f;
                float charSignX = -charMidX;
                float slope = 0.11f * charSignX;
                float len = Mathf.Sqrt(1f + slope * slope);
                float tx = 1f / len;
                float tz = slope / len;
                float nx = -slope / len;
                float nz = 1f / len;

                float ribbonY = 10.470f - 0.0607f * charSignX;
                float ribbonZ = 6.340f + 0.055f * charSignX * charSignX;

                for (int v = 0; v < 4; v++)
                {
                    float dx = -(origVerts[v].x - charMidX);
                    float dy = origVerts[v].y - charMidY;

                    float rotX = dx * tx + dy * nx;
                    float rotZ = dx * tz + dy * nz;

                    float posX = charSignX + rotX;
                    float posY = ribbonY - 0.0607f * rotX;
                    float posZ = ribbonZ + rotZ;

                    verts[vertexIndex + v] = new Vector3(posX, posY, posZ);
                }
            }

            for (int m = 0; m < textInfo.meshInfo.Length; m++)
            {
                textInfo.meshInfo[m].mesh.vertices = textInfo.meshInfo[m].vertices;
                textInfo.meshInfo[m].mesh.RecalculateNormals();
                textInfo.meshInfo[m].mesh.RecalculateBounds();
                _textComponent.UpdateGeometry(textInfo.meshInfo[m].mesh, m);
            }
        }
    }
}
