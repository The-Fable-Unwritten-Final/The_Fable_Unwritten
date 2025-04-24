using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    private static MainCanvas instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("[MainCanvas] 중복 Canvas 발견 → 제거됨");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
