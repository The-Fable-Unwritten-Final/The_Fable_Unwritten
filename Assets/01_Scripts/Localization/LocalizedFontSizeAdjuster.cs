using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedFontSizeAdjuster : MonoBehaviour
{
    public float fontSize_EN;
    public float fontSize_KO;
    public float fontSize_JA;

    private TMP_Text _text;

    void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        ApplyFontSize(LocalizationSettings.SelectedLocale);
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        ApplyFontSize(locale);
    }

    private void ApplyFontSize(Locale locale)
    {
        string code = locale.Identifier.Code.ToLower();

        switch (code)
        {
            case "ko":
                _text.fontSize = fontSize_KO;
                break;
            case "ja":
                _text.fontSize = fontSize_JA;
                break;
            case "en":
                _text.fontSize = fontSize_EN;
                break;
            default:
                _text.fontSize = fontSize_EN;
                break;
        }
    }
}
