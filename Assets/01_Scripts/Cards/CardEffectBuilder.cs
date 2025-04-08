using System.Collections.Generic;
using UnityEngine;

public static class CardEffectBuilder
{
    const string path = "Cards/Effects";

    public static List<CardEffectBase> Build(CardData data)
    {
        var effects = new List<CardEffectBase>();

        if (data.damage != 0)
        {
            if(data.damage>0)
            {
                var dmg = Load<DamageEffect>("DamageEffect");
                dmg.amount = data.damage;
                effects.Add(dmg);
            }
            else
            {
                var heal = Load<HealEffect>("HealEffect");
                heal.amount = data.damage * -1;
                effects.Add(heal);
            }
        }

        if (data.draw != 0)
        {
            if(data.draw > 0)
            {
                var draw = Load<DrawCardEffect>("DrawCardEffect");
                draw.amount = data.draw;
                effects.Add(draw);
            }
            else
            {
                var discard = Load<DiscardCardEffect>("DiscardCardEffect");
                discard.discardCount = data.draw*-1;
            }
        }

        if (data.redraw > 0)
        {
            var recycle = Load<RecycleCardEffect>("RecycleCardEffect");
            recycle.amount = data.redraw;
            effects.Add(recycle);
        }

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

        if (data.selfDamage > 0)
        {
            var self = Load<SelfDamageEffect>("SelfDamageEffect");
            self.amount = data.selfDamage;
            effects.Add(self);
        }

        if (data.block)
            effects.Add(Load<BlockEffect>("BlockEffect"));

        if (data.blind)
            effects.Add(Load<BlindEffect>("BlindEffect"));

        if (data.stun)
            effects.Add(Load<StunEffect>("StunEffect"));

        if (data.characterIgnite)
            effects.Add(Load<IgniteConditionEffect>("IgniteConditionEffect"));

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
        return Object.Instantiate(asset);
    }
}