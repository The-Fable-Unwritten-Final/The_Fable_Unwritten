using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/CharacterData")]


public class PlayerData : ScriptableObject
{
    [Header("Basic Stat")]
    public int IDNum; //고유번호
    public string CharacterName; //캐릭터이름
    public Sprite Icon; //Icon
    public float MaxHP; //체력
    public float ATK; //공격력
    public float DEF; //방어력
    public CharacterClass CharacterClass => (CharacterClass)IDNum;



    [Header("Stat Value")]
    public float[] HPValue; //체력변화
    public float[] ATKValue; //공격력 변화

    public enum StancType
    {
        High, //상단
        Middle, //중단
        Low //하단
    }
    [System.Serializable]
    public class StancValue
    {
        public float defenseBonus; //방어력 보너스
        public float attackBonus; //공격력 보너스
        public StancType stencType; //StencType 표시
        public Sprite stanceIcon; //자세 움직임 Sprite
    }

    public List<StancValue> allStances; //임시 작성
    public StancValue currentStance;

    public List<int> defaultDeckIndexes;// 시작 덱 카드 인덱스 저장 (CSV 기준 index)
    public List<int> currentDeckIndexes;// 현재 덱 카드 인덱스 저장 (CSV 기준 index, 세이브 로드 용)
    public List<CardModel> defaultDeck; // 처음 시작할 때 사용할 고정 덱
    public List<CardModel> currentDeck; // 게임 중 변화 가능한 덱

    public void ResetDeckToDefault()
    {
        currentDeck = new List<CardModel>(defaultDeck);
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
        currentDeck = new List<CardModel>();
        foreach (var index in currentDeckIndexes)
        {
            var card = cardPool.Find(c => c.index == index);
            if (card != null)
                currentDeck.Add(card);
        }
    }
}
