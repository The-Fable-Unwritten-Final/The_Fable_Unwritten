using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowDescOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject descPanel;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (descPanel != null)
        {
            descPanel.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (descPanel != null)
        {
            descPanel.SetActive(false);
        }
    }
}
