using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using UnityEngine.Analytics;
using Unity.Services.Core;

public class AnalyticsLogger : MonoBehaviour
{
    /*
    public void LogNodeInfo(int stage, int nodeSelected)
    {
        AnalyticsService.Instance.CustomEvent("NodeInfo", new Dictionary<string, object>
        {
            { "Stage", stage },           // 0: 스테이지 2, 1: 스테이지 3
            { "NodeSelected", nodeSelected } // 0: 일반전투, 1: 엘리트, 2: 이벤트, 3: 야영
        });

        UnityEngine.Debug.Log($"[Analytics] NodeInfo: Stage={stage}, NodeSelected={nodeSelected}");

        Analytics.CustomEvent("NodeInfo", new Dictionary<string, object>
        {
            { "Stage", stage },           // 0: 스테이지 2, 1: 스테이지 3
            { "NodeSelected", nodeSelected } // 0: 일반전투, 1: 엘리트, 2: 이벤트, 3: 야영
        });

        
    }*/
}
