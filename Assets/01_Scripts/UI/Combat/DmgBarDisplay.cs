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

        // 프리팹의 컴포넌트 가져오기
        TextMeshProUGUI tmpText = dmgInstance.GetComponentInChildren<TextMeshProUGUI>();
        Image icon = dmgInstance.GetComponentInChildren<Image>();
        CanvasGroup canvasGroup = dmgInstance.GetComponent<CanvasGroup>();

        // 위치 설정
        dmgInstance.transform.position = target.position + Vector3.up * offsetY;

        // 아이콘 설정
        /* 아이콘 리소스 받기 전까지 주석 처리
        Sprite typeIcon = SelectTypeIcon(data);
        if (typeIcon != null)
            icon.sprite = typeIcon;*/
            
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
            DmgTextType.Buff => 2,
            DmgTextType.Debuff => 3,
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

        int prefix = (int)data.type;
        StringBuilder sb = new StringBuilder();

        // 현재 타입별 스프라이트 등록이 안 되어있어서 임시로 타입 0으로 고정 // 해당 데이터는 tmp 컴포넌트의 Extra settings/sprite asset 에서 접근 가능
        prefix = 0;
        foreach (char c in dataT)
        {
            if (char.IsDigit(c))
            {
                // 숫자: {prefix}{digit}
                // 숫자 only 스프라이트
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