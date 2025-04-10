using System.Collections.Generic;
using UnityEngine;

public static class CardEffectBuilder
{
    const string path = "Cards/Effects";

    public static List<CardEffectBase> Build(CardData data)
    {
        var effects = new List<CardEffectBase>();

        // Damage or Heal
        if (data.damage != 0)
        {
            if (data.damage > 0)
            {
                var dmg = Load<DamageEffect>("DamageEffect");
                dmg.amount = data.damage;
                
                effects.Add(dmg);
            }
            else
            {
                var heal = Load<HealEffect>("HealEffect");
                heal.amount = -data.damage;
                effects.Add(heal);
            }
        }

        // Discount (코스트 감소)
        if (data.discount != 0)
        {
            var discount = Load<ReduceNextCardCostEffect>("ReduceNextCardCostEffect"); // 이 스크립트가 필요해
            discount.amount = data.discount;
            effects.Add(discount);
        }

        // Draw or Discard
        if (data.draw != 0)
        {
            if (data.draw > 0)
            {
                var draw = Load<DrawCardEffect>("DrawCardEffect");
                draw.amount = data.draw;
                effects.Add(draw);
            }
            else
            {
                var discard = Load<DiscardCardEffect>("DiscardCardEffect");
                discard.discardCount = -data.draw;
                effects.Add(discard);
            }
        }

        // Recycle (used → hand)
        if (data.redraw > 0)
        {
            var recycle = Load<RecycleCardEffect>("RecycleCardEffect");
            recycle.amount = data.redraw;
            effects.Add(recycle);
        }

        // Buff / Debuff
        if (data.atkBuff != 0)
        {
            var buff = Load<ApplyStatusEffect>("ApplyBuff");
            buff.statType = BuffStatType.Attack;
            buff.value = data.atkBuff;
            buff.duration = 2;
            effects.Add(buff);
        }

        if (data.defBuff != 0)
        {
            var buff = Load<ApplyStatusEffect>("ApplyBuff");
            buff.statType = BuffStatType.Defense;
            buff.value = data.defBuff;
            buff.duration = 2;
            effects.Add(buff);
        }

        // Self Damage
        if (data.selfDamage > 0)
        {
            var self = Load<SelfDamageEffect>("SelfDamageEffect");
            self.amount = data.selfDamage;
            effects.Add(self);
        }

        // Block, Blind, Stun (정수형)
        if (data.block > 0)
        {
            var block = Load<BlockEffect>("BlockEffect");
            block.blockTargetClass = (CharacterClass)data.block;// 예: 0 = Leon
            effects.Add(block);
        }

        if (data.blind > 0)
        {
            var blind = Load<BlindEffect>("BlindEffect");
            if (System.Enum.IsDefined(typeof(PlayerData.StancType), data.blind))
            {
                blind.blockedStance = (PlayerData.StancType)data.blind; // enum 캐스팅
            }
            else
            {
                Debug.LogWarning($"Blind 값 {data.blind}는 StencType에 유효하지 않습니다.");
            }

            effects.Add(blind);
            effects.Add(blind);
        }

        if (data.stun > 0)
        {
            var stun = Load<StunEffect>("StunEffect");
            stun.duration = data.stun;
            effects.Add(stun);
        }

        if (!string.IsNullOrEmpty(data.characterStance))
        {
            var stance = Load<StanceConditionEffect>("StanceConditionEffect");
            stance.requiredStance = data.characterStance;
            effects.Add(stance);
        }

        return effects;
    }

    static T Load<T>(string name) where T : CardEffectBase
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