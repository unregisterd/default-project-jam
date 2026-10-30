using System;
using UnityEngine;

[Serializable]
public class LocSpeaker
{
    public string zh;
    public string en;
}

[Serializable]
public class LocText
{
    public string zh;
    public string en;
}

[Serializable]
public class LocEntry
{
    public string id;
    public string type;
    public LocSpeaker speaker;
    public LocText text;
    public string note;
}

[Serializable]
public class LocDocument
{
    public LocEntry[] texts;
}

public static class Loc
{
    private const string ResourcePath = "Localization/texts";
    private const string LanguagePrefsKey = "loc_language";
    private const string DefaultLanguage = "en";

    private static LocDocument _document;
    private static bool _loaded;

    private static string _language;

    public static string Language
    {
        get
        {
            if (string.IsNullOrEmpty(_language))
            {
                _language = PlayerPrefs.GetString(LanguagePrefsKey, DefaultLanguage);
            }

            return _language;
        }
    }

    public static event Action LanguageChanged;

    public static void SetLanguage(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return;
        }

        bool changed = Language != language;
        _language = language;
        PlayerPrefs.SetString(LanguagePrefsKey, language);
        PlayerPrefs.Save();

        if (changed)
        {
            LanguageChanged?.Invoke();
        }
    }

    public static void ToggleLanguage()
    {
        SetLanguage(Language == "zh" ? "en" : "zh");
    }

    public static LocEntry Get(string id)
    {
        Load();

        if (_document == null || _document.texts == null || string.IsNullOrEmpty(id))
        {
            return null;
        }

        for (int i = 0; i < _document.texts.Length; i++)
        {
            if (_document.texts[i] != null && _document.texts[i].id == id)
            {
                return _document.texts[i];
            }
        }

        return null;
    }

    public static string Text(string id)
    {
        LocEntry entry = Get(id);

        if (entry == null || entry.text == null)
        {
            return id;
        }

        string value = Language == "en" ? entry.text.en : entry.text.zh;
        return string.IsNullOrEmpty(value) ? entry.text.zh : value;
    }

    public static string Speaker(string id)
    {
        LocEntry entry = Get(id);

        if (entry == null || entry.speaker == null)
        {
            return string.Empty;
        }

        string value = Language == "en" ? entry.speaker.en : entry.speaker.zh;
        return string.IsNullOrEmpty(value) ? entry.speaker.zh : value;
    }

    private static void Load()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        TextAsset asset = Resources.Load<TextAsset>(ResourcePath);

        if (asset == null)
        {
            Debug.LogWarning($"[Loc] 找不到 Resources/{ResourcePath}.json");
            return;
        }

        _document = JsonUtility.FromJson<LocDocument>(asset.text);
    }
}
