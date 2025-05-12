using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardUnlockRecipe
{
    public CharacterClass character;
    public CardType resultType;

    public List<RequiredLoot> requiredTypes; //<전리품 인덱스, 개수>
}


[System.Serializable]
public class RequiredLoot
{
    public int lootIndex; // ex. 0: 백색 결정
    public int count;
}