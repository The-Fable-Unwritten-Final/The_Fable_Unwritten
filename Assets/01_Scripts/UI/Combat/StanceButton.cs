using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 클릭 시 상/중/하 버튼으로 스탠스를 변경하도록 처리하는 컴포넌트
/// BattleFlowController를 참고해 플레이어 턴에만 반응합니다.
/// </summary>
public class StanceButton : MonoBehaviour
{
    [Tooltip("토글할 StanceSlot 창")]
    [SerializeField] private GameObject stanceSlot;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (stanceSlot == null)
            Debug.LogError($"{name}: stanceSlot(GameObject)을 할당해주세요.");

        button.onClick.AddListener(ToggleStanceSlot);
    }

    private void ToggleStanceSlot()
    {
        if (stanceSlot != null)
            stanceSlot.SetActive(!stanceSlot.activeSelf);
    }
}
