using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stage/MapData")]
public class StageMapData : ScriptableObject
{
    public List<StageNode> nodes = new List<StageNode>();
}
