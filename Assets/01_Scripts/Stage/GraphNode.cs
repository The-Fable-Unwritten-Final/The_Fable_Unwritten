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

    public void SetRandomCurvePoints(float offsetFromNode = 55f)
    {
        const float maxBendAngle = 38f;
        int nextNodeCount = nextNodes.Count;
        curvePointList.Clear();
        
        for (int i = 0; i < nextNodeCount; i++)
        {
            // 거리 먼저 계산
            Vector2 lineDir = (nextNodes[i].position - this.position).normalized;
            float lineDistance = Vector2.Distance(this.position, nextNodes[i].position);
            
            // 거리가 245 미만이면 직선 제한, 그 이상이면 곡선 가능 (거리가 짧은 경우 곡선을 구현하려할 때 선 생성에 왜곡이 심해져서 직선으로 고정)
            int controlPointCount = lineDistance < 245f ? 0 : Random.Range(1, 3);
            CurvePoint curvePoint = new CurvePoint();
            curvePoint.position = new Vector2[controlPointCount];
            
            // 모든 제어점에 적용할 일관된 곡선 방향 결정
            float consistentAngle = Random.Range(-maxBendAngle, maxBendAngle);
            
            // 회전 계산 (Z축 기준으로 회전)
            Quaternion rotation = Quaternion.AngleAxis(consistentAngle, Vector3.forward);
            Vector3 bendDir = rotation * new Vector3(lineDir.x, lineDir.y, 0);
            
            // offset이 적용된 실제 시작/끝 위치
            Vector2 insetStart = this.position + lineDir * offsetFromNode;
            Vector2 insetEnd = nextNodes[i].position - lineDir * offsetFromNode;
            
            for (int j = 0; j < controlPointCount; j++)
            {
                float t = (j + 1f) / (controlPointCount + 1f);
                Vector2 basePos = Vector2.Lerp(this.position, nextNodes[i].position, t);
                
                // 곡선의 중앙이 가장 많이 구부러지도록 (포물선 형태) - 모든 제어점에 동일한 구부러짐 적용
                float bendAmount = Mathf.Sin(Mathf.PI * 0.5f) * lineDistance * 0.30f;
                curvePoint.position[j] = basePos + ((Vector2)bendDir).normalized * bendAmount;
                
                // 제어점이 offset된 범위를 넘지 않도록 제한 (낚시바늘 모양 등의 왜곡 방지)
                float minX = Mathf.Min(insetStart.x, insetEnd.x);
                float maxX = Mathf.Max(insetStart.x, insetEnd.x);
                curvePoint.position[j].x = Mathf.Clamp(curvePoint.position[j].x, minX, maxX);
            }
            
            curvePointList.Add(curvePoint);
        }
    }
}
