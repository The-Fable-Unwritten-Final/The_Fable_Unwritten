using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StyleDisplay : MonoBehaviour
{
    // 문체의 UI를 담당하는 스크립트 (ui 적인 조작을 메인으로 사용 => 노드 선택 씬에서만 존재)

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        // 노드 씬으로 온 경우 활성화.
        // 기본적으로 빈 오브젝트 안에 넣고, 내부의 자식 오브젝트에 UI 배치
        // 스테이지 번호 확인 후, 기본 문체 적용 + UI 활성화 조정
    }
}
