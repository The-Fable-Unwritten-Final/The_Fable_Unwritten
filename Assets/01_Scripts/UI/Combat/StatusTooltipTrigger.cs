using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class StatusTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string keywordType;

    private object[] tooltipArgs;

    public void SetKeyword(string type, params object[] args)
    {
        keywordType = type;
        tooltipArgs = args;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(keywordType))
            return;

        StatusTooltipUI.Instance.Show(keywordType, eventData.position, tooltipArgs);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StatusTooltipUI.Instance.Hide();
    }
}