using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UILineDrawer
{
    public static void DrawLine(RectTransform from, RectTransform to, Transform parent, GameObject linePrefab)
    {
        Vector2 start = from.anchoredPosition;
        Vector2 end = to.anchoredPosition;
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        GameObject lineObj = GameObject.Instantiate(linePrefab, parent);
        RectTransform rt = lineObj.GetComponent<RectTransform>();

        rt.anchoredPosition = start + direction / 2f;
        rt.sizeDelta = new Vector2(distance, rt.sizeDelta.y); // 가로 길이만 조정
        rt.rotation = Quaternion.FromToRotation(Vector3.right, direction.normalized); // 방향 회전
    }
}

