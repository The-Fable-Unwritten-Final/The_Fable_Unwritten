using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 스테이지 노드 데이터 생성하고 노드끼리 연결을 구성해 주는 클래스
/// </summary>
public static class StageGraphGenerator
{
    /// <summary>
    /// 스테이지 인덱스와 간격 확인 후 노드 구조를 생성
    /// </summary>
    public static StageData Generate(int stageIndex, Vector2 spacing)
    {
        var stage = new StageData { stageIndex = stageIndex };
        int id = 0;

        stage.columnCount = (stageIndex == 1 || stageIndex == 5) ? 4 : 7;

        // 시작 노드 (0열 고정)
        stage.columns.Add(new List<GraphNode> {
            new GraphNode { id = id++, type = NodeType.Start, columnIndex = 0, position = Vector2.zero }
        });

        if (stageIndex == 1)
        {
            GenerateTutorialStage(stage, spacing, ref id);
        }
        else if (stageIndex == 5)
        {
            GenerateBossStage(stage, spacing, ref id);
        }
        else
        {
            GenerateStandardStage(stage, spacing, ref id);
        }

        LinkNodes(stage);
        return stage;
    }

    /// <summary>
    /// 튜토리얼 스테이지 구성 (1열~3열 일반 전투)
    /// </summary>
    private static void GenerateTutorialStage(StageData stage, Vector2 spacing, ref int id)
    {
        for (int i = 1; i < 4; i++)
        {
            stage.columns.Add(new List<GraphNode> {
                new GraphNode
                {
                    id = id++,
                    type = NodeType.NormalBattle,
                    columnIndex = i,
                    position = new Vector2(i * spacing.x, 0)
                }
            });
        }
    }

    /// <summary>
    /// 보스 스테이지 구성 (시작, 엘리트, 이벤트, 보스)
    /// </summary>
    private static void GenerateBossStage(StageData stage, Vector2 spacing, ref int id)
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

    /// <summary>
    /// 일반 스테이지 구성 (2~4 스테이지): 노드 타입별 총 12개, 열당 1~3개 분배
    /// </summary>
    private static void GenerateStandardStage(StageData stage, Vector2 spacing, ref int id)
    {
        var pool = new List<NodeType>();
        pool.AddRange(Enumerable.Repeat(NodeType.NormalBattle, 6));
        pool.AddRange(Enumerable.Repeat(NodeType.EliteBattle, 1));
        pool.AddRange(Enumerable.Repeat(NodeType.RandomEvent, 3));
        pool.AddRange(Enumerable.Repeat(NodeType.Camp, 2));
        pool = pool.OrderBy(_ => Random.value).ToList();

        int[] counts = new int[5]; // 열 1~5
        for (int i = 0; i < 5; i++) counts[i] = 1;

        int remaining = 12 - 5;
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
                indices.Remove(i);
            }
        }

        for (int col = 1; col <= 5; col++)
        {
            var column = new List<GraphNode>();
            int count = counts[col - 1];
            float totalHeight = (count - 1) * spacing.y;

            for (int i = 0; i < count; i++)
            {
                if (pool.Count == 0) break;
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

        // 마지막 보스 열 추가
        stage.columns.Add(new List<GraphNode> {
            new GraphNode { id = id++, type = NodeType.Boss, columnIndex = 6, position = new Vector2(6 * spacing.x, 0) }
        });
    }

    /// <summary>
    /// 생성된 노드들 간의 연결 관계 시켜주는 함수
    /// </summary>
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

                // 마지막 노드일 경우 전부 연결 시켜주기
                var lastFrom = fromCol[^1];
                var lastTo = toCol[^1];
                if (!lastFrom.nextNodes.Contains(lastTo))
                {
                    lastFrom.nextNodes.Add(lastTo);
                }

                // 연결되지 않은 노드도 마지막 노드에서 연결
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
