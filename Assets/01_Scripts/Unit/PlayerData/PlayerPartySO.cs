using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player")]
public class PlayerPartySO : ScriptableObject
{
    public List<PlayerData> allPlayers;
}