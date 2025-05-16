using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(menuName = "Card/CardModel")]
public class CardModel : ScriptableObject
{
    [Header("Card Identity")]
    public int index;
    public string cardName;                 //카드 이름
    public string cardText;                 // 카드 설명 필드 추가 (CSV의 text 대응)
    public string FlavorText;               // 카드의 배경적 설명
    public bool isOneUse { get; set; }           //일회성 카드인지 확인
    public bool isMaintain { get; set; }          //한턴 유지 카드인지 확인

    [Header("Core Cost")]
    public int manaCost;                    //카드 코스트
    private int temporaryCostModifier;       //할인 값 확인용
    private int persistentCostModifier = 0;
    private bool consumesDiscountOnce = false;

    public bool ConsumesDiscountOnce { get => consumesDiscountOnce; }

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

    [Header("Skill Effect")]
    public string skillEffectName;   // 스킬 이펙트 이름

    public bool isUnlocked = true;              //카드가 해금 되었는지

    public bool isEnhanced = false;                 //카드가 연계효과로 강화 되었는지

    // ==== 사용 조건 및 비용 ====

    private void OnEnable()
    {
        isMaintain = true;
        isOneUse = false;
    }


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

    public void Play(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        GameManager.Instance.StartCoroutine(PlayWithAnimation(caster, targets));
    }

    private IEnumerator PlayWithAnimation(IStatusReceiver caster, List<IStatusReceiver> targets)
    {
        float totalDuration = 2f;  // 카메라 줌인 + 줌아웃 포함 총 연출 시간

        // 1. 카메라 연출
        GameManager.Instance.combatCameraController.PlayCombatCamera(caster, targets, totalDuration);


        // 2. 공격 애니메이션
        yield return new WaitForSeconds(0.2f); // 애니메이션 길이에 맞게 조정
        caster.PlayAttackAnimation(); //시전자의 공격 애니메이션 적용
        SoundManager.Instance.PlaySFX(SoundCategory.Card, (int)type);

        yield return new WaitForSeconds(0.2f); // 애니메이션 길이에 맞게 조정

        // 3. 이펙트 재생 + 피격 애니메이션 동시에 진행
        if (!string.IsNullOrEmpty(skillEffectName) && targets.Count > 0)
        {
            foreach (var t in targets)
            {
                float scaleFactor = DetermineEffectScale(GetEffectiveCost());

                var effectAnim = DataManager.Instance.CardEffects.TryGetValue(skillEffectName, out var animInfo) ? animInfo : null;

                if (effectAnim == null)
                    continue;

                if (effectAnim.animationType == AnimationType.Projectile)
                {
                    //  Projectile → 이펙트 끝나고 Hit 처리
                    GameManager.Instance.turnController.battleFlow.effectManage.PlayProjectileEffect(
                        skillEffectName, caster.CachedTransform, t.CachedTransform, scaleFactor,
                        () =>
                        {
                            if (effects.Exists(e => e.isTriggerHitAnim) && t.IsAlive())
                                t.PlayHitAnimation();
                        }
                    );
                }
                else
                {
                    // 일반 이펙트 → 즉시 재생 + Hit
                    GameManager.Instance.turnController.battleFlow.effectManage.PlayEffect(
                        skillEffectName, caster.CachedTransform, t.CachedTransform, false, scaleFactor
                    );

                    if (effects.Exists(e => e.isTriggerHitAnim) && t.IsAlive())
                        t.PlayHitAnimation();
                }
            }
        }
        yield return new WaitForSeconds(0.9f); // 이펙트와 피격 연출 대기


        // 4. 효과 적용
        foreach (var effect in effects)
            effect.Apply(caster, targets, isEnhanced);

        yield return new WaitForSeconds(0.2f); // 효과 적용 후 약간 대기
        GameManager.Instance.combatUIController.CardStatusUpdate?.Invoke();
    }

    private float DetermineEffectScale(int cost)
    {
        float baseScale = 1f;

        // 일반 + 카드 코스트 기준
        if (cost <= 1) return baseScale * 0.5f;
        if (cost <= 3) return baseScale;
        return baseScale * 1.5f;
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

    public string GetFormattedCardText(IStatusReceiver caster)
    {
        string result = cardText;

        foreach (var effect in effects)
        {
            if(effect is DamageEffect damageEffect)
            {
                Match match = Regex.Match(result, @"(\d+)(?=의 피해)");

                if (match.Success)
                {
                    int baseDamage = int.Parse(match.Value);

                    // 2. 공격자(caster) 기준으로 예측 피해 계산
                    float predicted = (caster != null)
                        ? damageEffect.PredictPureDamage(caster)
                        : baseDamage;

                    // 3. "피해 숫자"만 교체
                    if (isEnhanced)
                    {
                        predicted *= 1.5f;      //소수점 남기나?
                        result = Regex.Replace(result, @"(\d+)(?=의 피해)", $"‘{(int)predicted}’");
                    }
                    else
                    {
                        result = Regex.Replace(result, @"(\d+)(?=의 피해)", ((int)predicted).ToString());
                    }
                }
            }
            // 필요한 경우 atk_buff, def_buff 별도 처리 가능
        }
        return result;
    }

    public void UpdateEnhancedState()
    {
        isEnhanced = BattleLogManager.Instance.isEnhanced(this);
    }
}
