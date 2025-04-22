using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ReduceNextCardCostEffect;


[CreateAssetMenu(menuName = "Card/CardModel")]
public class CardModel : ScriptableObject
{
    [Header("Card Identity")]
    public int index;
    public string cardName;                 //카드 이름
    public string cardText;                 // 카드 설명 필드 추가 (CSV의 text 대응)
    public string FlavorText;               // 카드의 배경적 설명
    public bool isOneUse = false;           //일회성 카드인지 확인
    public bool isMaintain = true;          //한턴 유지 카드인지 확인

    [Header("Core Cost")]
    public int manaCost;                    //카드 코스트
    public int temporaryCostModifier = 0;       //할인 값 확인용
    public int persistentCostModifier = 0;
    public bool consumesDiscountOnce = false;

    [Header("Card Meta")]
    public CharacterClass characterClass;   //누구의 카드인지
    public CardType type;           //카드 타입(전격, 힐 등)
    public int targetCount;         //카드 지정 개수
    public TargetType targetType;   //아군 한정, 적군 한정 등
    public string characterStance;  //자세에 따른 추가 효과 기대   
    public string note;             //특수 효과

    [Header("Visuals")]
    public Sprite illustration;   // 일러스트 이미지 이름
    public Sprite chClass;       //캐릭터 클래스에 따른 이미지
    public Sprite cardType;      //카드 타입에 따른 이미지
    public string cardImage;      // 카드 프레임 또는 배경 이미지
    public Sprite cardFrame;         //카드 프레임 이미지

    [Header("Card Effects")]
    public List<CardEffectBase> effects = new();    //어떤 효과를 가졌는지

    public bool isUnlocked = true;              //카드가 해금 되었는지


    // ==== 사용 조건 및 비용 ====


    /// <summary>
    /// 현재 마나로 카드 사용한지 확인하는 코드
    /// </summary>
    /// <param name="currentMana">현재 마나</param>
    /// <returns>사용 가능한지 아닌지 bool 값으로 반환</returns>
    public bool IsUsable(int currentMana) => currentMana >= GetEffectiveCost();

    public int GetEffectiveCost()
    {
        int totalDiscount = temporaryCostModifier + persistentCostModifier;
        return Mathf.Max(0, manaCost - totalDiscount);
    }

    // ==== 카드 사용 ====

    public void Play(IStatusReceiver caster, IStatusReceiver target)
    {
        foreach (var effect in effects)
            effect.Apply(caster, target);

        GameManager.Instance.combatUIController.CardStatusUpdate?.Invoke();
    }

    // ==== 타겟 유효성 ====

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


    // ==== 할인 제어 ====

    public void ApplyTemporaryDiscount(int amount)
    {
        temporaryCostModifier += amount;
        consumesDiscountOnce = true;
    }

    public void ApplyPersistentDiscount(int amount)
    {
        persistentCostModifier += amount;
    }

    public void ClearTemporaryDiscount()
    {
        if (consumesDiscountOnce)
        {
            temporaryCostModifier = 0;
            consumesDiscountOnce = false;
        }
    }

    public void ClearAllDiscount()
    {
        temporaryCostModifier = 0;
        persistentCostModifier = 0;
        consumesDiscountOnce = false;
    }
    public bool HasAnyDiscount() => temporaryCostModifier > 0 || persistentCostModifier > 0;
}
