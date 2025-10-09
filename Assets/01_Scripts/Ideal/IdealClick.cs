using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IdealClick : MonoBehaviour
{
    public PlayerController owner;           // 없으면 자동 탐색
    public IdealManager idealManager;        // 씬의 IdealManager

    private void Awake()
    {
        if (!owner) owner = GetComponentInParent<PlayerController>();
    }

    private void OnMouseUpAsButton() // Collider 필요, 카메라 Raycaster 없어도 동작
    {
        // UI 위를 클릭한 경우 무시(선택)
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;
        idealManager?.OpenFor(owner);
    }
}
