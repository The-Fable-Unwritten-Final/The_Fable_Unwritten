using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StanceOptionButton : MonoBehaviour
{
    [Tooltip("이 버튼이 설정할 자세 타입")]
    public PlayerData.StancType stanceType;

    [Tooltip("선택 시 강조할 색")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [Tooltip("기본 버튼 색")]
    [SerializeField] private Color normalColor = Color.white;//+
    [SerializeField] private Image backgroundImage;//+
    private PlayerController owner;
    private Button button;

    /// <summary>
    /// StanceButton에서 owner를 넘겨 호출
    /// </summary>
    public void Initialize(PlayerController ownerController)
    {
        owner = ownerController;
        button = GetComponent<Button>(); 

        // 클릭 시 동작
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);

        // 열릴 때 한 번만 highlight 업데이트
        UpdateHighlight();
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
        Debug.Log($"[Stance] {owner.playerData.CharacterName} 자세 변경 → {owner.CurrentStance}");

        // 다시 highlight
        UpdateHighlight();

        // 3) 팝업 닫기
        var slot = transform.parent.gameObject; //+
        transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 현재 owner의 stance와 같으면 강조색, 아니면 기본색으로
    /// </summary>
    private void UpdateHighlight()//+
    {
        if (backgroundImage == null) return;
        bool isSelected = owner.playerData.currentStance == stanceType;
        backgroundImage.color = isSelected ? highlightColor : normalColor;
    }
}
