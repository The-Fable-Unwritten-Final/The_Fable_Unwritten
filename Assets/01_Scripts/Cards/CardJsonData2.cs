
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardJsonData2
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
    public List<CardEffect2> effects;

    public List<string> keywords;
    public string switchType;
    public int evolveCount;
    public int evolveTarget;
}

[System.Serializable]
public class CardEffect2
{
    public string type;
    public int value;
    public int duration;
    public int target;
    public string owner;
    public EffectCondition2 condition;
    public ResultEffect2 result;
}

[System.Serializable]
public class EffectCondition2
{
    public string trigger;
    public List<string> value;
}

[System.Serializable]
public class ResultEffect2
{
    public string type;
    public int value;
    public int duration;
    public int target;
}

