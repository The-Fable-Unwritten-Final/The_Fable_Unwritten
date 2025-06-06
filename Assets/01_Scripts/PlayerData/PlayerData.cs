using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/CharacterData")]


public class PlayerData : ScriptableObject
{
    public event Action<float, float> OnHpChanged; //체력 변경 감지 이벤트

    [Header("Basic Stat")]
    public int IDNum; //고유번호
    public string CharacterName; //캐릭터이름
    public Sprite Icon; //Icon

    [SerializeField] private float _maxHP;  //최대 체력
    public float MaxHP
    {
        get => _maxHP;
        set
        {
            _maxHP = value;
            OnHpChanged?.Invoke(currentHP, _maxHP);
        }
    }

    [SerializeField] private float _currentHP;   //현재 체력
    public float currentHP
    {
        get => _currentHP;
        set
        {
            _currentHP = Mathf.Clamp(value, 0, MaxHP);
            OnHpChanged?.Invoke(_currentHP, MaxHP);
        }
    }

    public float ATK; //공격력
    public float DEF; //방어력
    public CharacterClass CharacterClass => (CharacterClass)IDNum;



    public RuntimeAnimatorController animationController;


    [Header("Stat Value")]
    public float[] HPValue; //체력변화
    public float[] ATKValue; //공격력 변화

    public enum StancType
    {
        refine,
        mix,
        grace,
        judge,
        guard,
        rush
    }

    public StancType currentStance;//현재 자세
    public CardType? FirstTimeUsedType;  //소피아 처음 사용 카드 확인 용

    public List<int> defaultDeckIndexes;// 시작 덱 카드 인덱스 저장 (CSV 기준 index)
    public List<int> currentDeckIndexes;// 현재 덱 카드 인덱스 저장 (CSV 기준 index, 세이브 로드 용)
    public List<CardModel> currentDeck; // 게임 중 변화 가능한 덱

    /// <summary>
    /// 기본 덱 인덱스로 현재 덱 인덱스 초기화
    /// </summary>
    public void ResetDeckIndexesToDefault()
    {
        currentDeckIndexes = new List<int>(defaultDeckIndexes);
    }

    // 세이브를 위한 현재 덱 인덱스 변환
    public void UpdateCurrentDeckIndexes()
    {
        currentDeckIndexes = new List<int>();
        foreach (var card in currentDeck)
            currentDeckIndexes.Add(card.index);
    }

    // 로드 시 인덱스 기반으로 덱 구성
    public void LoadDeckFromIndexes(List<CardModel> cardPool)
    {
        // 인덱스가 비어 있으면 기본 덱 인덱스로 설정
        if (currentDeckIndexes == null || currentDeckIndexes.Count == 0)
        {
            ResetDeckIndexesToDefault();
        }

        // 인덱스를 기준으로 현재 덱 구성
        currentDeck = new List<CardModel>();
        foreach (var index in currentDeckIndexes)
        {
            var card = cardPool.Find(c => c.index == index);
            if (card != null)
                currentDeck.Add(card);
        }
    }
    //체력 초기화 용
    public void ResetHPToMax()
    {
        currentHP = MaxHP;
    }
    //부활 용
    public void ReviveIfDead()
    {
        if (currentHP <= 0)
            currentHP = 1;
    }

    public void ResetCurCard()
    {
        FirstTimeUsedType = null;
    }
}
