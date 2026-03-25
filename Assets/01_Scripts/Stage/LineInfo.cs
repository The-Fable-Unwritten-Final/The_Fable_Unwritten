using UnityEngine;

public class LineInfo
{
    public GraphNode from;     // 시작 노드
    public GraphNode to;       // 도착 노드
    public GameObject lineObj; // 연결선 오브젝트
    public Vector2[] originalPoints; // 원본 라인 포인트
}
