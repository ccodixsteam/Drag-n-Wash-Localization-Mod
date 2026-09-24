using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace DragNWashLocalization
{
    public static class LanguageManager
    {
        public static GameLanguage CurrentLanguage { get; private set; } = GameLanguage.English;
        public static event Action<GameLanguage>? OnLanguageChanged;

        private static string _dataDirectory = string.Empty;
        private static bool _initialized = false;

        public static string DataDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_dataDirectory))
                {
                    string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                    string subData = Path.Combine(assemblyDir, "data");
                    _dataDirectory = Directory.Exists(subData) ? subData : assemblyDir;
                }
                return _dataDirectory;
            }
        }

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            string configPath = Path.Combine(DataDirectory, "selected_language.txt");
            if (File.Exists(configPath))
            {
                try
                {
                    string text = File.ReadAllText(configPath).Trim();
                    if (Enum.TryParse(text, true, out GameLanguage savedLang))
                    {
                        CurrentLanguage = savedLang;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("[LocalizationMod] Error loading language config: " + ex.Message);
                }
            }
        }

        public static void SetLanguage(GameLanguage newLanguage)
        {
            if (CurrentLanguage == newLanguage)
            {
                return;
            }

            CurrentLanguage = newLanguage;

            try
            {
                string configPath = Path.Combine(DataDirectory, "selected_language.txt");
                File.WriteAllText(configPath, CurrentLanguage.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalizationMod] Error saving language config: " + ex.Message);
            }

            if (CurrentLanguage != GameLanguage.English)
            {
                FontManager.ApplyToAllFonts();
            }

            DialogueManager.ApplyLanguage(CurrentLanguage);
            UIManager.ApplyLanguage(CurrentLanguage);
            TextureManager.ApplyLanguage(CurrentLanguage);
            MenuCredits.UpdateCredits();

            OnLanguageChanged?.Invoke(CurrentLanguage);
        }
    }
}
