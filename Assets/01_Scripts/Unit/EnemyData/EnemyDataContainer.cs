using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDataContainer", menuName = "PrefapData/EnemyDataContainer")]
public class EnemyDataContainer : ScriptableObject
{
    [SerializeField] private List<EnemyData> enemyDataList;

    private Dictionary<int, EnemyData> dataDict;

    private void OnEnable()
    {
        dataDict = new Dictionary<int, EnemyData>();

        foreach (var data in enemyDataList)
        {
            if (data != null && !dataDict.ContainsKey(data.IDNum))
            {
                dataDict.Add(data.IDNum, data);
            }
        }
    }

    public EnemyData GetData(int id)
    {
        if (dataDict == null || !dataDict.ContainsKey(id)) return null;

        return dataDict[id];
    }
}
