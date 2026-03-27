using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupUI_MiniMap : BasePopupUI
{
    [Header("UI Components")]
    [SerializeField] private RectTransform minimapContainer;      // 미니맵 컨테이너
    [SerializeField] private GameObject miniNodePrefab;           // 미니 노드 프리팹
    [SerializeField] private GameObject miniLinePrefab;           // 미니 연결선 프리팹

    [Header("Node Icons")]
    [SerializeField] private Sprite startIcon;
    [SerializeField] private Sprite normalIcon, eliteIcon, randomIcon, campIcon;
    [SerializeField] private Sprite[] bossStageIcons;

    [Header("Scale Settings")]
    [SerializeField] private float minimapScale = 0.6f;          // 미니맵 축소 비율
    [SerializeField] private Vector2 minimapOffset = Vector2.zero; // 미니맵 오프셋

    [Header("Node Colors")]
    [SerializeField] private Color visitedNodeColor = new Color(0.36f, 0.28f, 0.2f, 1f);   // 방문한 노드 색상
    [SerializeField] private Color currentNodeColor = new Color(0.36f, 0.28f, 0.2f, 1f);          // 현재 노드 색상
    [SerializeField] private Color unvisitedNodeColor = new Color(1f, 1f, 1f, 0.7f);      // 미방문 노드 색상

    [Header("Line Settings")]
    [SerializeField] private Color visitedLineColor = new Color(0.45f, 0.31f, 0.1f, 1f);    // 방문한 경로 색상
    [SerializeField] private Color unvisitedLineColor = new Color(0.45f, 0.31f, 0.1f, 0.66f); // 미방문 경로 색상
    [SerializeField] private float lineWidth = 2f;                                        // 직선 폭

    private Dictionary<GraphNode, Image> nodeImageMap = new();     // 노드-UI 매핑
    private List<GameObject> pooledNodes = new();                  // 풀링된 노드
    private List<GameObject> pooledLines = new();                  // 풀링된 라인
    private ProgressDataManager progressManager;
    private StageData currentStageData;
    private List<GraphNode> visitedNodes;
    private bool isInitialized = false;
    private int lastKnownStageIndex = -1;  // 마지막으로 초기화한 스테이지 인덱스

    private void Awake()
    {
        // 최초 초기화 (Start() 보다 먼저 실행)
        progressManager = ProgressDataManager.Instance;
    }

    private void Start()
    {
        // Awake에서 이미 초기화됨
    }

    public override void Open()
    {
        base.Open();
        
        // 필수 데이터 검증
        if (progressManager == null)
        {
            Debug.LogError("[PopupUI_MiniMap] progressManager is null");
            return;
        }

        if (minimapContainer == null)
        {
            Debug.LogError("[PopupUI_MiniMap] minimapContainer is not assigned in Inspector");
            return;
        }

        if (miniNodePrefab == null || miniLinePrefab == null)
        {
            Debug.LogError("[PopupUI_MiniMap] miniNodePrefab or miniLinePrefab is not assigned in Inspector");
            return;
        }

        // 스테이지 변경 감지 - 풀 재초기화 필요한지 확인
        int currentStageIndex = progressManager.StageIndex;
        if (lastKnownStageIndex != currentStageIndex)
        {
            // 스테이지 변경 감지됨 - 풀 캐시 리셋 및 재초기화
            ResetPoolCache();
            lastKnownStageIndex = currentStageIndex;
            isInitialized = false;
        }

        if (!isInitialized)
        {
            InitializeMinimap();
            isInitialized = true;
        }
        
        RefreshMinimap();
    }

    /// <summary>
    /// 풀 캐시 리셋 (스테이지 변경 시 호출)
    /// </summary>
    private void ResetPoolCache()
    {
        foreach (var node in pooledNodes)
        {
            if (node != null)
                Destroy(node);
        }
        foreach (var line in pooledLines)
        {
            if (line != null)
                Destroy(line);
        }

        pooledNodes.Clear();
        pooledLines.Clear();
        nodeImageMap.Clear();
    }

    public override void Close()
    {
        // 모든 풀 UI 비활성화 (파괴 안 함)
        foreach (var node in pooledNodes)
            node.SetActive(false);
        foreach (var line in pooledLines)
            line.SetActive(false);

        base.Close();
    }

    /// <summary>
    /// 미니맵 초기화 (최초 1회만 실행) - UI 프리팹 풀링
    /// </summary>
    private void InitializeMinimap()
    {
        try
        {
            currentStageData = progressManager.SavedStageData;
            if (currentStageData == null)
            {
                Debug.LogWarning("[InitializeMinimap] SavedStageData is null");
                return;
            }

            if (currentStageData.columns == null || currentStageData.columns.Count == 0)
            {
                Debug.LogWarning("[InitializeMinimap] Stage columns is null or empty");
                return;
            }

            // 예상되는 최대 노드 수 계산
            int totalNodes = currentStageData.columns.Sum(col => col?.Count ?? 0);
            
            // 마지막 열(보스)을 제외한 모든 노드의 연결선 계산
            int totalLines = currentStageData.columns
                .Select((col, idx) => 
                    idx == currentStageData.columns.Count - 1 ? 0 : col?.Sum(node => node.nextNodes?.Count ?? 0) ?? 0
                )
                .Sum();

            // 미리 풀 생성 (예상 수량보다 20% 더 많이)
            int nodePoolCount = Mathf.Max(1, (int)(totalNodes * 1.2f));
            int linePoolCount = Mathf.Max(1, (int)(totalLines * 1.2f));

            for (int i = 0; i < nodePoolCount; i++)
            {
                GameObject nodeGo = Instantiate(miniNodePrefab, minimapContainer);
                nodeGo.SetActive(false);
                pooledNodes.Add(nodeGo);
            }

            for (int i = 0; i < linePoolCount; i++)
            {
                GameObject lineGo = Instantiate(miniLinePrefab, minimapContainer);
                lineGo.SetActive(false);
                pooledLines.Add(lineGo);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InitializeMinimap] 초기화 중 오류:\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 미니맵 갱신 - 현재 진행 정보 기반으로 업데이트 (파괴 없이 재사용)
    /// </summary>
    private void RefreshMinimap()
    {
        try
        {
            // 모든 풀링된 UI 비활성화
            foreach (var node in pooledNodes)
                node.SetActive(false);
            foreach (var line in pooledLines)
                line.SetActive(false);

            nodeImageMap.Clear();

            currentStageData = progressManager.SavedStageData;
            visitedNodes = progressManager.VisitedNodes;
            var currentBattleNode = progressManager.CurrentBattleNode;

            if (currentStageData == null || currentStageData.columns == null || currentStageData.columns.Count == 0)
            {
                Debug.LogError($"[RefreshMinimap] Invalid StageData - SavedStageData: {currentStageData}, Columns: {currentStageData?.columns?.Count ?? -1}, StageIndex: {progressManager.StageIndex}");
                return;
            }

            if (visitedNodes == null)
            {
                Debug.LogWarning("[RefreshMinimap] VisitedNodes is null");
                return;
            }

            // 1단계: 모든 노드 활성화 및 설정
            CreateAllMiniNodes(currentBattleNode);

            // 2단계: 노드 간 연결선 그리기
            DrawConnectionLines();

            // 3단계: 노드 상태에 따른 색상 업데이트
            UpdateNodeColors();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[RefreshMinimap] 갱신 중 오류:\n{ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 모든 미니 노드 생성/갱신 (풀에서 재사용)
    /// </summary>
    private void CreateAllMiniNodes(GraphNode currentNode)
    {
        int nodeIndex = 0;

        foreach (var column in currentStageData.columns)
        {
            if (column == null)
            {
                Debug.LogWarning("[CreateAllMiniNodes] Null column encountered");
                continue;
            }

            foreach (var node in column)
            {
                if (node == null)
                {
                    Debug.LogWarning("[CreateAllMiniNodes] Null node encountered");
                    continue;
                }

                if (nodeIndex >= pooledNodes.Count)
                {
                    Debug.LogWarning($"[CreateAllMiniNodes] Not enough pooled nodes ({pooledNodes.Count}) for node count");
                    break;
                }

                GameObject miniNodeGo = pooledNodes[nodeIndex];
                miniNodeGo.SetActive(true);
                RectTransform miniNodeRt = miniNodeGo.GetComponent<RectTransform>();
                Image miniNodeImage = miniNodeGo.GetComponent<Image>();

                if (miniNodeRt == null || miniNodeImage == null)
                {
                    Debug.LogError("[CreateAllMiniNodes] miniNodePrefab is missing RectTransform or Image component");
                    continue;
                }

                // 위치 설정 (축소 비율 적용)
                Vector2 scaledPos = node.position * minimapScale + minimapOffset;
                
                // 보스 노드는 추가로 PosX +50 거리 설정
                if (node.type == NodeType.Boss)
                {
                    scaledPos.x += 50f;
                }
                
                miniNodeRt.anchoredPosition = scaledPos;

                // 크기 설정 (축소 비율 적용)
                Sprite nodeIcon = GetIcon(node.type);
                if (nodeIcon != null)
                {
                    Vector2 scaledSize = nodeIcon.rect.size * minimapScale;
                    
                    // 보스 노드는 추가로 70% 크기로 줄임
                    if (node.type == NodeType.Boss)
                    {
                        scaledSize *= 0.7f;
                    }
                    
                    miniNodeRt.sizeDelta = scaledSize;
                    miniNodeImage.sprite = nodeIcon;
                }
                else
                {
                    Debug.LogWarning($"[CreateAllMiniNodes] No icon found for node type {node.type}");
                }

                nodeImageMap[node] = miniNodeImage;
                nodeIndex++;
            }
        }
    }

    /// <summary>
    /// 노드 간 연결선 그리기 (풀에서 재사용)
    /// </summary>
    private void DrawConnectionLines()
    {
        int lineIndex = 0;

        foreach (var column in currentStageData.columns)
        {
            if (column == null) continue;

            foreach (var node in column)
            {
                if (node == null) continue;
                if (node.nextNodes == null) continue;

                foreach (var nextNode in node.nextNodes)
                {
                    if (nextNode == null) continue;
                    if (lineIndex >= pooledLines.Count)
                    {
                        Debug.LogWarning($"[DrawConnectionLines] Not enough pooled lines for connection count");
                        return;
                    }

                    if (nodeImageMap.TryGetValue(node, out var fromImage) && 
                        nodeImageMap.TryGetValue(nextNode, out var toImage))
                    {
                        DrawDirectLine(
                            pooledLines[lineIndex],
                            fromImage.rectTransform.anchoredPosition,
                            toImage.rectTransform.anchoredPosition,
                            node,
                            nextNode
                        );
                        lineIndex++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 두 노드 사이에 직선 그리기 (풀 재사용) - 노드 경계에서 시작/종료
    /// </summary>
    private void DrawDirectLine(GameObject lineGo, Vector2 fromPos, Vector2 toPos, GraphNode fromNode, GraphNode toNode)
    {
        if (lineGo == null)
        {
            Debug.LogError("[DrawDirectLine] lineGo is null");
            return;
        }

        lineGo.SetActive(true);
        lineGo.transform.SetAsFirstSibling(); // 노드 뒤에 표시

        RectTransform lineRt = lineGo.GetComponent<RectTransform>();
        Image lineImage = lineGo.GetComponent<Image>();

        if (lineRt == null || lineImage == null)
        {
            Debug.LogError("[DrawDirectLine] miniLinePrefab is missing RectTransform or Image component");
            return;
        }

        // 노드 이미지에서 크기 정보 가져오기
        float fromNodeRadius = 0f;
        float toNodeRadius = 0f;
        
        if (nodeImageMap.TryGetValue(fromNode, out var fromImage) && fromImage != null)
        {
            RectTransform fromRt = fromImage.rectTransform;
            fromNodeRadius = fromRt.sizeDelta.magnitude / 2f;  // 노드의 대략적인 반경
        }
        
        if (nodeImageMap.TryGetValue(toNode, out var toImage) && toImage != null)
        {
            RectTransform toRt = toImage.rectTransform;
            toNodeRadius = toRt.sizeDelta.magnitude / 2f;  // 노드의 대략적인 반경
            
            // 보스 노드는 선이 더 길게 보이도록 반경을 절반으로 줄임
            if (toNode.type == NodeType.Boss)
            {
                toNodeRadius *= 0.5f;
            }
        }

        // 직선의 방향 벡터 계산
        Vector2 direction = toPos - fromPos;
        float fullDistance = direction.magnitude;
        Vector2 normalizedDirection = direction.normalized;

        // 노드 경계에서 시작/종료하도록 위치 조정
        Vector2 adjustedFromPos = fromPos + normalizedDirection * fromNodeRadius;
        Vector2 adjustedToPos = toPos - normalizedDirection * toNodeRadius;

        // 조정된 거리로 직선 계산
        Vector2 adjustedDirection = adjustedToPos - adjustedFromPos;
        float adjustedDistance = adjustedDirection.magnitude;
        float angle = Mathf.Atan2(adjustedDirection.y, adjustedDirection.x) * Mathf.Rad2Deg;

        // 중점 설정
        Vector2 midPoint = (adjustedFromPos + adjustedToPos) / 2f;
        lineRt.anchoredPosition = midPoint;

        // 크기 설정
        lineRt.sizeDelta = new Vector2(adjustedDistance, lineWidth);
        lineRt.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 경로 상태에 따른 색상 설정
        bool isVisitedPath = visitedNodes != null && visitedNodes.Contains(fromNode) && visitedNodes.Contains(toNode);
        lineImage.color = isVisitedPath ? visitedLineColor : unvisitedLineColor;
    }

    /// <summary>
    /// 노드 상태에 따른 색상 업데이트
    /// </summary>
    private void UpdateNodeColors()
    {
        var currentBattleNode = progressManager?.CurrentBattleNode;

        if (visitedNodes == null)
        {
            Debug.LogWarning("[UpdateNodeColors] visitedNodes is null");
            return;
        }

        // 시작노드 찾기 (columns[0][0])
        GraphNode startNode = null;
        if (currentStageData.columns.Count > 0 && currentStageData.columns[0].Count > 0)
        {
            startNode = currentStageData.columns[0][0];
        }

        foreach (var kvp in nodeImageMap)
        {
            GraphNode node = kvp.Key;
            Image nodeImage = kvp.Value;

            if (node == null || nodeImage == null) continue;

            if (node == currentBattleNode)
            {
                // 현재 노드: 노란색으로 강조
                nodeImage.color = currentNodeColor;
            }
            else if (node == startNode)
            {
                // 시작 노드: 완료된 상태(녹색)
                nodeImage.color = visitedNodeColor;
            }
            else if (visitedNodes.Contains(node))
            {
                // 방문한 노드: 녹색
                nodeImage.color = visitedNodeColor;
            }
            else
            {
                // 미방문 노드: 흰색 (반투명)
                nodeImage.color = unvisitedNodeColor;
            }
        }
    }

    /// <summary>
    /// 노드 타입별 아이콘 반환
    /// </summary>
    private Sprite GetIcon(NodeType type)
    {
        try
        {
            switch (type)
            {
                case NodeType.Start:
                    if (startIcon == null) Debug.LogWarning("startIcon is not assigned");
                    return startIcon;
                case NodeType.NormalBattle:
                    if (normalIcon == null) Debug.LogWarning("normalIcon is not assigned");
                    return normalIcon;
                case NodeType.EliteBattle:
                    if (eliteIcon == null) Debug.LogWarning("eliteIcon is not assigned");
                    return eliteIcon;
                case NodeType.RandomEvent:
                    if (randomIcon == null) Debug.LogWarning("randomIcon is not assigned");
                    return randomIcon;
                case NodeType.Camp:
                    if (campIcon == null) Debug.LogWarning("campIcon is not assigned");
                    return campIcon;
                case NodeType.Boss:
                    return GetBossIcon(progressManager?.StageIndex ?? 1);
                default:
                    Debug.LogWarning($"Unknown NodeType: {type}");
                    return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting icon for type {type}:\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 스테이지 인덱스별 보스 아이콘 반환
    /// </summary>
    private Sprite GetBossIcon(int stageIndex)
    {
        try
        {
            if (bossStageIcons == null || bossStageIcons.Length == 0)
            {
                Debug.LogWarning("bossStageIcons is not assigned or empty");
                return null;
            }

            if (stageIndex == 5) 
            {
                if (bossStageIcons.Length > 3)
                    return bossStageIcons[3];
                else
                    return bossStageIcons[bossStageIcons.Length - 1];
            }
            
            int bossIconIndex = stageIndex - 2;
            if (bossIconIndex < 0 || bossIconIndex >= bossStageIcons.Length) 
                bossIconIndex = 0;
                
            return bossStageIcons[bossIconIndex];
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting boss icon for stage {stageIndex}:\n{ex.Message}");
            return null;
        }
    }
}
