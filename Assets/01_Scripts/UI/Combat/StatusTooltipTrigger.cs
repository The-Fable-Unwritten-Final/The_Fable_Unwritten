using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class StatusTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string keywordType;

    public void SetKeyword(string type)
    {
        keywordType = type;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(keywordType))
            return;

        StatusTooltipUI.Instance.Show(keywordType, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StatusTooltipUI.Instance.Hide();
    }
}