using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Styles/StyleDefinition")]
public class StyleDefinition : ScriptableObject
{
    public enum StyleRank
    {
        High = 1,
        Medium = 2,
        Low = 3
    }

    [Header("Meta")]
    public int styleId;                  // 스타일 고유 ID
    public string displayName;              // 스타일 이름
    public string description;              // 스타일 설명 (플레이버 텍스트)
    public string plusEffectDescription;    // + 효과 설명 텍스트 (로컬라이제이션을 적용 시 사용하는 key 값 설명 => ~~~ n 만큼 증가 같은 포멧 대응 가능하도록, 로컬라이제이션 쪽 별도 처리 필요)
    public string minusEffectEffectDesc;    // - 효과 설명 
    [Tooltip("문체 등급 => 1: 상급, 2: 중급, 3: 하급")]
    public StyleRank rank = StyleRank.High; // 등급 (1 == 상급, 2 == 중급, 3 == 하급 문체)
    public int maxPlusLevel = 3;             // 최대 +강화 단계 (고정 1단계 or 3단계 까지)
    public int currentPlus = 1;             // 현재 +강화 단계
    public int maxMinusLevel = 3;            // 최대 -강화 단계 (고정 1단계 or 3단계 까지)
    public int currentMinus = 1;            // 현재 -강화 단계

    [Header("+ 효과 단계별 목록")]
    public List<TierEntry> plusTiers = new();

    [Header("- 효과 단계별 목록")]
    public List<TierEntry> minusTiers = new();

    public bool isUnlocked;

    [Serializable]
    public class TierEntry
    {
        public int level; // 강화 단계
        public int cost; // 잉크 소모 비용
        public List<StyleEffect> effects = new();
    }
    public void ResetProgress()
    {
        currentPlus = 1;
        currentMinus = 1;
    }
}
