using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.UI.Extensions;
using System.Collections;

/// <summary>
/// 스테이지 노드와 연결선을 UI에 표시해주는 클래스
/// </summary>
public class StageMapRenderer : MonoBehaviour
{
    [Header("Prefabs & UI")]
    [SerializeField] GameObject nodePrefab;        // 노드 UI 프리팹
    [SerializeField] GameObject lineBasicPrefab;        // 라인 UI 프리팹
    [SerializeField] private Sprite lineBasicSprite;    // 기본 라인 Sprite
    [SerializeField] private Sprite lineCompleteSprite; // 지나온 라인 Sprite
    [SerializeField] RectTransform nodesContainer; // 노드 부모 오브젝트
    [SerializeField] RectTransform linesContainer; // 라인 부모 오브젝트

    [Header("Node Icons")]
    [SerializeField] Sprite startIcon;
    [SerializeField] Sprite normalIcon, eliteIcon, randomIcon, campIcon; // 노드아이콘 설정
    [SerializeField] private Sprite[] bossStageIcons;
    [SerializeField] private float[] bossIconCenterOffset; // 각 보스 아이콘별 중앙 위치 (점선의 목표 지점 설정용)
    [Header("Offsets")]
    [SerializeField] private float BossNodeXPos = 1450f;

    [Header("UI For Dialogue")]
    [SerializeField] private GameObject[] hideDuringDialogue;

    public GameObject[] GetUIToHideDuringDialogue() => hideDuringDialogue;

    public Dictionary<GraphNode, RectTransform> nodeUIMap = new();
    private readonly List<LineInfo> lineInfos = new();
    private bool isAnimating = false; // 애니메이션 중 클릭 방지

    /// <summary>
    /// 스테이지 노드 및 연결선을 UI에 표시 </summary>
    public void Render(StageData stage, Action<GraphNode> onClick)
    {
        nodeUIMap.Clear();
        lineInfos.Clear();

        foreach (var column in stage.columns)
        {
            foreach (var node in column)
            {
                var go = Instantiate(nodePrefab, nodesContainer);   // 해당 노드 UI 생성
                var rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = node.position;                // 해당 노드 위치 설정
                nodeUIMap[node] = rt;                               // 해당 노드의 UI 위치 저장 (라인 이어주기 위해)

                var ui = go.GetComponent<StageNode>();
                Sprite sp = GetIcon(node.type);
                ui.Setup(node.type, sp);                            // 해당 노드 이미지 설정
                rt.sizeDelta = sp.rect.size;                        // 노드 크기 아이콘 크기에 맞게 조절

                go.GetComponent<Button>().onClick.AddListener(() => onClick(node)); // 해당 노드 버튼 이벤트 연결

                if(node.type == NodeType.Boss)
                {
                    rt.anchoredPosition = new Vector2(BossNodeXPos, rt.anchoredPosition.y); // 보스 노드 위치 조정
                }
            }
        }

        //모든 노드에 대해 연결 된 노드들 정보 확인 후 라인 그려주기
        foreach (var node in nodeUIMap.Keys)
        {
            int index = 0;
            int bossIconIndex = ProgressDataManager.Instance.StageIndex - 2;
            if (bossIconIndex < 0 || bossIconIndex >= bossStageIcons.Length) bossIconIndex = 0;

            foreach (var next in node.nextNodes)
                {
                    var fromRT = nodeUIMap[node];
                    var toRT = nodeUIMap[next];

                    // 보스 노드의 도착 선 위치 세부 조정을 위해, 노드의 위치 임시 조정
                    Vector2 originalToPos = toRT.anchoredPosition;
                    if (next.type == NodeType.Boss)
                    {
                        toRT.anchoredPosition += Vector2.left * bossIconCenterOffset[bossIconIndex];                                  
                    }

                    GameObject line = LineDrawer.DrawLine(node.curvePointList[index], fromRT, toRT, linesContainer, lineBasicPrefab, 60f, next.type == NodeType.Boss);
                    var lineRenderer = line.GetComponent<UILineRenderer>();
                    var originalPoints = (Vector2[])lineRenderer.Points.Clone(); // 원본 포인트 저장
                    lineInfos.Add(new LineInfo { from = node, to = next, lineObj = line, originalPoints = originalPoints });

                    // 위치 복원
                    toRT.anchoredPosition = originalToPos;

                    index++;
                }
                
        }

        // 보스 노드와 다른 노드들 간의 최소 거리 보장 (라인 그린 후 실행)
        AdjustNodeSpacingForBoss();
        // 보스노드의 렌더링 조정 (점선이 상단에 표시되도록 >> 아트의 요청)
        foreach (var kvp in nodeUIMap)
        {
            if (kvp.Key.type == NodeType.Boss)
            {
                kvp.Value.SetAsFirstSibling();
                break;
            }
        }
    }

