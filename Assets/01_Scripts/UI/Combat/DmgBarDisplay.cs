using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public enum DmgTextType
{
    Normal,
    Heal,
    // 버프류
    AttackBuff, DefenseBuff, Bless, Penance, Guard,
    // 디버프류
    AttackDebuff, DefenseDebuff, Burn, Freeze, Activate, Crime, Scar, Stun,
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
    //
    // 색상 정의
    // 0. 공격 : 주황
    // 1. 치유 : 초록
    // 2. 버프류 : 시안
    // 3. 나머지 디버프 상태 이상류 : 마젠타
    // 색상의 어느정도 통일성 유지 필요
    // 
}


public class DmgBarDisplay : MonoBehaviour
{
    [SerializeField] private GameObject dmgPrintPrefab;
    [SerializeField] private Sprite[] typeIcons; // 타입별 아이콘 스프라이트 배열
    
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
        dmgInstance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;  // RectTransform 위치 초기화

        // 프리팹의 컴포넌트 가져오기
        TextMeshProUGUI tmpText = dmgInstance.GetComponentInChildren<TextMeshProUGUI>();
        Image icon = dmgInstance.GetComponentInChildren<Image>();
        CanvasGroup canvasGroup = dmgInstance.GetComponent<CanvasGroup>();

        // 위치 설정
        RectTransform rect = dmgInstance.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        dmgInstance.transform.position += Vector3.up * offsetY;
        

        // 아이콘 설정
        Sprite typeIcon = SelectTypeIcon(data);
        if (typeIcon != null)
            icon.sprite = typeIcon;
            
        // 텍스트 및 스타일 설정
        tmpText.text = NumberSpriteShift(data);
        tmpText.fontSize = (data.isStanceEnhanced || data.isCardEnhanced) ? 0.8f : 0.5f;
        // 아이콘 강화 별 위치 및 크기 조정 
        icon.GetComponent<RectTransform>().anchoredPosition = (data.isStanceEnhanced || data.isCardEnhanced) ? new Vector2(-1.2f, 0.5f) : new Vector2(-1f, 0.3f);
        icon.GetComponent<RectTransform>().sizeDelta = (data.isStanceEnhanced || data.isCardEnhanced) ? new Vector2(1.5f, 1.5f) : new Vector2(1f, 1f);

        // 스케일 설정 (시작: 1.6배 크기)
        dmgInstance.transform.localScale = Vector3.one * 1.6f;

        canvasGroup.alpha = 1f;

        StartCoroutine(FadeAndFloat(dmgInstance, canvasGroup, tmpText));
    }
    private Sprite SelectTypeIcon(DmgTextData data)
    {
        int index = data.type switch
        {
            DmgTextType.Normal => 0,
            DmgTextType.Heal => 1,
            DmgTextType.AttackBuff => 2,
            DmgTextType.DefenseBuff => 3,
            DmgTextType.Bless => 4,
            DmgTextType.Penance => 5,
            DmgTextType.Guard => 6,
            DmgTextType.AttackDebuff => 7,
            DmgTextType.DefenseDebuff => 8,
            DmgTextType.Burn => 9,
            DmgTextType.Freeze => 10,
            DmgTextType.Activate => 11,
            DmgTextType.Crime => 12,
            DmgTextType.Scar => 13,
            DmgTextType.Stun => 14,
            _ => 0
        };

        if (index >= 0 && index < typeIcons.Length)
            return typeIcons[index];
        else
            return null;
    }
    private string NumberSpriteShift(DmgTextData data)
    {
        string dataT = data.Text;
        if (string.IsNullOrEmpty(dataT)) return "";

        // 색상 정의에 따라 prefix 설정
        // 0. 공격 : 흰색
        // 1. 주황 : 미정
        // 2. 치유 : 초록
        // 3. 버프류 : 시안
        // 4. 나머지 디버프 상태 이상류 : 마젠타
        int prefix = data.type switch
        {
            DmgTextType.Normal => 0,
            DmgTextType.Heal => 2,
            DmgTextType.AttackBuff => 3,
            DmgTextType.DefenseBuff => 3,
            DmgTextType.Bless => 3,
            DmgTextType.Penance => 3,
            DmgTextType.Guard => 3,
            DmgTextType.AttackDebuff => 4,
            DmgTextType.DefenseDebuff => 4,
            DmgTextType.Burn => 4,
            DmgTextType.Freeze => 4,
            DmgTextType.Activate => 4,
            DmgTextType.Crime => 4,
            DmgTextType.Scar => 4,
            DmgTextType.Stun => 4,
            _ => 0
        };
        StringBuilder sb = new StringBuilder();

        foreach (char c in dataT)
        {
            if (char.IsDigit(c))
            {
                // 숫자: {prefix}{digit} 형식의 name을 가진 sprite character
                sb.Append($"<sprite name=\"{prefix}{c}\">");
            }
            else
            {
                // +, -, % 을 포함한 문자는 그대로 출력
                switch (c)
                {
                    case '+':
                        sb.Append($"<sprite name=\"{prefix}plus\">");
                        break;
                    case '-':
                        sb.Append($"<sprite name=\"{prefix}minus\">");
                        break;
                    case '%':
                        sb.Append($"<sprite name=\"{prefix}percent\">");
                        break;
                    default:
                        // 기타 문자 (공백 등)
                        sb.Append(c);
                        break;
                }
            }
        }

        return sb.ToString();
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
}