using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableTest : MonoBehaviour
{
    public GameObject prefabTest;
    private AsyncOperationHandle<GameObject>? prefabHandle;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(LoadPrefab("TestPopup"));
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            UnloadPrefab();
        }
    }

    IEnumerator LoadPrefab(string key)
    {
        if (prefabHandle.HasValue)
            Addressables.Release(prefabHandle.Value); // 이전 로드 해제

        var handle = Addressables.LoadAssetAsync<GameObject>(key);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            prefabTest = handle.Result;
            prefabHandle = handle;
        }
        else
        {
            Debug.LogError("Failed to load prefab.");
        }
    }

    void UnloadPrefab()
    {
        if (prefabHandle.HasValue)
        {
            Addressables.Release(prefabHandle.Value);
            prefabHandle = null;
            prefabTest = null;
            Debug.Log("Prefab released.");
        }
    }
}