    /// <summary>
    /// 보스 노드와 다른 노드들 사이의 최소 거리(315)를 보장하는 메서드
    /// 각 열 간의 최소 거리를 확인하고, 필요한 경우에만 조정하여 자연스러운 분산 유도
    /// </summary>
    private void AdjustNodeSpacingForBoss()
    {
        const float minSpacing = 305f;
        const float minStartSpacing = 150f; // 시작 노드와 2번째 노드 간의 최소 거리

        // 보스 노드 찾기
        RectTransform bossRT = null;
        foreach (var kvp in nodeUIMap)
        {
            if (kvp.Key.type == NodeType.Boss)
            {
                bossRT = kvp.Value;
                break;
            }
        }

        if (bossRT == null) return;

        // 열별로 노드들을 그룹핑 (시작 노드와 보스 노드 제외)
        Dictionary<int, List<GraphNode>> columnGroups = new();
        foreach (var kvp in nodeUIMap)
        {
            var node = kvp.Key;
            if (node.type != NodeType.Boss && node.type != NodeType.Start)
            {
                if (!columnGroups.ContainsKey(node.columnIndex))
                    columnGroups[node.columnIndex] = new List<GraphNode>();
                columnGroups[node.columnIndex].Add(node);
            }
        }

        if (columnGroups.Count == 0) return;

        // 열의 최대 인덱스 구하기
        int maxColumnIndex = 0;
        foreach (var key in columnGroups.Keys)
        {
            if (key > maxColumnIndex) maxColumnIndex = key;
        }

        // 각 열의 오른쪽 끝 X값 계산
        Dictionary<int, float> columnMaxX = new();
        for (int col = 0; col <= maxColumnIndex; col++)
        {
            if (columnGroups.ContainsKey(col))
            {
                float maxX = float.MinValue;
                foreach (var node in columnGroups[col])
                {
                    var rt = nodeUIMap[node];
                    if (rt.anchoredPosition.x > maxX)
                        maxX = rt.anchoredPosition.x;
                }
                columnMaxX[col] = maxX;
            }
        }

        // 시작 노드 찾기 및 위치 확인
        RectTransform startRT = null;
        foreach (var kvp in nodeUIMap)
        {
            if (kvp.Key.type == NodeType.Start)
            {
                startRT = kvp.Value;
                break;
            }
        }

        // 보스 노드와의 최소 거리 확인
        float bossX = bossRT.anchoredPosition.x;
        float startX = startRT != null ? startRT.anchoredPosition.x : float.MinValue;
        float totalShiftNeeded = 0f;
        Dictionary<GraphNode, float> nodeShiftAmount = new();

        // 뒤에서 앞으로 이동하면서 누적된 이동량 적용
        for (int col = maxColumnIndex; col >= 0; col--)
        {
            if (!columnGroups.ContainsKey(col)) continue;

            float currentMaxX = columnMaxX[col];
            float distanceToBoss = bossX - currentMaxX;
            
            // 현재 열과 보스 사이의 거리가 부족하면 조정
            if (distanceToBoss < minSpacing)
            {
                float shortfallForThisColumn = minSpacing - distanceToBoss;
                totalShiftNeeded += shortfallForThisColumn;
            }

            // 2번째 열(columnIndex 1)의 경우 시작 노드와의 거리도 확인
            if (col == 1 && startRT != null)
            {
                float distanceToStart = currentMaxX - startX;
                if (distanceToStart < minStartSpacing)
                {
                    float extraShiftNeeded = minStartSpacing - distanceToStart;
                    totalShiftNeeded = Mathf.Max(totalShiftNeeded, extraShiftNeeded);
                }
            }

            // 현재 열의 모든 노드에 누적된 이동량 적용
            foreach (var node in columnGroups[col])
            {
                nodeShiftAmount[node] = totalShiftNeeded;
            }
        }

        // 계산된 이동량에 따라 노드 이동
        foreach (var kvp in nodeUIMap)
        {
            if (nodeShiftAmount.ContainsKey(kvp.Key))
            {
                Vector2 pos = kvp.Value.anchoredPosition;
                pos.x -= nodeShiftAmount[kvp.Key];
                kvp.Value.anchoredPosition = pos;
            }
        }

        // 이동한 노드들에 연결된 라인 포인트도 함께 조정
        foreach (var info in lineInfos)
        {
            float fromShift = nodeShiftAmount.ContainsKey(info.from) ? nodeShiftAmount[info.from] : 0f;
            float toShift = nodeShiftAmount.ContainsKey(info.to) ? nodeShiftAmount[info.to] : 0f;

            if (fromShift > 0 || toShift > 0)
            {
                var lineRenderer = info.lineObj.GetComponent<UILineRenderer>();
                Vector2[] points = (Vector2[])info.originalPoints.Clone();

                for (int i = 0; i < points.Length; i++)
                {
                    // 시작점과 끝점의 이동량에 따라 선형 보간으로 중간 지점 조정
                    float t = (float)i / (points.Length - 1);
                    float interpolatedShift = Mathf.Lerp(fromShift, toShift, t);
                    points[i].x -= interpolatedShift;
                }

                lineRenderer.Points = points;
            }
        }
    }
        
