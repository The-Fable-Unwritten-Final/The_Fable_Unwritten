using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StanceOptionButton : MonoBehaviour
{
    [Tooltip("이 버튼이 설정할 태세 타입")]
    [SerializeField] private StancType stanceType;

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
        string code = LocalizationSettings.SelectedLocale.Identifier.Code.ToLower();

        return (characterClass, stanceType) switch
        {
            (CharacterClass.Sophia, StancType.Seek) => code switch
            {
                   "ko" => "<b><color=#FFD700>정제</color></b>\n<color=#ff5757>처음 사용한 타입 피해 50% 증가</color>\n<color=#518bff>이외 타입 피해 50% 감소</color>",
                   "ja" => "<b><color=#FFD700>精製</color></b>\n<color=#ff5757>最初に使用したタイプのダメージが50%増加</color>\n<color=#518bff>その他のタイプのダメージが50%減少</color>",
                   "en" => "<b><color=#FFD700>Refine</color></b>\n<color=#ff5757>Type of the first used card, damage increased by 50%</color>\n<color=#518bff>Other types of card damage decreased by 50%</color>",
                   _ => "Unknown code"
            },

            (CharacterClass.Sophia, StancType.Insight) => code switch 
            { 
                "ko" => "<b><color=#FFD700>혼합</color></b>\n<color=#ff5757>타입마다 첫 카드 피해 50% 증가</color>\n<color=#518bff>이후 카드 피해 50% 감소</color>",
                "ja" => "<b><color=#FFD700>混合</color></b>\n<color=#ff5757>タイプごとに最初のカードダメージが50%増加</color>\n<color=#518bff>その後のカードダメージが50%減少</color>",
                "en" => "<b><color=#FFD700>Mix</color></b>\n<color=#ff5757>First card damage of each type increased by 50%</color>\n<color=#518bff>Subsequent cards of the same type's damage decreased by 50%</color>",
                _ => "Unknown code"
            },

            (CharacterClass.Kayla, StancType.Mercy) => code switch
            {
                "ko" => "<b><color=#FFD700>축복</color></b>\n<color=#ff5757>회복량 50% 증가, 버프 수치 1 증가</color>\n<color=#518bff>피해량 50% 감소, 디버프 수치 1 감소</color>",
                "ja" => "<b><color=#FFD700>祝福</color></b>\n<color=#ff5757>回復量50%増加 バフ数値1増加</color>\n<color=#518bff>ダメージ量50%減少、デバフ数値1減少</color>",
                "en" => "<b><color=#FFD700>Blessing</color></b>\n<color=#ff5757>Healing amount increased by 50%, buff value increased by 1</color>\n<color=#518bff>Damage decreased by 50%, debuff value decreased by 1</color>",
                _ => "Unknown code"
            },

            (CharacterClass.Kayla, StancType.Discipline) => code switch
            { 
                "ko" => "<b><color=#FFD700>심판</color></b>\n<color=#ff5757>피해량 50% 증가, 디버프 수치 1 증가</color>\n<color=#518bff>회복량 50% 감소, 버프 수치 1 감소</color>",
                "ja" => "<b><color=#FFD700>審判</color></b>\n<color=#ff5757>ダメージ量50%増加 デバフ数値1増加</color>\n<color=#518bff>回復量50%減少 バフ数値1減少</color>",
                "en" => "<b><color=#FFD700>Judgment</color></b>\n<color=#ff5757>Damage increased by 50%, debuff value increased by 1</color>\n<color=#518bff>Healing amount decreased by 50%, buff value decreased by 1</color>",
                _ => "Unknown code"
            },

            (CharacterClass.Leon, StancType.Defense) => code switch
            { 
                "ko" => "<b><color=#FFD700>방어</color></b>\n<color=#ff5757>받는 피해 50% 감소</color>\n<color=#518bff>주는 피해 50% 감소</color>",
                "ja" => "<b><color=#FFD700>防御</color></b>\n<color=#ff5757>受けるダメージ50%減少</color>\n<color=#518bff>与えるダメージ50%減少</color>",
                "en" => "<b><color=#FFD700>Guard</color></b>\n<color=#ff5757>Take 50% less damage</color>\n<color=#518bff>Deal 50% less damage</color>",
                _ => "Unknown code"
            },

            (CharacterClass.Leon, StancType.Rush) => code switch
            { 
                "ko" => "<b><color=#FFD700>돌진</color></b>\n<color=#ff5757>주는 피해 50% 증가</color>\n<color=#518bff>받는 피해 50% 증가</color>",
                "ja" => "<b><color=#FFD700>突進</color></b>\n<color=#ff5757>与えるダメージ50%増加</color>\n<color=#518bff>受けるダメージ50%増加</color>",
                "en" => "<b><color=#FFD700>Rush</color></b>\n<color=#ff5757>Deal 50% more damage</color>\n<color=#518bff>Take 50% more damage</color>",
                _ => "Unknown code"
            },

            _ => "알 수 없는 태세"
        };
    }

}
