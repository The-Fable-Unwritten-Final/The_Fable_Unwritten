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
    public static GameObject DrawLine(CurvePoint curvePoint, RectTransform from, RectTransform to, Transform parent, GameObject linePrefab, float offsetFromNode = 60f, bool isBossDestination = false)
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
        float dotSpacing = 9f;
        int dotCount = Mathf.Max(4, Mathf.FloorToInt(distance / dotSpacing)); // 최소 4개의 점

        // LineList 모드에서는 짝수 개의 점이 필요 (시작과 끝의 시각적 일치를 위해)
        if (dotCount % 2 != 0) dotCount++;
        
        // 점들의 위치 배열 생성
        Vector2[] points = new Vector2[dotCount];
        // 베지어 곡선 (0~2 개의 제어점 사용 곡선 버전)
        for (int i = 0; i < dotCount; i++)
        {
            float t = i / (dotCount - 1f);

            // 0개 >> 직선
            if (curvePoint == null || curvePoint.position == null || curvePoint.position.Length == 0)
            {
                points[i] = Vector2.Lerp(start, end, t);
            }
            // 1개 >> 2차 베지어
            else if (curvePoint.position.Length == 1)
            {
                Vector2 p1 = curvePoint.position[0];
                float u = 1f - t;
                points[i] =
                    u * u * start +
                    2f * u * t * p1 +
                    t * t * end;
            }
            // 2개 >> 3차 베지어
            else
            {
                Vector2 p1 = curvePoint.position[0];
                Vector2 p2 = curvePoint.position[1];
                float u = 1f - t;

                points[i] =
                    u * u * u * start +
                    3f * u * u * t * p1 +
                    3f * u * t * t * p2 +
                    t * t * t * end;
            }
        }
        /* 베지어 곡선 미사용 버전(직선)
        for (int i = 0; i < dotCount; i++)
        {
            float t = i / (dotCount - 1f);
            points[i] = Vector2.Lerp(start, end, t);
        }*/
        

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

    /// <summary>
    /// 제어점의 각도가 최대 50도를 넘지 않도록 제한
    /// </summary>
    private static Vector2 ClampControlPoint(Vector2 controlPoint, Vector2 start, Vector2 end)
    {
        const float maxBendAngle = 45f;
        
        Vector2 lineDir = (end - start).normalized;
        Vector2 cpDir = controlPoint - start;
        
        if (cpDir.magnitude < 0.001f) return controlPoint;
        
        cpDir = cpDir.normalized;
        float angle = Vector2.Angle(lineDir, cpDir);
        
        if (angle > maxBendAngle)
        {
            float clampedAngle = Mathf.Clamp(angle, 0, maxBendAngle);
            Quaternion rotation = Quaternion.AngleAxis(clampedAngle, Vector3.forward);
            Vector3 clampedDir = rotation * new Vector3(lineDir.x, lineDir.y, 0);
            
            return start + ((Vector2)clampedDir).normalized * (controlPoint - start).magnitude;
        }
        
        return controlPoint;
    }
}

