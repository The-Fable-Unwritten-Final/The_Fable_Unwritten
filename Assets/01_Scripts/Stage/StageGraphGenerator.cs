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
            // ✅ 1. 노드 종류별 개수 지정
            var pool = new List<NodeType>();
            pool.AddRange(Enumerable.Repeat(NodeType.NormalBattle, 6));
            pool.AddRange(Enumerable.Repeat(NodeType.EliteBattle, 1));
            pool.AddRange(Enumerable.Repeat(NodeType.RandomEvent, 3));
            pool.AddRange(Enumerable.Repeat(NodeType.Camp, 2));
            pool = pool.OrderBy(_ => Random.value).ToList(); // 섞기

            // 2. 열당 노드 수 분배 (열 1~5)
            int[] counts = new int[5]; // 열 1~5
            for (int i = 0; i < 5; i++) counts[i] = 1; // 최소 1개씩

            int remaining = 12 - 5; // 남은 7개 분배
            List<int> indices = Enumerable.Range(0, 5).ToList();

            while (remaining > 0 && indices.Count > 0)
            {
                int i = indices[Random.Range(0, indices.Count)];

                if (counts[i] < 3)
                {
                    counts[i]++;
                    remaining--;
                }

                if (counts[i] == 3)
                {
                    indices.Remove(i); // 더 이상 추가 불가한 열 제거
                }
            }

            // ✅ 3. 열마다 노드 배치
            for (int col = 1; col <= 5; col++)
            {
                var column = new List<GraphNode>();
                int count = counts[col - 1];

                // 👇 열 내 전체 높이 계산 (노드 간 간격 유지)
                float totalHeight = (count - 1) * spacing.y;

                for (int i = 0; i < count; i++)
                {
                    if (pool.Count == 0) break;

                    // 👇 위에서 아래로 노드를 고르게 분산
                    float y = totalHeight / 2f - i * spacing.y;

                    column.Add(new GraphNode
                    {
                        id = id++,
                        type = pool[0],
                        columnIndex = col,
                        position = new Vector2(col * spacing.x, y)
                    });

                    pool.RemoveAt(0);
                }

                stage.columns.Add(column);
            }

            // ✅ 4. 마지막 Boss 열
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
                int toCount = toCol.Count;
                int fromCount = fromCol.Count;
                int toIndex = 0;

                for (int j = 0; j < fromCount; j++)
                {
                    var from = fromCol[j];

                    // 기본 연결
                    if (toIndex < toCount)
                    {
                        from.nextNodes.Add(toCol[toIndex]);
                    }

                    if (toIndex + 1 < toCount && Random.value < 0.5f)
                    {
                        from.nextNodes.Add(toCol[toIndex + 1]);
                    }

                    toIndex = Mathf.Min(toIndex + 1, toCount - 1);
                }

                // ✅ [추가1] 마지막 노드는 다음 열의 마지막 노드와 연결
                var lastFrom = fromCol[^1];
                var lastTo = toCol[^1];
                if (!lastFrom.nextNodes.Contains(lastTo))
                {
                    lastFrom.nextNodes.Add(lastTo);
                }

                // ✅ [추가2] 연결되지 않은 노드도 마지막 노드에서 연결
                var connectedToNodes = new HashSet<GraphNode>(
                    fromCol.SelectMany(f => f.nextNodes)
                );

                foreach (var to in toCol)
                {
                    if (!connectedToNodes.Contains(to) && !lastFrom.nextNodes.Contains(to))
                    {
                        lastFrom.nextNodes.Add(to);
                    }
                }
            }
        }
    }
}
