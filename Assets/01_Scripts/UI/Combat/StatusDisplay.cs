using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusDisplay : MonoBehaviour
{
    [Serializable]
    public class StatusIconBinding
    {
        public BuffStatType type;
        public Sprite icon;
        public bool hideNumber;
    }

    private struct StatusDisplayEntry
    {
        public BuffStatType type;
        public int value;
        public Sprite icon;
        public bool hideNumber;
        public int priority;
    }

    [Header("Slot UI")]
    [SerializeField] private List<StatusSlot> slots = new List<StatusSlot>();

    [Header("Default Status Icons")]
    [SerializeField] private List<StatusIconBinding> iconBindings = new List<StatusIconBinding>();

    [Header("Attack / Defense Icons")]
    [SerializeField] private Sprite atkUpIcon;
    [SerializeField] private Sprite atkDownIcon;
    [SerializeField] private Sprite defUpIcon;
    [SerializeField] private Sprite defDownIcon;

    [Header("Extra Icons")]
    [SerializeField] private Sprite blockIcon;

    [Header("Options")]
    [SerializeField] private int maxVisibleCount = 9;

    private Dictionary<BuffStatType, StatusIconBinding> iconMap;

    private PlayerController player;
    private Enemy enemy;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        enemy = GetComponentInParent<Enemy>();
        BuildIconMap();
    }

    private void BuildIconMap()
    {
        iconMap = new Dictionary<BuffStatType, StatusIconBinding>();

        foreach (var binding in iconBindings)
        {
            if (binding == null || binding.icon == null)
                continue;

            if (!iconMap.ContainsKey(binding.type))
                iconMap.Add(binding.type, binding);
        }
    }

    public void PlayerUpdateUI()
    {
        if (player == null) return;
        Refresh(player.tickEffects, player.instantEffects, player.hasBlock, player);
    }

    public void EnemyUpdateUI()
    {
        if (enemy == null) return;
        Refresh(enemy.tickEffects, enemy.instantEffects, enemy.hasBlock, enemy);
    }

    private void Refresh(
        List<TickEffect> tickEffects,
        List<InstanceEffect> instantEffects,
        bool hasBlock,
        IStatusReceiver receiver)
    {
        ClearAllSlots();

        List<StatusDisplayEntry> entries = BuildEntries(tickEffects, instantEffects, hasBlock, receiver);

        int count = Mathf.Min(entries.Count, Mathf.Min(maxVisibleCount, slots.Count));

        for (int i = 0; i < count; i++)
        {
            slots[i].Bind(
                entries[i].type.ToString(),
                entries[i].icon,
                entries[i].value,
                hideNumber: entries[i].hideNumber
            );
        }
    }

    private List<StatusDisplayEntry> BuildEntries(
        List<TickEffect> tickEffects,
        List<InstanceEffect> instantEffects,
        bool hasBlock,
        IStatusReceiver receiver)
    {
        List<StatusDisplayEntry> results = new List<StatusDisplayEntry>();

        // 1. instantEffects 기반 일반 상태
        Dictionary<BuffStatType, int> mergedInstant = new Dictionary<BuffStatType, int>();

        foreach (var effect in instantEffects)
        {
            if (effect == null || effect.value <= 0)
                continue;

            if (effect.statType == BuffStatType.Attack || effect.statType == BuffStatType.Defend)
                continue;

            if (mergedInstant.ContainsKey(effect.statType))
                mergedInstant[effect.statType] += Mathf.RoundToInt(effect.value);
            else
                mergedInstant.Add(effect.statType, Mathf.RoundToInt(effect.value));
        }

        foreach (var pair in mergedInstant)
        {
            if (!TryGetDefaultIcon(pair.Key, out var binding))
                continue;

            results.Add(new StatusDisplayEntry
            {
                type = pair.Key,
                value = pair.Value,
                icon = binding.icon,
                hideNumber = binding.hideNumber,
                priority = GetPriority(pair.Key)
            });
        }

        // 2. Attack 표시
        int atkValue = Mathf.RoundToInt(receiver.GetEffectValue(BuffStatType.Attack));
        if (atkValue != 0)
        {
            Sprite atkIcon = atkValue > 0 ? atkUpIcon : atkDownIcon;

            if (atkIcon != null)
            {
                results.Add(new StatusDisplayEntry
                {
                    type = BuffStatType.Attack,
                    value = Mathf.Abs(atkValue),
                    icon = atkIcon,
                    hideNumber = false,
                    priority = 20
                });
            }
        }

        // 3. Defense 표시
        int defValue = Mathf.RoundToInt(receiver.GetEffectValue(BuffStatType.Defend));
        if (defValue != 0)
        {
            Sprite defIcon = defValue > 0 ? defUpIcon : defDownIcon;

            if (defIcon != null)
            {
                results.Add(new StatusDisplayEntry
                {
                    type = BuffStatType.Defend,
                    value = Mathf.Abs(defValue),
                    icon = defIcon,
                    hideNumber = false,
                    priority = 21
                });
            }
        }

        // 4. block
        if (hasBlock && blockIcon != null)
        {
            results.Add(new StatusDisplayEntry
            {
                type = BuffStatType.None,
                value = 0,
                icon = blockIcon,
                hideNumber = true,
                priority = 999
            });
        }

        return results
            .OrderBy(x => x.priority)
            .ThenBy(x => x.type.ToString())
            .ToList();
    }

    private bool TryGetDefaultIcon(BuffStatType type, out StatusIconBinding binding)
    {
        if (iconMap == null || iconMap.Count == 0)
            BuildIconMap();

        return iconMap.TryGetValue(type, out binding);
    }

    private int GetPriority(BuffStatType type)
    {
        return type switch
        {
            BuffStatType.Stun => 0,
            BuffStatType.Guard => 1,
            BuffStatType.Burn => 2,
            BuffStatType.Freeze => 3,
            BuffStatType.Activate => 4,
            BuffStatType.Bless => 5,
            BuffStatType.Penance => 6,
            BuffStatType.Crime => 7,
            BuffStatType.Scar => 8,
            _ => 100
        };
    }

    private void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
                slot.Clear();
        }
    }
}