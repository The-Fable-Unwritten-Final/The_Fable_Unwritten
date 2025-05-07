using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressableTest : MonoBehaviour
{
    public GameObject prefabTest;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            prefabTest = Addressables.LoadAssetAsync<GameObject>("TestPopup").WaitForCompletion();
        }
    }
}
