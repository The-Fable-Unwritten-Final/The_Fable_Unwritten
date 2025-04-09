using System.Collections.Generic;
using UnityEngine;

public enum TargetType { None = 0, Ally = 1, Enemy = 2 }

public enum CardType
{
    Fire = 0,
    Ice = 1,
    Electric = 2,
    Nature = 3,
    Buff = 4,
    Debuff = 5,
    Holy = 6,
    Heal = 7,
    Slash = 8,
    Strike = 9,
    Pierce = 10,
    Defense = 11
}

[CreateAssetMenu(menuName = "Card/CardModel")]
public class CardModel : ScriptableObject
{
    public int index;
    public string cardName;                 //카드 이름
    public string cardText;                 // 카드 설명 필드 추가 (CSV의 text 대응)
    public int manaCost;                    //카드 코스트
    public CharacterClass characterClass;   //누구의 카드인지

    public string illustration;   // 일러스트 이미지 이름
    public string cardImage;      // 카드 프레임 또는 배경 이미지

    public CardType type;

    public int targetCount;
    public TargetType targetType;
    public string characterStance;
    public string note;

    public List<CardEffectBase> effects = new();    //어떤 효과를 가졌는지


    /// <summary>
    /// 현재 마나로 카드 사용한지 확인하는 코드
    /// </summary>
    /// <param name="currentMana">현재 마나</param>
    /// <returns>사용 가능한지 아닌지 bool 값으로 반환</returns>
    public bool IsUsable(int currentMana) => currentMana >= manaCost;

    public void Play(IStatusReceiver caster, IStatusReceiver target)
    {
        foreach (var effect in effects)
            effect.Apply(caster, target);
    }
}
