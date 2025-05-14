using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ReduceNextCardCostEffect;

public class PlayerController : MonoBehaviour, IStatusReceiver
{
    public PlayerData playerData;       //플레이어의 데이타
    public DeckModel deckModel;         //플레이어가 들고 있는 덱
    public bool hasBlock = false;           //방어막 획득 여부
    [SerializeField] private HpBarDisplay hpBarDisplay;

    private Animator animator;

    public void OnClickHighStance() => ChangeStance(PlayerData.StancType.High);
    public void OnClickMidStance() => ChangeStance(PlayerData.StancType.Middle);
    public void OnClickLowStance() => ChangeStance(PlayerData.StancType.Low);

    public DeckModel Deck => deckModel;     //덱 변환 함수
    public bool IsIgnited => false;  // 점화 여부 - 추후 확장
    public string CurrentStance => playerData.currentStance.stencType.ToString();       //현재의 자세를 가져옴
    public CharacterClass ChClass{get; set;}
    //현재 캐릭터의 클래스를 가져옴.



    [SerializeField]private List<StatusEffect> activeEffects = new List<StatusEffect>();        //현재 가지고 있는 상태이상 및 버프

    private void Awake()
    {

        animator = GetComponent<Animator>();

        if (playerData != null && playerData.animationController != null)
        {
            animator.runtimeAnimatorController = playerData.animationController;
        }
        else
        {
            Debug.LogWarning($"[{name}] PlayerData 또는 AnimationController가 누락되었습니다.");
        }
    }

    public void BindHpBar(HpBarDisplay bar)
    {
        hpBarDisplay = bar;
        hpBarDisplay.BindPlayerData(playerData);
    }


    /// <summary>
    /// 상태이상 혹은 버프를 적용하여 리스트에 추가
    /// </summary>
    /// <param name="effect">적용할 효과</param>
    public void ApplyStatusEffect(StatusEffect effect)
    {
        Debug.Log($"[버프 적용] {playerData.CharacterName} 에게 {effect.statType} +{effect.value} ({effect.duration}턴)");
        activeEffects.Add(new StatusEffect
        {
            statType = effect.statType,
            value = effect.value,
            duration = effect.duration
        });
    }

    /// <summary>
    /// 해당 스탯에 현재 적용 중인 버프를 계산하여 반환
    /// </summary>
    /// <param name="statType">수정할 스탯 타입</param>
    /// <param name="baseValue">기본값</param>
    /// <returns>버프 적용 후 최종 값</returns>
    public float ModifyStat(BuffStatType statType, float baseValue)
    {
        float modifiedValue = baseValue;
        foreach (var effect in activeEffects)
        {
            if (effect.statType == statType)
                modifiedValue += effect.value;
        }
        return modifiedValue;
    }

