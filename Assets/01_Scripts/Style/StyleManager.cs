using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V20;

[System.Serializable]
public class PlayerStyleState {
    public int styleId;
    public int plusTier; // +강화 단계 (1,2,3 or 1 만)
    public int minusTier; // -강화 단계 (1,2,3 or 1 만)
}

// StyleManager.cs (skeleton)
public class StyleManager : MonoSingleton<StyleManager>
{
    public bool isFirstTurnCard = false; // 전투 입장 후 첫번째 턴 카드, 행동을 하게되면 false로 변경
    public bool isStartOfTurnCard = false; // 턴 시작 후 첫번째 턴 카드, 행동을 하게되면 false로 변경
    public PlayerStyleState CurrentState { get; private set; } = new PlayerStyleState();

    private Dictionary<int, StyleDefinition> StyleDic = new(); // 문체 딕셔너리
    private Dictionary<EffectCallTime, CompiledEntry> compiledEntries = new(); // 효과별 컴파일된 델리게이트 모음
    public Dictionary<int, CardModel> costDiscountCards = new(); // 코스트 할인 적용된 카드들 (CardCostModifier, TempCardCostModifier의 경우 사용)

    // 델리게이트 정의 (이때 여기서 인자값 으로 들어가는 각 타입들을 조건으로 체킹한다면, 컴파일 계산식 위치에서 조건 체크가 가능하다)
    public delegate int FirstCardCostModifier(CardModel card, int baseCost); // (카드 정보, 디폴트 값 0 입력)
    public delegate int CardCostModifier(CardModel card, int baseCost); // (카드 정보, 디폴트 값 0 입력)
    public delegate int StartTurnCardCostModifier(CardModel card, int baseCost); // (카드 정보, 디폴트 값 0 입력)
    public delegate float DamageIncomeModifier(IStatusReceiver caster, IStatusReceiver target, float baseDamage);
    public delegate float DamageGiveModifier(IStatusReceiver caster, IStatusReceiver target, CardModel card, float baseDamage);
    public delegate float HealAmountModifier(IStatusReceiver caster, float baseHeal);
    public delegate int BuffDebuffAmountModifier(IStatusReceiver target, BuffStatType statType, int baseAmount);
    public delegate int EnemyHpModifier(Enemy enemy, int baseHp);
    public delegate int SupplyManaAtStartOfTurn(int baseMana);

    /// 밸류 변환형이 아닌 효과 델리게이트
    public delegate void RandomDebuffSingleAlly(IStatusReceiver target); // 이때 외부 호출 함수 시점에서 target의 생존 여부 후 호출할 것.
    public delegate void StunAllAllies(List<IStatusReceiver> target); // 이때 외부 호출 함수 시점에서 target의 생존 여부 후 호출할 것.
    private class CompiledEntry
    {
        // 각 문체 효과들이 사용될 때 호출되는 델리게이트 모음
        public FirstCardCostModifier FirstCardCostModFunc; // 첫 카드 코스트 감소 
        public CardCostModifier CostModFunc;               // 카드 코스트 영구 환급 (변경 전까지 유지) => 코스트 환급형으로 구현.
        public StartTurnCardCostModifier TempCostModFunc;       // 카드 코스트 임시 환급 (매 턴마다 적용할 효과) => 코스트 환급형으로 구현.
        public DamageIncomeModifier DmgFunc;               // 받는 데미지 배율
        public DamageGiveModifier DmgGiveFunc;             // 주는 데미지 배율 (only 카드 사용 데미지)
        public HealAmountModifier HealFunc;                // 힐량 변경
        public BuffDebuffAmountModifier BuffDebuffFunc;    // 버프/디버프 수치 변경
        public EnemyHpModifier EnemyHpFunc;                // 적 체력 변경
        public SupplyManaAtStartOfTurn SupplyManaAtStartOfTurnFunc; // 턴 시작시 마나 공급량 변동

        // 특수한 방식의 효과 적용 시 사용 (호출 조건 또는 일부 수정이 있을 경우, 컴파일 시점의 enum 조건 체크 필요)
        public RandomDebuffSingleAlly randomDebuffSingleFunc; // 아군 단일 대상 랜덤 디버프 적용
        public StunAllAllies stunAllAlliesFunc; // 아군 전체 스턴 적용
    }

    public event Action<PlayerStyleState> OnStyleChanged;
    public event Action<PlayerStyleState> OnStyleUpgraded;

