using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpBarDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image hpBar;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, -0.6f, 0); // 스프라이트 아래 위치

    private Coroutine changeHpCoroutine;
    private PlayerData linkedPlayerData;
    private EnemyData linkedEnemyData;

    private Transform targetTransform;
    private SpriteRenderer targetRenderer;

    // 플레이어 데이터 연결
    public void BindPlayerData(PlayerData data)
    {
        if (linkedPlayerData != null)
            linkedPlayerData.OnHpChanged -= OnHpChanged;

        linkedPlayerData = data;
        linkedPlayerData.OnHpChanged += OnHpChanged;

        SetHpBar(linkedPlayerData.currentHP, linkedPlayerData.MaxHP);
    }

    // 적 데이터 연결
    public void BindEnemyData(EnemyData data)
    {
        if (linkedEnemyData != null)
            linkedEnemyData.OnHpChanged -= OnHpChanged;

        linkedEnemyData = data;
        linkedEnemyData.OnHpChanged += OnHpChanged;

        SetHpBar(linkedEnemyData.CurrentHP, linkedEnemyData.MaxHP);
    }

    private void OnDestroy()
    {
        if (linkedPlayerData != null)
            linkedPlayerData.OnHpChanged -= OnHpChanged;

        if (linkedEnemyData != null)
            linkedEnemyData.OnHpChanged -= OnHpChanged;
    }

    private void OnHpChanged(float hp, float maxHp)
    {
        ChangeHpBar(hp, maxHp);
    }

    public void SetHpBar(float hp, float maxHp)
    {
        if (hpBar == null) return;
        hpBar.fillAmount = hp / maxHp;

        if (hpText != null)
            hpText.text = Mathf.FloorToInt(hp).ToString();
    }

    public void ChangeHpBar(float hp, float maxHp)
    {
        if (hpBar == null) return;

        float targetFill = hp / maxHp;

        hpText.text = hp.ToString() + "/" + maxHp.ToString();

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

    // 🔹 SpriteRenderer 기준 위치/크기 추적
    public void FollowTarget(SpriteRenderer spriteRenderer)
    {
        targetTransform = spriteRenderer.transform;
        targetRenderer = spriteRenderer;
    }

    /*void LateUpdate()
    {
        if (targetTransform == null || targetRenderer == null)
            return;

        // 💡 스프라이트 아래 (min.y) 좌표 기준 위치 조정
        float spriteBottom = targetRenderer.bounds.min.y;

        transform.position = new Vector3(
            targetTransform.position.x,
            spriteBottom + offset.y,
            targetTransform.position.z
        );
    }*/
}