    /// <summary>
    /// 게임 진행 중 노드 상태 관리 매서드 </summary>
    public void UpdateInteractables(GraphNode current, List<GraphNode> visited)
    {
        // 
        foreach (var node in nodeUIMap.Keys)
        {
            var rt = nodeUIMap[node];
            var btn = rt.GetComponent<Button>();
            var stageNode = rt.GetComponent<StageNode>();

            // 지나온 노드 버튼 비활성화, 색상 초기화
            if (visited.Contains(node))
            {
                btn.interactable = false;
                btn.enabled = false;
                btn.image.color = new Color(0.36f, 0.28f, 0.20f); // 지나온 노드 색상
                stageNode.ShowClearCircle();
                stageNode.StopPulse();
            }
            // 진행가능 노드 버튼 활성화, 색상 초기화
            else if (current.nextNodes.Contains(node))
            {
                btn.enabled = true;
                btn.interactable = true;
                btn.image.color = Color.white;
                stageNode.HideClearCircle();
                stageNode.PlayPulse();
            }
            // 그 외 노드 버튼 비활성화, 색상 흐리게
            else
            {
                //btn.enabled = false;
                btn.interactable = false;
                //btn.image.color = new Color(1, 1, 1, 0.6f); 버튼 노드 불투명도 제거 요청
                btn.image.color = Color.white;
                stageNode.HideClearCircle();
                stageNode.StopPulse();
            }
        }

        HighlightLines(current, visited);
    }

    // 지나온 라인과 진행 못한 라인 표시해주는 함수
    private void HighlightLines(GraphNode current, List<GraphNode> visited)
    {
        foreach (var info in lineInfos)
        {
            var line = info.lineObj.GetComponent<UILineRenderer>();

            bool isVisitedFrom = visited.Contains(info.from);
            bool isVisitedTo = visited.Contains(info.to);
            bool isCompletePath = isVisitedFrom && isVisitedTo;

            Color lineColor = line.color;
            lineColor.a = isCompletePath ? 1f : 0.66f;
            line.color = lineColor;
        }


        /*
        foreach (var info in lineInfos)
        {
            var img = info.lineObj.GetComponent<Image>();

            bool isVisitedFrom = visited.Contains(info.from);
            bool isVisitedTo = visited.Contains(info.to);
            bool isCompletePath = isVisitedFrom && isVisitedTo;

            img.sprite = isCompletePath ? lineCompleteSprite : lineBasicSprite;
            img.color = Color.white;
        }*/
    }

    /// <summary>
    /// 기존 맵의 노드 및 라인들 제거 하는 매서드
    /// </summary>
    public void ClearMap()
    {
        foreach (var ui in nodeUIMap.Values)
            Destroy(ui.gameObject);
        nodeUIMap.Clear();

        foreach (var line in lineInfos)
            Destroy(line.lineObj);
        lineInfos.Clear();
    }

