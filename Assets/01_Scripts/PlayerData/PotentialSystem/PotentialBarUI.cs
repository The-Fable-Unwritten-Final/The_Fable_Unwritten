using UnityEngine;
using UnityEngine.UI;

public class PotentialBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;

    private PlayerController owner;

    public void Bind(PlayerController player)
    {
        Unbind();

        owner = player;

        if (owner == null || owner.StanceSystem == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        owner.StanceSystem.OnGaugeChanged += HandleGaugeChanged;
        owner.StanceSystem.OnStanceChanged += HandleStanceChanged;

        Refresh();
    }

    public void Unbind()
    {
        if (owner != null && owner.StanceSystem != null)
        {
            owner.StanceSystem.OnGaugeChanged -= HandleGaugeChanged;
            owner.StanceSystem.OnStanceChanged -= HandleStanceChanged;
        }

        owner = null;
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void HandleGaugeChanged(int currentGauge)
    {
        //Debug.Log($"[PotentialBarUI] {owner?.playerData?.CharacterName} gauge changed -> {currentGauge}");
        RefreshGauge(currentGauge);
    }

    private void HandleStanceChanged(StancType stance)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (owner == null || owner.StanceSystem == null || fillImage == null)
            return;

        RefreshGauge(owner.StanceSystem.Gauge.CurrentGauge);
    }

    private void RefreshGauge(int currentGauge)
    {
        if (fillImage == null)
            return;

        int maxGauge = PotentialGauge.MAX_GAUGE;
        float normalized = Mathf.Clamp01((float)currentGauge / maxGauge);
        fillImage.fillAmount = normalized;
    }
}