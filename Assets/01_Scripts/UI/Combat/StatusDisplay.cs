using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusDisplay : MonoBehaviour
{
    [Header("Status Images")]
    [SerializeField] private GameObject atk_Img;
    [SerializeField] private GameObject def_Img;
    [SerializeField] private GameObject block_Img;
    [SerializeField] private GameObject blindTop_Img;
    [SerializeField] private GameObject blindMiddle_Img;
    [SerializeField] private GameObject blindBottom_Img;
    [SerializeField] private GameObject stun_Img;

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

        atk_Img.SetActive(player.HasEffect(BuffStatType.Attack));
        def_Img.SetActive(player.HasEffect(BuffStatType.Defense));
        stun_Img.SetActive(player.HasEffect(BuffStatType.stun));

        block_Img.SetActive(player.hasBlock);

        bool hasBlindTop = player.activeEffects.Exists(e => e.statType == BuffStatType.blind && e.value == 0 && e.duration > 0);
        bool hasBlindMid = player.activeEffects.Exists(e => e.statType == BuffStatType.blind && e.value == 1 && e.duration > 0);
        bool hasBlindBot = player.activeEffects.Exists(e => e.statType == BuffStatType.blind && e.value == 2 && e.duration > 0);

        blindTop_Img.SetActive(hasBlindTop);
        blindMiddle_Img.SetActive(hasBlindMid);
        blindBottom_Img.SetActive(hasBlindBot);
    }

    public void EnemyUpdateUI()
    {
        if (enemy == null) return;

        atk_Img.SetActive(enemy.HasEffect(BuffStatType.Attack));
        def_Img.SetActive(enemy.HasEffect(BuffStatType.Defense));
        stun_Img.SetActive(enemy.HasEffect(BuffStatType.stun));
        block_Img.SetActive(enemy.hasBlock);

        bool hasBlindTop = enemy.activeEffects.Exists(e => e.statType == BuffStatType.blind && e.value == 0 && e.duration > 0);
        bool hasBlindMid = enemy.activeEffects.Exists(e => e.statType == BuffStatType.blind && e.value == 1 && e.duration > 0);
        bool hasBlindBot = enemy.activeEffects.Exists(e => e.statType == BuffStatType.blind && e.value == 2 && e.duration > 0);

        blindTop_Img.SetActive(hasBlindTop);
        blindMiddle_Img.SetActive(hasBlindMid);
        blindBottom_Img.SetActive(hasBlindBot);
    }
}
