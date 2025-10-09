using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterIdealPair
{
    public CharacterClass owner;           // Sophia/Kayla/Leon
    public IdealSkillBase idealA;          // 첫 번째 이상실현
    public IdealSkillBase idealB;          // 두 번째 이상실현
}

[CreateAssetMenu(menuName = "Fable/Ideal/Database")]
public class IdealDatabase : ScriptableObject
{
    public List<CharacterIdealPair> entries = new();

    public bool TryGetPair(CharacterClass owner, out IdealSkillBase a, out IdealSkillBase b)
    {
        foreach (var e in entries)
        {
            if (e.owner == owner)
            {
                a = e.idealA;
                b = e.idealB;
                return true;
            }
        }
        a = b = null;
        return false;
    }
}
