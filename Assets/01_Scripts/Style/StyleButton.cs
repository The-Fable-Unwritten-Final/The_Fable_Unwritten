using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StyleButton : MonoBehaviour
{
    public StyleDefinition definition;
    public Image styleIcon; // 문체 아이콘
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] Image _nameImage; // 문체 이름 이미지 (한글, 영문, 일문)
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
        
        // 로케일에 따른 문체 이름 이미지 업데이트
        Sprite targetSprite = null;
        switch (LocaleDataManager.CurrentLocale)
        {
            case LocaleDataManager.SystemLocale.Korean:
                targetSprite = sty.krNameSprite;
                break;
            case LocaleDataManager.SystemLocale.Japanese:
                targetSprite = sty.jpNameSprite;
                break;
            case LocaleDataManager.SystemLocale.English:
            default:
                targetSprite = sty.enNameSprite;
                break;
        }
        
        _nameImage.sprite = targetSprite;
        
        // 스프라이트 크기를 너비 범위 내에서 조정 (종횡비 유지)
        if (targetSprite != null)
        {
            float minWidth, maxWidth;
            
            // 로케일별로 다른 너비 범위 설정
            switch (LocaleDataManager.CurrentLocale)
            {
                case LocaleDataManager.SystemLocale.Korean:
                    minWidth = 250f;
                    maxWidth = 250f;  // 고정값
                    break;
                case LocaleDataManager.SystemLocale.Japanese:
                    minWidth = 270f;
                    maxWidth = 290f;
                    break;
                case LocaleDataManager.SystemLocale.English:
                default:
                    minWidth = 300f;
                    maxWidth = 320f;
                    break;
            }
            
            float spriteWidth = targetSprite.rect.width;
            float spriteHeight = targetSprite.rect.height;
            
            float targetWidth = Mathf.Clamp(spriteWidth, minWidth, maxWidth);
            float aspectRatio = spriteHeight / spriteWidth;
            float targetHeight = targetWidth * aspectRatio;
            
            _nameImage.GetComponent<RectTransform>().sizeDelta = new Vector2(targetWidth, targetHeight);
        }
        
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
        _nameImage.gameObject.SetActive(false);
        icon1.SetActive(false);
        icon2.SetActive(false);
        iconink.SetActive(false);
    }
    public void OnButtonClicked() // 문체 교체를 위한 버튼 클릭 시 호출
    {
        StyleManager.Instance.tempSty = definition;

        UIManager.Instance.ShowPopupByName("PopupUI_StyleChange");
    }
}
