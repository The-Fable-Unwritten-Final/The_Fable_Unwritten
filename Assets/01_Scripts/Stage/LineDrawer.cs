using UnityEngine;

/// <summary>
/// 라인 셋팅해주는 클래스
/// </summary>
public static class LineDrawer
{
    /// <summary>
    /// 두 노드 위치 확인 후 연결해 주는 선 생성
    /// </summary>
    public static GameObject DrawLine(RectTransform from, RectTransform to, Transform parent, GameObject linePrefab)
    {
        GameObject lineObj = GameObject.Instantiate(linePrefab, parent);
        RectTransform rt = lineObj.GetComponent<RectTransform>();

        // 시작 점과 끝점 두 위치를 RectTransform 기준의 로컬 좌표로 전환(계산 작업을 위해)
        Vector2 start = WorldToLocal(from.position, parent as RectTransform);
        Vector2 end = WorldToLocal(to.position, parent as RectTransform);
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        // 선 위치 - 두 점의 중간
        rt.anchoredPosition = start + direction / 2f;

        // 선 길이 조절
        rt.sizeDelta = new Vector2(distance, rt.sizeDelta.y);
       
        // 선 각도 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0, 0, angle);

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

