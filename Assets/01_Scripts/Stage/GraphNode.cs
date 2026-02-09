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
public class CurvePoint
{
    public Vector2[] position; // 곡선 제어점 위치
}
[SerializeField]
public class GraphNode
{
    public int id;                             // 노드 ID
    public NodeType type;                      // 노드 타입
    public Vector2 position;                   // 노드 위치
    public int columnIndex;                    // 현재 노드가 속한 열 인덱스
    public List<GraphNode> nextNodes = new();  // 다음 노드 리스트
    public List<CurvePoint> curvePointList = new(); // 다음 노드 별 곡선 제어점 리스트

    public void SetRandomCurvePoints()
    {
        const float maxBendAngle = 45f; // 곡선 제한
        int nextNodeCount = nextNodes.Count;
        curvePointList.Clear();
        for (int i = 0; i < nextNodeCount; i++)
        {
            int controlPointCount = Random.Range(0, 3); // 0~2개의 제어점 생성
            CurvePoint curvePoint = new CurvePoint();
            curvePoint.position = new Vector2[controlPointCount];
            // 본인의 position과 nextNodes[i] 의 위치를 기준으로 제어점 위치 설정
            for (int j = 0; j < controlPointCount; j++)
            {
                float t = (j + 1f) / (controlPointCount + 1f); // 0~1 사이의 비율
                Vector2 basePos = Vector2.Lerp(this.position, nextNodes[i].position, t);
                Vector2 lineDir = (nextNodes[i].position - this.position).normalized;

                // 최대 bend 각도 범위 내에서 랜덤 오프셋 생성
                float randomAngle = Random.Range(-maxBendAngle, maxBendAngle);
                float distance = Random.Range(20f, 60f);
                
                Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
                Vector3 offsetDir = rotation * new Vector3(lineDir.x, lineDir.y, 0);
                
                curvePoint.position[j] = basePos + ((Vector2)offsetDir).normalized * distance;
            }
            curvePointList.Add(curvePoint);
        }
    }
}
