using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Experimental.GraphView;

public class IdealSlotUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image icon;
    [SerializeField] private Image silhouette; // 잠금 시 표시(회색 실루엣)
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI nameText;

    private IdealSkillBase skill;
    private PlayerController owner;
    private System.Action<IdealSkillBase, PlayerController> onSelect;

    public void Bind(IdealSkillBase s, PlayerController pc, System.Action<IdealSkillBase, PlayerController> onClick)
    {
        skill = s;
        owner = pc;
        onSelect = onClick;

        // 기본 안전 장치
        button.onClick.RemoveAllListeners();

        // 데이터 자체가 없으면 완전 비움 (실루엣만 ON)
        if (s == null)
        {
            SetActive(icon, false);
            SetActive(button, false);
            SetActive(nameText, false);
            SetActive(silhouette, true);
            return;
        }

        // 이름/아이콘 바인딩
        if (icon != null) icon.sprite = s.Icon;
        if (nameText != null) nameText.text = s.DisplayName;

        var state = s.GetUseInfo(pc);
        bool isLocked = (state == IdealAvailability.Locked);
        bool isAvailable = (state == IdealAvailability.Available);


        // 잠금일 때
        if (isLocked)
        {
            SetActive(icon, false);
            SetActive(button, false);      // 버튼 자체도 숨김 (실루엣만 보이게)
            SetActive(nameText, false);    // 이름도 숨김
            SetActive(silhouette, true);
            return;
        }

        // 잠금이 아닐 때 (아이콘+버튼만)
        SetActive(silhouette, false);
        SetActive(icon, true);
        SetActive(button, true);
        SetActive(nameText, true);

        // 버튼 상호작용/아이콘 색
        button.interactable = isAvailable;
        if (icon != null) icon.color = isAvailable ? Color.white : Color.gray;

        if (isAvailable)
            button.onClick.AddListener(() => onSelect?.Invoke(skill, owner));
    }

    // 편의 on/off
    private static void SetActive(Behaviour comp, bool on)
    {
        if (comp != null && comp.gameObject.activeSelf != on)
            comp.gameObject.SetActive(on);
    }
}