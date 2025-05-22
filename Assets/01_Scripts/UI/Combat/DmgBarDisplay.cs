using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum DmgTextType
{
    Normal,
    Heal,
    Buff,
    Debuff,
}

public struct DmgTextData
{
    public string Text;
    public DmgTextType type;

    public bool isStanceEnhanced;
    public bool isCardEnhanced;
    public bool isWeakened;
}

public static class DmgTextColors
{
    // 데미지
    public static readonly Color Normal = Color.white;
    public static readonly Color StanceDamage = new Color(1f, 0.65f, 0f);       // #FFA500
    public static readonly Color EnhanceDamage = new Color(1f, 0f, 0f);         // #FF0000
    public static readonly Color FullEnhanceDamage = new Color(0.6f, 0f, 0f);   // #990000

    // 힐
    public static readonly Color Heal = new Color(0.66f, 0.9f, 0.64f);          // #A8E6A3
    public static readonly Color StanceHeal = new Color(0.4f, 0.8f, 0.4f);      // #66CC66
    public static readonly Color EnhanceHeal = new Color(0.13f, 0.55f, 0.13f);  // #228B22
    public static readonly Color FullEnhanceHeal = new Color(0.08f, 0.36f, 0.08f); // #145C14

    // 버프
    public static readonly Color Buff = new Color(0.4f, 0.7f, 1f);              // #66CCFF
    public static readonly Color EnhanceBuff = new Color(0f, 0.4f, 1f);         // #0066FF

    // 디버프
    public static readonly Color Debuff = new Color(1f, 0.87f, 0.4f);           // #FFDD66
    public static readonly Color EnhanceDebuff = new Color(1f, 0.67f, 0f);      // #FFAA00
}


public class DmgBarDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private CanvasGroup canvasGroup;

    private Vector3 floatOffset = Vector3.up * 1f;
    private float floatDuration = 1f;

    private void Awake()
    {
        // 최초에 텍스트는 숨긴 상태로 시작
        canvasGroup.alpha = 0f;
        damageText.text = "";
    }


    public void Initialize(DmgTextData data, Vector3 worldPos)
    {
        transform.position = worldPos;

        damageText.text = (data.isStanceEnhanced || data.isCardEnhanced) ? $"<b>{data.Text}</b>" : data.Text;
        damageText.color = GetFinalColor(ResolveColor(data), data.isWeakened);
        damageText.fontSize = (data.isStanceEnhanced || data.isCardEnhanced) ? 0.8f : 0.5f;

        canvasGroup.alpha = 1f;

        StopAllCoroutines(); // 이전 연출 중단
        StartCoroutine(FadeAndFloat());
    }


    private IEnumerator FadeAndFloat()
    {
        Vector3 start = transform.position;
        Vector3 end = start + floatOffset;
        float time = 0;

        while (time < floatDuration)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, time / floatDuration);
            canvasGroup.alpha = 1f - (time / floatDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        damageText.text = ""; // 다음 표시를 위해 초기화
    }

    private Color GetFinalColor(Color baseColor, bool isWeakened)
    {
        return isWeakened ? baseColor * 0.6f : baseColor;
    }

    private Color ResolveColor(DmgTextData data)
    {
        return data.type switch
        {
            DmgTextType.Heal => ResolveHealColor(data),
            DmgTextType.Buff => data.isStanceEnhanced ? DmgTextColors.EnhanceBuff : DmgTextColors.Buff,
            DmgTextType.Debuff => data.isStanceEnhanced ? DmgTextColors.EnhanceDebuff : DmgTextColors.Debuff,
            _ => ResolveDamageColor(data),
        };
    }

    private Color ResolveDamageColor(DmgTextData data)
    {
        if (data.isStanceEnhanced && data.isCardEnhanced) return DmgTextColors.FullEnhanceDamage;
        if (data.isCardEnhanced) return DmgTextColors.EnhanceDamage;
        if (data.isStanceEnhanced) return DmgTextColors.StanceDamage;
        return DmgTextColors.Normal;
    }

    private Color ResolveHealColor(DmgTextData data)
    {
        if (data.isStanceEnhanced && data.isCardEnhanced) return DmgTextColors.FullEnhanceHeal;
        if (data.isCardEnhanced) return DmgTextColors.EnhanceHeal;
        if (data.isStanceEnhanced) return DmgTextColors.StanceHeal;
        return DmgTextColors.Heal;
    }
}