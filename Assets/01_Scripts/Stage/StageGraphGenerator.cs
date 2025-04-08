using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StageGraphGenerator
{
    public static StageData Generate(int stageIndex, Vector2 spacing)
    {
        var stage = new StageData { stageIndex = stageIndex };
        int id = 0;

        stage.columnCount = (stageIndex == 1 || stageIndex == 5) ? 4 : 7;

        // 1열: Start
        stage.columns.Add(new List<GraphNode> {
            new GraphNode { id = id++, type = NodeType.Start, columnIndex = 0, position = Vector2.zero }
        });

        if (stageIndex == 1)
        {
            for (int i = 1; i < 4; i++)
            {
                stage.columns.Add(new List<GraphNode> {
                    new GraphNode { id = id++, type = NodeType.NormalBattle, columnIndex = i, position = new Vector2(i * spacing.x, 0) }
                });
            }
        }
        else if (stageIndex == 5)
        {
            stage.columns.Add(new List<GraphNode> {
                new GraphNode { id = id++, type = NodeType.EliteBattle, columnIndex = 1, position = new Vector2(1 * spacing.x, 0) }
            });
            stage.columns.Add(new List<GraphNode> {
                new GraphNode { id = id++, type = NodeType.RandomEvent, columnIndex = 2, position = new Vector2(2 * spacing.x, 0) }
            });
            stage.columns.Add(new List<GraphNode> {
                new GraphNode { id = id++, type = NodeType.Boss, columnIndex = 3, position = new Vector2(3 * spacing.x, 0) }
            });
        }
        else
        {
            var nodeCounts = new[] { 6, 1, 3, 2 };
            var nodeTypes = new[] {
                NodeType.NormalBattle, NodeType.EliteBattle,
                NodeType.RandomEvent, NodeType.Camp
            };

            var pool = new List<NodeType>();
            for (int i = 0; i < nodeCounts.Length; i++)
                pool.AddRange(Enumerable.Repeat(nodeTypes[i], nodeCounts[i]));
            pool = pool.OrderBy(_ => Random.value).ToList();

            for (int col = 1; col <= 5; col++)
            {
                int count = Random.Range(2, 4);
                var column = new List<GraphNode>();
                for (int i = 0; i < count && pool.Count > 0; i++)
                {
                    column.Add(new GraphNode
                    {
                        id = id++,
                        type = pool[0],
                        columnIndex = col,
                        position = new Vector2(col * spacing.x, (i - count / 2f) * -spacing.y)
                    });
                    pool.RemoveAt(0);
                }
                stage.columns.Add(column);
            }

            stage.columns.Add(new List<GraphNode> {
                new GraphNode { id = id++, type = NodeType.Boss, columnIndex = 6, position = new Vector2(6 * spacing.x, 0) }
            });
        }

        LinkNodes(stage);
        return stage;
    }

    private static void LinkNodes(StageData stage)
    {
        for (int i = 0; i < stage.columns.Count - 1; i++)
        {
            var fromCol = stage.columns[i];
            var toCol = stage.columns[i + 1];

            bool fullyConnect = (i == 0 || i == stage.columns.Count - 2);

            if (fullyConnect)
            {
                foreach (var from in fromCol)
                    foreach (var to in toCol)
                        from.nextNodes.Add(to);
            }
            else
            {
                var toSorted = toCol.OrderBy(n => n.position.y).ToList();
                for (int j = 0; j < fromCol.Count; j++)
                {
                    var from = fromCol[j];
                    int count = Random.Range(1, 3);
                    for (int k = 0; k < count && k < toSorted.Count; k++)
                        from.nextNodes.Add(toSorted[k]);
                }
            }
        }
    }
}