    public void Initialize() //호출순위 나머지 데이터가 로드가 끝난 이후
    {
        // 모든 문체 정보 로드 및 딕셔너리에 저장
        // 저장되어 있는, 종료 전의 문체 정보 로드
        OnStyleChanged += RegisCardOnChange;
        // 아래에서, 기존에 저장된 문체가 있으면 setstate 호출
    }

    public void SetState(int styleId, int plusTier, int minusTier)
    {
        var state = new PlayerStyleState
        {
            styleId = styleId,
            plusTier = plusTier,
            minusTier = minusTier
        };
        CurrentState = state;
        OnStyleChanged?.Invoke(CurrentState);
        RebuildCompiledFuncs();
    }
    public void ChangeStyle(int id) // 문체 변경
    {
        if (!StyleDic.ContainsKey(id)) { Debug.LogWarning("Key for current Style has not found"); return; }
        CurrentState.styleId = id;
        CurrentState.plusTier = StyleDic[id].currentPlus;
        CurrentState.minusTier = StyleDic[id].currentMinus;

        OnStyleChanged?.Invoke(CurrentState);
        RebuildCompiledFuncs();
    }

    public bool UpgradePlus()
    {
        var def = StyleDic[CurrentState.styleId];
        if (CurrentState.plusTier >= def.maxPlusLevel) return false;
        CurrentState.plusTier++;
        def.currentPlus++;

        OnStyleUpgraded?.Invoke(CurrentState);
        ProgressDataManager.Instance.SaveProgress(true);
        RebuildCompiledFuncs();
        return true;
    }
    public bool UpgradeMinus()
    {
        var def = StyleDic[CurrentState.styleId];
        if (CurrentState.minusTier >= def.maxMinusLevel) return false;
        CurrentState.minusTier++;
        def.currentMinus++;

        OnStyleUpgraded?.Invoke(CurrentState);
        ProgressDataManager.Instance.SaveProgress(true);
        RebuildCompiledFuncs();
        return true;
    }
    private void RegisCardOnChange(PlayerStyleState state) // 카드 코스트 관련 효과 존재시 costDiscountCards에 등록
    {
        costDiscountCards.Clear();
        StyleDefinition def = StyleDic[state.styleId];
        if(def.plusTiers[0].effects != null && def.minusTiers[0].effects != null)
        {   
            // 카드 코스트 변환 효과가 있을 경우 costDiscountCards에 등록 
            // (이후 StyleDefinition에 카드 타입 변수를 추가하면, 이곳에서 특정 카드만 등록하는 방식도 가능)
            if(def.plusTiers[0].effects.Exists(eff => eff.target == EffectTarget.CardCost) ||
               def.minusTiers[0].effects.Exists(eff => eff.target == EffectTarget.CardCost))
            {
                // 카드 코스트 관련 효과 존재 시, costDiscountCards 에 등록
                foreach (var card in DataManager.Instance.allCards)
                {
                    costDiscountCards[card.index] = card;
                }
            }
        }
    }
    private List<StyleEffect> GatherActiveEffects()
    {
        var list = new List<StyleEffect>();
        if (!StyleDic.TryGetValue(CurrentState.styleId, out var def) || def == null) return list;

        // 현재 문체의 + 효과들 정리 및 list에 추가
        foreach (var te in def.plusTiers)
        {
            if (te.effects != null)
                list.AddRange(te.effects);
        }

        // 현재 문체의 - 효과들 정리 및 list에 추가
        foreach (var te in def.minusTiers)
        {
            if (te.effects != null)
                list.AddRange(te.effects);
        }

        return list;
    }

