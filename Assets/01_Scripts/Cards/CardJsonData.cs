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

    public int evolveCount;
    public int evolveTarget;
    public List<string> keywords;
    public string switchType;

    public List<CardEffect> effects;

    public List<int> soundIndexes;
    public List<float> soundDelays;
}

[System.Serializable]
public class CardEffect
{
    public string type;
    public int value;
    public int duration;
    public int target;

    public string owner;

    public EffectCondition condition;
    public ResultEffect result;
}

[System.Serializable]
public class EffectCondition
{
    public string trigger;
    public List<string> value;
}

[System.Serializable]
public class ResultEffect
{
    public string type;
    public int value;
    public int duration;
    public int target;
}