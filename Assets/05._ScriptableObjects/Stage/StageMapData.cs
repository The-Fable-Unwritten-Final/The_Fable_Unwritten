using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageType
{
    Intro,
    Middle,
    Final
}

[CreateAssetMenu(menuName = "Stage/MapData")]
public class StageMapData : ScriptableObject
{
    public StageType Type;
    public int StageLevel;
    public int Row; // 행 - 가로
    public int Col; // 열 - 세로
}
