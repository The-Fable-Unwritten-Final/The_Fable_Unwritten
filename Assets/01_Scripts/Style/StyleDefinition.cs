using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Styles/StyleDefinition")]
public class StyleDefinition : ScriptableObject
{
    [Header("Meta")]
    public string styleId;                  // 스타일 고유 ID
    public string displayName;              // 스타일 이름
    public string description;              // 스타일 설명 (플레이버 텍스트)
    public int maxPlusTier = 3;             // 최대 +강화 단계 (고정 or 3)
    public int maxMinusTier = 3;            // 최대 -강화 단계 (고정 or 3)

    [Header("1단계 기본 효과")]
    public List<StyleEffect> basePlusEffects = new();
    public List<StyleEffect> baseMinusEffects = new();

    [Header("+ 효과 단계별 목록")]
    public List<TierEntry> plusTiers = new();  // length <= maxPlusTier

    [Header("- 효과 단계별 목록")]
    public List<TierEntry> minusTiers = new(); // length <= maxMinusTier

    [Serializable]
    public class TierEntry {
        public int tier; // 강화 단계
        public int cost; // 잉크 소모 비용
        public List<StyleEffect> effects = new();
    }
}
