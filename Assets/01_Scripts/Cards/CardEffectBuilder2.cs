using System;
using System.Collections.Generic;
using UnityEngine;
using static PlayerData;

public static class CardEffectBuilder2
{
    private const string path = "Cards/Effects";

    public static List<CardEffectBase> Build(CardJsonData2 data)
    {
        var effects = new List<CardEffectBase>();

        if (data == null || data.effects == null)
            return effects;

        foreach (var effect in data.effects)
        {
            var built = BuildEffectRecursive(effect, data.index);
            if (built != null)
                effects.Add(built);
        }

        return effects;
    }

    private static int ParseInt(string value, int defaultValue = 0)
    {
        if (int.TryParse(value, out int result))
            return result;
        return defaultValue;
    }

    private static CardType ParseCardType(string value)
    {
        return value switch
        {
            "Fire" => CardType.Fire,
            "Ice" => CardType.Ice,
            "Nature" => CardType.Nature,
            "Prayer" => CardType.Pray,
            "Holy" => CardType.Holy,
            "Baptism" => CardType.baptism,
            "Slash" => CardType.Slash,
            "Strike" => CardType.Strike,
            "Defense" => CardType.Defense,
            "Taboo" => CardType.Taboo,
            _ => throw new Exception($"Unknown CardType string: {value}")
        };
    }

    private static StancType ParseStance(string value)
    {
        return value switch
        {
            "Seek" => StancType.Seek,
            "Insight" => StancType.Insight,
            "Mercy" => StancType.Mercy,
            "Discipline" => StancType.Discipline,
            "Rush" => StancType.Rush,
            "Defense" => StancType.Defense,
            _ => throw new Exception($"Unknown Stance string: {value}")
        };
    }

    private static BuffStatType ParseBuffStatType(string value)
    {
        return value switch
        {
            "atk" => BuffStatType.Attack,
            "def" => BuffStatType.Defend,
            "burn" => BuffStatType.Burn,
            "freeze" => BuffStatType.Freeze,
            "active" => BuffStatType.Activate,
            "activate" => BuffStatType.Activate,
            "bless" => BuffStatType.Bless,
            "sin" => BuffStatType.Crime,
            "crime" => BuffStatType.Crime,
            "penance" => BuffStatType.Penance,
            "grace" => BuffStatType.Penance,
            "scar" => BuffStatType.Scar,
            "bleed" => BuffStatType.Scar,
            "stun" => BuffStatType.Stun,
            "guard" => BuffStatType.Guard,
            _ => BuffStatType.None
        };
    }

