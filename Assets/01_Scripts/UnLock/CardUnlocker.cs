using System;
using System.Linq;
using UnityEngine;

public class CardUnlocker : MonoBehaviour
{

    public static event Action<CharacterClass> OnRecipeUnlocked;
    public static event Action<CardModel> OnCardUnlocked;

    public static bool CanUnlock(UnlockRecipe recipe)
    {
        foreach (var pair in recipe.materials)
        {
            if (ProgressDataManager.Instance.itemCounts[pair.index] < pair.count)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 조합식에 따라 해당 타입의 해금되지 않은 카드 중 하나를 해금하고 반환함
    /// </summary>
    public static CardModel TryUnlock(UnlockRecipe recipe)
    {
        if (!CanUnlock(recipe))
            return null;

        // 해금되지 않은 카드 중에서 해당 조건에 맞는 것만 찾음
        var allCards = DataManager.Instance.AllCards;

        var candidates = allCards
           .Where(card =>
               card.characterClass == recipe.character &&
               card.type == recipe.result &&
               !card.isUnlocked)
           .ToList();

        if (candidates.Count == 0)
            return null;

        // 재료 소모
        foreach (var pair in recipe.materials)
        {
            ProgressDataManager.Instance.itemCounts[pair.index] -= pair.count;
        }

        // 랜덤 해금
        var selected = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        selected.isUnlocked = true;
        // 애널리틱스 (해금된 카드 타입)
        GameManager.Instance.analyticsLogger.LogUnlockTypeInfo((int)selected.type);

        ProgressDataManager.Instance.unlockedCards.Add(selected.index);

        OnRecipeUnlocked?.Invoke(recipe.character);
        OnCardUnlocked?.Invoke(selected);

        return selected;
    }
}
