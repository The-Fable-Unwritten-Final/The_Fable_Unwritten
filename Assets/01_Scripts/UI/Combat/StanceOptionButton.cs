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
            (CharacterClass.Sophia, PlayerData.StancType.refine) => "정제(Refine)\n처음 사용한 타입 피해 50% 증가\n이외 타입 피해 50% 감소",
            (CharacterClass.Sophia, PlayerData.StancType.mix) => "혼합(Mix)\n타입마다 처음 사용한 카드 피해 50% 증가\n이후 카드 피해 50% 감소",
            (CharacterClass.Kayla, PlayerData.StancType.grace) => "축복(Grace)\n회복량 50% 증가, 버프 수치 1 증가\n피해량 50% 감소, 디버프 수치 1 감소",
            (CharacterClass.Kayla, PlayerData.StancType.judge) => "심판(Judge)\n회복량 50% 감소, 버프 수치 1 감소\n피해량 50% 증가, 디버프 수치 1 증가",
            (CharacterClass.Leon, PlayerData.StancType.guard) => "방어(Guard)\n주는 피해 50% 감소\n받는 피해 50% 감소",
            (CharacterClass.Leon, PlayerData.StancType.rush) => "돌진(Rush)\n주는 피해 50% 증가\n받는 피해 50% 증가",
            _ => "알 수 없는 태세"
        };
    }

}
