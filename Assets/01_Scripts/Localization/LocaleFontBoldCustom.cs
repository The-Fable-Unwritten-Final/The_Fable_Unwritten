using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

[RequireComponent(typeof(TMP_Text))]
public class LocaleFontBoldCustom : MonoBehaviour
{
    public bool bold_EN = false;
    public bool bold_KO = false;
    public bool bold_JA = false;

    [SerializeField]private TMP_Text _text;

    void Awake()
    {
        if(_text == null)
            _text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        ApplyFontStyle(LocalizationSettings.SelectedLocale);
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        ApplyFontStyle(locale);
    }

    private void ApplyFontStyle(Locale locale)
    {
        string code = locale.Identifier.Code.ToLower();
        bool isBold = false;

        switch (code)
        {
            case "ko":
                isBold = bold_KO;
                break;
            case "ja":
                isBold = bold_JA;
                break;
            case "en":
                isBold = bold_EN;
                break;
            default:
                // 디폴트 영어
                isBold = bold_EN;
                break;
        }

        // 볼드 적용
        if (isBold)
        {
            _text.fontStyle = FontStyles.Bold;
        }
        else
        {
            _text.fontStyle = FontStyles.Normal;
        }
    }
}
