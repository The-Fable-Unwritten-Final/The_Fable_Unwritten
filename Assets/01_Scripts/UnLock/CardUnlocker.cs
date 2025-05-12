using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardUnlocker : MonoBehaviour
{
    public static bool CanUnlock(CardUnlockRecipe recipe)
    {
        foreach (var pair in recipe.requiredTypes)
        {
            if (ProgressDataManager.Instance.itemCounts[pair.lootIndex] < pair.count)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 조합식에 따라 해당 타입의 해금되지 않은 카드 중 하나를 해금하고 반환함
    /// </summary>
    public static CardModel TryUnlock(CardUnlockRecipe recipe)
    {
        if (!CanUnlock(recipe))
            return null;

        // 해금되지 않은 카드 중에서 해당 조건에 맞는 것만 찾음
        var allCards = DataManager.Instance.AllCards;

        var candidates = allCards
           .Where(card =>
               card.characterClass == recipe.character &&
               card.type == recipe.resultType &&
               !card.isUnlocked)
           .ToList();

        if (candidates.Count == 0)
            return null;

        // 재료 소모
        foreach (var pair in recipe.requiredTypes)
        {
            ProgressDataManager.Instance.itemCounts[pair.lootIndex] -= pair.count;
        }

        // 랜덤 해금
        var selected = candidates[Random.Range(0, candidates.Count)];

        selected.isUnlocked = true;
        ProgressDataManager.Instance.unlockedCards.Add(selected.index);

        return selected;
    }
}
