using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnlockRecipe
{
    public CharacterClass character;
    public List<MaterialRequirement> materials;
    public CardType result;
}

[System.Serializable]
public class MaterialRequirement
{
    public int index;
    public int count;
}