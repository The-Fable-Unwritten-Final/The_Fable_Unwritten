using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeTpye
{
    Start, NormalBattle, EliteBattle, RandomEvent, Camp, Boss
}

[Serializable]
public class StageNodeData
{
    public int row;
    public int col;
    public NodeTpye type;

    public static NodeTpye GetRandomType()
    {
        NodeTpye[] types = new NodeTpye[]
        {
            NodeTpye.NormalBattle,
            NodeTpye.EliteBattle,
            NodeTpye.RandomEvent,
            NodeTpye.Camp
        };

        return types[UnityEngine.Random.Range(0, types.Length)];
    }
}
