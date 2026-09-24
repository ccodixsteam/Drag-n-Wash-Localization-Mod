using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Yarn.Unity;

namespace DragNWashLocalization
{
    public static class DialogueManager
    {
        private static readonly Dictionary<string, string> _originalStrings = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> _russianStrings = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> _ukrainianStrings = new Dictionary<string, string>(StringComparer.Ordinal);
        private static bool _loaded = false;

        public static void Load(string dataPath)
        {
            if (_loaded)
            {
                return;
            }

            LoadFile(Path.Combine(dataPath, "dialogues_ru.json"), _russianStrings);
            LoadFile(Path.Combine(dataPath, "dialogues_ua.json"), _ukrainianStrings);
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
                Debug.LogError("[LocalizationMod] Error loading dialogue file " + path + ": " + ex.Message);
            }
        }

        public static void ApplyLanguage(GameLanguage language)
        {
            if (!_loaded)
            {
                Load(LanguageManager.DataDirectory);
            }

            try
            {
                Yarn.Unity.Localization[] locs = Resources.FindObjectsOfTypeAll<Yarn.Unity.Localization>();
                foreach (Yarn.Unity.Localization loc in locs)
                {
                    ApplyToLocalization(loc, language);
                }

                YarnProject[] projects = Resources.FindObjectsOfTypeAll<YarnProject>();
                foreach (YarnProject proj in projects)
                {
                    if (proj.baseLocalization != null)
                    {
                        ApplyToLocalization(proj.baseLocalization, language);
                    }
                    if (proj.localizations != null)
                    {
                        foreach (KeyValuePair<string, Yarn.Unity.Localization> pair in proj.localizations)
                        {
                            if (pair.Value != null)
                            {
                                ApplyToLocalization(pair.Value, language);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error applying dialogue translations: " + ex.Message);
            }
        }

        private static void ApplyToLocalization(Yarn.Unity.Localization loc, GameLanguage language)
        {
            if (loc == null)
            {
                return;
            }

            if (_originalStrings.Count == 0)
            {
                BackupOriginals(loc);
            }

            Dictionary<string, string>? sourceMap = null;
            if (language == GameLanguage.Russian)
            {
                sourceMap = _russianStrings;
            }
            else if (language == GameLanguage.Ukrainian)
            {
                sourceMap = _ukrainianStrings;
            }
            else
            {
                sourceMap = _originalStrings;
            }

            if (sourceMap != null && sourceMap.Count > 0)
            {
                var field = typeof(Yarn.Unity.Localization).GetField("_runtimeStringTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?? typeof(Yarn.Unity.Localization).GetField("_stringTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Dictionary<string, string>? table = field?.GetValue(loc) as Dictionary<string, string>;

                if (table == null && field != null)
                {
                    table = new Dictionary<string, string>();
                    field.SetValue(loc, table);
                }

                foreach (KeyValuePair<string, string> entry in sourceMap)
                {
                    try
                    {
                        if (table != null)
                        {
                            table[entry.Key] = entry.Value;
                        }
                        else if (loc.ContainsLocalizedString(entry.Key))
                        {
                            table = field?.GetValue(loc) as Dictionary<string, string>;
                            if (table != null)
                            {
                                table[entry.Key] = entry.Value;
                            }
                        }
                        else
                        {
                            loc.AddLocalizedString(entry.Key, entry.Value);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void BackupOriginals(Yarn.Unity.Localization loc)
        {
            try
            {
                foreach (string id in loc.GetLineIDs())
                {
                    if (!_originalStrings.ContainsKey(id))
                    {
                        try
                        {
                            string? val = loc.GetLocalizedString(id);
                            if (val != null)
                            {
                                _originalStrings[id] = val;
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                var field = typeof(Yarn.Unity.Localization).GetField("_runtimeStringTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?? typeof(Yarn.Unity.Localization).GetField("_stringTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var table = field.GetValue(loc) as Dictionary<string, string>;
                    if (table != null)
                    {
                        foreach (var kvp in table)
                        {
                            if (!_originalStrings.ContainsKey(kvp.Key))
                            {
                                _originalStrings[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error backing up Yarn strings: " + ex.Message);
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
