using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Start = 0,        // 시작노드
    NormalBattle = 1, // 일반전투
    EliteBattle = 2,  // 정예전투 
    Boss = 3,         // 보스
    RandomEvent = 4,  // 랜덤이벤트
    Camp = 5          // 야영지    
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
