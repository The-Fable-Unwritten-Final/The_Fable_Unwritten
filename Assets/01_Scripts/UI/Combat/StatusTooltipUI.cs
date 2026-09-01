using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusTooltipUI : MonoSingleton<StatusTooltipUI>
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;

    private RectTransform panelRect;

    private void Awake()
    {
        panelRect = panel.GetComponent<RectTransform>();
    }

    private void Start()
    {
        Hide();
    }

    public void Show(string keywordType, Vector2 screenPos)
    {
        if (panel == null || titleText == null || descText == null)
            return;

        string nameKey = TooltipKeyMapper.GetNameKey(keywordType);
        string descKey = TooltipKeyMapper.GetDescKey(keywordType);

        titleText.text = LocaleDataManager.GetLocalizedStringTable("Card Tooltip", nameKey);
        descText.text = LocaleDataManager.GetLocalizedStringTable("Card Tooltip", descKey);

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        Vector2 pos = screenPos;

        float width = panelRect.rect.width;
        float height = panelRect.rect.height;

        if (pos.x + width > Screen.width)
            pos.x = Screen.width - width;

        if (pos.y - height < 0)
            pos.y = height;

        panelRect.position = pos;
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}


public static class TooltipKeyMapper
{
    public static string GetNameKey(string keywordType)
    {
        return keywordType switch
        {
            "Burn" => "Type_Name_1",
            "Freeze" => "Type_Name_2",
            "Activate" => "Type_Name_3",
            "Bless" => "Type_Name_4",
            "Crime" => "Type_Name_5",
            "Penance" => "Type_Name_6",
            "Scar" => "Type_Name_7",
            "Stun" => "Type_Name_8",
            "Guard" => "Type_Name_9",

            "Exhaust" => "Keyword_Name_1",
            "Retain" => "Keyword_Name_2",
            "Temporary" => "Keyword_Name_3",
            "Copy" => "Keyword_Name_4",
            "Innate" => "Keyword_Name_5",
            "Kill" => "Keyword_Name_6",
            "Grow" => "Keyword_Name_7",
            "Critical" => "Keyword_Name_8",
            "Switch" => "Keyword_Name_9",

            "Seek" => "Stance_Name_1",
            "Insight" => "Stance_Name_2",
            "Mercy" => "Stance_Name_3",
            "Discipline" => "Stance_Name_4",
            "Rush" => "Stance_Name_5",
            "Defense" => "Stance_Name_6",

            _ => keywordType
        };
    }

    public static string GetDescKey(string keywordType)
    {
        return keywordType switch
        {
            "Burn" => "Type_Text_1",
            "Freeze" => "Type_Text_2",
            "Activate" => "Type_Text_4",
            "Bless" => "Type_Text_3",
            "Crime" => "Type_Text_5",
            "Penance" => "Type_Text_6",
            "Scar" => "Type_Text_7",
            "Stun" => "Type_Text_8",
            "Guard" => "Type_Text_9",

            "Exhaust" => "Keyword_Text_1",
            "Retain" => "Keyword_Text_2",
            "Temporary" => "Keyword_Text_3",
            "Copy" => "Keyword_Text_4",
            "Innate" => "Keyword_Text_5",
            "Kill" => "Keyword_Text_6",
            "Grow" => "Keyword_Text_7",
            "Critical" => "Keyword_Text_8",
            "Switch" => "Keyword_Text_9",

            "Seek" => "Stance_Text_1",
            "Insight" => "Stance_Text_2",
            "Mercy" => "Stance_Text_3",
            "Discipline" => "Stance_Text_4",
            "Rush" => "Stance_Text_5",
            "Defense" => "Stance_Text_6",

            _ => keywordType
        };
    }
}