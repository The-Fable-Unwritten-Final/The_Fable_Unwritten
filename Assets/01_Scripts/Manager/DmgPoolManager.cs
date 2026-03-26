using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DmgPoolManager : MonoSingleton<DmgPoolManager>
{
    [SerializeField] private DmgBarDisplay dmgTextPrefab;
    [SerializeField] private int poolSize = 10;

    private readonly Queue<DmgBarDisplay> pool = new();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewInstance();
        }
    }

    /// <summary>
    /// 풀에서 오브젝트 하나 꺼냄
    /// </summary>
    public DmgBarDisplay Get()
    {
        DmgBarDisplay obj = (pool.Count > 0) ? pool.Dequeue() : CreateNewInstance();
        obj.gameObject.SetActive(true);
        return obj;
    }
    
    /// <summary>
     /// 사용한 오브젝트를 풀에 반환
     /// </summary>
    public void Return(DmgBarDisplay obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }

    private DmgBarDisplay CreateNewInstance()
    {
        var obj = Instantiate(dmgTextPrefab);
        obj.transform.SetParent(transform, false);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }
}