    /// <summary>
    /// 맵 중심이 화명 중앙에 오도록 정렬하는 매서드
    /// </summary>
    public void CenterMap()
    {
        nodesContainer.anchoredPosition = new Vector2(-800, 0);
        return;; // 임시로 중앙 정렬 비활성화
        if (nodeUIMap.Count == 0) return;

        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;

        // 노드들의 최소값, 최대값 확인
        foreach (var rt in nodeUIMap.Values)
        {
            Vector2 pos = rt.anchoredPosition;
            min = Vector2.Min(min, pos); // 가장 왼쪽 위
            max = Vector2.Max(max, pos); // 가장 오른쪽 아래
        }

        Vector2 center = (min + max) / 2f;
        nodesContainer.anchoredPosition = -center; // 노드 부모 위치 이동 
    }

    // 노드 타입에 따라 아이콘 변환
    private Sprite GetIcon(NodeType type)
    {
        switch (type)
        {
            case NodeType.Start:
                return startIcon;
            case NodeType.NormalBattle:
                return normalIcon;
            case NodeType.EliteBattle:
                return eliteIcon;
            case NodeType.RandomEvent:
                return randomIcon;
            case NodeType.Camp:
                return campIcon;
            case NodeType.Boss:
                return GetBossIcon(ProgressDataManager.Instance.StageIndex);
            default: return null;
        }
    }

    private Sprite GetBossIcon(int stageIndex)
    {
        var theme = ProgressDataManager.Instance.CurrentTheme;

        if (stageIndex == 5) return bossStageIcons.ElementAtOrDefault(3); // 보스스테이지 4번째 아이콘 가져오기

        int index = theme switch
        {
            StageTheme.Wisdom => 0,
            StageTheme.Love => 1,
            StageTheme.Courage => 2,
            _ => -1
        };

        if (index >= 0 && index < bossStageIcons.Length)
        {
            return bossStageIcons[index];
        }

        return null;
    }

    /// <summary>
    /// 노드 클릭 시 이전 노드에서 클릭한 노드로 가는 라인을 채우는 애니메이션
    /// </summary>
    public void AnimateLineFill(GraphNode from, GraphNode to, Action onComplete)
    {
        if (isAnimating) return; // 애니메이션 중이면 진행 안 함
        
        var linesToAnimate = lineInfos.Where(l => l.from == from && l.to == to).ToList();

        if (linesToAnimate.Count > 0)
        {
            StartCoroutine(AnimateLinesFillCoroutine(linesToAnimate, onComplete));
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private IEnumerator AnimateLinesFillCoroutine(List<LineInfo> lines, Action onComplete)
    {
        isAnimating = true;
        float duration = 2f; // 애니메이션 시간
        float elapsed = 0f;
        
        // 애니메이션용 임시 라인 생성
        List<GameObject> tempLines = new();
        foreach (var lineInfo in lines)
        {
            GameObject tempLine = Instantiate(lineInfo.lineObj, linesContainer);
            tempLine.name = lineInfo.lineObj.name + " (Temp)";
            tempLines.Add(tempLine);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < tempLines.Count; i++)
            {
                var tempLine = tempLines[i];
                var lineRenderer = tempLine.GetComponent<UILineRenderer>();
                // 현재 표시되는 라인의 조정된 포인트를 사용 (왼쪽 밀기 적용 후)
                var currentPoints = lines[i].lineObj.GetComponent<UILineRenderer>().Points;
                
                // 포인트 개수에서 현재 진행도만큼만 포인트 선택
                int totalPoints = currentPoints.Length;
                int visiblePointCount = Mathf.Max(2, Mathf.CeilToInt(totalPoints * t));
                
                // 채워질 부분의 포인트 배열 생성
                Vector2[] filledPoints = new Vector2[visiblePointCount];
                System.Array.Copy(currentPoints, filledPoints, visiblePointCount);
                
                lineRenderer.Points = filledPoints;
                
                // 색상 설정 744E19
                Color lineColor = new Color(0.455f, 0.306f, 0.098f);
                lineRenderer.color = lineColor;
            }

            yield return null;
        }
        
        // 임시 라인 제거
        foreach (var tempLine in tempLines)
        {
            Destroy(tempLine);
        }
        
        // 원본 라인의 알파값을 1로 설정
        foreach (var lineInfo in lines)
        {
            var line = lineInfo.lineObj.GetComponent<UILineRenderer>();
            Color lineColor = line.color;
            lineColor.a = 1f;
            line.color = lineColor;
        }
        
        isAnimating = false;
        onComplete?.Invoke();
    }
}
