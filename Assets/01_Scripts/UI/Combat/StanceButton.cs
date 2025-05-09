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
    [SerializeField] private Image iconImage;


    private PlayerController ownerController;
    private BattleFlowController battleFlowController;
    private Button buttonComponent;

    public void Initialize(PlayerController owner)
    {
        ownerController = owner;
        battleFlowController = FindObjectOfType<BattleFlowController>();
        buttonComponent = GetComponent<Button>();

        var stanceValue = ownerController.playerData.allStances
            .Find(s => s.stencType == stanceType);
        if (stanceValue != null) iconImage.sprite = stanceValue.stanceIcon;

        buttonComponent.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (battleFlowController == null || !battleFlowController.IsPlayerTurn())
            return;

        ownerController.ChangeStance(stanceType);
        Destroy(transform.parent.gameObject);
    }
}
