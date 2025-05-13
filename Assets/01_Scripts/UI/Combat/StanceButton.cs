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

    [Tooltip("이 버튼이 영향을 줄 PlayerController")]
    [SerializeField] private PlayerController owner;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
            Debug.LogError($"[{name}] Button 컴포넌트가 없습니다.");

        button.onClick.AddListener(ToggleStanceSlot);

    }

    private void ToggleStanceSlot()
    {
        // 1) 할당 안 된 필드 체크
        if (stanceSlot == null)
        {
            Debug.LogError($"[{name}] stanceSlot(GameObject)이 할당되지 않았습니다.");
            return;
        }
        if (owner == null)
        {
            Debug.LogError($"[{name}] owner(PlayerController)가 할당되지 않았습니다.");
            return;
        }

        // 2) on/off 토글
        bool open = !stanceSlot.activeSelf;
        stanceSlot.SetActive(open);

        // 3) 팝업이 열릴 때만 하위 옵션 초기화
        if (open)
        {
            foreach (var opt in stanceSlot.GetComponentsInChildren<StanceOptionButton>(true))
                opt.Initialize(owner);
        }
    }
}
