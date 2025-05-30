using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StanceOptionButton : MonoBehaviour
{
    [Tooltip("이 버튼이 설정할 태세 타입")]
    [SerializeField] private PlayerData.StancType stanceType;

    [Tooltip("이 버튼이 속한 캐릭터 클래스")]
    [SerializeField] private CharacterClass characterClass;

    [Tooltip("선택 시 색상")]
    [SerializeField] private Color highlightColor = new Color32(255, 255, 255, 255);

    [Tooltip("비활성 시 색상")]
    [SerializeField] private Color normalColor = new Color32(122, 122, 122, 255);

    [SerializeField] private Image backgroundImage;

    [SerializeField] private PlayerController playerController; // 에디터에서 연결하거나 Find로 자동

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void Start()
    {
        // Start 또는 Inspector 연결로 playerController 확보되어야 함
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        UpdateHighlight();
    }

    private void Update()
    {
        UpdateHighlight();
    }

    private void OnClick()
    {
        if (playerController == null) return;

        playerController.ChangeStance(stanceType);
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        if (playerController == null || backgroundImage == null) return;

        bool isSelected = playerController.playerData.currentStance == stanceType;
        backgroundImage.color = isSelected ? highlightColor : normalColor;
    }

    public string GetDescription()
    {
        return (characterClass, stanceType) switch
        {
            (CharacterClass.Sophia, PlayerData.StancType.refine) =>
                "<b><color=#FFD700>정제</color></b>\n<color=#ff5757>처음 사용한 타입 피해 50% 증가</color>\n<color=#518bff>이외 타입 피해 50% 감소</color>",

            (CharacterClass.Sophia, PlayerData.StancType.mix) =>
                "<b><color=#FFD700>혼합</color></b>\n<color=#ff5757>타입마다 첫 카드 피해 50% 증가</color>\n<color=#518bff>이후 카드 피해 50% 감소</color>",

            (CharacterClass.Kayla, PlayerData.StancType.grace) =>
                "<b><color=#FFD700>축복</color></b>\n<color=#ff5757>회복량 50% 증가, 버프 수치 1 증가</color>\n<color=#518bff>피해량 50% 감소, 디버프 수치 1 감소</color>",

            (CharacterClass.Kayla, PlayerData.StancType.judge) =>
                "<b><color=#FFD700>심판</color></b>\n<color=#ff5757>회복량 50% 감소, 버프 수치 1 감소</color>\n<color=#518bff>피해량 50% 증가, 디버프 수치 1 증가</color>",

            (CharacterClass.Leon, PlayerData.StancType.guard) =>
                "<b><color=#FFD700>방어</color></b>\n<color=#ff5757>주는 피해 50% 감소</color>\n<color=#518bff>받는 피해 50% 감소</color>",

            (CharacterClass.Leon, PlayerData.StancType.rush) =>
                "<b><color=#FFD700>돌진</color></b>\n<color=#ff5757>주는 피해 50% 증가</color>\n<color=#518bff>받는 피해 50% 증가</color>",

            _ => "알 수 없는 태세"
        };
    }

}
