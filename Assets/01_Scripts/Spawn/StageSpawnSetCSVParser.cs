using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageSpawnSetCSVParser
{
    [Header("CSV 파일 (Resources 폴더 하위 경로)")]
    public static string csvPath = "ExternalFiles/EnemySpawnSetData"; // Resources 폴더 기준

    public static List<EnemyStageSpawnData> LoadEnemySpawnSet()
    {
        TextAsset csvFile = Resources.Load<TextAsset>(csvPath);
        if (csvFile == null)
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {csvPath}");
            return null;
        }

        var normalizedText = csvFile.text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = normalizedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 파일에 데이터가 없습니다.");
            return null;
        }

        // 헤더 스킵
        var dataLines = lines.Skip(1);

        // stageTheme, type / 스폰세트
        Dictionary<(StageTheme, NodeType), List<EnemySpawnSet>> spawnMap = new();

        foreach (var line in dataLines)
        {
            var cols = line.Split(',');
            if (cols.Length < 6) continue;

            string setName = cols[0];             // 배치 세트 indexName 셋팅

            if (!int.TryParse(cols[1], out int themeInt)) continue;
            StageTheme theme = (StageTheme)themeInt;  // 테마 셋팅

            if (!int.TryParse(cols[2], out int typeInt)) continue;
            NodeType type = (NodeType)typeInt;    // 노드 타입 셋팅


            // Slot By Enemy Data 셋팅
            List<EnemySlotData> slots = new();    

            for (int i = 0; i < 3; i++)
            {
                string raw = cols[3 + i];
                if (int.TryParse(raw, out int enemyId))
                {
                    slots.Add(new EnemySlotData { slotIndex = i, enemyId = enemyId });
                }
            }

            // Enemy Set 추가
            var spawnSet = new EnemySpawnSet
            {
                setName = setName,
                slots = slots
            };

            var key = (theme, type);
            if (!spawnMap.ContainsKey(key))
                spawnMap[key] = new List<EnemySpawnSet>();

            spawnMap[key].Add(spawnSet);
        }

        List<EnemyStageSpawnData> result = new();

        foreach (var kye in spawnMap)
        {
            var data = new EnemyStageSpawnData
            {
                theme = kye.Key.Item1,
                type = kye.Key.Item2,
                spawnSets = kye.Value
            };
            result.Add(data);
        }

        //Debug.Log($"CSV 로드 완료: {result.Count}개의 EnemyStageSpawnData 생성됨");
        return result;
    }
}
