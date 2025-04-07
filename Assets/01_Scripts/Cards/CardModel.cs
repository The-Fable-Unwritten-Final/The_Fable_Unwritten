using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardModel")]
public class CardModel : ScriptableObject
{
    public string cardName;                 //카드 이름
    public int manaCost;                    //카드 코스트
    public CharacterClass characterClass;   //누구의 카드인지
    public List<CardEffectBase> effects;    //어떤 효과를 가졌는지


    /// <summary>
    /// 현재 마나로 카드 사용한지 확인하는 코드
    /// </summary>
    /// <param name="currentMana">현재 마나</param>
    /// <returns>사용 가능한지 아닌지 bool 값으로 반환</returns>
    public bool IsUsable(int currentMana) => currentMana >= manaCost;
}
