using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DragNWashLocalization
{
    public static class UIManager
    {
        private static readonly Dictionary<string, string> _russianUI = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> _ukrainianUI = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<int, string> _originalTmpTexts = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> _originalLegacyTexts = new Dictionary<int, string>();
        private static readonly Dictionary<int, List<string>> _originalDropdownOptions = new Dictionary<int, List<string>>();
        private static bool _loaded = false;

        public static void Load(string dataPath)
        {
            if (_loaded)
            {
                return;
            }

            LoadFile(Path.Combine(dataPath, "ui_ru.json"), _russianUI);
            LoadFile(Path.Combine(dataPath, "ui_ua.json"), _ukrainianUI);
            _loaded = true;
        }

        private static void LoadFile(string path, Dictionary<string, string> target)
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    ParseJsonDictionary(json, target);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error loading UI file " + path + ": " + ex.Message);
            }
        }

        public static void ApplyLanguage(GameLanguage language)
        {
            if (!_loaded)
            {
                Load(LanguageManager.DataDirectory);
            }

            TMP_Text[] tmpTexts = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text tmp in tmpTexts)
            {
                TranslateTmpText(tmp, language);
            }

            TMP_Dropdown[] dropdowns = UnityEngine.Object.FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Dropdown dd in dropdowns)
            {
                TranslateDropdown(dd, language);
            }

            Text[] legacyTexts = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Text txt in legacyTexts)
            {
                TranslateLegacyText(txt, language);
            }
        }

        public static void TranslateDropdown(TMP_Dropdown dd, GameLanguage language)
        {
            if (dd == null || dd.options == null)
            {
                return;
            }

            int id = dd.GetInstanceID();
            if (!_originalDropdownOptions.TryGetValue(id, out var originalList))
            {
                originalList = new List<string>();
                foreach (var opt in dd.options)
                {
                    originalList.Add(opt != null ? opt.text : "");
                }
                _originalDropdownOptions[id] = originalList;
            }

            for (int i = 0; i < dd.options.Count && i < originalList.Count; i++)
            {
                var opt = dd.options[i];
                string orig = originalList[i];
                if (opt != null)
                {
                    if (language == GameLanguage.English)
                    {
                        opt.text = orig;
                    }
                    else if (TryTranslate(orig, language, out string tr))
                    {
                        opt.text = tr;
                    }
                }
            }

            if (dd.captionText != null)
            {
                if (dd.value >= 0 && dd.value < dd.options.Count && dd.options[dd.value] != null)
                {
                    dd.captionText.text = dd.options[dd.value].text;
                }
                else
                {
                    TranslateTmpText(dd.captionText, language);
                }
            }
        }

        public static bool IsVersionText(TMP_Text tmp)
        {
            if (tmp == null)
            {
                return false;
            }

            string goName = tmp.gameObject.name;
            if (goName.IndexOf("version", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (tmp.GetComponent("VersionNumber") != null)
            {
                return true;
            }

            string current = tmp.text;
            if (!string.IsNullOrEmpty(current))
            {
                string trimmed = current.Trim();
                if (trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase) && trimmed.Length > 1 && (char.IsDigit(trimmed[1]) || trimmed[1] == '.' || trimmed[1] == ' '))
                {
                    return true;
                }

                if (char.IsDigit(trimmed[0]) && trimmed.IndexOf('.') > 0)
                {
                    bool allVersion = true;
                    for (int i = 0; i < trimmed.Length; i++)
                    {
                        char c = trimmed[i];
                        if (!char.IsDigit(c) && c != '.' && c != 'a' && c != 'b' && c != 'f' && c != 'p')
                        {
                            allVersion = false;
                            break;
                        }
                    }
                    if (allVersion)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryTranslate(string text, GameLanguage language, out string translation)
        {
            translation = text;
            if (string.IsNullOrWhiteSpace(text) || language == GameLanguage.English)
            {
                return false;
            }

            if (!_loaded)
            {
                Load(LanguageManager.DataDirectory);
            }

            Dictionary<string, string> dict = language == GameLanguage.Ukrainian ? _ukrainianUI : _russianUI;
            string trimmed = text.Trim();

            if (dict.TryGetValue(trimmed, out translation))
            {
                return true;
            }

            foreach (KeyValuePair<string, string> entry in dict)
            {
                if (trimmed.Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                {
                    translation = entry.Value;
                    return true;
                }
            }

            if (trimmed.Contains("\n"))
            {
                string[] lines = trimmed.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                bool anyTranslated = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    string lineTrimmed = lines[i].Trim();
                    if (string.IsNullOrEmpty(lineTrimmed))
                    {
                        continue;
                    }
                    if (TryTranslate(lineTrimmed, language, out string lineTrans))
                    {
                        lines[i] = lineTrans;
                        anyTranslated = true;
                    }
                }
                if (anyTranslated)
                {
                    translation = string.Join("\n", lines);
                    return true;
                }
            }

            return false;
        }

        public static void TranslateTmpText(TMP_Text tmp, GameLanguage language)
        {
            if (tmp == null || IsVersionText(tmp))
            {
                return;
            }

            int id = tmp.GetInstanceID();
            if (!_originalTmpTexts.ContainsKey(id))
            {
                _originalTmpTexts[id] = tmp.text;
            }

            string baseText = _originalTmpTexts[id];
            if (string.IsNullOrWhiteSpace(baseText))
            {
                return;
            }

            if (language == GameLanguage.English)
            {
                tmp.text = baseText;
                return;
            }

            if (TryTranslate(baseText, language, out string translation))
            {
                tmp.text = translation;
            }
        }

        private static void TranslateLegacyText(Text txt, GameLanguage language)
        {
            if (txt == null)
            {
                return;
            }

            int id = txt.GetInstanceID();
            if (!_originalLegacyTexts.ContainsKey(id))
            {
                _originalLegacyTexts[id] = txt.text;
            }

            string baseText = _originalLegacyTexts[id];
            if (string.IsNullOrWhiteSpace(baseText))
            {
                return;
            }

            if (language == GameLanguage.English)
            {
                txt.text = baseText;
                return;
            }

            Dictionary<string, string> dict = language == GameLanguage.Ukrainian ? _ukrainianUI : _russianUI;
            string trimmed = baseText.Trim();

            if (dict.TryGetValue(trimmed, out string translation))
            {
                txt.text = translation;
                return;
            }

            foreach (KeyValuePair<string, string> entry in dict)
            {
                if (trimmed.Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                {
                    txt.text = entry.Value;
                    return;
                }
            }
        }

        private static void ParseJsonDictionary(string json, Dictionary<string, string> target)
        {
            int index = 0;
            while (index < json.Length)
            {
                int keyStart = json.IndexOf('\"', index);
                if (keyStart == -1) break;

                int keyEnd = FindClosingQuote(json, keyStart + 1);
                if (keyEnd == -1) break;

                string key = Unescape(json.Substring(keyStart + 1, keyEnd - keyStart - 1));

                int colon = json.IndexOf(':', keyEnd);
                if (colon == -1) break;

                int valStart = json.IndexOf('\"', colon);
                if (valStart == -1) break;

                int valEnd = FindClosingQuote(json, valStart + 1);
                if (valEnd == -1) break;

                string val = Unescape(json.Substring(valStart + 1, valEnd - valStart - 1));

                target[key] = val;
                index = valEnd + 1;
            }
        }

        private static int FindClosingQuote(string str, int start)
        {
            for (int i = start; i < str.Length; i++)
            {
                if (str[i] == '\"' && (i == 0 || str[i - 1] != '\\'))
                {
                    return i;
                }
            }
            return -1;
        }

        private static string Unescape(string s)
        {
            return s.Replace("\\\"", "\"")
                    .Replace("\\\\", "\\")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t");
        }
    }
}
