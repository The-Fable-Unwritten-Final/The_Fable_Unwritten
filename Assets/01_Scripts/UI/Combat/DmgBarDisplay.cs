using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum DmgTextType
{
    Normal,
    Heal,
    Buff, // 약간의 딜레이
    Debuff, // 약간의 딜레이
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
    [SerializeField] private GameObject dmgPrintPrefab;
    
    private Queue<GameObject> dmgPrintPool = new Queue<GameObject>();
    private const int maxPoolSize = 10; // 최대 풀 크기

    private float floatOffset = 0.7f;
    private float floatDuration = 0.9f;

    public void Initialize(DmgTextData data, Transform target, float offsetY = 1f)
    {
        // 풀에서 가져오기 또는 새로 생성
        GameObject dmgInstance = dmgPrintPool.Count > 0 
            ? dmgPrintPool.Dequeue() 
            : Instantiate(dmgPrintPrefab, transform);
        
        dmgInstance.SetActive(true);

        // 프리팹의 컴포넌트 가져오기
        TextMeshProUGUI tmpText = dmgInstance.GetComponentInChildren<TextMeshProUGUI>();
        CanvasGroup canvasGroup = dmgInstance.GetComponent<CanvasGroup>();

        // 위치 설정
        dmgInstance.transform.position = target.position + Vector3.up * offsetY;

        // 텍스트 및 스타일 설정
        tmpText.text = NumberSpriteShift(data.Text);
        tmpText.color = GetFinalColor(ResolveColor(data), data.isWeakened);
        tmpText.fontSize = (data.isStanceEnhanced || data.isCardEnhanced) ? 0.8f : 0.5f;

        // 스케일 설정 (시작: 1.6배 크기)
        dmgInstance.transform.localScale = Vector3.one * 1.6f;

        canvasGroup.alpha = 1f;

        StartCoroutine(FadeAndFloat(dmgInstance, canvasGroup, tmpText));
    }

    private string NumberSpriteShift(string dataT)
    {
        string result = "";
        foreach (char c in dataT)
        {
            if (char.IsDigit(c))
            {
                result += $"<sprite={c}>";
            }
            else
            {
                result += c;
            }
        }
        return result;
    }
    private IEnumerator FadeAndFloat(GameObject dmgInstance, CanvasGroup canvasGroup, TextMeshProUGUI tmpText)
    {
        Vector3 start = dmgInstance.transform.position;
        Vector3 end = start + Vector3.up * floatOffset;
        Vector3 startScale = dmgInstance.transform.localScale;
        Vector3 targetScale = Vector3.one;
        
        // 1단계: 빠른 크기 축소 (충격 효과) - 0.15초
        float scaleDownDuration = 0.15f;
        float time = 0;
        while (time < scaleDownDuration)
        {
            time += Time.deltaTime;
            float t = time / scaleDownDuration;
            dmgInstance.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        dmgInstance.transform.localScale = targetScale;

        // 2단계: 딜레이 - 0.2초 (원래 크기 유지)
        yield return new WaitForSeconds(0.2f);

        // 3단계: 위로 올라가며 페이드 아웃 - 남은 시간에 걸쳐 animate
        float floatAndFadeDuration = floatDuration - scaleDownDuration - 0.3f; // 약 0.45초
        time = 0;
        while (time < floatAndFadeDuration)
        {
            time += Time.deltaTime;
            float t = time / floatAndFadeDuration;
            dmgInstance.transform.position = Vector3.Lerp(start, end, t);
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        tmpText.text = "";
        dmgInstance.transform.localScale = Vector3.one;
        dmgInstance.SetActive(false);
        
        // 풀 크기 제한
        if (dmgPrintPool.Count < maxPoolSize)
            dmgPrintPool.Enqueue(dmgInstance);
        else
            Destroy(dmgInstance);
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