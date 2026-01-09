using UnityEngine;
using UnityEngine.UI.Extensions;

/// <summary>
/// 라인 셋팅해주는 클래스
/// </summary>
public static class LineDrawer
{
    /// <summary>
    /// 두 노드 위치 확인 후 연결해 주는 점선 생성
    /// </summary>
    public static GameObject DrawLine(RectTransform from, RectTransform to, Transform parent, GameObject linePrefab, float offsetFromNode = 50f)
    {
        GameObject lineObj = GameObject.Instantiate(linePrefab, parent);
        UILineRenderer lineRenderer = lineObj.GetComponent<UILineRenderer>();

        // 시작 점과 끝점 두 위치를 RectTransform 기준의 로컬 좌표로 전환(계산 작업을 위해)
        Vector2 start = WorldToLocal(from.position, parent as RectTransform);
        Vector2 end = WorldToLocal(to.position, parent as RectTransform);
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        // 노드 크기를 고려한 오프셋 적용 (노드의 중심에 겹침 방지)
        Vector2 normalizedDirection = direction.normalized;
        float fromNodeOffset = offsetFromNode > 0 ? offsetFromNode : from.rect.width / 2f; // 노드 반경 or 지정값
        float toNodeOffset = offsetFromNode > 0 ? offsetFromNode : to.rect.width / 2f;
        
        start += normalizedDirection * fromNodeOffset;
        end -= normalizedDirection * toNodeOffset;
        direction = end - start;
        distance = direction.magnitude;

        // 점 간격 설정 (픽셀 단위) - 필요시 조정 가능~
        float dotSpacing = 12f;
        int dotCount = Mathf.Max(2, Mathf.FloorToInt(distance / dotSpacing));
        
        // LineList 모드에서는 짝수 개의 점이 필요 (시작과 끝의 시각적 일치를 위해)
        if (dotCount % 2 != 0) dotCount++;

        // 점들의 위치 배열 생성
        Vector2[] points = new Vector2[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            float t = i / (dotCount - 1f);
            points[i] = Vector2.Lerp(start, end, t);
        }

        // UILineRenderer 설정
        lineRenderer.Points = points;
        lineRenderer.LineThickness = 8f; // 점의 굵기 - 필요시 조정 가능~
        lineRenderer.LineList = true;

        return lineObj;
    }

    // 월드 좌표를  RectTransform 기준의 로컬 좌표로 전환
    private static Vector2 WorldToLocal(Vector3 worldPos, RectTransform parent)
    {
        Vector2 localPoint;
        // 스크린 좌표를 RectTransform(parnt) 기준의 로컬 좌표로 전환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(null, worldPos), // 월드 좌표를 스크린 좌표로 전환(Cam 없는 상태이므로 null 로 지정)
            null,
            out localPoint
        );
        return localPoint;
    }
}

