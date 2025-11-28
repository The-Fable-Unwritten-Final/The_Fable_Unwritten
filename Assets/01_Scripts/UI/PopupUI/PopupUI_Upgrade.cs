using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupUI_Upgrade : BasePopupUI
{
    public void TryUpgrade()
    {
        var stm = StyleManager.Instance;
        StyleState state = stm.CurrentState;
        int reqInk = 0;

        if (stm.isPlus)
        {
            reqInk = stm.StyleDic[state.styleId].plusTiers[state.plusTier-1].cost;
            if (stm.TryInkUse(reqInk))
            {
                Close();
                stm.UpgradePlus();
                stm.display.UpdateCurrentStyle(stm.StyleDic[state.styleId]);
            }
            else
            {
                // 잉크 부족으로 교환 실패
                Close();
                UIManager.Instance.ShowPopupByName("PopupUI_LackInk");
            }
        }
        else
        {
            reqInk = stm.StyleDic[state.styleId].minusTiers[state.minusTier-1].cost;
            if (stm.TryInkUse(reqInk))
            {
                Close();
                stm.UpgradeMinus();
                stm.display.UpdateCurrentStyle(stm.StyleDic[state.styleId]);
            }
            else
            {
                // 잉크 부족으로 교환 실패
                Close();
                UIManager.Instance.ShowPopupByName("PopupUI_LackInk");
            }
        }
    }
}
