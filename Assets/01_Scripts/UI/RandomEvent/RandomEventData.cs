using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomEventData
{
    public int index;
    public StageTheme theme;
    public string bg_illust;
    public string title;
    public string illustration;
    public string description;

    public string option_0;
    public float percentage_0;
    public string description_01;
    public string description_02;
    public string result_01;
    public string result_02;

    public string option_1;
    public float percentage_1;
    public string description_11;
    public string description_12;
    public string result_11;
    public string result_12;

    [NonSerialized] public Sprite illustrationSprite;
}

[Serializable]
public class RandomEventList
{
    public List<RandomEventData> events;
}
