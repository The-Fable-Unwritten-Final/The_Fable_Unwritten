using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RandomEventData
{
    public int index;
    public int repeatIndex; // 반복 이벤트시 실행될 연속 인덱스
    public StageTheme theme;
    public string title;
    public string illustration;
    public string description;

    public string option_a;
    public string option_b;
    public float percentage_a;
    // 선택지를 선택했을때 출력되는 설명 (percentage의 확률로 1,2 가 나뉨)
    public string description_a1;
    public string description_a2;
    // 선택지별 결과(적용될 효과)값 index => 이 값을 토대로 실제 적용될 효과를 하단 버튼에 표시
    public string result_a1; // 만약 이게 -1 이면 반복 이벤트로 처리
    public string result_a2;
    public float percentage_b;
    // 선택지를 선택했을때 출력되는 설명
    public string description_b1;
    public string description_b2;
    // 선택지별 결과(적용될 효과)값 index
    public string result_b1;
    public string result_b2;

    [NonSerialized] public Sprite illustrationSprite;

    [NonSerialized] public List<int> parsed_result_a1 = new();
    [NonSerialized] public List<int> parsed_result_a2 = new();
    [NonSerialized] public List<int> parsed_result_b1 = new();
    [NonSerialized] public List<int> parsed_result_b2 = new();

    public void ParseResults()
    {
        parsed_result_a1 = ParseResultString(result_a1);
        parsed_result_a2 = ParseResultString(result_a2);
        parsed_result_b1 = ParseResultString(result_b1);
        parsed_result_b2 = ParseResultString(result_b2);
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
