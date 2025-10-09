using UnityEngine;
using UnityEngine.UI;

public class IdealOverlayUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject root;          // 최상위 패널
    [SerializeField] private CanvasGroup cg;           // 옵션: 페이드/차단
    [SerializeField] private Button backgroundClose;   // 배경 클릭 닫기
    [SerializeField] private IdealSlotUI slotLeft;
    [SerializeField] private IdealSlotUI slotRight;

    private void Awake()
    {
        HideImmediate();
        if (backgroundClose != null) backgroundClose.onClick.AddListener(Close);
    }


    public void Open(PlayerController owner, IdealSkillBase a, IdealSkillBase b,
                     System.Action<IdealSkillBase, PlayerController> onSelected)
    {
        Debug.Log($"[IdealOverlayUI] Open called by {owner.ChClass}");

        root.SetActive(true);
        if (cg) { cg.alpha = 1; cg.blocksRaycasts = true; }

        slotLeft.Bind(a, owner, (s, o) => { onSelected?.Invoke(s, o); Close(); });
        slotRight.Bind(b, owner, (s, o) => { onSelected?.Invoke(s, o); Close(); });
    }

    public void Close() => HideImmediate();

    private void HideImmediate()
    {
        if (cg) { cg.alpha = 0; cg.blocksRaycasts = false; }
        root.SetActive(false);
    }
}