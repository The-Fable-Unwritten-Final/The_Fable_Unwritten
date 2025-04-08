using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Start, NormalBattle, EliteBattle, RandomEvent, Camp, Boss
}

[SerializeField]
public class GraphNode
{
    public int id;
    public NodeType type;
    public Vector2 position;
    public int columnIndex;
    public List<GraphNode> nextNodes = new();
}
