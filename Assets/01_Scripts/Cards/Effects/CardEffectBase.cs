using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardEffectType { Damage, Heal, Buff, Debuff, Conditional, Chain }
public enum CharacterClass { Sophia, Kayla, Leon }
public enum SkillType { Fire, Ice, Electric, Nature, Buff, Debuff, Holy, Heal, Slash, Strike, Pierce, Defense }
public enum BuffStatType { None, Attack, Defense, Speed, ManaRegen }



public abstract class CardEffectBase : MonoBehaviour
{
    public abstract void Apply(/*player, enemy, buffstat*/);       //타겟(적, 플레이어)에게 어떤 효과를 주는 지
    public virtual string GetDescription() => name;
}

public interface IStatusReceiver
{
    void ApplyStatusEffect(StatusEffect effect);
    int ModifyStat(BuffStatType statType, int baseValue);
}
