using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class EnemySkillDataGenerator : MonoBehaviour
{
    private const string EnemySkillSavePath = "Assets/05._ScriptableObjects/Enemy/Skills";

    public static void GenerateFromParsed(List<EnemyAct> parsedList)
    {
        if(!Directory.Exists(EnemySkillSavePath))
            Directory.CreateDirectory(EnemySkillSavePath);

        foreach(var parsed in parsedList)
        {
            string fileName = $"Skill_{parsed.index}.asset";
            string fullPath = Path.Combine(EnemySkillSavePath, fileName);

            var existing = AssetDatabase.LoadAssetAtPath<EnemyAct>(fullPath);
            bool needCreate = existing == null;

            if (!needCreate && IsSame(existing, parsed))
                continue;

            var skillData = existing != null ? existing : ScriptableObject.CreateInstance<EnemyAct>();

            skillData.index = parsed.index;
            skillData.targetType = parsed.targetType;
            skillData.targetNum = parsed.targetNum;
            skillData.target_front = parsed.target_front;
            skillData.target_center = parsed.target_center;
            skillData.target_back = parsed.target_back;
            skillData.atk_buff = parsed.atk_buff;
            skillData.def_buff= parsed.def_buff;
            skillData.buff_time = parsed.buff_time;
            skillData.block = parsed.block;
            skillData.stun = parsed.stun;

            if (existing == null)
            {
                AssetDatabase.CreateAsset(skillData, fullPath);
                Debug.Log($"[EnemySkillDataGenerator] 생성됨: {fileName}");
            }
            else
            {
                EditorUtility.SetDirty(skillData);
                Debug.Log($"[EnemySkillDataGenerator] 갱신됨: {fileName}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static bool IsSame(EnemyAct data, EnemyAct act)
    {
        return
            data.index == act.index &&
            data.targetType == act.targetType &&
            data.targetNum == act.targetNum &&
            data.target_front == act.target_front &&
            data.target_center == act.target_center &&
            data.target_back == act.target_back &&
            Mathf.Approximately(data.atk_buff, act.atk_buff) &&
            Mathf.Approximately(data.def_buff, act.def_buff) &&
            data.buff_time == act.buff_time &&
            data.block == act.block &&
            data.stun == act.stun;
    }
}
