using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// MonoBehaviour 기반 싱글톤 패턴 클래스
/// 클래스 상속 시 씬 전환시에도 유지, 전역 접근 가능
/// </summary>
public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    instance = singletonObject.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(this);  // gameObject 대신 component만 파괴
        }
    }

    protected virtual void OnDestroy()
    {
        // 플레이 중이면 정리 안 함 (게임 종료 시만)
        #if !UNITY_EDITOR
            if (instance == this)
                instance = null;
        #else
            // 에디터에서는 항상 정리
            if (instance == this)
                instance = null;
        #endif
    }
}

