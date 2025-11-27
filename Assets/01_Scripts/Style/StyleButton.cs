using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StyleButton : MonoBehaviour
{
    public StyleDefinition definition;
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _flav;
    [SerializeField] TextMeshProUGUI _eff1;
    [SerializeField] TextMeshProUGUI _eff2;
    [SerializeField] TextMeshProUGUI ink;

    [SerializeField] GameObject icon1;
    [SerializeField] GameObject icon2;
    [SerializeField] GameObject iconink;

    public void SetDefinition(StyleDefinition sty, string ef1, string ef2)
    {
        definition = sty;
        _name.text = LocaleDataManager.GetLocalizedStyleEffect(sty.displayName);
        _flav.text = LocaleDataManager.GetLocalizedStyleEffect(sty.description);
        _eff1.text = ef1;
        _eff2.text = ef2;

        int cost = 0;
        switch (sty.rank)
        {
            case StyleDefinition.StyleRank.High:
                cost = 4;
                break;
            case StyleDefinition.StyleRank.Medium:
                cost = 3;
                break;
            case StyleDefinition.StyleRank.Low:
                cost = 3;
                break;
            default:
                break;
        }
        ink.text = cost.ToString();
    }
    public void TurnOffAll()
    {
        _name.text = "";
        _flav.text = "";
        icon1.SetActive(false);
        icon2.SetActive(false);
        iconink.SetActive(false);
    }
    public void OnButtonClicked() // 문체 교체를 위한 버튼 클릭 시 호출
    {
        Debug.Log("clicked");
        StyleManager.Instance.tempSty = definition;

        UIManager.Instance.ShowPopupByName("PopupUI_StyleChange");
    }
}
