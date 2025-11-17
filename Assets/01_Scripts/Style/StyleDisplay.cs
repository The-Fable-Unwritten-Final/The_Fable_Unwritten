using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StyleDisplay : MonoBehaviour
{
    // 문체의 UI를 담당하는 스크립트 (ui 적인 조작을 메인으로 사용 => 노드 선택 씬에서만 존재)
    string t ="string";
    [SerializeField] StyleDefinition currentStyle; // starting 문체 디폴트 값으로 넣어두기
    void Start()
    {
        // 일반 텍스트의 로컬라이제이션 데이터를 받아오는 경우 GetValueFullText 가 아니라 LocaleDataManager.GetLocalizedStyleEffect("key") 형식으로 가져올 것
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        // 노드 씬으로 온 경우 활성화.
        // 기본적으로 빈 오브젝트 안에 넣고, 내부의 자식 오브젝트에 UI 배치
        // 스테이지 번호 확인 후, 기본 문체 적용 + UI 활성화 조정
    }

    /// <summary>
    /// '문체 효과'가 고정 효과(등급 상승x)가 아닐 경우, 받아온 string 값의 ## << 부분에 value값을 변환해서 대입 후 출력
    /// </summary>
    private string GetValueFullText(string key, bool isPlus)
    {
        if(currentStyle == null) return "";
        string txt = LocaleDataManager.GetLocalizedStyleEffect("key");
        string valueStr;
        StyleEffect eff;

        if(isPlus)
            eff = currentStyle.plusTiers[currentStyle.currentPlus-1].effects[0];
        else
            eff = currentStyle.minusTiers[currentStyle.currentMinus-1].effects[0];

        switch (eff.operation)
        {
            case EffectOperation.MulPercent:
                // (eff.value - 1) * 100 을 백분율로 표기
                float rawPercent = (eff.value - 1f) * 100f;
                // 양수/음수 부호 유지, 크기는 올림 처리(예: 1.1 -> 10 -> +10%)
                int pct = Mathf.CeilToInt(Mathf.Abs(rawPercent));
                valueStr = (rawPercent >= 0 ? "+" : "-") + pct.ToString() + "%";
                break;

            case EffectOperation.Add:
                int AddVal = Mathf.CeilToInt(eff.value);
                valueStr = AddVal.ToString();
                break;

            case EffectOperation.Set:
                int setVal = Mathf.CeilToInt(eff.value);
                valueStr = setVal.ToString();
                break;

            case EffectOperation.RandomRange:
                int min = Mathf.CeilToInt(eff.valueRange.x);
                int max = Mathf.CeilToInt(eff.valueRange.y);
                valueStr = $"{min}~{max}";
                break;

            default:
                // 안전한 기본 포맷
                valueStr = eff.value.ToString();
                break;
        }

        if (txt.Contains("##"))
            txt = txt.Replace("##", valueStr); // value 값이 변하는 경우 csv의 텍스트 중간에 '##' 가 존재.

        return txt;
    }
}
