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


    private PlayerController owner;
    private BattleFlowController flow;
    private Button button;

    public void Initialize(PlayerController ownerController)
    {
        owner = ownerController;
        flow = FindObjectOfType<BattleFlowController>();
        button = GetComponent<Button>();

        // 버튼 아이콘 설정
        var sv = owner.playerData.allStances
                  .Find(s => s.stencType == stanceType);
        if (sv != null) iconImage.sprite = sv.stanceIcon;

        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (flow == null || !flow.IsPlayerTurn()) return;

        // 1) 스탠스 변경  
        owner.ChangeStance(stanceType);
        // 2) 팝업 닫기  
        Destroy(transform.parent.gameObject);
    }
}
