using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCardEffectBuilder : MonoBehaviour
{
    private const string path = "Cards/Effects";

    public static List<CardEffectBase> Build(CardJsonData data)
    {
        var effects = new List<CardEffectBase>();

        foreach (var effect in data.effects)
        {
            var built = BuildEffectRecursive(effect, data.index);
            if (built != null)
                effects.Add(built);
        }

        return effects;
    }

    private static CardEffectBase BuildEffectRecursive(CardEffect effectData, int index)
    {
        CardEffectBase effect = null;

        switch (effectData.type)
        {
            case "damage":
                var dmg = Load<DamageEffect>("DamageEffect");
                dmg.amount = effectData.value;
                effect = dmg;
                break;

            case "heal":
                var heal = Load<HealEffect>("HealEffect");
                heal.amount = effectData.value;
                heal.target = effectData.target;
                effect = heal;
                break;

            case "draw":
                var draw = Load<DrawCardEffect>("DrawCardEffect");
                draw.amount = effectData.value;
                draw.target = effectData.target;
                effect = draw;
                break;

            case "discard":
                var discard = Load<DiscardCardEffect>("DiscardCardEffect");
                discard.discardCount = effectData.value;
                effect = discard;
                break;

            case "duplicate":
                var dup = Load<DuplicateCardEffect>("DuplicateCardEffect");
                dup.duplicateNum = effectData.value;
                effect = dup;
                break;

            case "applyDamage":
                var applyDmg = Load<ApplyDamageEffect>("ApplyDamageEffect");
                applyDmg.value = effectData.value;
                applyDmg.index = index;
                effect = applyDmg;
                break;

            case "CantAtkinParticularStance":
                var pstance = ScriptableObject.CreateInstance<CantAttackInStance>();
                pstance.blockStance = (StancValue.EStancType)effectData.value;
                effect = pstance;
                break;

            case "redraw":
                var redraw = Load<RecycleCardEffect>("recycleCardEffect");
                redraw.amount = effectData.value;
                effect = redraw;
                break;

            case "atk":
            case "def":
            case "burn":
            case "freeze":
            case "active":
            case "bless":
            case "sin":
            case "penance":
            case "scar":
            case "stun":
            case "guard":
            case "undying":
                var buff = Load<ApplyStatusEffect>("ApplyBuff");

                buff.statType = effectData.type switch
                {
                    "atk" => BuffStatType.Attack,
                    "def" => BuffStatType.Defend,
                    "burn" => BuffStatType.Burn,
                    "freeze" => BuffStatType.Freeze,
                    "active" => BuffStatType.Activate,
                    "bless" => BuffStatType.Bless,
                    "sin" => BuffStatType.Sin,
                    "penance" => BuffStatType.Penance,
                    "scar" => BuffStatType.Scar,
                    "stun" => BuffStatType.Stun,
                    "guard" => BuffStatType.Guard,
                    "undying" => BuffStatType.Undying,
                    _ => BuffStatType.None
                };

                buff.value = effectData.value;
                buff.target = effectData.target;
                effect = buff;
                break;

            case "self_damage":
                var self = Load<SelfDamageEffect>("SelfDamageEffect");
                self.amount = effectData.value;
                effect = self;
                break;

            case "block":
                var block = Load<BlockEffect>("BlockEffect");
                block.blockTargetClass = (CharacterClass)effectData.target;
                effect = block;
                break;

            case "reduceCost":
            case "reduceNextCardCost":
                var cost = Load<ReduceNextCardCostEffect>("ReduceNextCardCostEffect");
                cost.amount = effectData.value;

                if (string.IsNullOrEmpty(effectData.owner))
                {
                    cost.allParty = true;
                }
                else
                {
                    cost.allParty = false;
                    cost.target = effectData.owner.ToLower() switch
                    {
                        "sophia" => 0,
                        "kayla" => 1,
                        "leon" => 2,
                        _ => null
                    };
                }

                effect = cost;
                break;

            case "blind":
                var blind = Load<BlindEffect>("BlindEffect");
                blind.blockedStance = (StancType)effectData.target;
                effect = blind;
                break;

            case "damagePercent":
                var damagePercent = Load<DamagePercentEffect>("DamagePercentEffect");
                damagePercent.percent = effectData.value;
                effect = damagePercent;
                break;

            case "reduceDrawnCardCost":
                var drawnCost = Load<ReduceDrawnCardCostEffect>("ReduceDrawnCardCostEffect");
                drawnCost.amount = effectData.value;
                effect = drawnCost;
                break;

            case "reduceRandomCardCostByFrozenEnemyCount":
                var frozenCost = Load<ReduceRandomCardCostByFrozenEnemyCountEffect>("ReduceRandomCardCostByFrozenEnemyCountEffect");
                frozenCost.amountPerEnemy = effectData.value;
                effect = frozenCost;
                break;

            case "truedamage":
                var trueDamage = Load<TrueDamageEffect>("TrueDamageEffect");
                trueDamage.amount = effectData.value;
                effect = trueDamage;
                break;

            case "multiplyBless":
                var multiplyBless = Load<MultiplyBlessEffect>("MultiplyBlessEffect");
                multiplyBless.value = effectData.value;
                effect = multiplyBless;
                break;

            case "multiplyBuff":
                var multiplyBuff = Load<MultiplyBuffEffect>("MultiplyBuffEffect");
                multiplyBuff.value = effectData.value;
                effect = multiplyBuff;
                break;

            case "healByPenance":
                var healByPenance = Load<HealByPenanceEffect>("HealByPenanceEffect");
                healByPenance.value = effectData.value;
                effect = healByPenance;
                break;

            case "removeDebuffFromEnemy":
                var removeDebuff = Load<RemoveDebuffFromEnemyEffect>("RemoveDebuffFromEnemyEffect");
                removeDebuff.value = effectData.value;
                effect = removeDebuff;
                break;

            case "damageByRemovedDebuffSumMultiplier":
                var removedDamage = Load<DamageByRemovedDebuffSumMultiplierEffect>("DamageByRemovedDebuffSumMultiplierEffect");
                removedDamage.multiplier = effectData.value;
                effect = removedDamage;
                break;

            case "triggerOppositeStanceEffect":
                var oppositeStance = Load<TriggerOppositeStanceEffect>("TriggerOppositeStanceEffect");
                effect = oppositeStance;
                break;

            case "autoCast":
                var autoCast = Load<AutoCastEffect>("AutoCastEffect");
                autoCast.count = effectData.value;
                effect = autoCast;
                break;

            case "lockPotentialCharge":
                var lockPotential = Load<LockPotentialChargeEffect>("LockPotentialChargeEffect");
                lockPotential.turns = effectData.value;
                effect = lockPotential;
                break;

            case "conditional":
                var conditional = Load<ConditionalEffect>("ConditionalEffect");

                var trigger = effectData.condition.trigger;
                var values = effectData.condition.value;

                TriggerCondition condition = null;

                switch (trigger)
                {
                    case "isUsedParticularCard":
                        var used = ScriptableObject.CreateInstance<UsedCardCondition>();
                        used.cardIndices = values.ConvertAll(v => ParseInt(v));
                        condition = used;
                        break;

                    case "isUsedCard":
                    case "isUsedAllCard":
                    case "usedCardType":
                    case "usedAnyCardType":
                    case "usedAllCardTypes":
                        var typeCond = ScriptableObject.CreateInstance<CurrentCardTypeCondition>();
                        typeCond.requiredTypes = ParseCardTypes(values);
                        typeCond.isAnd = trigger == "isUsedAllCard" || trigger == "usedAllCardTypes";
                        condition = typeCond;
                        break;

                    case "isDrawCard":
                        var drawCond = ScriptableObject.CreateInstance<DrawnCardCondition>();
                        drawCond.requiredTypes = ParseCardTypes(values);
                        condition = drawCond;
                        break;

                    case "isUnusedCard":
                        var unused = ScriptableObject.CreateInstance<NotCurrentCardTypeCondition>();
                        unused.forbiddenTypes = ParseCardTypes(values);
                        condition = unused;
                        break;

                    case "isStance":
                        var stance = ScriptableObject.CreateInstance<StanceCondition>();
                        stance.requiredStance = ParseStance(values[0]);
                        condition = stance;
                        break;

                    case "enemyHasStatus":
                        var enemyStatus = ScriptableObject.CreateInstance<EnemyHasStatusCondition>();
                        enemyStatus.statusType = ParseStatus(values[0]);
                        condition = enemyStatus;
                        break;

                    case "enemyStatusStackGte":
                        var enemyStack = ScriptableObject.CreateInstance<EnemyStatusStackCondition>();
                        enemyStack.statusType = ParseStatus(values[0]);
                        enemyStack.requiredStack = ParseInt(values[1]);
                        condition = enemyStack;
                        break;

                    case "potentialGte":
                        var potential = ScriptableObject.CreateInstance<PotentialCondition>();
                        potential.potentialGauge = ParseInt(values[0]) / 10;
                        condition = potential;
                        break;


                    case "potentialTriggered":
                        condition = ScriptableObject.CreateInstance<PotentialTriggeredCondition>();
                        break;

                    case "justAfterStance":
                        condition = ScriptableObject.CreateInstance<JustAfterStanceCondition>();
                        break;

                    case "justAfterSpecificStance":
                        var specificStance = ScriptableObject.CreateInstance<JustAfterSpecificStanceCondition>();
                        specificStance.requiredStance = ParseStance(values[0]);
                        condition = specificStance;
                        break;

                    case "justAfterStanceEffect":
                        condition = ScriptableObject.CreateInstance<JustAfterStanceEffectCondition>();
                        break;

                    case "sinTotalGte":
                        var sin = ScriptableObject.CreateInstance<SinTotalCondition>();
                        sin.requiredValue = ParseInt(values[0]);
                        condition = sin;
                        break;


                    default:
                        Debug.LogWarning($"[CardEffectBuilder] 알 수 없는 조건 트리거: {trigger}");
                        break;
                }
                conditional.condition = condition;

                if (effectData.result != null)
                {

                    var resultEffectData = new CardEffect
                    {
                        type = effectData.result.type,
                        value = effectData.result.value,
                        duration = effectData.result.duration,
                        target = effectData.result.target
                    };
                    conditional.effectIfTrue = BuildEffectRecursive(resultEffectData, index);
                }
                else
                {
                    Debug.LogWarning($"[CardEffectBuilder] ConditionalEffect({trigger})에 result가 없습니다.");
                }

                effect = conditional;
                break;

            default:
                Debug.LogWarning($"[CardEffectBuilder] Unknown effect type: {effectData.type}");
                break;
        }


        return effect;
    }

    private static T Load<T>(string name) where T : CardEffectBase
    {
        var asset = Resources.Load<T>($"{path}/{name}");
        if (asset == null)
        {
            Debug.LogWarning($"[CardEffectBuilder] {typeof(T).Name}({name}) 리소스를 찾을 수 없습니다.");
            return null;
        }
        return Object.Instantiate(asset);
    }

    private static List<CardType> ParseCardTypes(List<string> values)
    {
        var result = new List<CardType>();

        if (values == null)
            return result;

        foreach (var value in values)
        {
            if (System.Enum.TryParse(value, true, out CardType type))
                result.Add(type);
            else
                Debug.LogWarning($"[CardEffectBuilder] 알 수 없는 CardType: {value}");
        }

        return result;
    }

    private static StancType ParseStance(string value)
    {
        if (System.Enum.TryParse(value, true, out StancType stance))
            return stance;

        Debug.LogWarning($"[CardEffectBuilder] 알 수 없는 StancType: {value}");
        return default;
    }

    private static int ParseInt(string value)
    {
        if (int.TryParse(value, out int result))
            return result;

        Debug.LogWarning($"[CardEffectBuilder] 숫자 변환 실패: {value}");
        return 0;
    }

    private static BuffStatType ParseStatus(string value)
    {
        return value.ToLower() switch
        {
            "atk" => BuffStatType.Attack,
            "def" => BuffStatType.Defend,
            "burn" => BuffStatType.Burn,
            "freeze" => BuffStatType.Freeze,
            "active" => BuffStatType.Activate,
            "bless" => BuffStatType.Bless,
            "sin" => BuffStatType.Sin,
            "penance" => BuffStatType.Penance,
            "scar" => BuffStatType.Scar,
            "stun" => BuffStatType.Stun,
            "guard" => BuffStatType.Guard,
            "undying" => BuffStatType.Undying,
            _ => BuffStatType.None
        };
    }
}
