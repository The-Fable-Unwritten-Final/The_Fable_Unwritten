using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusDisplay : MonoBehaviour
{
    [Header("Status Images")]
    [SerializeField] private GameObject atk;
    [SerializeField] private GameObject atk_Img;
    [SerializeField] private GameObject def;
    [SerializeField] private GameObject def_Img;
    [SerializeField] private GameObject block_Img;
    [SerializeField] private GameObject blindTop_Img;
    [SerializeField] private GameObject blindMiddle_Img;
    [SerializeField] private GameObject blindBottom_Img;
    [SerializeField] private GameObject stun_Img;

    [Header("Attack Text")]
    [SerializeField] private TextMeshProUGUI atk_txt;
    [SerializeField] private TextMeshProUGUI def_txt;

    private PlayerController player;
    private Enemy enemy;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        enemy = GetComponentInParent<Enemy>();
    }

    public void PlayerUpdateUI()
    {
        if (player == null) return;
        UpdateUI(player.activeEffects, player.hasBlock);
    }

    public void EnemyUpdateUI()
    {
        if (enemy == null) return;
        UpdateUI(enemy.activeEffects, enemy.hasBlock);
    }

    private void UpdateUI(List<StatusEffect> effects, bool hasBlock)
    {
        Debug.Log("실행됨");
        atk.SetActive(effects.Exists(e => e.statType == BuffStatType.Attack && e.duration > 0));
        def.SetActive(effects.Exists(e => e.statType == BuffStatType.Defense && e.duration > 0));
        stun_Img.SetActive(effects.Exists(e => e.statType == BuffStatType.stun && e.duration > 0));
        block_Img.SetActive(hasBlock);

        blindTop_Img.SetActive(effects.Exists(e => e.statType == BuffStatType.CantAttackInStance && e.value == 0 && e.duration > 0));
        blindMiddle_Img.SetActive(effects.Exists(e => e.statType == BuffStatType.CantAttackInStance && e.value == 1 && e.duration > 0));
        blindBottom_Img.SetActive(effects.Exists(e => e.statType == BuffStatType.CantAttackInStance && e.value == 2 && e.duration > 0));

        var atkEffect = effects.Find(e => e.statType == BuffStatType.Attack && e.duration > 0);
        if (atkEffect != null)
        {
            float angleZ = atkEffect.value >= 0 ? 0f : 180f;
            atk_Img.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, angleZ);

            if (atk_txt != null)
                atk_txt.text = Mathf.Abs(atkEffect.value).ToString();
        }

        var defEffect = effects.Find(e => e.statType == BuffStatType.Defense && e.duration > 0);
        if (defEffect != null)
        {
            float angleZ = defEffect.value >= 0 ? 0f : 180f;
            def_Img.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, angleZ);

            if (def_txt != null)
                def_txt.text = Mathf.Abs(defEffect.value).ToString();
        }
    }
}
