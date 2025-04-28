using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 스테이지 노드 데이터 생성하고 노드끼리 연결을 구성해 주는 클래스
/// </summary>
public static class StageGraphGenerator
{
    /// <summary>
    /// 스테이지 노드 구조를 생성
    /// </summary>
    public static StageData Generate(int stageIndex, Vector2 spacing)
    {
        var stage = new StageData { stageIndex = stageIndex };
        int id = 0;

        // 1스테이지, 5스테이지는 4열 중간스테이지는 7열
        stage.columnCount = (stageIndex == 1 || stageIndex == 5) ? 4 : 8;

        // 시작 노드 (열 , id  0으로 고정)
        stage.columns.Add(new List<GraphNode> {
            new GraphNode {id = 0, type = NodeType.Start, columnIndex = 0 }
        });

        if (stageIndex == 1)
        {
            GenerateTutorialStage(stage, spacing, ref id);
        }
        else if (stageIndex == 5)
        {
            GenerateBossStage(stage, spacing);
        }
        else
        {
            GenerateStandardStage(stage, spacing, ref id);
        }

        LinkNodes(stage);
        return stage;
    }

    // 튜토리얼 스테이지 구성 (1열~3열 일반 전투)
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

    // 보스 스테이지 구성 (시작, 엘리트, 이벤트, 보스)
    private static void GenerateBossStage(StageData stage, Vector2 spacing)
    {
        stage.columns.Add(new List<GraphNode> {
            new GraphNode { type = NodeType.EliteBattle, columnIndex = 1, position = new Vector2(1 * spacing.x, 0) }
        });
        stage.columns.Add(new List<GraphNode> {
            new GraphNode { type = NodeType.RandomEvent, columnIndex = 2, position = new Vector2(2 * spacing.x, 0) }
        });
        stage.columns.Add(new List<GraphNode> {
            new GraphNode { type = NodeType.Boss, columnIndex = 3, position = new Vector2(3 * spacing.x, 0) }
        });
    }

    // 일반 스테이지 구성 (2~4 스테이지): 노드 타입별 총 14개, 열당 1~3개 분배
    private static void GenerateStandardStage(StageData stage, Vector2 spacing, ref int id)
    {
        // 일반 스테이지에 들어갈 노드들 무작위 배치를 위한 셔플 작업
        var pool = new List<NodeType>();
        pool.AddRange(Enumerable.Repeat(NodeType.NormalBattle, 7));
        pool.AddRange(Enumerable.Repeat(NodeType.EliteBattle, 1));
        pool.AddRange(Enumerable.Repeat(NodeType.RandomEvent, 3));
        pool.AddRange(Enumerable.Repeat(NodeType.Camp, 3));
        pool = pool.OrderBy(_ => Random.value).ToList();

        // 1~5열 까지 고르게 배치하는 로직
        int[] counts = new int[6];
        for (int i = 0; i < 6; i++) counts[i] = 1; // 최소 1개는 배치 하도록 초기값 1로 설정

        int remaining = 14 - 6;                    // 최소 배치하고 남은 노드 수(총 14개 중 각 열마다 1개씩 총 6개 사용)
        List<int> indices = Enumerable.Range(0, 6).ToList();

        // 각 열에 들어 갈 노드 수 랜덤하게 지정
        while (remaining > 0 && indices.Count > 0)
        {
            int i = indices[Random.Range(0, indices.Count)];

            // 각 열마다 최대 3개까지만 노드수 지정
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

        // 각 열에 들어갈 노드 수 정해 졌으면 각 열에 노드 넣어주기
        for (int col = 1; col <= 6; col++)
        {
            var column = new List<GraphNode>();
            int count = counts[col - 1];                   // 현재 열에 들어갈 노드 수
            float totalHeight = (count - 1) * spacing.y;   // 현재 열의 세로 높이 계산

            for (int i = 0; i < count; i++)
            {
                if (pool.Count == 0) break;
                float y = totalHeight / 2f - i * spacing.y; 

                column.Add(new GraphNode
                {
                    id = id++,
                    type = pool[0],
                    columnIndex = col,
                    position = new Vector2(col * spacing.x, y) // 해당 노드의 위치 값 저장
                });

                pool.RemoveAt(0);
            }

            stage.columns.Add(column);
        }

        // 마지막 보스 열 추가
        stage.columns.Add(new List<GraphNode> {
            new GraphNode { id = id++, type = NodeType.Boss, columnIndex = 7, position = new Vector2(7 * spacing.x, 0) }
        });
    }

    // 생성된 노드들 간의 연결 관계 시켜주는 함수
    private static void LinkNodes(StageData stage)
    {
        for (int i = 0; i < stage.columns.Count - 1; i++)
        {
            var fromCol = stage.columns[i];    // 현재 열
            var toCol = stage.columns[i + 1];  // 다음 열

            bool fullyConnect = (i == 0 || i == stage.columns.Count - 2);  

            if (fullyConnect)  
            {
                // 시작열의 노드와 마지막열의 노드는 전부 연결되야함
                foreach (var from in fromCol)
                {
                    foreach (var to in toCol)
                    {
                        from.nextNodes.Add(to);
                    }
                }
            }
            else
            {
                //중간열은 랜덤한 방식으로 연결
                int fromCount = fromCol.Count;    // 현재 열 노드 수 
                int toCount = toCol.Count;        // 다음 열 노드 수
                int toIndex = 0;

                for (int j = 0; j < fromCount; j++)
                {
                    var from = fromCol[j];

                    // 기본 연결
                    if (toIndex < toCount)
                    {
                        from.nextNodes.Add(toCol[toIndex]);
                    }

                    // 50% 확률로 추가적으로 랜덤으로 연결
                    if (toIndex + 1 < toCount && Random.value < 0.5f)
                    {
                        from.nextNodes.Add(toCol[toIndex + 1]);
                    }

                    toIndex = Mathf.Min(toIndex + 1, toCount - 1);
                }

                
                var lastFrom = fromCol[^1];   // 현재 열 마지막 노드
                var lastTo = toCol[^1];       // 다음 열 마지막 노드

                // 해당 열의 마지막 노드일 경우 다음 노드의 마지막과 연결 안되어 있으면 노드 연결 시켜주기
                if (!lastFrom.nextNodes.Contains(lastTo))
                {
                    lastFrom.nextNodes.Add(lastTo);
                }

                // 현재 열의 노드들이 연결된 다음 열의 노드들 확인
                var connectedToNodes = new HashSet<GraphNode>(fromCol.SelectMany(f => f.nextNodes));

                // 연결 안된 노드의 경우 마지막 노드에서 연결
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