    private static CardEffectBase BuildEffectRecursive(CardEffect2 effectData, int cardIndex)
    {
        if (effectData == null)
            return null;

        CardEffectBase effect = null;

        switch (effectData.type)
        {
            case "damage":
                {
                    var e = Load<DamageEffect>("DamageEffect");
                    if (e == null) return null;
                    e.amount = effectData.value;
                    effect = e;
                    break;
                }

            case "heal":
                {
                    var e = Load<HealEffect>("HealEffect");
                    if (e == null) return null;
                    e.amount = effectData.value;
                    e.target = effectData.target;
                    effect = e;
                    break;
                }

            case "draw":
                {
                    var e = Load<DrawCardEffect>("DrawCardEffect");
                    if (e == null) return null;
                    e.amount = effectData.value;
                    e.target = effectData.target;
                    effect = e;
                    break;
                }

            case "discard":
                {
                    var e = Load<DiscardCardEffect>("DiscardCardEffect");
                    if (e == null) return null;
                    e.discardCount = effectData.value;
                    effect = e;
                    break;
                }

            case "duplicate":
                {
                    var e = Load<DuplicateCardEffect>("DuplicateCardEffect");
                    if (e == null) return null;
                    e.duplicateNum = effectData.value;
                    effect = e;
                    break;
                }

            case "applyDamage":
                {
                    var e = Load<ApplyDamageEffect>("ApplyDamageEffect");
                    if (e == null) return null;
                    e.value = effectData.value;
                    e.index = cardIndex;
                    effect = e;
                    break;
                }

            case "self_damage":
                {
                    var e = Load<SelfDamageEffect>("SelfDamageEffect");
                    if (e == null) return null;
                    e.amount = effectData.value;
                    effect = e;
                    break;
                }

            case "reduceCost":
            case "reduceNextCardCost":
            case "reduceDrawnCardCost":
                {
                    var e = Load<ReduceNextCardCostEffect>("ReduceNextCardCostEffect");
                    if (e == null) return null;
                    e.amount = effectData.value;
                    e.target = effectData.target;
                    effect = e;
                    break;
                }

            case "reduceRandomCardCostByFrozenEnemyCount":
                {
                    var e = ScriptableObject.CreateInstance<ReduceRandomCardCostByFrozenEnemyCountEffect>();
                    e.amountPerEnemy = effectData.value;
                    effect = e;
                    break;
                }

            case "atk":
            case "def":
            case "burn":
            case "freeze":
            case "active":
            case "activate":
            case "bless":
            case "sin":
            case "crime":
            case "penance":
            case "grace":
            case "scar":
            case "bleed":
            case "stun":
            case "guard":
                {
                    var e = Load<ApplyStatusEffect>("ApplyBuff");
                    if (e == null) return null;
                    e.statType = ParseBuffStatType(effectData.type);
                    e.value = effectData.value;
                    e.target = effectData.target;
                    effect = e;
                    break;
                }

            case "damagePercent":
                {
                    var e = ScriptableObject.CreateInstance<DamagePercentEffect>();
                    e.percent = effectData.value;
                    effect = e;
                    break;
                }

            case "repeat":
                {
                    var e = ScriptableObject.CreateInstance<RepeatEffect>();
                    e.repeatCount = effectData.value;
                    effect = e;
                    break;
                }

            case "autoCast":
                {
                    var e = ScriptableObject.CreateInstance<AutoCastEffect>();
                    e.count = effectData.value;
                    effect = e;
                    break;
                }

            case "lockPotentialCharge":
                {
                    var e = ScriptableObject.CreateInstance<LockPotentialChargeEffect>();
                    e.turns = effectData.value;
                    effect = e;
                    break;
                }

            case "multiplyBless":
                {
                    var e = ScriptableObject.CreateInstance<MultiplyBlessEffect>();
                    e.value = effectData.value;
                    effect = e;
                    break;
                }

            case "multiplyBuff":
                {
                    var e = ScriptableObject.CreateInstance<MultiplyBuffEffect>();
                    e.value = effectData.value;
                    effect = e;
                    break;
                }

            case "healByBless":
                {
                    var e = ScriptableObject.CreateInstance<HealByBlessEffect>();
                    e.value = effectData.value;
                    effect = e;
                    break;
                }

            case "noBlessConsume":
                {
                    var e = ScriptableObject.CreateInstance<NoBlessConsumeEffect>();
                    e.value = effectData.value;
                    effect = e;
                    break;
                }

            case "triggerBlessImmediately":
                {
                    var e = ScriptableObject.CreateInstance<TriggerBlessImmediatelyEffect>();
                    e.value = effectData.value;
                    effect = e;
                    break;
                }

            case "removeDebuffFromEnemy":
                {
                    var e = ScriptableObject.CreateInstance<RemoveDebuffFromEnemyEffect>();
                    e.value = effectData.value;
                    effect = e;
                    break;
                }

            case "damageByRemovedDebuffSumMultiplier":
                {
                    var e = ScriptableObject.CreateInstance<DamageByRemovedDebuffSumMultiplierEffect>();
                    e.multiplier = effectData.value;
                    effect = e;
                    break;
                }

            case "triggerOppositeStanceEffect":
                {
                    var e = ScriptableObject.CreateInstance<TriggerOppositeStanceEffect>();
                    e.value = effectData.value;
                    effect = e;
                    break;
                }

            case "immortalThreshold":
                {
                    var e = ScriptableObject.CreateInstance<ImmortalThresholdEffect>();

                    // 현재 JSON 구조상 minHp/count 분리 필드가 없다면
                    // CardEffect2 확장이 필요하지만 일단 임시 기본값
                    e.minHp = 1;
                    e.count = 15;
                    effect = e;
                    break;
                }

            case "conditional":
                {
                    var conditional = Load<ConditionalEffect>("ConditionalEffect");
                    if (conditional == null) return null;

                    if (effectData.condition == null)
                    {
                        Debug.LogWarning($"[CardEffectBuilder2] Conditional effect has no condition. card={cardIndex}");
                        return null;
                    }

                    var trigger = effectData.condition.trigger;
                    var values = effectData.condition.value ?? new List<string>();

                    TriggerCondition condition = BuildCondition(trigger, values);
                    if (condition == null)
                    {
                        Debug.LogWarning($"[CardEffectBuilder2] Failed to build condition: {trigger}, card={cardIndex}");
                        return null;
                    }

                    conditional.condition = condition;

                    if (effectData.result != null)
                    {
                        var resultEffectData = new CardEffect2
                        {
                            type = effectData.result.type,
                            value = effectData.result.value,
                            duration = effectData.result.duration,
                            target = effectData.result.target
                        };

                        conditional.effectIfTrue = BuildEffectRecursive(resultEffectData, cardIndex);
                    }
                    else
                    {
                        Debug.LogWarning($"[CardEffectBuilder2] Conditional effect has no result. trigger={trigger}, card={cardIndex}");
                    }

                    effect = conditional;
                    break;
                }

            default:
                Debug.LogWarning($"[CardEffectBuilder2] Unknown effect type: {effectData.type}, card={cardIndex}");
                break;
        }

        return effect;
    }