    /// <summary>
    /// 데미지를 받아 처리하는 함수
    /// </summary>
    /// <param name="amount">데미지 량</param>
    public void TakeDamage(float amount)
    {
        float reduced = amount - ModifyStat(BuffStatType.Defense, 0f);
        reduced = Mathf.Max(reduced, 1f);

        playerData.currentHP = Mathf.Max(0, playerData.currentHP - reduced);
        Debug.Log($"{playerData.CharacterName} 피해: {reduced}, 현재 체력: {playerData.currentHP}");

        if (playerData.currentHP <= 0)
        {
            Debug.Log($"{playerData.CharacterName} 사망");

            gameObject.SetActive(false); // ▶ 사망 시 비활성화
            GameManager.Instance.combatUIController?.CardStatusUpdate();
            if (GameManager.Instance != null && GameManager.Instance.turnController.battleFlow != null)
            {
                GameManager.Instance.turnController.battleFlow.CheckBattleEnd();
            }
        }
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    /// <param name="amount">회복량</param>
    public void Heal(float amount)
    {
        playerData.currentHP = Mathf.Min(playerData.MaxHP, playerData.currentHP +amount);
        Debug.Log($"{playerData.CharacterName} 회복: {amount}, 현재 체력: {playerData.currentHP}");
    }

    /// <summary>
    /// 생존 여부 확인
    /// </summary>
    /// <returns>체력이 0 초과인지 여부</returns>
    public bool IsAlive()
    {
        return playerData.currentHP > 0;
    }

    /// <summary>
    /// 플레이어 초기화 (데이터 및 클래스 설정)
    /// </summary>
    /// <param name="data">플레이어 데이터</param>
    /// <param name="charClass">캐릭터 클래스</param>
    public void Initialize(PlayerData data, CharacterClass charClass)
    {
        playerData = data;
        ChClass = charClass;
        deckModel = new DeckModel(); // 덱은 여기서 직접 생성하거나 외부에서 주입
    }

    /// <summary>
    /// 현재 덱 상태 출력 (디버그용)
    /// </summary>
    public void PrintDeckState()
    {
        Debug.Log($"[{playerData.CharacterName}] Hand: {Deck.Hand.Count}, Used: {Deck.UsedCount()}");
    }


    /// <summary>
    /// 매 턴마다 상태효과 지속시간 감소 및 종료 처리
    /// </summary>
    public void TickStatusEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].duration--;
            if (activeEffects[i].duration <= 0)
            {
                Debug.Log($"[버프 종료] {playerData.CharacterName} 의 {activeEffects[i].statType} 효과 종료");
                activeEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 특정 타입의 버프/디버프가 있는지 확인
    /// </summary>
    /// <param name="type">스탯 타입</param>
    /// <returns>존재 여부</returns>
    public bool HasEffect(BuffStatType type)
    {
        return activeEffects.Exists(e => e.statType == type && e.duration > 0);
    }

    /// <summary>
    /// 스턴 상태인지 확인
    /// </summary>
    /// <returns>스턴 여부</returns>
    public bool IsStunned() => HasEffect(BuffStatType.stun);

    // block 부여
    public void GrantBlock()
    {
        hasBlock = true;
        Debug.Log($"{playerData.CharacterName}에게 block 부여 (1턴 1회 무효화)");
    }

    // block 제거
    public void ClearBlock()
    {
        if (hasBlock)
        {
            Debug.Log($"{playerData.CharacterName}의 block 효과 만료");
            hasBlock = false;
        }
    }

    public void Setup(PlayerData data)
    {
        playerData = data;
        ChClass = data.CharacterClass;

        deckModel = new DeckModel();

        if (data.currentDeck == null || data.currentDeck.Count != 5)
            data.ResetDeckIndexesToDefault();

        data.LoadDeckFromIndexes(DataManager.Instance.AllCards);
        deckModel.Initialize(data.currentDeck);

        if (!IsAlive())
            playerData.ReviveIfDead();

        if (hpBarDisplay != null)
            hpBarDisplay.BindPlayerData(playerData);
    }


    //---
    /// <summary>
    /// Sophia/Kayla/Leon 아이콘 버튼에 연결할 메서드
    /// </summary>
    public void OnCharacterIconClicked()
    {

    }
    //---
    public void ChangeStance(PlayerData.StancType newStance) //StancUI 함수
    {


        PlayerData.StancValue stance = playerData.allStances.Find(s => s.stencType == newStance);
        if (stance != null)
        {
            playerData.currentStance = stance;
            float finalAtk = playerData.ATK + stance.attackBonus; //스텐스 공격력 계산
            float finalDef = playerData.DEF + stance.defenseBonus;

        }
        // ──────── K.T.H 변경 ────────
        var sv = playerData.allStances.Find(s => s.stencType == newStance); // newStance 에 맞는 StancValue 찾아서 currentStance 에 할당
        if (sv == null) return;

        playerData.currentStance = sv;
        // ──────── K.T.H 변경 ────────
    }


    /// <summary>
    /// 적이 공격해올 때 호출
    /// enemyStance에 따라 baseDamage를 배율로 처리
    /// 스탠스 간 상성에 맞춰 1배·1.5배·0배(회피)를 적용
    /// </summary>
    public void ReceiveAttack(PlayerData.StancType enemyStance, float baseDamage)
    {
        // 플레이어의 현재 스탠스
        var playerStance = playerData.currentStance.stencType;

        float finalDamage;

        // 1) 플레이어 Middle이면 항상 기본 데미지
        if (playerStance == PlayerData.StancType.Middle)
        {
            finalDamage = baseDamage;
        }
        // 2) 적·플레이어 스탠스가 같으면 1.5배
        else if (playerStance == enemyStance)
        {
            finalDamage = baseDamage * 1.5f;
        }
        // 3) 적 Low→플레이어 High, 또는 적 High→플레이어 Low 면 회피
        else if ((enemyStance == PlayerData.StancType.Low && playerStance == PlayerData.StancType.High) ||
                 (enemyStance == PlayerData.StancType.High && playerStance == PlayerData.StancType.Low))
        {
            finalDamage = 0f;
        }
        // 4) 그 외(적 Middle vs High/Low)는 기본 데미지
        else
        {
            finalDamage = baseDamage;
        }

        // 데미지 적용 또는 회피 로그 // 데미지 처리 확인
        if (finalDamage > 0f)
        {
            TakeDamage(finalDamage);
            Debug.Log($"[피해] {baseDamage} → {finalDamage} (enemy:{enemyStance}, player:{playerStance})");
        }
        else
        {
            Debug.Log($"[회피] enemy:{enemyStance} → player:{playerStance}");
        }
    }


    //public void ReceiveAttack(PlayerData.StancType enemyAttackStance, float damage)
    //{
    //    var playerStance = playerData.currentStance.stencType;

    //    if (playerStance == enemyAttackStance)
    //    {
    //        // 같은 위치 공격 -> 1.5배 피해
    //        float finalDamage = damage * 1.5f;
    //        TakeDamage(finalDamage);
    //        Debug.Log($"[타격] 같은 자세 공격! 피해 {finalDamage} 적용");
    //    }
    //    else if ((playerStance == PlayerData.StancType.High && enemyAttackStance == PlayerData.StancType.Low) ||
    //             (playerStance == PlayerData.StancType.Low && enemyAttackStance == PlayerData.StancType.High))
    //    {
    //        // 반대 위치 -> 회피
    //        Debug.Log("[회피] 반대 자세 공격을 회피함!");
    //    }
    //    else
    //    {
    //        // 기본 피해
    //        TakeDamage(damage);
    //        Debug.Log($"[피해] 일반 공격 피해 {damage} 적용");
    //    }
    //}

    public void CameraActionPlay()
    {
        Debug.Log($"[카메라 액션] {playerData.CharacterName}의 카메라 액션 실행");
        if(GameManager.Instance == null || GameManager.Instance.combatCameraController == null)
        {
            Debug.LogError("CameraController is not initialized.");
            return;
        }
        else if(this == null)
        {
            Debug.LogError("null");
            return;
        }
        //GameManager.Instance.combatCameraController.CameraZoomInAction(transform);
    }

    // 최대 체력
    public float maxHP
    {
        get => playerData.MaxHP;
        set => playerData.MaxHP = value;
    }
    //현재 체력
    public float currentHP
    {
        get => playerData.currentHP;
        set => playerData.currentHP = value;
    }
    //체력 변화 시
    public void UpdateHpStatus()
    {
        maxHP = playerData.MaxHP;
        currentHP = playerData.currentHP;
    }

    //공격 애니메이션 호출 시
    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("Attack", true);
            GameManager.Instance.StartCoroutine(ResetBool("Attack", 1f));
        }
    }
    //피격 애니메이션 호출 시
    public void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("Hit", true);
            GameManager.Instance.StartCoroutine(ResetBool("Hit", 1f));
        }
    }

    private IEnumerator ResetBool(string param, float delay)
    {
        yield return new WaitForSeconds(delay);
        // 오브젝트가 살아있을 때만 실행
        if (this != null && animator != null)
        {
            animator.SetBool(param, false);
        }
    }

    /// <summary>
    /// 자신의 위치 돌려주기
    /// </summary>
    public Transform CachedTransform => transform;

}
