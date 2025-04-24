using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBarDisplay : MonoBehaviour
{
    [SerializeField] Image hpBar;
    private Coroutine changeHpCoroutine;

    /// <summary>
    /// 최초 객체의 데이터를 세팅해줄때 호출
    /// </summary>
    public void SetHpBar(float hp, float maxHp)
    {
        if (hpBar == null) return;

        hpBar.fillAmount = hp / maxHp;
    }

    /// <summary>
    /// 원하는 체력 값으로 변환해 줄때 호출
    /// </summary>
    public void ChangeHpBar(float hp, float maxHp)
    {
        if (hpBar == null) return;

        float targetFill = hp / maxHp;

        // 이전 코루틴이 있다면 멈추고 새로 시작 (체력 변경중, 체력변경 요청이 들어올 경우.)
        if (changeHpCoroutine != null)
            StopCoroutine(changeHpCoroutine);

        changeHpCoroutine = StartCoroutine(AnimateHpBarChange(targetFill, 0.4f));
    }

    private IEnumerator AnimateHpBarChange(float targetFill, float duration)
    {
        float startFill = hpBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            hpBar.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }

        hpBar.fillAmount = targetFill;
    }
}
