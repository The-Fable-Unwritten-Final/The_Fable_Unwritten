using UnityEngine;
using UnityEngine.EventSystems;

public class StanceTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private PlayerController owner;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner == null || owner.StanceSystem == null)
            return;

        string stance = owner.StanceSystem.CurrentStance.ToString();

        StatusTooltipUI.Instance.Show(
            stance,
            eventData.position
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StatusTooltipUI.Instance.Hide();
    }
}