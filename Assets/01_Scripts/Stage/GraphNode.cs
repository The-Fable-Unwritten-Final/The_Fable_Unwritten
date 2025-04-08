using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Start,        // 시작노드
    NormalBattle, // 일반전투
    EliteBattle,  // 정예전투 
    RandomEvent,  // 랜덤이벤트
    Camp,         // 야영지
    Boss          // 보스
}

[SerializeField]
public class GraphNode
{
    public int id;                             // 노드 ID
    public NodeType type;                      // 노드 타입
    public Vector2 position;                   // 노드 위치
    public int columnIndex;                    // 현재 노드가 속한 열 인덱스
    public List<GraphNode> nextNodes = new();  // 다음 노드 리스트
}