    private static TriggerCondition BuildCondition(string trigger, List<string> values)
    {
        switch (trigger)
        {
            case "isStance":
                {
                    var c = ScriptableObject.CreateInstance<StanceCondition>();
                    if (values.Count > 0)
                        c.requiredStance = ParseStance(values[0]);
                    return c;
                }

            case "usedCardType":
            case "usedAnyCardType":
                {
                    var c = ScriptableObject.CreateInstance<CurrentCardTypeCondition>();
                    c.requiredTypes = values.ConvertAll(ParseCardType);
                    c.isAnd = false;
                    return c;
                }

            case "usedAllCardTypes":
                {
                    var c = ScriptableObject.CreateInstance<CurrentCardTypeCondition>();
                    c.requiredTypes = values.ConvertAll(ParseCardType);
                    c.isAnd = true;
                    return c;
                }

            case "potentialGte":
                {
                    var c = ScriptableObject.CreateInstance<PotentialCondition>();
                    if (values.Count > 0)
                        c.potentialGauge = ParseInt(values[0]);
                    return c;
                }

            case "enemyHasStatus":
                {
                    var c = ScriptableObject.CreateInstance<EnemyHasStatusCondition>();
                    if (values.Count > 0)
                        c.statusType = ParseBuffStatType(values[0]);
                    return c;
                }

            case "enemyStatusStackGte":
                {
                    var c = ScriptableObject.CreateInstance<EnemyStatusStackCondition>();
                    if (values.Count > 0)
                        c.statusType = ParseBuffStatType(values[0]);
                    if (values.Count > 1)
                        c.requiredStack = ParseInt(values[1]);
                    return c;
                }

            case "sinTotalGte":
                {
                    var c = ScriptableObject.CreateInstance<SinTotalCondition>();
                    if (values.Count > 0)
                        c.requiredValue = ParseInt(values[0]);
                    return c;
                }

            case "justAfterStance":
                return ScriptableObject.CreateInstance<JustAfterStanceCondition>();

            case "justAfterSpecificStance":
                {
                    var c = ScriptableObject.CreateInstance<JustAfterSpecificStanceCondition>();
                    if (values.Count > 0)
                        c.requiredStance = ParseStance(values[0]);
                    return c;
                }

            case "justAfterStanceEffect":
                return ScriptableObject.CreateInstance<JustAfterStanceEffectCondition>();

            default:
                Debug.LogWarning($"[CardEffectBuilder2] Unknown trigger: {trigger}");
                return null;
        }
    }

    private static T Load<T>(string name) where T : CardEffectBase
    {
        var asset = Resources.Load<T>($"{path}/{name}");
        if (asset == null)
        {
            Debug.LogWarning($"[CardEffectBuilder2] {typeof(T).Name}({name}) 리소스를 찾을 수 없습니다.");
            return null;
        }
        return UnityEngine.Object.Instantiate(asset);
    }
}