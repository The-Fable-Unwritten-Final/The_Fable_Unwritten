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

    public void Show(string keywordType, Vector2 screenPos, params object[] args)
    {
        if (panel == null || titleText == null || descText == null)
            return;

        string tableName = TooltipKeyMapper.GetTableName(keywordType);

        string nameKey = TooltipKeyMapper.GetNameKey(keywordType);
        string descKey = TooltipKeyMapper.GetDescKey(keywordType);

        string title = LocaleDataManager.GetLocalizedStringTable(tableName, nameKey);
        string desc = LocaleDataManager.GetLocalizedStringTable(tableName, descKey);

        if (args != null && args.Length > 0)
        {
            try
            {
                desc = string.Format(desc, args);
            }
            catch (System.FormatException)
            {
                Debug.LogWarning(
                    $"[StatusTooltipUI] Tooltip Format 실패 " +
                    $"keyword={keywordType}, desc={desc}, args={args.Length}"
                );
            }
        }

        titleText.text = title;
        descText.text = desc;

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        Canvas.ForceUpdateCanvases();

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
            "AttackUp" => "Type_Name_11",
            "DefendUp" => "Type_Name_12",
            "AttackDown" => "Type_Name_13",
            "DefendDown" => "Type_Name_14",

            "Burn" => "Type_Name_1",
            "Freeze" => "Type_Name_2",
            "Activate" => "Type_Name_3",
            "Bless" => "Type_Name_4",
            "Sin" => "Type_Name_5",
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

            // Marnas
            "RuneShell" => "Type_Name_23_01",

            // Parmano
            "Scroll" => "Type_Name_24_01",

            // Grolly
            "Predation" => "Type_Name_34_01",
            "Swallowed" => "Type_Name_34_02",

            // Elua
            "Soprano" => "Type_Name_33_01",
            "MezzoSoprano" => "Type_Name_33_02",
            "Alto" => "Type_Name_33_03",

            // Izkal
            "UnreadRune" => "Type_Name_25_01",
            "CurrentRune" => "Type_Name_25_02",
            "RuneStorm" => "Type_Name_25_03",
            "RuneTrace" => "Type_Name_25_04",

            // Luciel
            "Agape" => "Type_Name_35_01",
            "DivineVeil" => "Type_Name_35_02",
            "Devotion" => "Type_Name_35_03",
            "Obsession" => "Type_Name_35_04",
            "Balance" => "Type_Name_35_05",

            _ => keywordType
        };
    }

    public static string GetDescKey(string keywordType)
    {
        return keywordType switch
        {
            "AttackUp" => "Type_Text_11",
            "DefendUp" => "Type_Text_12",
            "AttackDown" => "Type_Text_13",
            "DefendDown" => "Type_Text_14",

            "Burn" => "Type_Text_1",
            "Freeze" => "Type_Text_2",
            "Activate" => "Type_Text_4",
            "Bless" => "Type_Text_3",
            "Sin" => "Type_Text_5",
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

            // Marnas
            "RuneShell" => "Type_Text_23_01",

            // Parmano
            "Scroll" => "Type_Text_24_01",

            // Grolly
            "Predation" => "Type_Text_34_01",
            "Swallowed" => "Type_Text_34_02",

            // Elua
            "Soprano" => "Type_Text_33_01",
            "MezzoSoprano" => "Type_Text_33_02",
            "Alto" => "Type_Text_33_03",

            // Izkal
            "UnreadRune" => "Type_Text_25_01",
            "CurrentRune" => "Type_Text_25_02",
            "RuneStorm" => "Type_Text_25_03",
            "RuneTrace" => "Type_Text_25_04",

            // Luciel
            "Agape" => "Type_Text_35_01",
            "DivineVeil" => "Type_Text_35_02",
            "Devotion" => "Type_Text_35_03",
            "Obsession" => "Type_Text_35_04",
            "Balance" => "Type_Text_35_05",

            _ => keywordType
        };
    }

    public static string GetTableName(string keywordType)
    {
        return keywordType switch
        {
            // Enemy Mechanic
            "RuneShell" => "Elite Table",
            "Scroll" => "Elite Table",

            "Predation" => "Elite Table",
            "Swallowed" => "Elite Table",

            "Soprano" => "Elite Table",
            "MezzoSoprano" => "Elite Table",
            "Alto" => "Elite Table",

            "UnreadRune" => "Elite Table",
            "CurrentRune" => "Elite Table",
            "RuneStorm" => "Elite Table",
            "RuneTrace" => "Elite Table",

            "Agape" => "Elite Table",
            "DivineVeil" => "Elite Table",
            "Devotion" => "Elite Table",
            "Obsession" => "Elite Table",
            "Balance" => "Elite Table",

            // 기존 상태이상은 그대로
            _ => "Card Tooltip"
        };
    }
}