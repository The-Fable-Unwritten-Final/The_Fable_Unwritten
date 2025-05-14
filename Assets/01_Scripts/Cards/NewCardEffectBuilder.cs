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
                effect = heal;
                break;

            case "draw":
                var draw = Load<DrawCardEffect>("DrawCardEffect");
                draw.amount = effectData.value;
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

            case "atk_buff":
            case "def_buff":
                var buff = Load<ApplyStatusEffect>("ApplyBuff");
                buff.statType = effectData.type == "atk_buff" ? BuffStatType.Attack : BuffStatType.Defense;
                buff.value = effectData.value;
                buff.duration = effectData.duration;
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
                block.blockTargetClass = effectData.target == null ? null : (CharacterClass)effectData.target;
                effect = block;
                break;

            case "stun":
                var stun = Load<StunEffect>("StunEffect");
                stun.duration = effectData.value;
                effect = stun;
                break;

            case "reduceCost":
                var cost = Load<ReduceNextCardCostEffect>("ReduceNextCardCostEffect");
                cost.amount = effectData.value;
                cost.target = effectData.target;
                effect = cost;
                break;

            case "blind":
                var blind = Load<BlindEffect>("BlindEffect");
                blind.blockedStance = (PlayerData.StancType)effectData.target;
                effect = blind;
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
                        used.cardIndices = values;
                        condition = used;
                        break;

                    case "isUsedCard":
                    case "isUsedAllCard":
                        var typeCond = ScriptableObject.CreateInstance<CurrentCardTypeCondition>();
                        typeCond.requiredTypes = values.ConvertAll(v => (CardType)v);
                        typeCond.isAnd = trigger == "isUsedAllCard";
                        condition = typeCond;
                        break;

                    case "isDrawCard":
                        var drawCond = ScriptableObject.CreateInstance<DrawnCardCondition>();
                        drawCond.requiredTypes = values.ConvertAll(v => (CardType)v);
                        condition = drawCond;
                        break;

                    case "isUnusedCard":
                        var unused = ScriptableObject.CreateInstance<NotCurrentCardTypeCondition>();
                        unused.forbiddenTypes = values.ConvertAll(v=> (CardType)v);
                        condition = unused;
                        break;

                    case "isStance":
                        var stance = ScriptableObject.CreateInstance<StanceCondition>();
                        stance.requiredStance = (StancValue.EStancType)values[0];
                        condition = stance;
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
}
