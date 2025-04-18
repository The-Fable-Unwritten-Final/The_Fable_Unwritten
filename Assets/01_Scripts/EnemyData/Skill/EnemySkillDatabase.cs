using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 EnemyAct 스킬 데이터를 저장하고 조회하는 싱글톤 클래스
/// </summary>
public class EnemySkillDatabase : MonoBehaviour
{
    public static EnemySkillDatabase Instance { get; private set; }

    // 인스펙터에서 연결된 스킬 ScriptableObject 목록
    [SerializeField]
    public List<EnemyAct> skillList;

    // 내부에서 인덱스로 빠르게 조회하기 위한 Dictionary
    private Dictionary<int, EnemyAct> skillDict = new();

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSkillDictionary();
    }

    /// <summary>
    /// skillList를 기반으로 Dictionary 초기화
    /// </summary>
    private void InitializeSkillDictionary()
    {
        skillDict.Clear();

        foreach (var act in skillList)
        {
            if (act == null) continue;

            if (skillDict.ContainsKey(act.index))
            {
                Debug.LogWarning($"[EnemySkillDatabase] 중복된 스킬 인덱스 감지: {act.index}");
                continue;
            }

            skillDict[act.index] = act;
        }

        Debug.Log($"[EnemySkillDatabase] 총 {skillDict.Count}개의 스킬 데이터 등록됨");
    }

    /// <summary>
    /// index로 스킬 데이터를 가져오는 함수
    /// </summary>
    /// <param name="index">스킬 인덱스</param>
    /// <returns>해당 EnemyAct 또는 null</returns>
    public EnemyAct Get(int index)
    {
        if (skillDict.TryGetValue(index, out var act))
        {
            return act;
        }

        Debug.LogWarning($"[EnemySkillDatabase] 스킬 인덱스 {index}에 해당하는 데이터가 없습니다.");
        return null;
    }
}
