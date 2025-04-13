using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(menuName = "Card/CardModel")]
public class CardModel : ScriptableObject
{
    public int index;
    public string cardName;                 //카드 이름
    public string cardText;                 // 카드 설명 필드 추가 (CSV의 text 대응)
    public int manaCost;                    //카드 코스트
    public CharacterClass characterClass;   //누구의 카드인지

    public Image illustration;   // 일러스트 이미지 이름
    public Image chClass;       //캐릭터 클래스에 따른 이미지
    public Image cardType;      //카드 타입에 따른 이미지
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

        GameManager.Instance.combatUIController.CardStatusUpdate?.Invoke();
    }

    /// <summary>
    /// 카드 사용자가 이 카드를 사용할 수 있는지 확인하는 카드
    /// </summary>
    /// <param name="casterClass"></param>
    /// <returns></returns>
    public bool CanBeUsedBy(CharacterClass casterClass)
    {
        return characterClass == casterClass;
        // 혹은 None이나 공용 카드 처리도 가능
    }


    /// <summary>
    /// 지정한 타겟이 카드의 타겟과 일치하는지 확인하는 함수
    /// </summary>
    /// <param name="caster">시전자</param>
    /// <param name="target">타겟</param>
    /// <returns>시전이 가능한지 결과 반환</returns>
    public bool IsTargetValid(IStatusReceiver caster, IStatusReceiver target)
    {
        if (targetType == TargetType.None)
            return true;

        bool casterIsEnemy = caster.ChClass == CharacterClass.Enemy;
        bool targetIsEnemy = target.ChClass == CharacterClass.Enemy;

        bool isSameTeam = casterIsEnemy == targetIsEnemy;

        if (targetType == TargetType.Ally)
            return isSameTeam;

        if (targetType == TargetType.Enemy)
            return !isSameTeam;

        return false;
    }
}
