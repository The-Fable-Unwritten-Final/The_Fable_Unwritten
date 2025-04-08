using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageData
{
    public int stageIndex;                        // 스테이지 인덱스
    public int columnCount;                       // 스테이지 보유 열 개수
    public List<List<GraphNode>> columns = new(); // 스테이지 보유 열(노드리스트) 정보 리스트
}
