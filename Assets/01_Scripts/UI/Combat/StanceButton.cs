using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 클릭 시 상/중/하 버튼으로 스탠스를 변경하도록 처리하는 컴포넌트
/// BattleFlowController를 참고해 플레이어 턴에만 반응합니다.
/// </summary>
public class StanceButton : MonoBehaviour
{
    [Tooltip("이 버튼이 설정할 StancType")]
    public PlayerData.StancType stanceType;
    [Tooltip("이 버튼이 보여줄 자세 아이콘")]
    [SerializeField]public Image iconImage;

    private PlayerController ownerController;
    private BattleFlowController battleFlowController;
    private Button buttonComponent;

    /// <summary>
    /// 생성 직후 반드시 호출해서, 주인과 BattleFlowController를 연결해 주세요.
    /// </summary>
    public void Initialize(PlayerController owner)
    {
        ownerController = owner;
        battleFlowController = FindObjectOfType<BattleFlowController>();
        buttonComponent = GetComponent<Button>();

        if (buttonComponent == null)
        {
            Debug.LogError("[StanceButton] Button 컴포넌트가 없습니다.");
            return;
        }

        // 아이콘 세팅
        var stanceValue = ownerController.playerData.allStances
            .Find(s => s.stencType == stanceType);
        if (stanceValue != null && iconImage != null)
        {
            iconImage.sprite = stanceValue.stanceIcon;
        }

        buttonComponent.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        // 플레이어 턴이 아닐 땐 무시
        if (battleFlowController == null || !battleFlowController.IsPlayerTurn())
            return;

        // 스탠스 변경
        ownerController.ChangeStance(stanceType);

        // 버튼 컨테이너 닫기
        Destroy(transform.parent.gameObject);
    }
}
