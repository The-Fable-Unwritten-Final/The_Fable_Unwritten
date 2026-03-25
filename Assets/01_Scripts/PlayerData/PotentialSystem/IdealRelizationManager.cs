using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class IdealRealizationManager : MonoBehaviour
{
    public static IdealRealizationManager Instance { get; private set; }

    [SerializeField] private int idealCardIndex = 4001;

    // 이번 전투에서 이미 사용한 이상실현
    private readonly HashSet<string> usedIdealKeysThisBattle = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ResetBattleState()
    {
        usedIdealKeysThisBattle.Clear();
    }

    public void OnAnyAllyStanceEffectTriggered(PlayerController triggerPlayer, BattleFlowController battleFlow)
    {
        if (battleFlow == null || triggerPlayer == null)
            return;

        if (!CanGenerateIdealCard(battleFlow))
            return;

        CreateIdealCardToRightmostHand(battleFlow);
    }

    private bool CanGenerateIdealCard(BattleFlowController battleFlow)
    {
        if (HasUnusedIdealCardInAnyHand(battleFlow))
            return false;

        if (!HasAnyAvailableIdealRemaining(battleFlow))
            return false;

        return true;
    }

    private bool HasUnusedIdealCardInAnyHand(BattleFlowController battleFlow)
    {
        foreach (var receiver in battleFlow.playerParty)
        {
            if (receiver is PlayerController pc)
            {
                if (pc.Deck != null && pc.Deck.Hand.Any(c => c.index == idealCardIndex))
                    return true;
            }
        }
        return false;
    }

    private bool HasAnyAvailableIdealRemaining(BattleFlowController battleFlow)
    {
        foreach (var receiver in battleFlow.playerParty)
        {
            if (receiver is PlayerController pc)
            {
                foreach (var stance in GetUnlockedIdealStances(pc))
                {
                    string key = GetIdealKey(pc, stance);
                    if (!usedIdealKeysThisBattle.Contains(key))
                        return true;
                }
            }
        }

        return false;
    }

    private IEnumerable<StancType> GetUnlockedIdealStances(PlayerController pc)
    {
        // TODO: 실제 해금 데이터 구조로 교체
        return pc.ChClass switch
        {
            CharacterClass.Sophia => new[] { StancType.Seek, StancType.Insight },
            CharacterClass.Kayla => new[] { StancType.Mercy, StancType.Discipline },
            CharacterClass.Leon => new[] { StancType.Rush, StancType.Defense },
            _ => System.Array.Empty<StancType>()
        };
    }

    private string GetIdealKey(PlayerController pc, StancType stance)
    {
        return $"{pc.ChClass}_{stance}";
    }

    private void CreateIdealCardToRightmostHand(BattleFlowController battleFlow)
    {
        var targetPlayer = battleFlow.playerParty
            .OfType<PlayerController>()
            .FirstOrDefault(p => p.IsAlive());

        if (targetPlayer == null || targetPlayer.Deck == null)
            return;

        CardModel origin = DataManager.Instance.GetCardByIndex(idealCardIndex);
        if (origin == null)
        {
            Debug.LogWarning($"[IdealRealizationManager] index={idealCardIndex} 카드가 없음");
            return;
        }

        CardModel newCard = origin.Clone();
        targetPlayer.Deck.AddToHandRightmost(newCard);

        Debug.Log("[IdealRealizationManager] 이상실현 카드(4001) 생성");
    }

    public bool TryUseIdeal(PlayerController targetPlayer)
    {
        if (targetPlayer == null)
            return false;

        var stance = targetPlayer.playerData.currentStance;
        string key = GetIdealKey(targetPlayer, stance);

        if (!GetUnlockedIdealStances(targetPlayer).Contains(stance))
            return false;

        if (usedIdealKeysThisBattle.Contains(key))
            return false;

        usedIdealKeysThisBattle.Add(key);
        targetPlayer.MarkIdealThisStage();
        return true;
    }
}