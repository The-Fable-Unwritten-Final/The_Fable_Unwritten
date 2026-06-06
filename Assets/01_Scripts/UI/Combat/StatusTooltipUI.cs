using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusTooltipUI : MonoSingleton<StatusTooltipUI>
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;

    private void Start()
    {
        panel.SetActive(false);
    }

    public void Show(string keywordType, Vector2 screenPos)
    {
        if (panel == null) return;

        string nameKey = TooltipKeyMapper.GetNameKey(keywordType);
        string descKey = TooltipKeyMapper.GetDescKey(keywordType);

        titleText.text = TempKeywordKrDB.Get(nameKey);
        descText.text = TempKeywordKrDB.Get(descKey);

        panel.SetActive(true);
        ((RectTransform)transform).position = screenPos + new Vector2(20f, -20f);
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
            "Active" => "Type_Name_3",
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

            _ => keywordType
        };
    }

    public static string GetDescKey(string keywordType)
    {
        return keywordType switch
        {
            "Burn" => "Type_Text_1",
            "Freeze" => "Type_Text_2",
            "Active" => "Type_Text_3",
            "Bless" => "Type_Text_4",
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

            _ => keywordType
        };
    }
}

public static class TempKeywordKrDB
{
    private static readonly Dictionary<string, string> kr = new()
    {
        ["Type_Name_1"] = "화상",
        ["Type_Text_1"] = "턴 시작 시 수치만큼 피해를 입고, 발동 후 수치는 절반으로 감소",

        ["Type_Name_2"] = "빙결",
        ["Type_Text_2"] = "공격 피해가 수치만큼 감소하며 턴 종료 시 사라짐",

        ["Type_Name_3"] = "활성",
        ["Type_Text_3"] = "다음에 적용되는 모든 상태이상의 수치를 증가시키고 발동 후 사라짐",

        ["Type_Name_4"] = "축복",
        ["Type_Text_4"] = "이로운 효과의 수치를 증가시키며 없을 경우 다음 이로운 효과에 적용",

        ["Type_Name_5"] = "죄악",
        ["Type_Text_5"] = "턴 종료 시 수치만큼 피해를 입음",

        ["Type_Name_6"] = "참회",
        ["Type_Text_6"] = "해로운 효과의 수치를 감소시키며 없을 경우 다음 해로운 효과에 적용",

        ["Type_Name_7"] = "상처",
        ["Type_Text_7"] = "강타 카드가 수치만큼 추가 피해를 줌",

        ["Type_Name_8"] = "기절",
        ["Type_Text_8"] = "확률적으로 1턴 동안 행동할 수 없게 함",

        ["Type_Name_9"] = "수호",
        ["Type_Text_9"] = "적의 공격을 자신에게 유도하고 받는 피해를 감소",

        ["Keyword_Name_1"] = "소멸",
        ["Keyword_Text_1"] = "사용 시 해당 전투동안 덱에서 제외",

        ["Keyword_Name_2"] = "보존",
        ["Keyword_Text_2"] = "턴 종료 시 패에 유지",

        ["Keyword_Name_3"] = "증발",
        ["Keyword_Text_3"] = "미사용 시 해당 전투동안 덱에서 제외",

        ["Keyword_Name_4"] = "복사",
        ["Keyword_Text_4"] = "사용 시 동일한 카드 생성",

        ["Keyword_Name_5"] = "개전",
        ["Keyword_Text_5"] = "전투 시작 시 반드시 패에 드로우",

        ["Keyword_Name_6"] = "결정타",
        ["Keyword_Text_6"] = "",

        ["Keyword_Name_7"] = "성장",
        ["Keyword_Text_7"] = "{0}회 사용 시 카드 진화",

        ["Keyword_Name_8"] = "강타",
        ["Keyword_Text_8"] = "상처만큼 추가 피해",

        ["Keyword_Name_9"] = "전환",
        ["Keyword_Text_9"] = "스탠스를 {0}로 변경",

        ["Stance_Name_1"] = "탐구",
        ["Stance_Text_1"] = "카드 1장을 드로우하고, 1턴 동안 그 카드의 비용이 0이 됩니다.",

        ["Stance_Name_2"] = "통찰",
        ["Stance_Text_2"] = "다음에 사용하는 카드의 효과가 2번 적용됩니다.",

        ["Stance_Name_3"] = "자비",
        ["Stance_Text_3"] = "카드 1장을 드로우하고, 이번 턴 회복량이 30% 증가합니다.",

        ["Stance_Name_4"] = "규율",
        ["Stance_Text_4"] = "다음 공격 시, 부여하는 정화 수치만큼 아군의 공격력과 방어력이 증가합니다.",

        ["Stance_Name_5"] = "돌진",
        ["Stance_Text_5"] = "다음 공격의 피해가 100% 증가합니다.",

        ["Stance_Name_6"] = "수비",
        ["Stance_Text_6"] = "최대 체력이 20% 증가하고, 수호를 15 얻습니다."
    };

    public static string Get(string key)
    {
        return kr.TryGetValue(key, out var value) ? value : key;
    }
}