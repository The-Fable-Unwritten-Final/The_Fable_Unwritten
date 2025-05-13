using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// StanceSlot 하위의 상/중/하 버튼용 컴포넌트
/// 플레이어 소유자를 받아 스탠스 변경 후 팝업을 닫습니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class StanceOptionButton : MonoBehaviour
{
    [Tooltip("이 버튼이 설정할 StancType")]
    public PlayerData.StancType stanceType;

    private PlayerController owner;
    private Button button;

    /// <summary>
    /// 소유할 PlayerController를 초기화하고 클릭 리스너를 등록합니다.
    /// </summary>
    public void Initialize(PlayerController ownerController)
    {
        owner = ownerController;
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (owner == null)
        {
            Debug.LogError($"[{name}] owner가 설정되지 않았습니다.");
            return;
        }

        // 스탠스 변경
        owner.ChangeStance(stanceType);

        // 팝업 닫기
        var slot = transform.parent.gameObject;
        slot.SetActive(false);
    }
}