    // 델리게이트 컴파일 캐싱 (setstate/ 문체변경/ 강화 시점에 호출)
    private void RebuildCompiledFuncs()
    {
        compiledEntries.Clear();
        var effects = GatherActiveEffects();

        // 카드 코스트 관련 연산
        // 이때 card, caster 등을 통해서, 효과 조건의 교차 검증도 가능 (ex: 특정 카드에 한정 등)
        FirstCardCostModifier firstCardCostFunc = (card, baseCost) =>
        {
            int cost = baseCost;
            foreach (var eff in effects)
            {
                // 효과의 조건 확인
                if (eff.callTime != EffectCallTime.OnStartOfBattle) continue;
                if (eff.target == EffectTarget.FirstCardCost)
                {
                    cost = (int)eff.value; // value 값 만큼 할인
                }
            }
            return Mathf.Max(0, cost);
        };
        CardCostModifier costFunc = (card, baseCost) =>
        {
            int cost = 0;
            foreach (var eff in effects)
            {
                // 효과의 조건 확인
                if (eff.callTime != EffectCallTime.OnCardUse) continue;
                if (eff.target == EffectTarget.CardCost)
                {
                    cost = card.GetEffectiveCost(); // 조건 만족 시 카드의 코스트 만큼 환급
                }
            }
            return Mathf.Max(0, cost);
        };
        StartTurnCardCostModifier startTurnCostFunc = (card, baseCost) =>
        {
            int cost = baseCost;
            foreach (var eff in effects)
            {
                // 효과의 조건 확인
                if (eff.callTime != EffectCallTime.OnStartOfTurn) continue;
                if (eff.target == EffectTarget.CardCost)
                {
                    cost = card.GetEffectiveCost(); // 조건 만족 시 카드의 코스트 만큼 환급
                }
            }
            return Mathf.Max(0, cost);
        };
        DamageIncomeModifier dmgFunc = (caster, target, baseDamage) => // 받는 데미지 관련 연산
        {
            float dmg = baseDamage;
            foreach (var eff in effects)
            {
                if (eff.callTime != EffectCallTime.OnGettingDamage) continue;
                if (eff.target == EffectTarget.IncomingDamage)
                {
                    switch (eff.operation)
                    {
                        case EffectOperation.MulPercent:
                            dmg = Mathf.RoundToInt(dmg * eff.value);
                            break;
                        case EffectOperation.Add:
                            dmg += Mathf.RoundToInt(eff.value);
                            break;
                        case EffectOperation.RandomRange: // 데미지 랜덤 구간 보정 수치 효과 -2 ~ +2 등..
                            dmg += UnityEngine.Random.Range(Mathf.RoundToInt(eff.valueRange.x), Mathf.RoundToInt(eff.valueRange.y) + 1);
                            dmg = Mathf.Max(0, dmg);
                            break;
                    }
                }
            }
            return dmg;
        };
        DamageGiveModifier dmgGiveFunc = (caster, target, card, baseDamage) => // 주는 데미지
        {
            float dmg = baseDamage;
            foreach (var eff in effects)
            {
                if (eff.callTime != EffectCallTime.OnCardUse) continue;
                if (eff.target == EffectTarget.DamageGive)
                {
                    switch (eff.operation)
                    {
                        case EffectOperation.MulPercent: // 최소 1 이상은 오르도록 올림 형식 int 변환
                            dmg = Mathf.CeilToInt(dmg * eff.value);
                            break;
                        case EffectOperation.Add:
                            dmg += Mathf.RoundToInt(eff.value);
                            break;
                        case EffectOperation.RandomRange: // 데미지 랜덤 구간 보정 수치 효과 -2 ~ +2 등..
                            dmg += UnityEngine.Random.Range(Mathf.RoundToInt(eff.valueRange.x), Mathf.RoundToInt(eff.valueRange.y) + 1);
                            dmg = Mathf.Max(0, dmg);
                            break;
                    }
                }
            }
            return Mathf.Max(0, dmg);
        };
        HealAmountModifier healFunc = (caster, baseHeal) => // 힐 관련 연산
        {
            float heal = baseHeal;
            foreach (var eff in effects)
            {
                if (eff.callTime != EffectCallTime.OnCardUse) continue;
                if (eff.target == EffectTarget.HealAmount)
                {
                    switch (eff.operation)
                    {
                        case EffectOperation.Add:
                            heal += Mathf.RoundToInt(eff.value);
                            break;
                        case EffectOperation.MulPercent: // 데미지와는 다르게 올림이 아니라 내림 형식으로 (힐 관련 수치는 보수적으로 잡기)
                            heal = Mathf.FloorToInt(heal * eff.value);
                            break;
                        case EffectOperation.Set:
                            heal = Mathf.RoundToInt(eff.value);
                            break;
                    }
                }
            }
            return Mathf.Max(0, heal);
        };
        BuffDebuffAmountModifier buffDebuffFunc = (target, statType, baseAmount) =>
        {
            int amount = baseAmount;
            foreach (var eff in effects)
            {
                if (eff.callTime != EffectCallTime.OnApplyBuff && eff.callTime != EffectCallTime.OnApplyDebuff) continue;
                // 버프/디버프의 관련 효과인지 확인 => 맞다면 value 변환 적용
                if ((eff.callTime == EffectCallTime.OnApplyBuff && Buff.IsBuff(statType, baseAmount)) ||
                    (eff.callTime == EffectCallTime.OnApplyDebuff && !Buff.IsBuff(statType, baseAmount)))
                {
                    switch (eff.operation)
                    {
                        case EffectOperation.Add:
                            amount += Mathf.RoundToInt(eff.value);
                            break;
                        case EffectOperation.Set:
                            amount = Mathf.RoundToInt(eff.value);
                            break;
                    }
                }
            }
            return Mathf.Max(0, amount);
        };
        EnemyHpModifier enemyHpFunc = (enemy, baseHp) =>
        {
            int hp = baseHp;
            foreach (var eff in effects)
            {
                if (eff.callTime != EffectCallTime.OnStartOfBattle) continue;
                if (eff.target == EffectTarget.EnemyMaxHP)
                {
                    switch (eff.operation)
                    {
                        case EffectOperation.Add:
                            hp += Mathf.RoundToInt(eff.value);
                            break;
                        case EffectOperation.MulPercent:
                            hp = Mathf.RoundToInt(hp * eff.value);
                            break;
                        case EffectOperation.Set:
                            hp = Mathf.RoundToInt(eff.value);
                            break;
                    }
                }
            }
            return Mathf.Max(1, hp);
        };
        SupplyManaAtStartOfTurn supplyManaAtStartOfTurnFunc = (baseMana) =>
        {
            int mana = baseMana;
            foreach (var eff in effects)
            {
                if (eff.callTime != EffectCallTime.OnStartOfTurn) continue;
                if (eff.target == EffectTarget.Mana)
                {
                    switch (eff.operation)
                    {
                        case EffectOperation.Add:
                            mana += Mathf.RoundToInt(eff.value);
                            break;
                        case EffectOperation.Set:
                            mana = Mathf.RoundToInt(eff.value);
                            break;
                    }
                }
            }
            return Mathf.Max(0, mana);
        };

        // 밸류 변환 형식이 아닌 효과들 직접 처리 델리게이트 컴파일
        RandomDebuffSingleAlly randomDebuffFunc = (player) =>
        {
            bool applied = false;
            foreach (var eff in effects)
            {
                if (eff.callTime != EffectCallTime.OnStartOfTurn) continue;
                if (eff.target == EffectTarget.ApplyRandomDebuffToSingleAlly)
                {
                    applied = true;
                }
            }

            if(applied)
            {
                ApplyStatusEffect stef = Debuff.GetRandomDebuffEffect(); // 랜덤한 디버프 1개를 선정해 player에게 적용
                stef.target = (int)player.ChClass;
                var p = new List<IStatusReceiver> { player }; // list 형으로 변환
                stef.Apply(player,p);
            }
        };
        StunAllAllies stunAllFunc = (players) =>
        {
            bool applied = false;
            foreach (var eff in effects)
            {
                if (eff.callTime != EffectCallTime.OnStartOfBattle) continue;
                if (eff.target == EffectTarget.ApplyStunToAllAllies)
                {
                    applied = true;
                }
            }
            
            if(applied)
            {
                foreach (var pc in players)
                {
                    ApplyStatusEffect stef = Debuff.GetStunEffect(1);
                    stef.target = (int)pc.ChClass;
                    var p = new List<IStatusReceiver> { pc }; // list 형으로 변환
                    stef.Apply(pc,p); // 각각의 player 들에게 효과 부여
                }
            }
        };

        // 컴파일 된 델리게이트들 EffectCallTime 별로 저장
        var entryCardUse = new CompiledEntry { HealFunc = healFunc, DmgGiveFunc = dmgGiveFunc };
        var entryGettingDamage = new CompiledEntry { DmgFunc = dmgFunc };
        var entryBuffDebuff = new CompiledEntry { BuffDebuffFunc = buffDebuffFunc };

        var entryStartOfTurn = new CompiledEntry { randomDebuffSingleFunc = randomDebuffFunc, TempCostModFunc = startTurnCostFunc, SupplyManaAtStartOfTurnFunc = supplyManaAtStartOfTurnFunc };
        var entryStartOfBattle = new CompiledEntry { stunAllAlliesFunc = stunAllFunc, EnemyHpFunc = enemyHpFunc, FirstCardCostModFunc = firstCardCostFunc, CostModFunc = costFunc };

        // compiledEntries 딕셔너리를 통해 호출 가능한 형태로 등록
        compiledEntries[EffectCallTime.OnCardUse] = entryCardUse;
        compiledEntries[EffectCallTime.OnGettingDamage] = entryGettingDamage;
        compiledEntries[EffectCallTime.OnApplyBuff] = entryBuffDebuff;
        compiledEntries[EffectCallTime.OnApplyDebuff] = entryBuffDebuff;

        compiledEntries[EffectCallTime.OnStartOfTurn] = entryStartOfTurn;
        compiledEntries[EffectCallTime.OnStartOfBattle] = entryStartOfBattle;
    }




