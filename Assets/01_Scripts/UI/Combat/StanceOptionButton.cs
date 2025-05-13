using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StanceOptionButton : MonoBehaviour
{
    [Tooltip("이 버튼이 설정할 자세 타입")]
    public PlayerData.StancType stanceType;

    private PlayerController owner;
    private Button button;

    /// <summary>
    /// StanceButton에서 owner를 넘겨 호출
    /// </summary>
    public void Initialize(PlayerController ownerController)
    {
        owner = ownerController;
        button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (owner == null)
        {
            Debug.LogError($"{name}: owner가 설정되지 않았습니다.");
            return;
        }

        // 1) 스탠스 변경
        owner.ChangeStance(stanceType);

        // 2) 변경된 스탠스를 로그로 확인
        Debug.Log($"[StanceOptionButton] {owner.ChClass} 스탠스 → {stanceType}");

        // 3) 팝업 닫기
        transform.parent.gameObject.SetActive(false);
    }
}
