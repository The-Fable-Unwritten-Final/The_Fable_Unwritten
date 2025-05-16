using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StanceButton : MonoBehaviour
{
    [Tooltip("토글할 StanceSlot 팝업(부모 오브젝트)")]
    [SerializeField] private GameObject stanceSlot;

    [Tooltip("이 버튼이 제어할 플레이어 컨트롤러")]
    [SerializeField] private PlayerController owner;

    private Button button;


    private void Awake()
    {
        button = GetComponent<Button>();
        if (stanceSlot == null) Debug.LogError($"{name}: stanceSlot 할당해주세요.");
        if (owner == null) Debug.LogError($"{name}: owner(PlayerController) 할당해주세요.");

        button.onClick.AddListener(TogglePopup);
    }

    private void TogglePopup()
    {
        if (stanceSlot == null || owner == null) return;

        // 팝업 on/off
        bool show = !stanceSlot.activeSelf;
        stanceSlot.SetActive(show);

        // 팝업을 연 경우에만, 하위 옵션 초기화
        if (show) BindOptions();
    }

    private void BindOptions()
    {
        // 팝업 내부 StanceOptionButton 컴포넌트 전부 찾아서 owner 세팅
        var options = stanceSlot.GetComponentsInChildren<StanceOptionButton>(true);
        foreach (var opt in options)
            opt.Initialize(owner);
    }
}
