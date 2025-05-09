using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RandomEventData
{
    public int index;
    public StageTheme theme;
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

    [NonSerialized] public List<int> parsed_result_01 = new();
    [NonSerialized] public List<int> parsed_result_02 = new();
    [NonSerialized] public List<int> parsed_result_11 = new();
    [NonSerialized] public List<int> parsed_result_12 = new();

    public void ParseResults()
    {
        parsed_result_01 = ParseResultString(result_01);
        parsed_result_02 = ParseResultString(result_02);
        parsed_result_11 = ParseResultString(result_11);
        parsed_result_12 = ParseResultString(result_12);
    }

    private List<int> ParseResultString(string resultStr)
    {
        if (string.IsNullOrWhiteSpace(resultStr)) return new();
        return resultStr
            .Split('&')
            .Select(s => int.TryParse(s.Trim(), out int value) ? value : -1)
            .Where(i => i >= 0)
            .ToList();
    }

    
}

[Serializable]
public class RandomEventList
{
    public List<RandomEventData> events;
}
