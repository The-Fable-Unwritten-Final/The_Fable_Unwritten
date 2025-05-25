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
    // Damage
    public static readonly Color Damage = Color.white;
    public static readonly Color StanceDamage = new Color(1f, 0.42f, 0.42f);      // #FF6B6B
    public static readonly Color EnhanceDamage = new Color(1f, 0.118f, 0.118f);   // #FF1E1E
    public static readonly Color FullEnhanceDamage = new Color(0.698f, 0f, 0f);   // #B20000

    // Heal
    public static readonly Color Heal = Color.white;
    public static readonly Color StanceHeal = new Color(0.643f, 1f, 0.69f);       // #A4FFB0
    public static readonly Color EnhanceHeal = new Color(0.365f, 1f, 0.533f);     // #5DFF88
    public static readonly Color FullEnhanceHeal = new Color(0.122f, 0.651f, 0.298f); // #1FA64C

    // Buff
    public static readonly Color Buff = Color.white;
    public static readonly Color StanceBuff = new Color(1f, 0.878f, 0.4f);        // #FFE066

    // Debuff
    public static readonly Color Debuff = Color.white;
    public static readonly Color StanceDebuff = new Color(0.71f, 0.6f, 1f);       // #B599FF
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
        transform.position = new Vector3(worldPos.x, worldPos.y - 1, worldPos.z);

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
            DmgTextType.Buff => data.isStanceEnhanced ? DmgTextColors.StanceBuff : DmgTextColors.Buff,
            DmgTextType.Debuff => data.isStanceEnhanced ? DmgTextColors.StanceDebuff : DmgTextColors.Debuff,
            _ => ResolveDamageColor(data),
        };
    }

    private Color ResolveDamageColor(DmgTextData data)
    {
        if (data.isStanceEnhanced && data.isCardEnhanced) return DmgTextColors.FullEnhanceDamage;
        if (data.isCardEnhanced) return DmgTextColors.EnhanceDamage;
        if (data.isStanceEnhanced) return DmgTextColors.StanceDamage;
        return DmgTextColors.Damage;
    }

    private Color ResolveHealColor(DmgTextData data)
    {
        if (data.isStanceEnhanced && data.isCardEnhanced) return DmgTextColors.FullEnhanceHeal;
        if (data.isCardEnhanced) return DmgTextColors.EnhanceHeal;
        if (data.isStanceEnhanced) return DmgTextColors.StanceHeal;
        return DmgTextColors.Heal;
    }
}