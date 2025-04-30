using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public event Action<float, float> OnHpChanged; // 체력 변경 시 호출할 이벤트 추가

    [SerializeField] private int idNum;
    public int IDNum { get => idNum; set => idNum = value; }

    [SerializeField] private string enemyName;
    public string EnemyName { get => enemyName; set => enemyName = value; }

    [SerializeField] private float maxHP;
    public float MaxHP
    {
        get => maxHP;
        set
        {
            maxHP = value;
            OnHpChanged?.Invoke(currentHP, maxHP); // 최대 체력 변경 시 알림
        }
    }

    [SerializeField] private float currentHP;
    public float CurrentHP
    {
        get => currentHP;
        set
        {
            currentHP = Mathf.Clamp(value, 0, MaxHP);
            OnHpChanged?.Invoke(currentHP, MaxHP); // 현재 체력 변경 시 알림
        }
    }

    [SerializeField] private int aTK;
    public int ATK { get => aTK; set => aTK = value; }

    [SerializeField] private float aTKValue;
    public float ATKValue { get => aTKValue; set => aTKValue = value; }

    [SerializeField] private float dEF;
    public float DEF { get => dEF; set => dEF = value; }

    [SerializeField] private float dEFValue;
    public float DEFValue { get => dEFValue; set => dEFValue = value; }

    [Header("스킬 목록")]
    [SerializeField] private List<EnemySkill> skillList = new();
    public List<EnemySkill> SkillList
    {
        get => skillList;
        set => skillList = value ?? new List<EnemySkill>();
    }

    [System.Serializable]
    public class StancValue
    {
        public enum EStancType
        {
            High,
            Middle,
            Low
        }

        public float defenseBonus;
        public float attackBonus;
    }

    public string illust;       //캐릭터 이미지 이름

    public Sprite enemyImage;   //실제 적용할 캐릭터 이미지


    public StancValue.EStancType currentStance;

    public RuntimeAnimatorController animationController;



    // 스킬 추가
    public void AddSkill(EnemySkill skill)
    {
        if (skill != null && !skillList.Contains(skill))
            skillList.Add(skill);
    }

    // 스킬 모두 삭제
    public void ClearSkills() => skillList.Clear();

    public void LoadIllust()
    {
        if (string.IsNullOrEmpty(illust))
        {
            Debug.LogWarning($"[EnemyData] illust 이름이 비어있습니다.");
            return;
        }

        string path = $"Images/Enemy/{illust}";
        Sprite loaded = Resources.Load<Sprite>(path);

        if (loaded != null)
        {
            enemyImage = loaded;
            Debug.Log($"[EnemyData] '{path}' 이미지 로드 성공.");
        }
        else
        {
            Debug.LogError($"[EnemyData] '{path}' 이미지 로드 실패. 파일이 Resources 안에 있는지 확인하세요.");
        }
    }
}
