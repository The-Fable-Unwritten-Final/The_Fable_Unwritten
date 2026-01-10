using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupUI_StyleChange : BasePopupUI
{
    public void TryChange()
    {
        StyleDefinition sty = StyleManager.Instance.tempSty;
        int reqInk = 0;

        switch (sty.rank)
        {
            case StyleDefinition.StyleRank.High:
                reqInk = 4;
                break;
            case StyleDefinition.StyleRank.Medium:
                reqInk = 3;
                break;
            case StyleDefinition.StyleRank.Low:
                reqInk = 3;
                break;
            default:
                break;
        }

        if (StyleManager.Instance.TryInkUse(reqInk))
        {           
            Close();
            StyleManager.Instance.ChangeStyle(sty.styleId);
        }
        else
        {
            // 잉크 부족으로 교환 실패
            Close();
            UIManager.Instance.ShowPopupByName("PopupUI_LackInk");
        }
    }
}
