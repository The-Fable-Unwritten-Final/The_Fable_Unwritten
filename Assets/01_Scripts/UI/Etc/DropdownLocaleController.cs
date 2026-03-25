using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownLocaleController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown myDropdown;

    void Update()
    {
        // 특정 조건을 만족할 때만 드롭다운 활성화
        bool condition = CheckSomething();

        myDropdown.interactable = condition;
    }

    bool CheckSomething()
    {
        // 현재 씬이 타이틀 씬일때만 상호작용 가능
        return SceneNameData.TitleScene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name ||
            SceneNameData.SubTitleScene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
}
