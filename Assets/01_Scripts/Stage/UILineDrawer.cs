using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UILineDrawer
{
    /// <summary>
    /// 시작 노드, 도착 노드 위치 확인 후 연결해 주는 선 생성
    /// </summary>
    public static GameObject DrawLine(RectTransform from, RectTransform to, Transform parent, GameObject linePrefab)
    {
        GameObject lineObj = GameObject.Instantiate(linePrefab, parent);
        RectTransform rt = lineObj.GetComponent<RectTransform>();

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

    /// <summary>
    /// 월드 좌표를 지정된 부모 RectTransform 기준의 로컬 좌표로 전환
    /// </summary>
    private static Vector2 WorldToLocal(Vector3 worldPos, RectTransform parent)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out localPoint
        );
        return localPoint;
    }
}