    // 외부 호출용 함수 (value 변환 효과들 적용 위치에서 호출)
    // 문체에 관련 효과가 없을 시 기본 비용 반환
    public int GetFirstCardCostModifier(CardModel card, int baseCost)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnStartOfBattle, out var e) && e.FirstCardCostModFunc != null)
            return e.FirstCardCostModFunc(card, baseCost);
        return baseCost;
    }
    /// <summary>
    /// 전투마다 영구적으로 코스트 변환 적용시
    /// </summary>
    public int GetModifiedCardCost(CardModel card, int baseCost)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnStartOfBattle, out var e) && e.CostModFunc != null)
            return e.CostModFunc(card, baseCost);
        return baseCost;
    }
    /// <summary>
    /// 매 턴마다 임시로 코스트 변환 적용시
    /// </summary>
    public int GetStartTurnModifiedCardCost(CardModel card, int baseCost)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnStartOfTurn, out var e) && e.TempCostModFunc != null)
            return e.TempCostModFunc(card, baseCost);
        return baseCost;
    }
    /// <summary>
    /// '받는 데미지' 변환 적용 문체
    /// </summary>
    public float GetOnComingDamageModify(IStatusReceiver caster, IStatusReceiver target, float baseDamage)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnGettingDamage, out var e) && e.DmgFunc != null)
            return e.DmgFunc(caster, target, baseDamage);
        return baseDamage;
    }
    /// <summary>
    /// 카드 사용 시 '주는 데미지' 변환 적용 문체
    /// </summary>
    public float GetDamageGiveModify(IStatusReceiver caster, IStatusReceiver target, CardModel card, float baseDamage)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnCardUse, out var e) && e.DmgGiveFunc != null)
            return e.DmgGiveFunc(caster, target, card, baseDamage);
        return baseDamage;
    }
    public float GetHealOnCardUse(IStatusReceiver caster, float baseHeal)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnCardUse, out var e) && e.HealFunc != null)
            return e.HealFunc(caster, baseHeal);
        return baseHeal;
    }
    /// <summary>
    /// 버프/디버프 수치 변환 적용 문체
    /// </summary>
    public int ModifyBuffDebuffAmount(IStatusReceiver target, BuffStatType statType, int baseAmount)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnApplyBuff, out var e) && e.BuffDebuffFunc != null)
            return e.BuffDebuffFunc(target, statType, baseAmount);
        if (compiledEntries.TryGetValue(EffectCallTime.OnApplyDebuff, out var e2) && e2.BuffDebuffFunc != null)
            return e2.BuffDebuffFunc(target, statType, baseAmount);
        return baseAmount;
    }
    /// <summary>
    /// 적 체력 변환 적용 문체
    /// </summary>
    public int ModifyEnemyMaxHp(Enemy enemy, int baseHp)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnStartOfBattle, out var e) && e.EnemyHpFunc != null)
            return e.EnemyHpFunc(enemy, baseHp);
        return baseHp;
    }
    /// <summary>
    /// 턴 시작시 마나 공급량 변환 적용 문체
    /// </summary>
    public int ModifySupplyManaAtStartOfTurn(int baseMana)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnStartOfTurn, out var e) && e.SupplyManaAtStartOfTurnFunc != null)
            return e.SupplyManaAtStartOfTurnFunc(baseMana);
        return baseMana;
    }

    // 일부 value 변환 형식이 아닌 효과들 직접 처리 (호출 시점에서 bool 체킹만 하고 효과 구현 or 실행을 해당 위치에서 할 것)
    public void ApplyRandomDebuffToSingleAlly(IStatusReceiver player)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnStartOfTurn, out var e) && e.randomDebuffSingleFunc != null)
            e.randomDebuffSingleFunc(player);
    }
    public void ApplyStunToAllAllies(List<IStatusReceiver> players)
    {
        if (compiledEntries.TryGetValue(EffectCallTime.OnStartOfBattle, out var e) && e.stunAllAlliesFunc != null)
            e.stunAllAlliesFunc(players);
    }
}

