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
    public static GameObject DrawLine(CurvePoint curvePoint, RectTransform from, RectTransform to, Transform parent, GameObject linePrefab, float offsetFromNode = 55f, bool isBossDestination = false)
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

        // 점 간격 설정 (픽셀 단위) - 거리에 따라 동적으로 조정하여 장거리에서 점 개수 제한
        float dotSpacing = 9f + (distance / 100f);
        int dotCount = Mathf.Max(4, Mathf.FloorToInt(distance / dotSpacing)); // 최소 4개의 점

        // 일정 길이 이하인 경우만 최대 개수 제한 (거리 55 미만이면 최대 4개, 이때 55는 실제 원하는 값 - 2*offsetFromNode의 값) // 짧은 거리에서 많은 점이 생기는 경우 방지
        if (distance < 55f)
        {
            dotCount = Mathf.Min(dotCount, 4);
        }

        // 최대 점 개수 제한 (18개까지만)
        dotCount = Mathf.Min(dotCount, 18);

        // LineList 모드에서는 짝수 개의 점이 필요 (시작과 끝의 시각적 일치를 위해)
        if (dotCount % 2 != 0) dotCount++;
        
        // 점들의 위치 배열 생성
        Vector2[] points = new Vector2[dotCount];
        
        // 먼저 충분한 개수의 샘플 점을 생성
        int sampleCount = dotCount * 4; // 더 많은 샘플 생성
        Vector2[] samplePoints = new Vector2[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (sampleCount - 1f);

            // 0개 >> 직선
            if (curvePoint == null || curvePoint.position == null || curvePoint.position.Length == 0)
            {
                samplePoints[i] = Vector2.Lerp(start, end, t);
            }
            // 1개 >> 2차 베지어
            else if (curvePoint.position.Length == 1)
            {
                Vector2 p1 = curvePoint.position[0];
                float u = 1f - t;
                samplePoints[i] =
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

                samplePoints[i] =
                    u * u * u * start +
                    3f * u * u * t * p1 +
                    3f * u * t * t * p2 +
                    t * t * t * end;
            }
        }

        // 누적 거리 계산 (arc-length)
        float[] cumulativeDistance = new float[sampleCount];
        cumulativeDistance[0] = 0;
        for (int i = 1; i < sampleCount; i++)
        {
            cumulativeDistance[i] = cumulativeDistance[i - 1] + Vector2.Distance(samplePoints[i], samplePoints[i - 1]);
        }

        float totalLength = cumulativeDistance[sampleCount - 1];

        // 균등 거리로 재샘플링
        for (int i = 0; i < dotCount; i++)
        {
            float targetDistance = (i / (dotCount - 1f)) * totalLength;
            
            // 이분 탐색으로 해당 거리를 가진 인덱스 찾기
            int idx = System.Array.BinarySearch(cumulativeDistance, targetDistance);
            if (idx < 0) idx = ~idx;
            
            if (idx >= sampleCount) idx = sampleCount - 1;
            
            Vector2 basePoint;
            // 선형 보간
            if (idx > 0 && idx < sampleCount && cumulativeDistance[idx] != cumulativeDistance[idx - 1])
            {
                float t = (targetDistance - cumulativeDistance[idx - 1]) / (cumulativeDistance[idx] - cumulativeDistance[idx - 1]);
                basePoint = Vector2.Lerp(samplePoints[idx - 1], samplePoints[idx], t);
            }
            else
            {
                basePoint = samplePoints[idx];
            }
            
            // 곡선에 수직인 방향으로 작은 랜덤 오프셋 추가 (손으로 그린 듯한 자연스러움)
            Vector2 tangent;
            if (idx > 0 && idx < sampleCount - 1)
            {
                tangent = (samplePoints[idx + 1] - samplePoints[idx - 1]).normalized;
            }
            else if (idx > 0)
            {
                tangent = (samplePoints[idx] - samplePoints[idx - 1]).normalized;
            }
            else
            {
                tangent = (samplePoints[1] - samplePoints[0]).normalized;
            }
            
            Vector2 perpendicular = new Vector2(-tangent.y, tangent.x);
            float randomOffset = Random.Range(-1.5f, 1.5f); // 수정: -2~2 범위로 조정 가능
            points[i] = basePoint + perpendicular * randomOffset;
        }
        /* 베지어 곡선 미사용 버전(직선)
        for (int i = 0; i < dotCount; i++)
        {
            float t = i / (dotCount - 1f);
            points[i] = Vector2.Lerp(start, end, t);
        }*/
        
        // 곡선 왜곡 방지: X 역방향이 충분히 클 때만 포인트 제거 (낚시바늘 모양 왜곡 방지)
        int validPointCount = dotCount;
        for (int i = 1; i < dotCount; i++)
        {
            Vector2 dir = points[i] - points[i - 1];
            if (dir.x < 0 && Mathf.Abs(dir.x) > Mathf.Abs(dir.y) * 0.5f)
            {
                validPointCount = i;
                break;
            }
        }

        // 최소 2개의 포인트 보장 (너무 가까울 경우 점이 하나도 안 찍히는 경우 방지)
        validPointCount = Mathf.Max(validPointCount, 2);

        if (validPointCount < dotCount)
        {
            System.Array.Resize(ref points, validPointCount);
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

