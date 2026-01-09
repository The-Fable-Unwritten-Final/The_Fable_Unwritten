
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardJsonData
{
    public int index;
    public int cost;
    public string illustration;
    public string name;
    public string text;
    public int type;
    public int @class;
    public string cardframe;
    public int target_type;
    public int target_num;
    public string note;
    public string flavortext;
    public string skilleffect;
    public List<CardEffect> effects;

    public List<string> keywords;    // 키워드 배열
    public string switchType;        // 스위치 스탠스
    public int evolveCount;          // 성장 횟수
    public int evolveTarget;         // 성장 후 카드 index
}

[System.Serializable]
public class CardEffect
{
    public string type;
    public int value;
    public int duration;
    public int target;
    public EffectCondition condition;
    public ResultEffect result;
}

[System.Serializable]
public class EffectCondition
{
    public string trigger;
    public List<int> value;
}

[System.Serializable]
public class ResultEffect
{
    public string type;
    public int value;
    public int duration;
    public int target;
}

