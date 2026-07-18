using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 노드 텔레포트 이벤트 효과 (현재는 마지막 열(보스) 바로 앞 열로 이동 하는 효과만 존재)
/// </summary>
public class NodeTeleportEventEffect : EventEffects
{

    public override void Apply()
    {
        var progressDataManager = ProgressDataManager.Instance;

        // 현재 스테이지 데이터가 없으면 실행 불가
        if (progressDataManager.SavedStageData == null)
        {
            Debug.LogWarning("[NodeTeleportEventEffect] SavedStageData가 없습니다.");
            return;
        }

        var stageData = progressDataManager.SavedStageData;
        var currentNode = progressDataManager.CurrentNode;

        // 현재 노드가 없으면 실행 불가
        if (currentNode == null)
        {
            Debug.LogWarning("[NodeTeleportEventEffect] CurrentNode가 없습니다.");
            return;
        }

        // 마지막 열(보스) 바로 앞 열 = columnCount - 2
        int targetColumnIndex = stageData.columnCount - 2;

        // 이미 해당 열에 있으면 무시
        if (currentNode.columnIndex >= targetColumnIndex)
        {
            Debug.Log($"[NodeTeleportEventEffect] 이미 {targetColumnIndex}열 이상에 있어서 텔레포트 무시됨");
            return;
        }

        // targetColumn의 첫 번째 행 노드 선택
        var targetColumn = stageData.columns[targetColumnIndex];
        if (targetColumn == null || targetColumn.Count == 0)
        {
            Debug.LogWarning($"[NodeTeleportEventEffect] {targetColumnIndex}열에 노드가 없습니다.");
            return;
        }

        GraphNode targetNode = targetColumn[0]; // 첫 번째 행

        // 현재 노드를 목표 노드로 변경
        progressDataManager.CurrentNode = targetNode;

        // VisitedNodes에 이미 없으면 추가
        if (!progressDataManager.VisitedNodes.Contains(targetNode))
        {
            progressDataManager.VisitedNodes.Add(targetNode);
        }

        // 데이터 저장
        progressDataManager.SaveProgress(true);
    }

    public override void UnApply()
    {
    }

    public override EventEffects Clone()
    {
        return new NodeTeleportEventEffect
        {
            index = this.index,
            text = this.text,
            eventType = this.eventType,
            duration = this.duration,
        };
    }
}
