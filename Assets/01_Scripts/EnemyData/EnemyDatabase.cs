using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Database/EnemyDatabase")]
public class EnemyDatabase : ScriptableObject
{
    public List<EnemyData> enemies = new();
}